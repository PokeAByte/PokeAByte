using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Mapper;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.PokeAByteProperties;

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
        Mapper = PokeAByteMapperXmlFactory.LoadMapperFromFile(mapperContent.Xml);
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

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(IPokeAByteMapper))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(IMemoryNamespace))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(IByteArray))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(IPokeAByteProperty))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PokeAByteProperty))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(System.Dynamic.ExpandoObject))]
    [MemberNotNull(nameof(JavascriptEngine))]
    private void InitializeJSEngine(MapperContent mapperContent)
    {
        var engineOptions = new Options { Strict = true };

        engineOptions.Host.StringCompilationAllowed = false;

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

    List<IPokeAByteProperty> _propertiesChanged = [];

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
        _propertiesChanged.Clear();
        foreach (var property in Mapper.Properties.Values)
        {
            property.FieldsChanged = FieldChanges.None;
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
        foreach (var property in Mapper.Properties.Values)
        {
            if (property.FieldsChanged != FieldChanges.None)
            {
                _propertiesChanged.Add(property);
            }
        }
        if (_propertiesChanged.Count > 0)
        {
            try
            {
                await ClientNotifier.SendPropertiesChanged(_propertiesChanged);
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

    public async Task WriteValue(IPokeAByteProperty target, string value, bool? freeze)
    {
        if (target is not PokeAByteProperty property)
        {
            return;
        }
        if (property.IsReadOnly)
        {
            return;
        }
        if (property.Bytes.Length == 0)
        {
            throw new Exception("Bytes array is empty.");
        }

        byte[] bytes;

        if (property.ShouldRunReferenceTransformer)
        {
            var reference = property.GetComputedReference(Mapper);
            if (reference == null) throw new Exception("Glossary is NULL.");
            bytes = BitConverter.GetBytes(reference.GetFirstByValue(value).Key);
        }
        else
        {
            bytes = property.BytesFromValue(value, Mapper);
        }

        if (property.BitIndexes != null)
        {
            var inputBits = new BitArray(bytes);
            var outputBits = new BitArray(property.Bytes);

            for (var i = 0; i < property.BitIndexes.Length; i++)
            {
                outputBits[property.BitIndexes[i]] = inputBits[i];
            }
            outputBits.CopyTo(bytes, 0);
        }

        if (property.BeforeWriteValueFunction != null && GetModuleFunctionResult(property.BeforeWriteValueFunction, property) == false)
        {
            // They want to do it themselves entirely in Javascript.
            return;
        }

        await WriteBytes(property, bytes, freeze);
    }

    public async Task WriteBytes(IPokeAByteProperty target, byte[] bytesToWrite, bool? freeze)
    {
        if (target is not PokeAByteProperty property)
        {
            return;
        }
        if (property.Address == null) throw new Exception($"{property.Path} does not have an address. Cannot write data to an empty address.");

        byte[] bytes = new byte[property.Length];

        // Overlay the bytes onto the buffer.
        // This ensures that we can't overflow the property.
        // It also ensures it can't underflow the property, it copies the remaining from Bytes.
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i < bytesToWrite.Length)
            {
                bytes[i] = bytesToWrite[i];
            }
            else if (property.Bytes.Length > 0)
            {
                bytes[i] = property.Bytes[i];
            }
        }

        if (property.WriteFunction != null && GetModuleFunctionResult(property.WriteFunction, property) == false)
        {
            // They want to do it themselves entirely in Javascript.
            return;
        }

        if (freeze == true)
        {
            // The property is frozen, but we want to write bytes anyway.
            // So this should replace the existing frozen bytes.
            property.BytesFrozen = bytes;
        }

        if (bytes.Length != property.Length)
        {
            throw new Exception($"Something went wrong with attempting to write bytes for {property.Path}. The bytes to write and the length of the property do not match. Will not proceed.");
        }

        await Driver.WriteBytes((MemoryAddress)property.Address, bytes);

        if (freeze == true)
        {
            await this.FreezeProperty(property, bytes);
        }
        else if (freeze == false)
        {
            await UnfreezeProperty(property);
        }
    }

    public async Task FreezeProperty(IPokeAByteProperty target, byte[] bytesFrozen)
    {
        if (target is PokeAByteProperty property)
        {
            property.BytesFrozen = bytesFrozen;
            await ClientNotifier.SendPropertiesChanged([property]);
        }
    }

    public async Task UnfreezeProperty(IPokeAByteProperty target)
    {
        if (target is PokeAByteProperty property)
        {
            property.BytesFrozen = [];
            await ClientNotifier.SendPropertiesChanged([property]);
        }
    }

}
