using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using PokeAByte.Application.Mappers;
using PokeAByte.Web.Helper;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Components.MapperManager.ArchiveManager;
public partial class RestoreArchive : ComponentBase
{
    [Inject] public MapperManagerService MapperManagerService { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    [Inject] private ILogger<RestoreArchive> Logger { get; set; }
    [Inject] private ISnackbar Snackbar { get; set; }
    private bool _isDataLoading;
    private string _mapperListFilter = "";

    private HashSet<MapperArchiveModel> _archivedMappers = [];
    private HashSet<MapperArchiveModel> ArchivedMapperListFiltered =>
        _archivedMappers.Where(x =>
                x.BasePath.Contains(_mapperListFilter, StringComparison.InvariantCultureIgnoreCase))
            .ToHashSet();
    
    private Color SetIconColor(bool isExpanded) =>
        isExpanded ? Color.Secondary : Color.Info;

    protected override async Task OnInitializedAsync()
    {
        GetArchivedMappers();
        await base.OnInitializedAsync();
        await Task.Delay(100);
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
        XPlatHelper.OpenFileManager(MapperEnvironment.MapperLocalDirectory);
    }

    private void OnClickOpenArchiveDirectory()
    {
        XPlatHelper.OpenFileManager(MapperEnvironment.MapperLocalArchiveDirectory);
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

    private bool _isChangingNames = false;
    private void OnClickChangeNameButton()
    {
        _isChangingNames = !_isChangingNames;
        StateHasChanged();
    }

    private void OnClickSaveNameButton(MapperArchiveModel item)
    {
        try
        {
            MapperManagerService.SaveArchivedItem(item);
            var name = string.IsNullOrWhiteSpace(item.DisplayName) ? item.BasePath : item.DisplayName;
            Snackbar.Add($"Successfully saved {name}", Severity.Success);
            item.IsChangingName = false;
            StateHasChanged();
        }
        catch (Exception e)
        {
            Snackbar.Add("Failed to save due to exception. " + e, Severity.Error);
            Logger.LogError(e, "Failed to save.");
        }
    }

    private static string GetTypeName(ArchiveType itemType)
    {
        return itemType switch
        {
            ArchiveType.None => "Not Found",
            ArchiveType.Archived => "Archived",
            ArchiveType.BackUp => "Backup",
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
        };
    }

    private void OnKeyDownHandler(KeyboardEventArgs args, MapperArchiveModel item)
    {
        if (args.Code is "Enter" or "NumpadEnter")
        {
            OnClickSaveNameButton(item);
        }
    }
}