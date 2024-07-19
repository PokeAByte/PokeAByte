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
                x.MapperModel.Mapper.Search(_mapperListFilter) ||
                x.MapperModel.FullPath.Contains(_mapperListFilter, StringComparison.InvariantCultureIgnoreCase))
            .ToList();
            
        

    public Action<MapperArchiveModel> OnSelectedItemChanged { get; set; }
    public Action<TableRowClickEventArgs<MapperArchiveModel>> OnRowClick { get; set; }
    private Action<HashSet<KeyValuePair<string, List<ArchivedMapperDto>>>>? _onSelectedChildItemChanged;
    private List<MapperArchiveModel> _selectedArchivedMappers = [];
    private MapperArchiveModel? _selectedValueFromTable;
    private TableGroupDefinition<MapperArchiveModel> _groupDefinition = new()
    {
        GroupName = "Archive Path Name",
        Indentation = true,
        IsInitiallyExpanded = false,
        Expandable = true,
        Selector = (e) => e.BasePath
    };

    private MudTable<MapperArchiveModel> _tableRef;

    protected override void OnInitialized()
    {
        OnSelectedItemChanged += OnSelectedItemsChangedHandler;
        OnRowClick += OnRowClickHandler;
        //OnSelectedItemsChanged += OnSelectedItemsChangedHandler;
        base.OnInitialized();
        GetArchivedMappers();
    }

    private void GetArchivedMappers()
    {
        _selectedArchivedMappers = [];
        _selectedValueFromTable = null;
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

    private void OnRowClickHandler(TableRowClickEventArgs<MapperArchiveModel> eventArgs)
    {
        eventArgs.Item.IsExpanded = !eventArgs.Item.IsExpanded;

    }
    private void OnSelectedItemsChangedHandler(MapperArchiveModel selectedValue)
    {
        _selectedValueFromTable = selectedValue;
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