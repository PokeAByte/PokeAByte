using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Web.Controllers;
using PokeAByte.Web.Services.Drivers;

namespace PokeAByte.Web.Services.Mapper;

public class MapperClientService(
    IMapperFilesystemProvider mapperFs,
    ILogger<MapperClientService> logger,
    IPokeAByteInstance instance,
    AppSettings appSettings,
    DriverService driverService
)
{
    private int _currentAttempt = 0;
    public static readonly int MaxAttempts = 10;
    private const int MaxWaitMs = 50;

    //Todo: change this in settings
    public string LoadedDriver { get; set; } = DriverModels.Bizhawk;

    public bool IsCurrentlyConnected
    {
        get => instance.Initalized && instance.Mapper != null;
    }

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
                var driverResult = await driverService.TestDrivers(driverTestActionHandler);
                if (string.IsNullOrWhiteSpace(driverResult))
                    return Result.Failure(Error.FailedToLoadMapper,
                        "No driver could connect to an emulator. Check your emulator settings.");
                LoadedDriver = driverResult;
                var result = await ReplaceMapper(mapperId);
                connected = result.IsSuccess;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
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
        await instance.ResetState();
        var mapper = new MapperReplaceModel(mapperId, LoadedDriver);
        try
        {
            var result = await LoadMapper(mapper);
            return result
                ? Result.Success()
                : Result.Failure(Error.FailedToLoadMapper, "Please see logs for more info.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to load mapper.");
            return Result.Exception(e);
        }
    }

    public IEnumerable<MapperFileModel> GetMappers()
    {
        mapperFs.CacheMapperFiles();
        return mapperFs.MapperFiles.Select(x => new MapperFileModel()
        {
            Id = x.Id,
            DisplayName = x.DisplayName
        });
    }

    public Result<List<GlossaryItemModel>> GetGlossaryByReferenceKey(string key)
    {
        if (instance.Initalized == false || instance.Mapper == null)
        {
            return Result.Failure<List<GlossaryItemModel>>(Error.NoGlossaryItemsFound);
        }
        var gotVal = instance.Mapper.References.TryGetValue(key, out var referenceItems);
        if (gotVal && referenceItems != null)
        {
            return Result.Success(
                referenceItems.Values
                    .Select(x => new GlossaryItemModel(x.Key, x.Value))
                    .ToList()
            );
        }
        return Result.Failure<List<GlossaryItemModel>>(Error.NoGlossaryItemsFound);
    }

    public Result<MapperMetaModel> GetMetaData()
    {
        if (instance.Initalized == false || instance.Mapper == null)
        {
            return Result.Failure<MapperMetaModel>(Error.MapperNotLoaded);
        }

        return Result.Success(
            new MapperMetaModel()
            {
                Id = instance.Mapper.Metadata.Id,
                GameName = instance.Mapper.Metadata.GameName,
                GamePlatform = instance.Mapper.Metadata.GamePlatform,
                MapperReleaseVersion = appSettings.MAPPER_VERSION
            }
        );
    }

    public async Task<Result> WritePropertyData(string propertyPath, string value, bool isFrozen)
    {
        if (string.IsNullOrEmpty(propertyPath) || string.IsNullOrEmpty(value))
        {
            return Result.Failure(Error.StringIsNullOrEmpty);
        }
        if (!instance.Initalized || instance.Mapper is null)
        {
            return Result.Failure<MapperMetaModel>(Error.MapperNotLoaded);
        }

        var path = propertyPath.StripEndingRoute().FromRouteToPath();
        try
        {
            var prop = instance.Mapper.Properties[path];

            if (prop.IsReadOnly)
            {
                return Result.Failure(Error.FailedToUpdateProperty);
            }

            await prop.WriteValue(value, isFrozen);
            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update property.");
            return Result.Failure(Error.FailedToUpdateProperty);
        }
    }

    public async Task UnloadMapper()
    {
        if (!instance.Initalized || instance.Mapper == null)
        {
            return;
        }

        await instance.ResetState();
    }

    private async Task<bool> LoadMapper(MapperReplaceModel mapper)
    {
        if (!instance.Initalized || instance.Mapper == null)
        {
            logger.LogDebug("Poke-A-Byte instance has not been initialized!");
        }
        logger.LogDebug("Replacing mapper.");
        switch (mapper.Driver)
        {
            case DriverModels.Bizhawk:
                await instance.Load(driverService.Bizhawk, mapper.Id);
                logger.LogDebug("Bizhawk driver loaded.");
                break;
            case DriverModels.RetroArch:
                await instance.Load(driverService.RetroArch, mapper.Id);
                logger.LogDebug("Retroarch driver loaded.");
                break;
            case DriverModels.StaticMemory:
                await instance.Load(driverService.StaticMemory, mapper.Id);
                logger.LogDebug("Static memory driver loaded.");
                break;
            default:
                logger.LogError("A valid driver was not supplied.");
                break;
        }
        return instance.Initalized && instance.Mapper != null;
    }
}