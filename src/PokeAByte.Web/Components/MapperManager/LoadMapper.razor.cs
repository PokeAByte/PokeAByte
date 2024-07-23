using System.Diagnostics;
using GameHook.Domain.Models.Mappers;
using GameHook.Mappers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.MapperManager;

public partial class LoadMapper : ComponentBase
{
    [Inject] public MapperClientService? MapperConnectionService { get; set; }
    [Inject] public NavigationService? NavigationService { get; set; }
    private IEnumerable<MapperFileModel>? _mapperFiles;
    private string _selectedMapperId = "";
    private bool _isMapperLoading = false;
    private string? _errorMessage = "";
    private string? _successMessage = "";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (MapperConnectionService is null)
            throw new InvalidOperationException("Mapper Connection service is null.");
        if (NavigationService is null)
            throw new InvalidOperationException("Navigation service is null.");
        _mapperFiles = MapperConnectionService.GetMappers();
    }
    private void SelectedValueChangeHandler(IEnumerable<string> values)
    {
        _selectedMapperId = values.FirstOrDefault() ?? "";
    }

    private async Task LoadMapperOnClickHandler(MouseEventArgs arg)
    {
        if (string.IsNullOrWhiteSpace(_selectedMapperId))
            return;
        if (MapperConnectionService is null)
            return; //todo log
        _isMapperLoading = true;
        var result = await MapperConnectionService?.ChangeMapper(_selectedMapperId)!;
        if (result.IsSuccess)
        {
            _isMapperLoading = false;
            NavigationService?.Navigate(NavigationService.Pages.DataProperties);
            StateHasChanged();
        }
        else
        {
            _isMapperLoading = false;
            _errorMessage = result.ToString();
        }
    }
    private void OpenMapperFolder()
    {
        Process.Start("explorer.exe",MapperEnvironment.MapperLocalDirectory);
    }
}