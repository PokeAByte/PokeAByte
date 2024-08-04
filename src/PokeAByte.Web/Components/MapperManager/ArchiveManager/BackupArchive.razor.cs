using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Application.Mappers;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Components.MapperManager.ArchiveManager;

public partial class BackupArchive : ComponentBase
{
    [Inject] public MapperManagerService ManagerService { get; set; }
    [Inject] public ILogger<BackupArchive> Logger { get; set; }
    [Inject] public ISnackbar Snackbar { get; set; }
    [Inject] private IDialogService DialogService { get; set; }
    private List<VisualMapperDto> _mapperList = [];
    private string _mapperListFilter = "";
    private List<VisualMapperDto> MapperListFiltered =>
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

    private void ArchiveMappers()
    {
        if (_selectedMappersToBackup.Count == 0) return;
        _isDataLoading = true;
        try
        {
            ManagerService.ArchiveMappers(_selectedMappersToBackup);
            Snackbar.Add("Mappers archived successfully!", Severity.Success);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Exception!");
            Snackbar.Add("Mappers failed to archive!", Severity.Error);
        }
        _selectedMappersToBackup = [];
        _isDataLoading = false;
        StateHasChanged();
        GetMapperList();
    }

    private void BackupMappers()
    {
        if (_selectedMappersToBackup.Count == 0) return;
        _isDataLoading = true;
        try
        {
            ManagerService.BackupMappers(_selectedMappersToBackup);
            Snackbar.Add("Mappers backed up successfully!", Severity.Success);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Exception!");
            Snackbar.Add("Mappers failed to be backed up!", Severity.Error);
        }
        _selectedMappersToBackup = [];
        _isDataLoading = false;
        StateHasChanged();
        GetMapperList();
    }

    private void OnClickReloadMapperList()
    {
        GetMapperList();
    }

    private async void OnClickArchiveAll()
    {
        if(_mapperList.Count == 0) return;
        var result = await DialogService.ShowMessageBox(
            "Warning", "All mappers you have downloaded will be archived. You can find these archived mappers in the 'Restore' tab.",
            "Archive All!", cancelText:"Cancel");
        if(result is null or false)
            return;
        _selectedMappersToBackup = _mapperList.Select(x => x.MapperDto).ToList();
        ArchiveMappers();
    }

    private async void OnClickArchiveSelected()
    {
        if(_selectedMapperListFromTable.Count == 0) return;
        var result = await DialogService.ShowMessageBox(
            "Warning", "Archiving selected mapper(s) will remove it from your list. You can find the archived mapper(s) in the 'Restore' tab.",
            "Archive!", cancelText:"Cancel");
        if(result is null or false)
            return;
        _selectedMappersToBackup = _selectedMapperListFromTable
            .Select(x => x.MapperDto)
            .ToList();
        _selectedMapperListFromTable = [];
        ArchiveMappers();
    }

    private void OnClickOpenMapperDirectory()
    {
        Process.Start("explorer.exe",MapperEnvironment.MapperLocalDirectory);
    }

    private void OnClickOpenArchiveDirectory()
    {
        Process.Start("explorer.exe",MapperEnvironment.MapperLocalArchiveDirectory);
    }

    private void OnClickBackupSelected()
    {        
        if(_selectedMapperListFromTable.Count == 0) return;
        _selectedMappersToBackup = _selectedMapperListFromTable
            .Select(x => x.MapperDto)
            .ToList();
        _selectedMapperListFromTable = [];
        BackupMappers();
    }

    private void OnClickBackupAll()
    {        
        if(_mapperList.Count == 0) return;
        _selectedMappersToBackup = _mapperList.Select(x => x.MapperDto).ToList();
        BackupMappers();
    }
}