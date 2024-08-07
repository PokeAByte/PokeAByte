using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PokeAByte.Application.Mappers;
using PokeAByte.Domain.Models.Mappers;
using PokeAByte.Web.Services;
using PokeAByte.Web.Services.Drivers;
using PokeAByte.Web.Services.Mapper;
using PokeAByte.Web.Services.Navigation;

namespace PokeAByte.Web.Components.MapperManager;

public partial class LoadMapper : ComponentBase
{
    [Inject] public MapperClientService? MapperConnectionService { get; set; }
    [Inject] public NavigationService? NavigationService { get; set; }
    public bool IsLoading => _isMapperLoading;
    public bool HasMappers => _mapperFiles?.Any() ?? false;

    private IEnumerable<MapperFileModel>? _mapperFiles;
    //private string _selectedMapperId = "";
    private string _selectedMapper = "";
    private bool _isMapperLoading = false;
    private string? _errorMessage = "";
    private string? _successMessage = "";
    private int _currentDriverProgressAttempt = 0;
    private int _currentDriverAttempt = 0;
    private int _currentMapperAttempt = 0;
    private int _currentMapperProgressAttempt = 0;


    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (MapperConnectionService is null)
            throw new InvalidOperationException("Mapper Connection service is null.");
        if (NavigationService is null)
            throw new InvalidOperationException("Navigation service is null.");
        _mapperFiles = MapperConnectionService.GetMappers();
    }
    /*private void SelectedValueChangeHandler(IEnumerable<string> values)
    {
        _selectedMapperId = values.FirstOrDefault() ?? "";
    }*/

    private async Task LoadMapperOnClickHandler()
    {
        if (string.IsNullOrWhiteSpace(_selectedMapper) || 
            _mapperFiles is null)
            return;
        if (MapperConnectionService is null)
            return; //todo log
        _isMapperLoading = true;
        var mapperId = "";
        try
        {
            mapperId = _mapperFiles
                .Where(x => x.DisplayName == _selectedMapper)
                .Select(x => x.Id)
                .SingleOrDefault();
        }
        catch (Exception e)
        {
            _errorMessage = $"{e.Message}";
            return;
        }

        if (string.IsNullOrWhiteSpace(mapperId))
        {
            _errorMessage = "Failed to find selected mapper!";
            return;
        }

        _currentDriverAttempt = 0;
        _currentMapperAttempt = 0;
        var result = await MapperConnectionService?
            .ChangeMapper(mapperId, OnDriverTestFailedCallback, OnMapperTestFailedCallback)!;
        if (result.IsSuccess)
        {
            //_selectedMapper = "";
            NavigationService?.TogglePropertiesButton();
            NavigationService?.Navigate(NavigationService.Pages.Properties);
            _isMapperLoading = false;
            StateHasChanged();
        }
        else
        {
            _isMapperLoading = false;
            _errorMessage = result.ToString();
        }
    }
    
    private async void OnDriverTestFailedCallback(int currentAttempt)
    {
        _currentDriverAttempt = currentAttempt;
        _currentDriverProgressAttempt = (int)((double)currentAttempt / DriverService.MaxAttempts * 100);
        await InvokeAsync(StateHasChanged);
    }
    private async void OnMapperTestFailedCallback(int currentAttempt)
    {
        _currentMapperAttempt = currentAttempt;
        _currentMapperProgressAttempt = (int)((double)currentAttempt / DriverService.MaxAttempts * 100);
        await InvokeAsync(StateHasChanged);
    }
    private void OpenMapperFolder()
    {
        Process.Start("explorer.exe",MapperEnvironment.MapperLocalDirectory);
    }

    private async Task<IEnumerable<string>> SearchForMapper(string searchArg, CancellationToken token)
    {
        if (_mapperFiles is null)
            return [];
        if (string.IsNullOrEmpty(searchArg))
            return _mapperFiles
                .Select(x => x.DisplayName);
        return _mapperFiles
            .Where(x => x.DisplayName
                .Contains(searchArg, StringComparison.InvariantCultureIgnoreCase))
            .Select(x => x.DisplayName);
    }

    private Task InputFocusLostHandler(FocusEventArgs arg)
    {
        return Task.CompletedTask;
    }

    private async void OnKeyDownHandler(KeyboardEventArgs keyboard)
    {
        if (keyboard.Code is "Enter" or "NumpadEnter" &&
            !string.IsNullOrEmpty(_selectedMapper))
        {
            await LoadMapperOnClickHandler();
        }
    }

    private void OnInputChanged(ChangeEventArgs arg)
    {
    }
}
