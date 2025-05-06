using System.Text.Json;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Application;

public class PokeAByteInstance : IPokeAByteInstance
{
    private readonly ILogger<PokeAByteInstance> _logger;
    private ScriptConsole ScriptConsoleAdapter { get; }
    private CancellationTokenSource? ReadLoopToken { get; set; }
    private IMapperFilesystemProvider MapperFilesystemProvider { get; }
    private MemoryAddressBlock[]? BlocksToRead { get; set; }
    public List<IClientNotifier> ClientNotifiers { get; }
    public bool Initalized { get; private set; }
    public IPokeAByteDriver? Driver { get; private set; }
    public IPokeAByteMapper? Mapper { get; private set; }
    public IMemoryManager MemoryContainerManager { get; private set; }
    public Dictionary<string, object?> State { get; private set; }
    public Dictionary<string, object?> Variables { get; private set; }
    private Engine? JavascriptEngine { get; set; }
    private ObjectInstance? JavascriptModuleInstance { get; set; }

    private bool HasPreprocessor { get; set; }
    private bool HasPostprocessor { get; set; }

#if DEBUG
    private bool DebugOutputMemoryLayoutToFilesystem { get; set; } = false;
#endif

    public PokeAByteInstance(
        ILogger<PokeAByteInstance> logger,
        ScriptConsole scriptConsoleAdapter,
        IMapperFilesystemProvider provider,
        IEnumerable<IClientNotifier> clientNotifiers)
    {
        _logger = logger;
        ScriptConsoleAdapter = scriptConsoleAdapter;
        MapperFilesystemProvider = provider;
        ClientNotifiers = clientNotifiers.ToList();

        MemoryContainerManager = new MemoryManager(0);
        State = [];
        Variables = [];
    }

    public async Task ResetState()
    {
        if (ReadLoopToken != null && ReadLoopToken.Token.CanBeCanceled)
        {
            ReadLoopToken.Cancel();
        }

        Initalized = false;
        ReadLoopToken = null;

        Mapper?.Dispose();
        JavascriptEngine?.Dispose();
        Driver?.Disconnect();
        Driver = null;
        Mapper = null;
        BlocksToRead = null;

        JavascriptModuleInstance = null;
        HasPreprocessor = false;
        HasPostprocessor = false;

        MemoryContainerManager = new MemoryManager(0);
        State = [];
        Variables = [];

        await ClientNotifiers.ForEachAsync(async x => await x.SendInstanceReset());
    }

    public async Task Load(IPokeAByteDriver driver, string mapperId)
    {
        try
        {
            await ResetState();

            _logger.LogDebug("Creating PokeAByte mapper instance...");

            Driver = driver;
            await Driver.EstablishConnection();

            // Load the mapper file.
            if (string.IsNullOrEmpty(mapperId))
            {
                throw new ArgumentException("ID was NULL or empty.", nameof(mapperId));
            }

            // Get the file path from the filesystem provider.
            var mapperContent = await MapperFilesystemProvider.LoadContentAsync(mapperId);
            Mapper = PokeAByteMapperXmlFactory.LoadMapperFromFile(this, mapperContent.Xml);
            MemoryContainerManager = new MemoryManager(Mapper.PlatformOptions.MemorySize);

            var engineOptions = new Options
            {
                Strict = true,
                StringCompilationAllowed = false
            };

            if (mapperContent.ScriptRoot != null && mapperContent.ScriptPath != null)
            {

                engineOptions.EnableModules(mapperContent.ScriptRoot, true);

                JavascriptEngine = new Engine(engineOptions)
                    .SetValue("__console", ScriptConsoleAdapter)
                    .SetValue("__state", State)
                    .SetValue("__variables", Variables)
                    .SetValue("__mapper", Mapper)
                    .SetValue("__memory", MemoryContainerManager)
                    .SetValue("__driver", Driver);

                JavascriptModuleInstance = JavascriptEngine.Modules.Import(mapperContent.ScriptPath);
                HasPreprocessor = JavascriptModuleInstance.HasProperty("preprocessor");
                HasPostprocessor = JavascriptModuleInstance.HasProperty("postprocessor");
            }
            else
            {
                JavascriptEngine = new Engine(engineOptions);
            }

            // Calculate the blocks to read from the mapper memory addresses.
            BlocksToRead = Mapper.Memory.ReadRanges.Select(x => new MemoryAddressBlock($"Range {x.Start}", x.Start, x.End)).ToArray();

            if (BlocksToRead.Any())
            {
                _logger.LogInformation($"Using {BlocksToRead.Count()} memory read ranges from mapper.");
            }
            else
            {
                _logger.LogInformation("Using default driver memory read ranges.");
                BlocksToRead = Mapper.PlatformOptions.Ranges;
            }

            // Read twice
            await Read();
            await Read();

            Initalized = true;

            await ClientNotifiers.ForEachAsync(async x => await x.SendMapperLoaded(Mapper));

            // Start the read loop once successfully running once.
            ReadLoopToken = new CancellationTokenSource();
            _ = Task.Run(ReadLoop, ReadLoopToken.Token);

            _logger.LogInformation($"Loaded mapper for {Mapper.Metadata.GameName} ({Mapper.Metadata.Id}).");
        }
        catch
        {
            await ResetState();
            throw;
        }
    }

