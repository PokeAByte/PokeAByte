using System.Diagnostics;
using GameHook.Domain.Models.Mappers;
using GameHook.Mappers;
using Microsoft.AspNetCore.Components;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.MapperManager.ArchiveManager;

public partial class BackupArchive : ComponentBase
{
    [Inject] public MapperManagerService ManagerService { get; set; }
    private List<VisualMapperDto> _mapperList = [];
    private string _mapperListFilter = "";
    private List<VisualMapperDto> _mapperListFiltered =>
        _mapperList.Where(x => x.MapperDto.Search(_mapperListFilter)).ToList();
    public Action<HashSet<VisualMapperDto>>? OnSelectedItemsChanged { get; set; }
    private HashSet<VisualMapperDto> _selectedMapperListFromTable = [];
    private List<MapperDto> _selectedMappersToBackup = [];
    private bool _isDataLoading = true;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        OnSelectedItemsChanged += OnSelectedItemsChangedHandler;
        GetMapperList();
    }

    private void ResetSelection()
    {
        foreach(var selected in 
                _mapperList.Where(x => x.IsSelected))
        {
            selected.IsSelected = false;
        }
    }
    private void OnSelectedItemsChangedHandler(HashSet<VisualMapperDto> items)
    {
        ResetSelection();
        _selectedMapperListFromTable = items;
        foreach (var item in items)
        {
            item.IsSelected = true;
        }
        StateHasChanged();
    }

    private void GetMapperList()
    {
        _isDataLoading = true;
        _mapperList = ManagerService
            .GetMapperList()
            .Select(x => new VisualMapperDto(x))
            .ToList();
        _mapperListFilter = "";
        _selectedMappersToBackup = [];
        _selectedMapperListFromTable = [];
        _isDataLoading = false;
        StateHasChanged();
    }

    private void OnClickReloadMapperList()
    {
        GetMapperList();
    }

    private void OnClickArchiveAll()
    {
        throw new NotImplementedException();
    }

    private void OnClickArchiveSelected()
    {
        throw new NotImplementedException();
    }

    private void OnClickOpenMapperDirectory()
    {
        Process.Start("explorer.exe",MapperEnvironment.MapperLocalDirectory);
    }

    private void OnClickOpenArchiveDirectory()
    {
        Process.Start("explorer.exe",MapperEnvironment.MapperLocalArchiveDirectory);
    }
}