using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Web.Services.Drivers;

namespace PokeAByte.Web.Services.Mapper;

public class MapperClientService
{
    private readonly IMapperFilesystemProvider _mapperFs;
    private readonly ILogger<MapperClientService> _logger;
    public readonly MapperClient Client;
    private readonly DriverService _driverService;
    private int _currentAttempt = 0;
    public static readonly int MaxAttempts = 10;
    private const int MaxWaitMs = 50;
    public string? LoadedMapperName { get; private set; }

    public MapperClientService(
        IMapperFilesystemProvider mapperFs,
        ILogger<MapperClientService> logger,
        MapperClient client,
        DriverService driverService)
    {
        _mapperFs = mapperFs;
        _logger = logger;
        Client = client;
        _driverService = driverService;
    }
    //Todo: change this in settings
    public string LoadedDriver { get; set; } = DriverModels.Bizhawk;

    public event Action? OnMapperIsUnloaded;

    public bool IsCurrentlyConnected
    {
        get
        {
            if (Client.IsMapperLoaded is false)
            {
                OnMapperIsUnloaded?.Invoke();
            }
            return Client.IsMapperLoaded;
        }
    }

    public List<PropertyModel> Properties { get; set; } = [];

    public async Task<Result> ChangeMapper(string mapperId,
        Action<int>? driverTestActionHandler,
        Action<int>? mapperTestActionHandler)
    {
        _currentAttempt = 0;
        var connected = false;
        while (!connected && _currentAttempt < MaxAttempts)
        {
            try
            {
                var driverResult = await _driverService.TestDrivers(driverTestActionHandler);
                if (string.IsNullOrWhiteSpace(driverResult))
                    return Result.Failure(Error.FailedToLoadMapper,
                        "No driver could connect to an emulator. Check your emulator settings.");
                LoadedDriver = driverResult;
                var result = await ReplaceMapper(mapperId);
                connected = result.IsSuccess;
                LoadedMapperName = mapperId;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                connected = false;
            }
            _currentAttempt += 1;
            mapperTestActionHandler?.Invoke(_currentAttempt);
            await Task.Delay(MaxWaitMs);
        }
        return connected ? Result.Success() : Result.Failure(Error.FailedToLoadMapper, "Max attempts reached.");
    }

    private async Task<Result> ReplaceMapper(string mapperId)
    {
        await Client.UnloadMapper();
        var mapper = new MapperReplaceModel(mapperId, LoadedDriver);
        try
        {
            var result = await Client.LoadMapper(mapper);
            if (result)
            {
                Properties = Client.GetProperties()?.ToList() ?? [];
            }
            return result
                ? Result.Success()
                : Result.Failure(Error.FailedToLoadMapper, "Please see logs for more info.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load mapper.");
            return Result.Exception(e);
        }
    }

    public IEnumerable<MapperFileModel> GetMappers()
    {
        _mapperFs.CacheMapperFiles();
        return _mapperFs.MapperFiles.Select(x => new MapperFileModel()
        {
            Id = x.Id,
            DisplayName = x.DisplayName
        });
    }

    public Result<List<GlossaryItemModel>> GetGlossaryByReferenceKey(string key)
    {
        var glossaryItems = Client.GetGlossaryByKey(key);
        var glossaryList = glossaryItems?.ToList();
        if (glossaryList is null || glossaryList.Count == 0)
            return Result.Failure<List<GlossaryItemModel>>(Error.NoGlossaryItemsFound);
        return Result.Success(glossaryList);
    }

    public Result<MapperMetaModel> GetMetaData()
    {
        if (!Client.IsMapperLoaded)
            return Result.Failure<MapperMetaModel>(Error.MapperNotLoaded);
        var meta = Client.GetMetaData();
        return meta is null ?
            Result.Failure<MapperMetaModel>(Error.FailedToLoadMetaData) :
            Result.Success(meta);
    }

    public async Task<Result> WritePropertyData(string propertyPath, string value, bool isFrozen)
    {
        if (string.IsNullOrEmpty(propertyPath) ||
            string.IsNullOrEmpty(value))
            return Result.Failure(Error.StringIsNullOrEmpty);
        if (!Client.IsMapperLoaded)
            return Result.Failure<MapperMetaModel>(Error.MapperNotLoaded);
        var path = propertyPath.StripEndingRoute().FromRouteToPath();
        //Console.WriteLine($"{path}: {value}");
        if (await Client.WriteProperty(path, value, isFrozen))
            return Result.Success();
        return Result.Failure(Error.FailedToUpdateProperty);
    }

    public async Task UnloadMapper()
    {
        if (!Client.IsMapperLoaded)
            return;
        await Client.UnloadMapper();
    }
}