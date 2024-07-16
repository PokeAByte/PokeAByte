using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Domain.Models.Mappers;
using Microsoft.AspNetCore.Components;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.MapperManager;

public partial class UpdateMappers : ComponentBase
{
    [Inject] public MapperManagerService MapperManagerService { get; set; }
    private string _filterString = "";
    private List<VisualMapperComparisonModel> _updatedMapperList = [];
    private List<VisualMapperComparisonModel> UpdatedMapperListFiltered =>
        _updatedMapperList
            .Where(x => x.MapperComparison.Search(_filterString))
            .ToList();

    public Action<HashSet<VisualMapperComparisonModel>>? OnSelectedItemsChanged { get; set; }

    private HashSet<VisualMapperComparisonModel> _selectedMappers = [];
    protected override async Task OnInitializedAsync()
    {
        OnSelectedItemsChanged += OnSelectedItemsChangedHandler;
        await base.OnInitializedAsync();
        var updatesFound = await MapperManagerService.CheckForUpdatesAsync();
        if (updatesFound)
        {
            _updatedMapperList = MapperManagerService
                .GetMapperUpdates()
                .Select(x => new VisualMapperComparisonModel(x))
                .ToList();
        }
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
        _selectedMappers = items;
        foreach (var item in items)
        {
            item.IsSelected = true;
        }
    }
}