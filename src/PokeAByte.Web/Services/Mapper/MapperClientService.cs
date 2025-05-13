using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Domain.Services.MapperFile;
using PokeAByte.Web.Services.Drivers;

namespace PokeAByte.Web.Services.Mapper;

public class MapperClientService(
    ILogger<MapperClientService> logger,
    IInstanceService instanceService,
    MapperFileService mapperFileService,
    AppSettings appSettings,
    IDriverService driverService
)
{
    private int _currentAttempt = 0;
    public static readonly int MaxAttempts = 10;
    private const int MaxWaitMs = 50;

    //Todo: change this in settings
    public string LoadedDriver { get; set; } = DriverModels.Bizhawk;

    public bool IsCurrentlyConnected => instanceService.Instance != null;

    public async Task<Result> ChangeMapper(string mapperId)
    {
        _currentAttempt = 0;
        var connected = false;
        while (!connected && _currentAttempt < MaxAttempts)
        {
            try
            {
                var driver = await driverService.TestDrivers();
                if (driver == null)
                    return Result.Failure(
                        Error.FailedToLoadMapper,
                        "No driver could connect to an emulator. Check your emulator settings."
                    );
                var result = await ReplaceMapper(mapperId, driver);
                connected = result.IsSuccess;
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                connected = false;
            }
            _currentAttempt += 1;
            await Task.Delay(MaxWaitMs);
        }
        return connected ? Result.Success() : Result.Failure(Error.FailedToLoadMapper, "Max attempts reached.");
    }

    private async Task<Result> ReplaceMapper(string mapperId, IPokeAByteDriver driver)
    {
        await instanceService.StopProcessing();
        try
        {
            var result = await LoadMapper(mapperId, driver);
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

    public Result<List<GlossaryItemModel>> GetGlossaryByReferenceKey(string key)
    {
        var instance = instanceService.Instance;
        if (instance == null)
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
        var instance = instanceService.Instance;
        if (instance == null)
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
        var instance = instanceService.Instance;
        if (instance == null)
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
        await instanceService.StopProcessing();
    }

    private async Task<bool> LoadMapper(string mapperId, IPokeAByteDriver driver)
    {
        // Load the mapper file.
        if (string.IsNullOrEmpty(mapperId))
        {
            throw new ArgumentException("ID was NULL or empty.", nameof(mapperId));
        }

        // Get the file path from the filesystem provider.
        var mapperContent = await mapperFileService.LoadContentAsync(mapperId);
        var instance = instanceService.Instance;
        if (instance == null)
        {
            logger.LogDebug("Poke-A-Byte instance has not been initialized!");
        }
        logger.LogDebug("Replacing mapper.");
        switch (driver)
        {
            case IBizhawkMemoryMapDriver:
                await instanceService.LoadMapper(mapperContent, driver);
                logger.LogDebug("Bizhawk driver loaded.");
                break;
            case IRetroArchUdpPollingDriver:
                await instanceService.LoadMapper(mapperContent, driver);
                logger.LogDebug("Retroarch driver loaded.");
                break;
            case IStaticMemoryDriver:
                await instanceService.LoadMapper(mapperContent, driver);
                logger.LogDebug("Static memory driver loaded.");
                break;
            default:
                if (driver is IPokeAByteDriver) {
                    await instanceService.LoadMapper(mapperContent, driver);
                    logger.LogInformation($"Driver '{driver.ProperName}' loaded.");
                } else {
                    logger.LogError("A valid driver was not supplied.");
                }
                break;
        }
        return instanceService.Instance != null;
    }
}