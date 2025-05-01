using PokeAByte.Application;
using PokeAByte.Application.Mappers;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web;

public class MapperClient(
    ILogger<MapperClient> logger,
    PokeAByteInstance instance,
    AppSettings appSettings,
    IBizhawkMemoryMapDriver bizhawkMemoryMapDriver,
    IRetroArchUdpPollingDriver retroArchUdpPollingDriver,
    IStaticMemoryDriver staticMemoryDriver,
    MapperSettingsService mapperSettings)
{
    public bool IsMapperLoaded => _mapperModel is not null;
    private MapperModel? _mapperModel;
    public async Task<bool> LoadMapper(MapperReplaceModel mapper)
    {
        if (!instance.Initalized)
        {
            logger.LogDebug("Poke-A-Byte instance has not been initialized!");
        }
        logger.LogDebug("Replacing mapper.");
        switch (mapper.Driver)
        {
            case DriverModels.Bizhawk:
                await instance.Load(bizhawkMemoryMapDriver, mapper.Id);
                logger.LogDebug("Bizhawk driver loaded.");
                break;
            case DriverModels.RetroArch:
                await instance.Load(retroArchUdpPollingDriver, mapper.Id);
                logger.LogDebug("Retroarch driver loaded.");
                break;
            case DriverModels.StaticMemory:
                await instance.Load(staticMemoryDriver, mapper.Id);
                logger.LogDebug("Static memory driver loaded.");
                break;
            default:
                logger.LogError("A valid driver was not supplied.");
                break;
        }
        _mapperModel = GetMapper();
        if (instance.Mapper != null) mapperSettings.SetCurrentMapper(instance.Mapper);
        return _mapperModel is not null;
    }
    private MapperModel? GetMapper()
    {
        if (instance.Initalized == false || instance.Mapper == null)
        {
            logger.LogError(Error.ClientInstanceNotInitialized.ToString());
            return null;
        }

        return new MapperModel
        {
            Meta = new MapperMetaModel()
            {
                Id = instance.Mapper.Metadata.Id,
                GameName = instance.Mapper.Metadata.GameName,
                GamePlatform = instance.Mapper.Metadata.GamePlatform,
                MapperReleaseVersion = appSettings.MAPPER_VERSION
            },
            Properties = instance.Mapper
                .Properties
                .Values
                .Select(x => x.MapToPropertyModel())
                .ToArray(),
            Glossary = instance.Mapper
                .References
                .Values
                .MapToDictionaryGlossaryItemModel()
        };
    }
    public async Task UnloadMapper()
    {
        _mapperModel = null;
        await instance.ResetState();
    }
    public MapperMetaModel? GetMetaData()
    {
        if (!IsMapperLoaded || !instance.Initalized)
            return null;
        return _mapperModel!.Meta;
    }

    public IEnumerable<GlossaryItemModel>? GetGlossaryByKey(string key)
    {
        if (!IsMapperLoaded || !instance.Initalized)
            return null;
        var gotVal = _mapperModel!.Glossary.TryGetValue(key, out var val);
        return gotVal ? val : null;
    }

    public IEnumerable<PropertyModel>? GetProperties()
    {
        if (!IsMapperLoaded || !instance.Initalized)
            return null;
        return _mapperModel!
            .Properties
            .AsEnumerable();
    }

    public void UpdateProperty(IPokeAByteProperty property)
    {
        if (!IsMapperLoaded || !instance.Initalized) return;
        _mapperModel!.Properties
            .FirstOrDefault(x => x.Path == property.Path)?
            .UpdatePropertyModel(property);
    }

    public async Task<bool> WriteProperty(string path, string value, bool isFrozen)
    {
        if (!IsMapperLoaded || !instance.Initalized || instance.Mapper is null) return false;
        try
        {
            var prop = instance.Mapper.Properties[path];

            if (prop.IsReadOnly)
            {
                return false;
            }

            await prop.WriteValue(value, isFrozen);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update property.");
            return false;
        }
    }
}