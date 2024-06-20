using GameHook.Domain.Interfaces;
using GameHook.Domain.Models;
using GameHook.Domain.Models.Mappers;
using GameHook.Domain.Models.Properties;
using MudBlazor;
using PokeAByte.Web.Models;

namespace PokeAByte.Web.Services;

public class MapperConnectionService(
    IMapperFilesystemProvider mapperFs,
    ILogger<MapperConnectionService> logger,
    MapperClient client)
{
    private const Color DisconnectedColor = Color.Error;
    private const Color ConnectedColor = Color.Success;

    //Todo: change this in settings
    public string LoadedDriver { get; set; } = DriverModels.Bizhawk;
    public bool IsCurrentlyConnected() => client.IsMapperLoaded;
    public Color GetCurrentConnectionColor() => client.IsMapperLoaded ? 
        ConnectedColor : DisconnectedColor;
    public string GetCurrentConnectionName() => client.IsMapperLoaded ?
        "Connected" : "Disconnected";

    public async Task<Result> ChangeMapper(string mapperId)
    {
        var mapper = new MapperReplaceModel(mapperId, LoadedDriver);
        try
        {
            var result = await client.LoadMapper(mapper);
            return result;
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

    public Result<List<PropertyModel>> GetProperties()
    {
        var mapperProps = client.GetProperties();
        var propertyModels = mapperProps?.ToList();
        if (propertyModels is null || propertyModels.Count == 0)
            return Result.Failure<List<PropertyModel>>(Error.NoMapperPropertiesFound);
        return Result.Success(propertyModels);
    }

    public Result<HashSet<MapperPropertyTreeModel>> GetPropertiesHashSet()
    {
        var propTree = client.GetHashSetTree();
        if (propTree is not null && propTree.Count > 0)
            return Result.Success(propTree);
        return Result
            .Failure<HashSet<MapperPropertyTreeModel>>(
                Error.NoMapperPropertiesFound);
    }

    public Result<MapperMetaModel> GetMetaData()
    {
        if (!client.IsMapperLoaded)
            return Result.Failure<MapperMetaModel>(Error.MapperNotLoaded);
        var meta = client.GetMetaData();
        return meta is null ? 
            Result.Failure<MapperMetaModel>(Error.FailedToLoadMetaData) :
            Result.Success(meta);
    }
}