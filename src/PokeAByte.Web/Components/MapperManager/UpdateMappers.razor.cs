using GameHook.Domain;
using GameHook.Domain.Interfaces;
using GameHook.Domain.Models.Mappers;
using Microsoft.AspNetCore.Components;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.MapperManager;

public partial class UpdateMappers : ComponentBase
{
    [Inject] public MapperManagerService MapperManagerService { get; set; }
    private string _filterString = "";
    private List<MapperComparisonDto> _updatedMapperList = [];
    private List<MapperComparisonDto> UpdatedMapperListFiltered =>
        _updatedMapperList
            .Where(x => x.Search(_filterString))
            .ToList();
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var updatesFound = await MapperManagerService.CheckForUpdatesAsync();
        if (updatesFound)
        {
            _updatedMapperList = MapperManagerService
                .GetMapperUpdates()
                .ToList();
        }
    }
}