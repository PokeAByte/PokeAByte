using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Application.Mappers;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Components.MapperManager.ArchiveManager;
public partial class RestoreArchive : ComponentBase
{
    [Inject] public MapperManagerService MapperManagerService { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    private bool _isDataLoading;
    private string _mapperListFilter = "";

    private HashSet<MapperArchiveModel> _archivedMappers = [];
    private HashSet<MapperArchiveModel> ArchivedMapperListFiltered =>
        _archivedMappers.Where(x =>
                x.BasePath.Contains(_mapperListFilter, StringComparison.InvariantCultureIgnoreCase))
            .ToHashSet();
    
    private Color SetIconColor(bool isExpanded) =>
        isExpanded ? Color.Secondary : Color.Info;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        GetArchivedMappers();
    }

    private void GetArchivedMappers()
    {
        _archivedMappers = MapperManagerService
            .CreateArchiveList()
            .ToHashSet();
        /*_archivedMappers = MapperManagerService.RefreshArchivedMappersList() ?? 
            new Dictionary<string, IReadOnlyList<ArchivedMapperDto>>().AsReadOnly();*/
        StateHasChanged();
    }

    private void OnClickReloadMapperList()
    {
        GetArchivedMappers();
    }

    private void OnClickOpenMapperDirectory()
    {
        Process.Start("explorer.exe",MapperEnvironment.MapperLocalDirectory);
    }

    private void OnClickOpenArchiveDirectory()
    {
        Process.Start("explorer.exe",MapperEnvironment.MapperLocalArchiveDirectory);
    }

    private async Task OnClickRestoreButton(MapperArchiveModel? item)
    {
        if (item is null)
            return;
        var result = await DialogService.ShowMessageBox(
            "Warning", "Restoring a set of mappers will archive any current copies of those mappers.",
            "Restore!", cancelText:"Cancel");
        if (result is null or false)
            return;
        MapperManagerService.RestoreArchivedMappers(item.MapperModels);
        GetArchivedMappers();
    }

    private async Task OnClickDeleteButton(MapperArchiveModel? item)
    {
        if (item is null)
            return;
        var result = await DialogService.ShowMessageBox(
            "Warning", "Deleting a set of archived mappers cannot be undone. Proceed with caution.",
            "Delete!", cancelText:"Cancel");
        if (result is null or false)
            return;
        MapperManagerService.DeleteArchivedMappers(item.MapperModels);
        GetArchivedMappers();
    }
}