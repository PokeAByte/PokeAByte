using System.Diagnostics;
using GameHook.Domain;
using GameHook.Mappers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.MapperManager.ArchiveManager;

public partial class RestoreArchive : ComponentBase
{
    [Inject] public MapperManagerService MapperManagerService { get; set; }
    private bool _isDataLoading;
    private string _mapperListFilter = "";

    private List<MapperArchiveModel> _archivedMappers = [];

    private List<MapperArchiveModel> ArchivedMapperListFiltered =>
        _archivedMappers.Where(x =>
            x.BasePath.Contains(_mapperListFilter, StringComparison.InvariantCultureIgnoreCase) ||
            x.MapperList
                .Any(k => 
                    k.Key.Contains(_mapperListFilter, StringComparison.InvariantCultureIgnoreCase)) ||
            x.MapperList
                .Any(v => 
                    v.Value.Any(y => y.Mapper.Search(_mapperListFilter))))
            .ToList();

    public Action<HashSet<MapperArchiveModel>> OnSelectedItemsChanged { get; set; }
    public Action<TableRowClickEventArgs<MapperArchiveModel>> OnRowClick { get; set; }
    private Action<HashSet<KeyValuePair<string, List<ArchivedMapperDto>>>>? _onSelectedChildItemChanged;
    private List<MapperArchiveModel> _selectedArchivedMappers = [];
    private HashSet<MapperArchiveModel> _selectedValuesFromTable = [];
    protected override void OnInitialized()
    {
        OnSelectedItemsChanged += OnSelectedItemsChangedHandler;
        OnRowClick += OnRowClickHandler;
        OnSelectedItemsChanged += OnSelectedItemsChangedHandler;
        base.OnInitialized();
        GetArchivedMappers();
    }

    private void GetArchivedMappers()
    {
        _selectedArchivedMappers = [];
        ResetSelection();
        _selectedValuesFromTable = [];
        _archivedMappers = MapperManagerService
            .RefreshArchivedMappersList();
        /*_archivedMappers = MapperManagerService.RefreshArchivedMappersList() ?? 
            new Dictionary<string, IReadOnlyList<ArchivedMapperDto>>().AsReadOnly();*/
        StateHasChanged();
    }
    private void OnClickRestoreSelected()
    {
        throw new NotImplementedException();
    }

    private void OnClickRestoreAll()
    {
        throw new NotImplementedException();
    }

    private void OnClickReloadMapperList()
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

    private void OnRowClickHandler(TableRowClickEventArgs<MapperArchiveModel> args)
    {
        ResetSelection();
        foreach (var item in _selectedValuesFromTable)
        {
            item.IsSelected = true;
        }
    }

    private void ResetSelection()
    {
        foreach (var item in _selectedValuesFromTable.Where(x => x.IsSelected))
        {
            item.IsSelected = false;
        }
    }
    private void OnSelectedItemsChangedHandler(HashSet<MapperArchiveModel> selectedValues)
    {
        _selectedValuesFromTable = selectedValues;
    }
    private void OnSelectedChildItemsChangedHandler(
        HashSet<KeyValuePair<string, List<ArchivedMapperDto>>> selectedValues)
    {
        foreach (var item in selectedValues)
        {
            var t = item.Key;
        }
    }
}