    private async Task ReadLoop()
    {
        if (Driver == null) throw new Exception("Driver is null.");

        while (ReadLoopToken != null && ReadLoopToken.IsCancellationRequested == false)
        {
            try
            {
                await Read();
                await Task.Delay(Driver.DelayMsBetweenReads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured when read looping the mapper.");
                await ResetState();
            }
        }
    }

    private async Task Read()
    {
        if (Driver == null) throw new Exception("Driver is null.");
        if (Mapper == null) throw new Exception("Mapper is null.");
        if (BlocksToRead == null) throw new Exception("BlocksToRead is null.");

        var driverResult = await Driver.ReadBytes(BlocksToRead);

        foreach (var result in driverResult)
        {
            MemoryContainerManager.DefaultNamespace.Fill(result.Start, result.Data);
        }

#if DEBUG
        if (MemoryContainerManager is not IStaticMemoryDriver && DebugOutputMemoryLayoutToFilesystem)
        {
            var memoryContainerPath = Path.GetFullPath(Path.Combine(BuildEnvironment.BinaryDirectoryPokeAByteFilePath, "..", "..", "..", "..", "..", "..", "PokeAByte.IntegrationTests", "Data", $"{Mapper.Metadata.Id}-0.json"));

            File.WriteAllText(memoryContainerPath, JsonSerializer.Serialize(driverResult));
            DebugOutputMemoryLayoutToFilesystem = false;
        }
#endif

        // Setup at start of loop
        foreach (var property in Mapper.Properties.Values)
        {
            property.FieldsChanged.Clear();
        }

        // Preprocessor
        if (HasPreprocessor)
        {
            if (JavascriptModuleInstance == null) throw new Exception("JavascriptModuleInstance is null.");

            if (JavascriptModuleInstance.Get("preprocessor").Call().ToObject() as bool? == false)
            {
                // The function returned false, which means we do not want to continue.
                return;
            }
        }

        // Processor
        var propertiesChanged = new List<IPokeAByteProperty>();
        object? reloadAddress;
        Variables.TryGetValue("reload_addresses", out reloadAddress);
        foreach (var property in Mapper.Properties.Values)
        {
            try
            {
                property.ProcessLoop(this, MemoryContainerManager, reloadAddress is true);
                if (property.FieldsChanged.Count > 0)
                {
                    propertiesChanged.Add(property);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Property {property.Path} failed to run processor. {ex.Message}");
            }
        }

        // Postprocessor
        if (HasPostprocessor)
        {
            if (JavascriptModuleInstance == null) throw new Exception("JavascriptModuleInstance is null.");

            if (JavascriptModuleInstance.Get("postprocessor").Call().ToObject() as bool? == false)
            {
                // The function returned false, which means we do not want to continue.
                return;
            }
            propertiesChanged.AddRange(
                this.Mapper.Properties.Values
                    .Where(x => !propertiesChanged.Contains(x))
                    .Where(x => x.FieldsChanged.Count > 0)
            );
        }

        // Fields Changed
        if (propertiesChanged.Count > 0)
        {
            try
            {
                foreach (var notifier in ClientNotifiers)
                {
                    await notifier.SendPropertiesChanged(propertiesChanged);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not send {propertiesChanged.Count} property change events.");
                throw new PropertyProcessException($"Could not send {propertiesChanged.Count} property change events.", ex);
            }
        }
    }

    public object? ExecuteModuleFunction(string? function, IPokeAByteProperty property)
    {
        if (string.IsNullOrEmpty(function)) { return null; }

        if (JavascriptEngine == null) throw new Exception("JavascriptEngine is null.");
        if (JavascriptModuleInstance == null) throw new Exception("JavascriptModuleInstance is null.");

        return JavascriptModuleInstance.Get(function).Call(JsValue.FromObject(JavascriptEngine, property)).ToObject();
    }

    public object? ExecuteExpression(string? expression, object x)
    {
        if (expression == null) { throw new Exception($"Expression is NULL when evaluating object {x}."); }
        if (JavascriptEngine == null) throw new Exception("JavascriptEngine is null.");

        return JavascriptEngine.SetValue("x", x).Evaluate(expression).ToObject();
    }

    public bool? GetModuleFunctionResult(string? function, IPokeAByteProperty property) => ExecuteModuleFunction(function, property) as bool?;
}
