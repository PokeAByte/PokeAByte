using System.Diagnostics;
using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Domain.Models.Mappers;
using GameHook.Mappers;
using Microsoft.AspNetCore.Components;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.MapperManager;

public partial class UpdateMappers : ComponentBase
{
    [Inject] public MapperManagerService MapperManagerService { get; set; }
    [Inject] public ILogger<UpdateMappers> Logger { get; set; }
    private string _filterString = "";
    private List<VisualMapperComparisonModel> _updatedMapperList = [];
    private List<VisualMapperComparisonModel> UpdatedMapperListFiltered =>
        _updatedMapperList
            .Where(x => x.MapperComparison.Search(_filterString))
            .ToList();
    public Action<HashSet<VisualMapperComparisonModel>>? OnSelectedItemsChanged { get; set; }
    private HashSet<VisualMapperComparisonModel> _selectedMappersFromTable = [];
    private List<MapperComparisonDto> _selectedMappersToDownload = [];
    private bool _isDataLoading = true;
    protected override async Task OnInitializedAsync()
    {
        OnSelectedItemsChanged += OnSelectedItemsChangedHandler;
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
            _updatedMapperList = MapperManagerService
                .GetMapperUpdates()
                .Select(x => new VisualMapperComparisonModel(x))
                .ToList();
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
        ResetSelection();
        _selectedMappersFromTable = items;
        foreach (var item in items)
        {
            item.IsSelected = true;
        }
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
}