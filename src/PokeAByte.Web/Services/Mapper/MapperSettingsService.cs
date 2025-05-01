using System.Text.Json;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Web.Models;

namespace PokeAByte.Web.Services.Mapper;

public class MapperSettingsService
{
    private List<MapperSettingsModel> _savedMappers = [];
    private ILogger<MapperSettingsService> _logger { get; set; }

    public MapperSettingsService(ILogger<MapperSettingsService> logger)
    {
        _logger = logger;
        LoadMappers();
    }

    private void LoadMappers()
    {
        if (!File.Exists(BuildEnvironment.MapperSettingsJson)) return;
        var jsonStr = File.ReadAllText(BuildEnvironment.MapperSettingsJson);
        if (string.IsNullOrWhiteSpace(jsonStr))
            return;
        _savedMappers = JsonSerializer.Deserialize<List<MapperSettingsModel>>(jsonStr) ?? [];
    }

    public void SetCurrentMapper(IPokeAByteMapper mapper)
    {
        //Check to see if the mapper exists
        var result = TryLoadMapper(mapper.Metadata.Id, mapper.Metadata.GameName);
        if (!result.IsSuccess)
        {
            //failed to find the mapper, create a new one
            var newMapper = new MapperSettingsModel
            {
                MapperGuid = mapper.Metadata.Id,
                MapperName = mapper.Metadata.GameName,
            };
            //add it to the list
            _savedMappers.Add(newMapper);
            //Persist data to disk
            SaveSettings();
        }
    }

    private Result<MapperSettingsModel> TryLoadMapper(Guid id, string name)
    {
        if (_savedMappers.Count == 0)
            return Result.Failure<MapperSettingsModel>(Error.ListIsEmpty,
                $"The saved mappers list is empty. Mapper Guid: {id}, Mapper Name: {name}");
        var mapper = _savedMappers
            .FirstOrDefault(x => x.MapperGuid == id && x.MapperName == name);
        if (mapper is null)
            return Result.Failure<MapperSettingsModel>(Error.FailedToFindMapper,
                $"Failed to find the saved mapper settings for {name} with id ${id}");
        return Result.Success(mapper);
    }

    private Result SaveSettings()
    {
        var jsonData = JsonSerializer.Serialize(_savedMappers);
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            var result = Result.Failure(Error.FailedToSaveData,
                $"Failed to save settings because the json was empty.");
            _logger.LogError(result.ToString());
            return result;
        }

        try
        {
            File.WriteAllText(BuildEnvironment.MapperSettingsJson, jsonData);
            return Result.Success();
        }
        catch (Exception e)
        {
            var result = Result.Exception(e);
            _logger.LogError(result.ToString(), e);
            return result;
        }
    }
}