using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Mapper;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.PokeAByteProperties;

namespace PokeAByte.Domain.Logic;

public class MapperProblem : IProblemDetails
{
    public MapperProblem(string title, string detail)
    {
        Title = title;
        Detail = detail;
    }

    public string Title { get; }
    public string Detail { get; }
}

public class PokeAByteInstance : IPokeAByteInstance
{
    private readonly ILogger<PokeAByteInstance> _logger;
    private ScriptConsole ScriptConsoleAdapter { get; }
    private CancellationTokenSource ReadLoopToken { get; set; }
    private BlockData[] _transferBlocks;
    private Engine JavascriptEngine { get; set; }
    private ObjectInstance? JavascriptModuleInstance { get; set; }
    private Lock _jsModuleLock = new Lock();

    [MemberNotNullWhen(true, nameof(JavascriptModuleInstance))]
    private bool HasPreprocessor { get; set; }

    [MemberNotNullWhen(true, nameof(JavascriptModuleInstance))]
    private bool HasPostprocessor { get; set; }

    [MemberNotNullWhen(true, nameof(JavascriptModuleInstance))]
    private bool HasContainerProcessor { get; set; }

    public event InstanceProcessingAbort? OnProcessingAbort;
    public IClientNotifier ClientNotifier { get; }
    public IPokeAByteDriver Driver { get; private set; }
    public IPokeAByteMapper Mapper { get; private set; }
    public IMemoryManager MemoryContainerManager { get; private set; }
    public Dictionary<string, object?> State { get; private set; }
    public Dictionary<string, object?> Variables { get; private set; }
    private Task? _readLoopTask = null;

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
        Mapper = PokeAByteMapperXmlFactory.LoadMapperFromFile(mapperContent.Xml, mapperContent.FileId);
        MemoryAddressBlock[]? blocksToRead = Mapper.PlatformOptions.Ranges;
        // Calculate the blocks to read from the mapper memory addresses.
        if (Driver is not IBizhawkMemoryMapDriver)
        {
            if (Mapper.Memory.ReadRanges.Any())
            {
                blocksToRead = Mapper.Memory.ReadRanges
                    .Select(x => new MemoryAddressBlock($"Range {x.Start}", x.Start, x.End + 1))
                    .ToArray();
                _logger.LogInformation($"Using {Mapper.Memory.ReadRanges.Count()} memory read ranges from mapper.");
            }
            else
            {
                _logger.LogInformation("Using default driver memory read ranges.");
            }
        }
        else
        {
            _logger.LogInformation("Bizhawk integration tool does not support custom read ranges. Using defaults.");
        }


