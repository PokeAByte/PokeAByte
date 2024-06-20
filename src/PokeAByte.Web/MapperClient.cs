using GameHook.Domain.Interfaces;
using GameHook.Domain.Models;
using GameHook.Domain.Models.Mappers;
using GameHook.Domain.Models.Properties;
using GameHook.Mappers;
using PokeAByte.Web.Models;

namespace PokeAByte.Web;

public class MapperClient(
    ILogger<MapperClient> logger,
    IGameHookInstance instance,
    AppSettings appSettings,
    IBizhawkMemoryMapDriver bizhawkMemoryMapDriver,
    IRetroArchUdpPollingDriver retroArchUdpPollingDriver,
    IStaticMemoryDriver staticMemoryDriver)
{
    public string ConnectionString { get; set; } = "";
    public bool IsMapperLoaded => _mapperModel is not null;
    private MapperModel? _mapperModel;
    public async Task<Result> LoadMapper(MapperReplaceModel mapper)
    {
        logger.LogDebug("Replacing mapper.");
        switch (mapper.Driver)
        {
            case DriverModels.Bizhawk:
                await instance.Load(bizhawkMemoryMapDriver, mapper.Id);
                logger.LogDebug("Bizhawk driver loaded."); 
                break;
            case DriverModels.Retroarch:
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
        var getMapperResult = GetMapper();
        if (getMapperResult.IsSuccess)
            _mapperModel = getMapperResult.ResultValue;
        else
            return getMapperResult;
        return Result.Success();
    }

    private Result<MapperModel> GetMapper()
    {
        if (instance.Initalized == false || instance.Mapper == null)
            return Result.Failure<MapperModel>(Error.ClientInstanceNotInitialized);
        return Result.Success(new MapperModel
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
        });
    }
    
    public void UnloadMapper()
    {
        _mapperModel = null;
    }

    public MapperMetaModel? GetMetaData() => 
        _mapperModel?.Meta;
    
    public PropertyModel? GetPropertyByPath(string path)
    {
        return _mapperModel?
            .Properties
            .FirstOrDefault(x => x.Path == path);
    }

    public IEnumerable<PropertyModel>? GetProperties() => _mapperModel?
        .Properties?
        .AsEnumerable();
    
    //Property Tree 
    private MapperPropertyTree _cachedMapperPropertyTree = new();

    public HashSet<MapperPropertyTreeModel>? GetHashSetTree()
    {
        if (_cachedMapperPropertyTree.Tree.Count > 0)
            return _cachedMapperPropertyTree.Tree;
        
        var props = GetProperties();
        if (props is null)
            return null;
        
        var propList = props.ToList();
        foreach (var prop in propList)
        {
            _cachedMapperPropertyTree.AddProperty(prop);
        }

        return _cachedMapperPropertyTree.Tree;
    }

    public void ClearCachedHashSetTree()
    {
        _cachedMapperPropertyTree.Dispose();
        _cachedMapperPropertyTree = new MapperPropertyTree();
    }
}