using System.Diagnostics;
using GameHook.Domain;
using GameHook.Mappers;
using Microsoft.AspNetCore.Components;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.MapperManager.ArchiveManager;

public partial class RestoreArchive : ComponentBase
{
    [Inject] public MapperManagerService MapperManagerService { get; set; }
    private bool _isDataLoading;
    private string _mapperListFilter = "";

    private IReadOnlyDictionary<string, IReadOnlyList<ArchivedMapperDto>> _archivedMappers =
        new Dictionary<string, IReadOnlyList<ArchivedMapperDto>>().AsReadOnly();
    private IReadOnlyDictionary<string, IReadOnlyList<ArchivedMapperDto>> ArchivedMapperListFiltered =>
        _archivedMappers.Where(x =>
                x.Key.Contains(_mapperListFilter, StringComparison.InvariantCultureIgnoreCase) ||
                x.Value.FirstOrDefault(y => y.Mapper.Search(_mapperListFilter)) != null)
            .ToDictionary()
            .AsReadOnly();

    private List<ArchivedMapperDto> _selectedArchivedMappers = [];
    protected override void OnInitialized()
    {
        base.OnInitialized();
        GetArchivedMappers();
    }

    private void GetArchivedMappers()
    {
        _selectedArchivedMappers = [];
        _archivedMappers = MapperManagerService.RefreshArchivedMappersList() ?? 
            new Dictionary<string, IReadOnlyList<ArchivedMapperDto>>().AsReadOnly();
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
}