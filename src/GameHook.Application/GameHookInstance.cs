using GameHook.Domain;
using GameHook.Domain.Implementations;
using GameHook.Domain.Interfaces;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace GameHook.Application
{
    public class GameHookInstance : IGameHookInstance
    {
        private readonly ILogger<GameHookInstance> _logger;
        private readonly AppSettings _appSettings;
        private ScriptConsole ScriptConsoleAdapter { get; }
        private CancellationTokenSource? ReadLoopToken { get; set; }
        private IMapperFilesystemProvider MapperFilesystemProvider { get; }
        private IEnumerable<MemoryAddressBlock>? BlocksToRead { get; set; }
        public List<IClientNotifier> ClientNotifiers { get; }
        public bool Initalized { get; private set; }
        public IGameHookDriver? Driver { get; private set; }
        public IGameHookMapper? Mapper { get; private set; }
        public IPlatformOptions? PlatformOptions { get; private set; }
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

        private Stopwatch ReadLoopStopwatch { get; set; } = new();
        private Stopwatch ReadDriverStopwatch { get; set; } = new();
        private Stopwatch PreprocessorStopwatch { get; set; } = new();
        private Stopwatch ProcessorStopwatch { get; set; } = new();
        private Stopwatch PostprocessorStopwatch { get; set; } = new();
        private Stopwatch FieldsChangedStopwatch { get; set; } = new();

        public GameHookInstance(ILogger<GameHookInstance> logger, AppSettings appSettings, ScriptConsole scriptConsoleAdapter, IMapperFilesystemProvider provider, IEnumerable<IClientNotifier> clientNotifiers)
        {
            _logger = logger;
            _appSettings = appSettings;
            ScriptConsoleAdapter = scriptConsoleAdapter;
            MapperFilesystemProvider = provider;
            ClientNotifiers = clientNotifiers.ToList();

            MemoryContainerManager = new MemoryManager();
            State = [];
            Variables = [];
        }

        private async Task ResetState()
        {
            if (ReadLoopToken != null && ReadLoopToken.Token.CanBeCanceled)
            {
                ReadLoopToken.Cancel();
            }

            Initalized = false;
            ReadLoopToken = null;

            Driver = null;
            Mapper = null;
            PlatformOptions = null;
            BlocksToRead = null;

            JavascriptModuleInstance = null;

            State = [];
            Variables = [];

            await ClientNotifiers.ForEachAsync(async x => await x.SendInstanceReset());
        }

        public async Task Load(IGameHookDriver driver, string mapperId)
        {
            try
            {
                await ResetState();

                _logger.LogDebug("Creating GameHook mapper instance...");

                Driver = driver;

                await Driver.EstablishConnection();

                // Load the mapper file.
                if (string.IsNullOrEmpty(mapperId))
                {
                    throw new ArgumentException("ID was NULL or empty.", nameof(mapperId));
                }

                // Get the file path from the filesystem provider.
                var mapperFile = MapperFilesystemProvider.MapperFiles.SingleOrDefault(x => x.Id == mapperId) ??
                                 throw new Exception($"Unable to determine a mapper with the ID of {mapperId}.");

                if (File.Exists(mapperFile.AbsolutePath) == false)
                {
                    throw new FileNotFoundException($"File was not found in the {mapperFile.Type} mapper folder.", mapperFile.DisplayName);
                }

                var mapperContents = await File.ReadAllTextAsync(mapperFile.AbsolutePath);
                if (mapperFile.AbsolutePath.EndsWith(".xml"))
                {
                    Mapper = GameHookMapperXmlFactory.LoadMapperFromFile(this, mapperFile.AbsolutePath, mapperContents);
                }
                else
                {
                    throw new Exception($"Invalid file extension for mapper.");
                }

                PlatformOptions = Mapper.Metadata.GamePlatform switch
                {
                    "NES" => new NES_PlatformOptions(),
                    "SNES" => new SNES_PlatformOptions(),
                    "GB" => new GB_PlatformOptions(),
                    "GBC" => new GBC_PlatformOptions(),
                    "GBA" => new GBA_PlatformOptions(),
                    "PSX" => new PSX_PlatformOptions(),
                    "NDS" => new NDS_PlatformOptions(),
                    _ => throw new Exception($"Unknown game platform {Mapper.Metadata.GamePlatform}.")
                };

                var engineOptions = new Options
                {
                    Strict = true,
                    StringCompilationAllowed = false
                };

                var javascriptAbsolutePath = mapperFile.AbsolutePath.Replace(".xml", ".js");
                if (File.Exists(javascriptAbsolutePath))
                {
                    var rootDirectory = MapperFilesystemProvider.GetMapperRootDirectory(javascriptAbsolutePath);
                    engineOptions.EnableModules(rootDirectory, true);

                    JavascriptEngine = new Engine(engineOptions)
                        .SetValue("__console", ScriptConsoleAdapter)
                        .SetValue("__state", State)
                        .SetValue("__variables", Variables)
                        .SetValue("__mapper", Mapper)
                        .SetValue("__memory", MemoryContainerManager)
                        .SetValue("__driver", Driver);

                    JavascriptModuleInstance = JavascriptEngine.Modules.Import(MapperFilesystemProvider.GetRelativePath(javascriptAbsolutePath));

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

                    BlocksToRead = PlatformOptions.Ranges.ToList();
                }

                // Read once
                await Read();

                // Read twice
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

                    if (_appSettings.SHOW_READ_LOOP_STATISTICS)
                    {
                        _logger.LogInformation($"ReadLoop took {ReadLoopStopwatch.ElapsedMilliseconds}ms, ReadDriver took {ReadDriverStopwatch.ElapsedMilliseconds}ms, Preprocessor took {PreprocessorStopwatch.ElapsedMilliseconds}ms, Processor took {ProcessorStopwatch.ElapsedMilliseconds}ms, Postprocessor took {PostprocessorStopwatch.ElapsedMilliseconds}ms FieldsChanged took {FieldsChangedStopwatch.ElapsedMilliseconds}ms. Javascript took {PreprocessorStopwatch.ElapsedMilliseconds + PostprocessorStopwatch.ElapsedMilliseconds}ms. Everything else took {ReadLoopStopwatch.ElapsedMilliseconds - PreprocessorStopwatch.ElapsedMilliseconds - PostprocessorStopwatch.ElapsedMilliseconds}ms.");
                    }

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
            if (PlatformOptions == null) throw new Exception("Platform options are null.");
            if (Mapper == null) throw new Exception("Mapper is null.");
            if (BlocksToRead == null) throw new Exception("BlocksToRead is null.");

            ReadLoopStopwatch.Restart();

            ReadDriverStopwatch.Restart();

            var driverResult = await Driver.ReadBytes(BlocksToRead);

            foreach (var result in driverResult)
            {
                MemoryContainerManager.DefaultNamespace.Fill(result.Key, result.Value);
            }

            ReadDriverStopwatch.Stop();

#if DEBUG
            if (MemoryContainerManager is not IStaticMemoryDriver && DebugOutputMemoryLayoutToFilesystem)
            {
                var memoryContainerPath = Path.GetFullPath(Path.Combine(BuildEnvironment.BinaryDirectoryGameHookFilePath, "..", "..", "..", "..", "..", "..", "GameHook.IntegrationTests", "Data", $"{Mapper.Metadata.Id}-0.json"));

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
            PreprocessorStopwatch.Restart();

            if (HasPreprocessor)
            {
                if (JavascriptModuleInstance == null) throw new Exception("JavascriptModuleInstance is null.");

                if (JavascriptModuleInstance.Get("preprocessor").Call().ToObject() as bool? == false)
                {
                    // The function returned false, which means we do not want to continue.
                    return;
                }
            }

            PreprocessorStopwatch.Stop();

            // Processor
            ProcessorStopwatch.Restart();

            foreach (var property in Mapper.Properties.Values)
            {
                try
                {
                    property.ProcessLoop(MemoryContainerManager);
                }
                catch (Exception ex)
                {
                    throw new PropertyProcessException($"Property {property.Path} failed to run processor. {ex.Message}", ex);
                }
            }

            ProcessorStopwatch.Stop();

            // Postprocessor
            PostprocessorStopwatch.Restart();

            if (HasPostprocessor)
            {
                if (JavascriptModuleInstance == null) throw new Exception("JavascriptModuleInstance is null.");

                if (JavascriptModuleInstance.Get("postprocessor").Call().ToObject() as bool? == false)
                {
                    // The function returned false, which means we do not want to continue.
                    return;
                }
            }

            PostprocessorStopwatch.Stop();

            // Fields Changed
            FieldsChangedStopwatch.Restart();

            var propertiesChanged = Mapper.Properties.Values.Where(x => x.FieldsChanged.Any()).ToArray();
            if (propertiesChanged.Length > 0)
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
                    _logger.LogError(ex, $"Could not send {propertiesChanged.Length} property change events.");
                    throw new PropertyProcessException($"Could not send {propertiesChanged.Length} property change events.", ex);
                }
            }

            FieldsChangedStopwatch.Stop();

            ReadLoopStopwatch.Stop();
        }

        public object? ExecuteModuleFunction(string? function, IGameHookProperty property)
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

        public bool? GetModuleFunctionResult(string? function, IGameHookProperty property) => ExecuteModuleFunction(function, property) as bool?;
    }
}
