using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Drivers;

namespace PokeAByte.Web.Services.Mapper;

public class MapperClientService(
    ILogger<MapperClientService> logger,
    IInstanceService instanceService,
    IMapperService mapperService,
    IDriverService driverService)
{
    private int _currentAttempt = 0;
    private static readonly int MaxAttempts = 10;
    private const int MaxWaitMs = 50;

    /// <summary>
    /// Whether Poke-A-Byte currently has a connection to an emulator and a mapper loaded.
    /// </summary>
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
                if (result.ExceptionValue is MapperException pokeAByteException)
                {
                    return Result.Failure(Error.FailedToLoadMapper, pokeAByteException.Message);
                }
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
        await UnloadMapper();
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

        return Result.Success(MapperMetaModel.FromMapperSection(instance.Mapper.Metadata));
    }

    public async Task<Result> WritePropertyData(string propertyPath, object? value, bool isFrozen)
    {
        if (string.IsNullOrEmpty(propertyPath))
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

            await instance.WriteValue(prop, value, isFrozen);
            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update property.");
            return Result.Failure(Error.FailedToUpdateProperty);
        }
    }

    /// <summary>
    /// Stop processing of the current mapper, disconnect the driver and unload the mapper.
    /// </summary>
    /// <returns> An awaitable task. </returns>
    public async Task UnloadMapper()
    {
        await instanceService.StopProcessing();
    }

    private async Task<bool> LoadMapper(string path, IPokeAByteDriver driver)
    {
        // Load the mapper file.
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path must not be NULL or empty.", nameof(path));
        }
        var mapperContent = await mapperService.LoadContentAsync(path);
        var instance = instanceService.Instance;
        if (instance == null)
        {
            logger.LogDebug("Poke-A-Byte instance has not been initialized!");
        }
        logger.LogDebug("Replacing mapper.");

        await instanceService.LoadMapper(mapperContent, driver);
        logger.LogInformation($"'{driver.ProperName}' driver loaded.");
        return instanceService.Instance != null;
    }
}