        var memory = new MemoryManager(blocksToRead);
        MemoryContainerManager = memory;
        _transferBlocks = new BlockData[blocksToRead.Length];
        int i = 0;
        foreach (var block in blocksToRead)
        {
            _transferBlocks[i] = new BlockData(
                block.StartingAddress,
                memory.GetDefaultMemory(block.StartingAddress, block.EndingAddress)
            );
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
        // Makes some javascript exceptions easier to follow with better stacktraces:
        engineOptions.Interop.ExceptionHandler = (_) => true;

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

            JavascriptEngine.Modules
                .Add("game_functions", b => b.ExportType<ScriptModules.PokemonFunctions>("pokemon"));
            JavascriptModuleInstance = JavascriptEngine.Modules.Import(mapperContent.ScriptPath);
            HasPreprocessor = JavascriptModuleInstance.HasProperty("preprocessor");
            HasPostprocessor = JavascriptModuleInstance.HasProperty("postprocessor");
            HasContainerProcessor = JavascriptModuleInstance.HasProperty("containerprocessor");
        }
        else
        {
            JavascriptEngine = new Engine(engineOptions);
        }
    }

    public async Task StartProcessing()
    {
        try
        {
            // Read twice
            await Read();
            await Read();
        }
        catch (Exception ex)
        {
            if (ex is JavaScriptException jsException)
            {
                var location = jsException.Location;
                throw new MapperException($"Error in mapper script: {jsException.Message}", ex);
            }
            else
            {
                await this.ClientNotifier.SendError(new MapperProblem("Error", ex.Message));
            }
            if (OnProcessingAbort != null)
            {
                await OnProcessingAbort.Invoke();
            }
            throw;
        }

        await ClientNotifier.SendMapperLoaded(Mapper);
        // Start the read loop once successfully running once.

        _readLoopTask = Task.Run(ReadLoop);
        _logger.LogInformation($"Loaded mapper for {Mapper.Metadata.GameName} ({Mapper.Metadata.Id}).");
    }

    private async Task ReadLoop()
    {
        try
        {
            while (ReadLoopToken.IsCancellationRequested == false)
            {
                if (HasContainerProcessor)
                {
                    foreach (var item in this.MemoryContainerManager.Namespaces)
                    {
                        if (item.Value is DynamicMemoryContainer dynamicContainer && dynamicContainer.IsDirty)
                        {
                            this.ExecuteContainerProcessor(item.Key, dynamicContainer.GetAllBytes());
                            dynamicContainer.ClearDirtyFlag();
                        }
                    }
                }
                await Read();
                await Task.Delay(Driver.DelayMsBetweenReads);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occured when read looping the mapper.");
            await this.ClientNotifier.SendError(new MapperProblem("Error", ex.Message));
            if (OnProcessingAbort != null)
            {
                await OnProcessingAbort.Invoke();
            }
        }
    }

    List<IPokeAByteProperty> _propertiesChanged = [];

    private async Task Read()
    {
        await Driver.ReadBytes(_transferBlocks);

#if DEBUG
        if (MemoryContainerManager is not IStaticMemoryDriver && DebugOutputMemoryLayoutToFilesystem)
        {
            var memoryContainerPath = Path.GetFullPath(Path.Combine(BuildEnvironment.BinaryDirectoryPokeAByteFilePath, "..", "..", "..", "..", "..", "..", "PokeAByte.IntegrationTests", "Data", $"{Mapper.Metadata.Id}-0.json"));

            File.WriteAllText(memoryContainerPath, JsonSerializer.Serialize(_transferBlocks));
            DebugOutputMemoryLayoutToFilesystem = false;
        }
#endif

        // Preprocessor
        if (HasPreprocessor)
        {
            lock (_jsModuleLock)
            {
                if (JavascriptModuleInstance.Get("preprocessor").Call().ToObject() as bool? == false)
                {
                    // The function returned false, which means we do not want to continue.
                    return;
                }
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
                await ClientNotifier.SendError(new MapperProblem("Error", ex.Message));
            }
        }

        // Postprocessor
        if (HasPostprocessor)
        {
            lock (_jsModuleLock)
            {
                if (JavascriptModuleInstance.Get("postprocessor").Call().ToObject() as bool? == false)
                {
                    // The function returned false, which means we do not want to continue.
                    return;
                }
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
                throw new MapperException($"Could not send property change events.", ex);
            }
            finally
            {
                // Reset property change tracking:
                foreach (var property in _propertiesChanged)
                {
                    property.FieldsChanged = FieldChanges.None;
                }
                _propertiesChanged.Clear();
            }
        }
    }

    public object? ExecuteModuleFunction(string function, IPokeAByteProperty property)
    {
        if (JavascriptModuleInstance == null) throw new Exception("JavascriptModuleInstance is null.");
        object? result;
        lock (_jsModuleLock)
        {
            result = JavascriptModuleInstance.Get(function)
                .Call(JsValue.FromObject(JavascriptEngine, property))
                .ToObject();
        }
        return result;
    }

    public void ExecuteContainerProcessor(string container, byte[] bytes)
    {
        if (JavascriptModuleInstance == null) throw new Exception("JavascriptModuleInstance is null.");
        lock (_jsModuleLock)
        {
            JavascriptModuleInstance.Get("containerprocessor")?.Call(
                JsValue.FromObject(JavascriptEngine, container),
                JsValue.FromObject(JavascriptEngine, bytes)
            );
        }
    }

    public object? ExecuteExpression(string expression, object x)
    {
        return JavascriptEngine.SetValue("x", x).Evaluate(expression).ToObject();
    }

    public bool? GetModuleFunctionResult(string function, IPokeAByteProperty property) => ExecuteModuleFunction(function, property) as bool?;

    public async ValueTask DisposeAsync()
    {
        await ReadLoopToken.CancelAsync();
        if (_readLoopTask != null)
        {
            await _readLoopTask;
        }
        Mapper.Dispose();
        JavascriptEngine?.Dispose();
        await Driver.Disconnect();
        await ClientNotifier.SendInstanceReset();
    }

    public async Task WriteValue(IPokeAByteProperty target, object? value, bool? freeze)
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
            ReferenceItem? item = null;
            if (value is long numericValue)
            {
                item = reference.GetSingleOrDefaultByKey((ulong)numericValue);
            }
            if (item == null)
            {
                item = reference.GetFirstByValue(value);
            }
            bytes = BitConverter.GetBytes(item.Key);
        }
        else
        {
            var stringValue = value?.ToString();
            if (stringValue == null)
            {
                throw new Exception($"Can not write null to property '{property.Path}'");
            }
            bytes = property.BytesFromValue(stringValue, Mapper);
        }

        if (property.BitIndexes != null)
        {
            var inputBits = new BitArray(bytes);
            var outputBits = new BitArray(property.Bytes);
            var combinedBytes = new byte[property.Bytes.Length];
            for (var i = 0; i < property.BitIndexes.Length; i++)
            {
                outputBits[property.BitIndexes[i]] = inputBits[i];
            }
            outputBits.CopyTo(combinedBytes, 0);
            bytes = combinedBytes;
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

        if (property.MemoryContainer != null && property.MemoryContainer != "default")
        {
            var container = (DynamicMemoryContainer)MemoryContainerManager.Namespaces[property.MemoryContainer];
            container.Fill((MemoryAddress)property.Address, bytes);
            this.ExecuteContainerProcessor(property.MemoryContainer, container.GetAllBytes());
            return;
        }
        if (freeze == true)
        {
            await FreezeProperty(property, bytes);
        }
        else if (freeze == false)
        {
            await UnfreezeProperty(property);
        }
        else if (property.IsFrozen)
        {
            property.BytesFrozen = bytes.ToArray();
        }

        await Driver.WriteBytes((MemoryAddress)property.Address, bytes);
    }

    public async Task FreezeProperty(IPokeAByteProperty target, byte[] bytesFrozen)
    {
        if (target is PokeAByteProperty property)
        {
            property.BytesFrozen = bytesFrozen;
            // We have to handle bitfield freezes with normal writes, unforunately. So we have to check that "Bits"
            // is not set or else we create problems as the emulator would write back the entire byte-array unchanged
            // instead of only the bitfield.
            if (property.Bits == null && property.Address != null && Driver is IPokeAByteFreezeDriver freezeDriver)
            {
                await freezeDriver.Freeze(property.Address.Value, bytesFrozen);
            }
        }
    }

    public async Task UnfreezeProperty(IPokeAByteProperty target)
    {
        if (target is PokeAByteProperty property)
        {
            property.BytesFrozen = [];
            // Again, exclude bitfield properties, see above.
            if (property.Bits == null && Driver is IPokeAByteFreezeDriver freezeDriver && property.Address != null)
            {
                await freezeDriver.Unfreeze(property.Address.Value);
            }
        }
    }

}
