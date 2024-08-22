using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Application.Mappers;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Components.MapperManager.RemoteMapperManager;

public partial class RemoteManager : ComponentBase
{
    [Inject] public MapperManagerService MapperManagerService { get; set; }
    [Inject] public ILogger<RemoteManager> Logger { get; set; }
    [Parameter] public bool IsDownloadMappersPage { get; set; }
    private string _filterString = "";
    private List<VisualMapperComparisonModel> _updatedMapperList = [];
    private List<VisualMapperComparisonModel> UpdatedMapperListFiltered =>
        _updatedMapperList
            .Where(x => x.MapperComparison.Search(_filterString))
            .ToList();
    public Action<HashSet<VisualMapperComparisonModel>>? OnSelectedItemsChanged { get; set; }
    public Action<TableRowClickEventArgs<VisualMapperComparisonModel>>? OnRowClick { get; set; }

    private HashSet<VisualMapperComparisonModel> _selectedMappersFromTable = [];
    private List<MapperComparisonDto> _selectedMappersToDownload = [];
    private bool _isDataLoading = true;
    private string ButtonLabel => IsDownloadMappersPage ? "Download" : "Update";
    private string CheckForLabel => IsDownloadMappersPage ? "Mappers" : "Updates";
    protected override async Task OnInitializedAsync()
    {
        OnSelectedItemsChanged += OnSelectedItemsChangedHandler;
        OnRowClick += OnRowClickHandler;
        await base.OnInitializedAsync();
        await GetUpdatedMappers();
    }

    private async Task GetUpdatedMappers()
    {
        _selectedMappersFromTable = [];
        _isDataLoading = true;
        var updatesFound = await MapperManagerService.CheckForUpdatesAsync();
        if (updatesFound)
        {
            if (IsDownloadMappersPage)
            {
                _updatedMapperList = MapperManagerService
                    .GetMapperUpdates()
                    .Where(x => x.CurrentVersion is null)
                    .Select(x => new VisualMapperComparisonModel(x))
                    .ToList();
            }
            else
            {
                _updatedMapperList = MapperManagerService
                    .GetMapperUpdates()
                    .Where(x => x.CurrentVersion is not null)
                    .Select(x => new VisualMapperComparisonModel(x))
                    .ToList();
            }
        }
        _isDataLoading = false;
        StateHasChanged();
    }

    private async Task DownloadUpdatedMapper()
    {
        _selectedMappersFromTable = [];
        _isDataLoading = true;
        try
        {
            await MapperManagerService.DownloadMapperUpdatesAsync(
                _selectedMappersToDownload,
                UpdateProcessedCount);
            _currentProcessedCount = -1;
            _currentProcessedCountLinear = 0;
            await GetUpdatedMappers();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Exception!");
        }
        _isDataLoading = false;
        StateHasChanged();
    }

    private int _currentProcessedCount = -1;
    private double _currentProcessedCountLinear = 0;

    private void UpdateProcessedCount(int count)
    {
        _currentProcessedCountLinear = ((double)count / _selectedMappersToDownload.Count) * 100.0;
        _currentProcessedCount = count;
        StateHasChanged();
    }
    private void ResetSelection()
    {
        foreach(var selected in 
                _updatedMapperList.Where(x => x.IsSelected))
        {
            selected.IsSelected = false;
        }
    }
    private void OnSelectedItemsChangedHandler(HashSet<VisualMapperComparisonModel> items)
    {
        _selectedMappersFromTable = items;

    }

    private VisualMapperComparisonModel? _prevSelected;
    private void OnRowClickHandler(TableRowClickEventArgs<VisualMapperComparisonModel> eventArgs)
    {
        if(_prevSelected is not null && _prevSelected != eventArgs.Item)
            _prevSelected.IsSelected = false;
        eventArgs.Item.IsSelected = !eventArgs.Item.IsSelected;
        _prevSelected = eventArgs.Item;
    }
    private async Task OnClickUpdateSelected()
    {
        if(_selectedMappersFromTable.Count == 0)
            return;
        _selectedMappersToDownload = _selectedMappersFromTable
            .Select(x => x.MapperComparison)
            .ToList();
        await DownloadUpdatedMapper();
    }
    private async Task OnClickUpdateAll()
    {
        if(_updatedMapperList.Count == 0)
            return;
        _selectedMappersToDownload = _updatedMapperList
            .Select(x => x.MapperComparison)
            .ToList();
        await DownloadUpdatedMapper();
    }

    private async void OnClickCheckForUpdates()
    {
        _updatedMapperList.Clear();
        await GetUpdatedMappers();
    }

    private void OnClickOpenMapperDirectory()
    {
        Process.Start("explorer.exe",MapperEnvironment.MapperLocalDirectory);
    }

    private string GetSelectedIcon(VisualMapperComparisonModel context)
    {
        return context.IsSelected ? 
            Icons.Material.Filled.ArrowDropUp : 
            Icons.Material.Filled.ArrowDropDown;
    }
}