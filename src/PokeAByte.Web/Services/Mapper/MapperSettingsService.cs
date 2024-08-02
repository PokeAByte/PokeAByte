﻿using System.Text.Json;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Web.Models;

namespace PokeAByte.Web.Services.Mapper;

public class MapperSettingsService
{
    private List<MapperSettingsModel> _savedMappers = [];
    private MapperSettingsModel? _currentMapperModel;
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
        if(string.IsNullOrWhiteSpace(jsonStr))
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
                Properties = []
            };
            //add it to the list
            _savedMappers.Add(newMapper);
            //set it as the current mapper
            _currentMapperModel = newMapper;
            //Persist data to disk
            SaveSettings();
        }
        else
        {
            //found mapper, set it as the current mapper
            _currentMapperModel = result.ResultValue;
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

    public void OnPropertyExpandedHandler(PropertyTreePresenter prop)
    {
        if (_currentMapperModel is null)
            return;
        if (_currentMapperModel.MapperGuid != prop.Value?.MapperId)
            return;
        var partialPath = prop.Value.FullPath[..prop.Value.FullPath.LastIndexOf('.')];
        //find the entry with path
        var foundEntry = _currentMapperModel
            .Properties
            .FirstOrDefault(x => x.PropertyPath == partialPath);

        if (prop.Expanded)
        {
            //if we found the entry just update it
            if (foundEntry is not null)
            {
                foundEntry.IsExpanded = true;
            }
            else
            {
                //otherwise create it
                var newPropModel = new PropertySettingsModel
                {
                    PropertyName = prop.Value.Name,
                    PropertyPath = partialPath,
                    IsExpanded = true,
                    HasChildren = prop.HasChildren,
                    HasProperty = prop.Value.PropertyModel is not null
                };
                _currentMapperModel.Properties.Add(newPropModel);
            }
        }
        else
        {
            //minimize
            //get all child paths
            var foundChildren = _currentMapperModel
                .Properties
                .Where(x => x.PropertyPath
                    .Contains(partialPath, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            //remove all children
            foreach (var child in foundChildren)
            {
                _currentMapperModel.Properties.Remove(child);
            }

            if (foundEntry is not null)
            {
                //remove self
                _currentMapperModel.Properties.Remove(foundEntry);
            }
        }
        var savedMapper = _savedMappers
            .FirstOrDefault(x => x == _currentMapperModel);
        if (savedMapper is null)
            _savedMappers.Add(_currentMapperModel);
        else
            savedMapper = _currentMapperModel;
        SaveSettings();
    }
    /*public void OnPropertyExpandedHandler(PropertyTreePresenter prop)
    {
        if (_currentMapperModel is null)
            return;
        if (_currentMapperModel.MapperGuid != prop.Value?.MapperId)
            return;
        var partialPath = prop.Value.FullPath[..prop.Value.FullPath.LastIndexOf('.')];
        //try to find it
        var found = _currentMapperModel
            .Properties
            .FirstOrDefault(x => x.PropertyPath == partialPath);
        if (prop.Expanded)
        {
            if (found is not null)
            {
                //update it
                found.IsExpanded = prop.Expanded;
            }
            else
            {
                //create it
                var newPropModel = new PropertySettingsModel
                {
                    PropertyName = prop.Value.Name,
                    PropertyPath = partialPath,
                    IsExpanded = prop.Expanded,
                    HasChildren = prop.HasChildren,
                    HasProperty = prop.Value.PropertyModel is not null
                };
                _currentMapperModel.Properties.Add(newPropModel);
            }
        }
        else
        {
            if (found is not null)
            {
                //remove it
                _currentMapperModel.Properties.Remove(found);
            }
            else
            {
                //parent is minimized
                //Find all children
                var children = _currentMapperModel
                    .Properties
                    .Where(x => x
                        .PropertyPath
                        .Contains(partialPath, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                //remove all children
                foreach (var child in children)
                {
                    _currentMapperModel.Properties.Remove(child);
                }
            }
        }
        
        var savedMapper = _savedMappers
            .FirstOrDefault(x => x == _currentMapperModel);
        if (savedMapper is null)
            _savedMappers.Add(_currentMapperModel);
        else
            savedMapper = _currentMapperModel;
        SaveSettings();
    }*/

    /*public void OnPropertyExpandedHandler(PropertyTreePresenter prop)
    {
        if (_currentMapperModel is null)
            return;
        if (_currentMapperModel.MapperGuid != prop.Value?.MapperId)
            return;
        var propModel = _currentMapperModel.Properties
            .FirstOrDefault(x => 
                x.PropertyPath == prop.Value.FullPath && 
                x.PropertyName == prop.Value.Name);
        if (propModel is null)
        {
            //make sure we don't add parents back in
            var childFound = _currentMapperModel
                .Properties
                .Any(x =>
                    x.PropertyPath != prop.Value.Name &&
                    x.PropertyPath.Contains(prop.Value.Name));
            if (childFound)
                return;
            var almostFullPath = prop.Value.FullPath[..prop.Value.FullPath.LastIndexOf('.')];
            var newPropModel = new PropertySettingsModel
            {
                PropertyName = prop.Value.Name,
                PropertyPath = almostFullPath,
                IsExpanded = prop.Expanded,
                HasChildren = prop.HasChildren,
                HasProperty = prop.Value.PropertyModel is not null
            };
            _currentMapperModel.Properties.Add(newPropModel);
        }
        else
        {
            propModel.IsExpanded = prop.Expanded;
            if (propModel.IsExpanded == false)
            {            
                //remove from list
                _currentMapperModel.Properties.RemoveAll(x => 
                    prop.Value.FullPath == x.PropertyPath);
            }
        }

        var savedMapper = _savedMappers
            .FirstOrDefault(x => x == _currentMapperModel);
        if (savedMapper is null)
            _savedMappers.Add(_currentMapperModel);
        else
            savedMapper = _currentMapperModel;
        SaveSettings();
    }*/
    public void OnPropertyExpandedHandler(OldMapperPropertyTreeModel prop)
    {
        //find the property
        if (_currentMapperModel is null)
            return;
        if (_currentMapperModel.MapperGuid != prop.MapperId)
            return;
        var propModel = _currentMapperModel.Properties
            .FirstOrDefault(x => 
                x.PropertyPath == prop.FullPath && 
                x.PropertyName == prop.Name);
        if (propModel is null)
        {
            //make sure we don't add parents back in
            var childFound = _currentMapperModel
                .Properties
                .Any(x => x.PropertyPath.Contains(prop.Name));
            if (childFound)
                return;
            var newPropModel = new PropertySettingsModel
            {
                PropertyName = prop.Name,
                PropertyPath = prop.FullPath,
                IsExpanded = prop.IsExpanded,
                HasChildren = prop.HasChildren,
                HasProperty = prop.Property is not null
            };
            _currentMapperModel.Properties.Add(newPropModel);
        }
        else
        {
            propModel.IsExpanded = prop.IsExpanded;
            if (propModel.IsExpanded == false)
            {            
                //remove from list
                _currentMapperModel.Properties.RemoveAll(x => 
                    prop.FullPath == x.PropertyPath);
            }
        }

        var savedMapper = _savedMappers
            .FirstOrDefault(x => x == _currentMapperModel);
        if (savedMapper is null)
            _savedMappers.Add(_currentMapperModel);
        else
            savedMapper = _currentMapperModel;
        SaveSettings();
    }

    public List<PropertySettingsModel>? GetMapperModelProperties()
    {
        if (_currentMapperModel is null)
            return null;
        return _currentMapperModel
            .Properties.Select(x => new PropertySettingsModel
            {
                PropertyName = x.PropertyName,
                PropertyPath = x.PropertyPath,
                IsExpanded = x.IsExpanded,
                HasChildren = x.HasChildren,
                HasProperty = x.HasProperty
            })
            .ToList();
    }
    public void InitializePropertyExpansions(HashSet<OldMapperPropertyTreeModel> tree)
    {
        if (_currentMapperModel is null)
            return;
        var props = _currentMapperModel
            .Properties.Select(x => new PropertySettingsModel
            {
                PropertyName = x.PropertyName,
                PropertyPath = x.PropertyPath,
                IsExpanded = x.IsExpanded,
                HasChildren = x.HasChildren,
                HasProperty = x.HasProperty
            })
        .ToList();
        foreach (var prop in props)
        {
            var splitPaths = prop.PropertyPath.Split('.');
            var currentBranch = tree
                .FirstOrDefault(x => x.FullPath == splitPaths[0]);
            if(currentBranch is null)
                continue;
            foreach (var path in splitPaths)
            {
                while (currentBranch.Name != path)
                {
                    currentBranch = currentBranch
                        .Children
                        .First(x => x.Name == path);
                    currentBranch.ShouldTriggerExpandedAction = false;
                    currentBranch.IsExpanded = prop.IsExpanded;
                    currentBranch.ShouldTriggerExpandedAction = true;
                }

                if (currentBranch.Name == path)
                {
                    currentBranch.ShouldTriggerExpandedAction = false;
                    currentBranch.IsExpanded = prop.IsExpanded;
                    currentBranch.ShouldTriggerExpandedAction = true;
                }
                /*if (path == currentBranch.Name)
                {
                    currentBranch.ShouldTriggerExpandedAction = false;
                    currentBranch.IsExpanded = prop.IsExpanded;
                    currentBranch.ShouldTriggerExpandedAction = true;
                }
                else
                {
                    while (currentBranch.Name != path)
                    {
                        currentBranch = currentBranch
                            .Children
                            .First(x => x.Name == path);
                        currentBranch.ShouldTriggerExpandedAction = false;
                        currentBranch.IsExpanded = prop.IsExpanded;
                        currentBranch.ShouldTriggerExpandedAction = true;
                    }
                }*/
            }
        }
    }
}