using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Mapper;
using PokeAByte.Domain.Models;

namespace PokeAByte.Domain.Logic;

public class PokeAByteInstance : IPokeAByteInstance
{
    private readonly ILogger<PokeAByteInstance> _logger;
    private ScriptConsole ScriptConsoleAdapter { get; }
    private CancellationTokenSource ReadLoopToken { get; set; }
    private BlockData[] _transferBlocks;
    private Engine JavascriptEngine { get; set; }
    private ObjectInstance? JavascriptModuleInstance { get; set; }

    [MemberNotNullWhen(true, nameof(JavascriptModuleInstance))]
    private bool HasPreprocessor { get; set; }
    [MemberNotNullWhen(true, nameof(JavascriptModuleInstance))]
    private bool HasPostprocessor { get; set; }

    public event InstanceProcessingAbort? OnProcessingAbort;
    public IClientNotifier ClientNotifier { get; }
    public IPokeAByteDriver Driver { get; private set; }
    public IPokeAByteMapper Mapper { get; private set; }
    public IMemoryManager MemoryContainerManager { get; private set; }
    public Dictionary<string, object?> State { get; private set; }
    public Dictionary<string, object?> Variables { get; private set; }

#if DEBUG
    private bool DebugOutputMemoryLayoutToFilesystem { get; set; } = false;
#endif

    public PokeAByteInstance(
        ILogger<PokeAByteInstance> logger,
        ScriptConsole scriptConsoleAdapter,
        IClientNotifier clientNotifier,
        MapperContent mapperContent,
        IPokeAByteDriver driver)
    {
        _logger = logger;
        ScriptConsoleAdapter = scriptConsoleAdapter;
        Driver = driver;
        State = [];
        Variables = [];
        ClientNotifier = clientNotifier;
        ReadLoopToken = new CancellationTokenSource();

        // Get the file path from the filesystem provider.
        Mapper = PokeAByteMapperXmlFactory.LoadMapperFromFile(this, mapperContent.Xml);
        // Calculate the blocks to read from the mapper memory addresses.
        var blocksToRead = Mapper.Memory.ReadRanges.Select(x => new MemoryAddressBlock($"Range {x.Start}", x.Start, x.End)).ToArray();
        if (blocksToRead.Any())
        {
            _logger.LogInformation($"Using {blocksToRead.Count()} memory read ranges from mapper.");
        }
        else
        {
            _logger.LogInformation("Using default driver memory read ranges.");
            blocksToRead = Mapper.PlatformOptions.Ranges;
        }
        var lastBlock = blocksToRead.OrderByDescending(x => x.EndingAddress).First();
        MemoryContainerManager = new MemoryManager(lastBlock.EndingAddress);
        _transferBlocks = new BlockData[blocksToRead.Length];
        int i = 0;
        foreach (var block in blocksToRead)
        {
            _transferBlocks[i] = new BlockData(block.StartingAddress, new byte[block.EndingAddress - block.StartingAddress]);
            i++;
        }

        InitializeJSEngine(mapperContent);
    }

    [MemberNotNull(nameof(JavascriptEngine))]
    private void InitializeJSEngine(MapperContent mapperContent)
    {
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
    }

    public async Task StartProcessing()
    {
        // Read twice
        await Read();
        await Read();

        await ClientNotifier.SendMapperLoaded(Mapper);
        // Start the read loop once successfully running once.

        _ = Task.Run(ReadLoop, ReadLoopToken.Token);
        _logger.LogInformation($"Loaded mapper for {Mapper.Metadata.GameName} ({Mapper.Metadata.Id}).");
    }

    private async Task ReadLoop()
    {
        while (ReadLoopToken.IsCancellationRequested == false)
        {
            try
            {
                await Read();
                await Task.Delay(Driver.DelayMsBetweenReads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured when read looping the mapper.");
                if (OnProcessingAbort != null)
                {
                    await OnProcessingAbort.Invoke();
                }
            }
        }
    }

    private async Task Read()
    {
        await Driver.ReadBytes(_transferBlocks);

        foreach (var result in _transferBlocks)
        {
            MemoryContainerManager.DefaultNamespace.Fill(result.Start, result.Data);
        }

#if DEBUG
        if (MemoryContainerManager is not IStaticMemoryDriver && DebugOutputMemoryLayoutToFilesystem)
        {
            var memoryContainerPath = Path.GetFullPath(Path.Combine(BuildEnvironment.BinaryDirectoryPokeAByteFilePath, "..", "..", "..", "..", "..", "..", "PokeAByte.IntegrationTests", "Data", $"{Mapper.Metadata.Id}-0.json"));

            File.WriteAllText(memoryContainerPath, JsonSerializer.Serialize(_transferBlocks));
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
            if (JavascriptModuleInstance.Get("preprocessor").Call().ToObject() as bool? == false)
            {
                // The function returned false, which means we do not want to continue.
                return;
            }
        }

        // Processor
        Variables.TryGetValue("reload_addresses", out object? reloadAddress);
        foreach (var property in Mapper.Properties.Values)
        {
            try
            {
                property.ProcessLoop(this, MemoryContainerManager, reloadAddress is true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Property {property.Path} failed to run processor. {ex.Message}");
            }
        }

        // Postprocessor
        if (HasPostprocessor)
        {
            if (JavascriptModuleInstance.Get("postprocessor").Call().ToObject() as bool? == false)
            {
                // The function returned false, which means we do not want to continue.
                return;
            }
        }

        // Fields Changed
        var propertiesChanged = this.Mapper.Properties.Values
            .Where(x => x.FieldsChanged.Count > 0)
            .ToArray();
        if (propertiesChanged.Length > 0) {
            try
            {
                await ClientNotifier.SendPropertiesChanged(propertiesChanged);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not send property change events.");
                throw new PropertyProcessException($"Could not send property change events.", ex);
            }
        }
    }

    public object? ExecuteModuleFunction(string function, IPokeAByteProperty property)
    {
        if (JavascriptModuleInstance == null) throw new Exception("JavascriptModuleInstance is null.");

        return JavascriptModuleInstance.Get(function).Call(JsValue.FromObject(JavascriptEngine, property)).ToObject();
    }

    public object? ExecuteExpression(string expression, object x)
    {
        return JavascriptEngine.SetValue("x", x).Evaluate(expression).ToObject();
    }

    public bool? GetModuleFunctionResult(string function, IPokeAByteProperty property) => ExecuteModuleFunction(function, property) as bool?;

    public async ValueTask DisposeAsync()
    {
        if (ReadLoopToken.Token.CanBeCanceled)
        {
            ReadLoopToken.Cancel();
        }
        Mapper.Dispose();
        JavascriptEngine?.Dispose();
        await Driver.Disconnect();
        await ClientNotifier.SendInstanceReset();
    }
}
