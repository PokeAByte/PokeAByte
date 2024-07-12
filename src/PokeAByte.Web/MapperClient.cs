using Antlr4.Runtime.Misc;
using GameHook.Application;
using GameHook.Domain.Interfaces;
using GameHook.Domain.Models;
using GameHook.Domain.Models.Mappers;
using GameHook.Domain.Models.Properties;
using GameHook.Mappers;
using PokeAByte.Web.ClientNotifiers;
using PokeAByte.Web.Models;

namespace PokeAByte.Web;

public class MapperClient
{    
    //Property Tree 
    private MapperPropertyTree _cachedMapperPropertyTree = new();
    private readonly ILogger<MapperClient> _logger;
    private readonly GameHookInstance _instance;
    private readonly AppSettings _appSettings;
    private readonly IBizhawkMemoryMapDriver _bizhawkMemoryMapDriver;
    private readonly IRetroArchUdpPollingDriver _retroArchUdpPollingDriver;
    private readonly IStaticMemoryDriver _staticMemoryDriver;

    public MapperClient(ILogger<MapperClient> logger,
        GameHookInstance instance,
        AppSettings appSettings,
        IBizhawkMemoryMapDriver bizhawkMemoryMapDriver,
        IRetroArchUdpPollingDriver retroArchUdpPollingDriver,
        IStaticMemoryDriver staticMemoryDriver)
    {
        _logger = logger;
        _instance = instance;
        _appSettings = appSettings;
        _bizhawkMemoryMapDriver = bizhawkMemoryMapDriver;
        _retroArchUdpPollingDriver = retroArchUdpPollingDriver;
        _staticMemoryDriver = staticMemoryDriver;
    }
    public string ConnectionString { get; set; } = "";
    public bool IsMapperLoaded => _mapperModel is not null;
    private MapperModel? _mapperModel;
    public async Task<bool> LoadMapper(MapperReplaceModel mapper)
    {
        if (!_instance.Initalized)
        {
            _logger.LogDebug("Poke-A-Byte instance has not been initialized!");
        }
        _logger.LogDebug("Replacing mapper.");
        switch (mapper.Driver)
        {
            case DriverModels.Bizhawk:
                await _instance.Load(_bizhawkMemoryMapDriver, mapper.Id);
                _logger.LogDebug("Bizhawk driver loaded."); 
                break;
            case DriverModels.Retroarch:
                await _instance.Load(_retroArchUdpPollingDriver, mapper.Id);
                _logger.LogDebug("Retroarch driver loaded.");
                break;
            case DriverModels.StaticMemory:
                await _instance.Load(_staticMemoryDriver, mapper.Id);
                _logger.LogDebug("Static memory driver loaded.");
                break;
            default:
                _logger.LogError("A valid driver was not supplied.");
                break;
        }
        _mapperModel = GetMapper();
        return _mapperModel is not null;
    }

    private MapperModel? GetMapper()
    {
        if (_instance.Initalized == false || _instance.Mapper == null)
        {
            _logger.LogError(Error.ClientInstanceNotInitialized.ToString());
            return null;
        }

        return new MapperModel
        {
            Meta = new MapperMetaModel()
            {
                Id = _instance.Mapper.Metadata.Id,
                GameName = _instance.Mapper.Metadata.GameName,
                GamePlatform = _instance.Mapper.Metadata.GamePlatform,
                MapperReleaseVersion = _appSettings.MAPPER_VERSION
            },
            Properties = _instance.Mapper
                .Properties
                .Values
                .Select(x => x.MapToPropertyModel())
                .ToArray(),
            Glossary = _instance.Mapper
                .References
                .Values
                .MapToDictionaryGlossaryItemModel()
        };
    }
    public void UnloadMapper()
    {
        _mapperModel = null;
    }

    public MapperMetaModel? GetMetaData()
    {
        if (!IsMapperLoaded || !_instance.Initalized)
            return null;
        return _mapperModel!.Meta;
    }

    public Dictionary<string, IEnumerable<GlossaryItemModel>>? GetAllGlossaryItems()
    {
        if (!IsMapperLoaded || !_instance.Initalized)
            return null;
        return _mapperModel!.Glossary;
    }

    public IEnumerable<GlossaryItemModel>? GetGlossaryByKey(string key)
    {
        if (!IsMapperLoaded || !_instance.Initalized)
            return null;
        var gotVal = _mapperModel!.Glossary.TryGetValue(key, out var val);
        return gotVal ? val : null;
    }

    public PropertyModel? GetPropertyByPath(string path)
    {
        if (!IsMapperLoaded || !_instance.Initalized)
            return null;
        return _mapperModel!
            .Properties
            .FirstOrDefault(x => x.Path == path);
    }

    public IEnumerable<PropertyModel>? GetProperties()
    {
        if (!IsMapperLoaded || !_instance.Initalized)
            return null;
        return _mapperModel!
            .Properties
            .AsEnumerable();
    }
    
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

    public void UpdateProperty(IGameHookProperty property)
    {
        if (!IsMapperLoaded || !_instance.Initalized) return;
        _mapperModel!.Properties
            .FirstOrDefault(x => x.Path == property.Path)?
            .UpdatePropertyModel(property);
    }

    public async Task<bool> WriteProperty(string path, string value, bool isFrozen)
    {
        if (!IsMapperLoaded || !_instance.Initalized || _instance.Mapper is null) return false;
        try
        {
            var prop = _instance.Mapper.Properties[path];
            if (prop.IsReadOnly)
                return false;
            await prop.WriteValue(value, isFrozen);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update property.");
            return false;
        }
    }
}