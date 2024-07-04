using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Domain.Models;
using GameHook.Domain.Models.Mappers;
using GameHook.Domain.Models.Properties;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Web.ClientNotifiers;
using PokeAByte.Web.Models;

namespace PokeAByte.Web.Services;

public class MapperClientService
{
    private readonly IMapperFilesystemProvider _mapperFs;
    private readonly ILogger<MapperClientService> _logger;
    private readonly MapperClient _client;
    private readonly PropertyUpdateService _propertyUpdateService;
    public MapperClientService(IMapperFilesystemProvider mapperFs,
        ILogger<MapperClientService> logger,
        MapperClient client,
        IClientNotifier clientNotifier, PropertyUpdateService propertyUpdateService)
    {
        _mapperFs = mapperFs;
        _logger = logger;
        _client = client;
        _propertyUpdateService = propertyUpdateService;
        clientNotifier.PropertyChangedEvent += HandlePropertyChangedEvent;
    }

    private const Color DisconnectedColor = Color.Error;
    private const Color ConnectedColor = Color.Success;

    //Todo: change this in settings
    public string LoadedDriver { get; set; } = DriverModels.Bizhawk;
    public bool IsCurrentlyConnected() => _client.IsMapperLoaded;
    public Color GetCurrentConnectionColor() => _client.IsMapperLoaded ? 
        ConnectedColor : DisconnectedColor;
    public string GetCurrentConnectionName() => _client.IsMapperLoaded ?
        "Connected" : "Disconnected";

    public List<PropertyModel> Properties { get; set; } = [];

    public async Task<Result> ChangeMapper(string mapperId)
    {
        var mapper = new MapperReplaceModel(mapperId, LoadedDriver);
        try
        {
            var result = await _client.LoadMapper(mapper);
            if (result)
            {
                Properties = _client.GetProperties()?.ToList() ?? [];
            }
            return result ? Result.Success() : Result.Failure(Error.FailedToLoadMapper);
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

    /*public Result<List<PropertyModel>> GetProperties()
    {
        var mapperProps = _client.GetProperties();
        var propertyModels = mapperProps?.ToList();
        if (propertyModels is null || propertyModels.Count == 0)
            return Result.Failure<List<PropertyModel>>(Error.NoMapperPropertiesFound);
        return Result.Success(propertyModels);
    }*/

    public Result<List<GlossaryItemModel>> GetGlossaryByReferenceKey(string key)
    {
        var glossaryItems = _client.GetGlossaryByKey(key);
        var glossaryList = glossaryItems?.ToList();
        if(glossaryList is null || glossaryList.Count == 0)
            return Result.Failure<List<GlossaryItemModel>>(Error.NoGlossaryItemsFound);
        return Result.Success(glossaryList);
    }

    public Result<HashSet<MapperPropertyTreeModel>> GetPropertiesHashSet()
    {
        var propTree = _client.GetHashSetTree();
        if (propTree is not null && propTree.Count > 0)
            return Result.Success(propTree);
        return Result
            .Failure<HashSet<MapperPropertyTreeModel>>(
                Error.NoMapperPropertiesFound);
    }

    public Result<MapperMetaModel> GetMetaData()
    {
        if (!_client.IsMapperLoaded)
            return Result.Failure<MapperMetaModel>(Error.MapperNotLoaded);
        var meta = _client.GetMetaData();
        return meta is null ? 
            Result.Failure<MapperMetaModel>(Error.FailedToLoadMetaData) :
            Result.Success(meta);
    }

    public async Task<Result> WritePropertyData(string propertyPath, string value, bool isFrozen)
    {
        if (string.IsNullOrEmpty(propertyPath) ||
            string.IsNullOrEmpty(value))
            return Result.Failure(Error.StringIsNullOrEmpty);
        if (!_client.IsMapperLoaded)
            return Result.Failure<MapperMetaModel>(Error.MapperNotLoaded);
        var path = propertyPath.StripEndingRoute().FromRouteToPath();
        if (await _client.WriteProperty(path, value, isFrozen))
            return Result.Success();
        return Result.Failure(Error.FailedToUpdateProperty);
    }
    private void HandlePropertyChangedEvent(object sender, PropertyChangedEventArgs args)
    {
        if(!_client.IsMapperLoaded)
            return;
        foreach (var prop in args.ChangedProperties)
        {
            _client.UpdateProperty(prop);
            _propertyUpdateService.NotifyChanges(prop.Path);
        }
    }
    public void UpdateEditPropertyModel(EditPropertyModel model)
    {        
        if(!_client.IsMapperLoaded)
            return;
        var prop = _client.GetPropertyByPath(model.Path);
        model.UpdateFromPropertyModel(prop);
    }
}