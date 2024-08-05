using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Infrastructure.Github;

namespace PokeAByte.Web.Components.Settings;

public partial class GitHubSettings : ComponentBase
{
    [Inject] public IGithubApiSettings ApiSettings { get; set; }
    [Inject] public IGithubRestApi RestApi { get; set; }
    [Inject] public ILogger<GitHubSettings> Logger { get; set; }
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public IDialogService  DialogService { get; set; }
    [Inject] public ISnackbar Snackbar { get; set; }
    [Parameter] public bool? IsInitiallyVisible { get; set; } = true;
    private bool _isVisible = true;

    private string BodyClass =>
        "content-anim " + (_isVisible ? "show" : "hide");

    private string IconStyle => _isVisible ? 
            Icons.Material.Filled.ArrowDropUp : 
            Icons.Material.Filled.ArrowDropDown;

    private GithubApiSettings _apiSettings = new();
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _isVisible = IsInitiallyVisible is null || IsInitiallyVisible.Value;
        try
        {
            _apiSettings.CopySettings(ApiSettings);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to get GitHub Api settings!");
        }
    }

    private (bool, string) _settingsTestResult = (false, "");
    private async void TestSettingsOnClickHandler()
    {
        ResetMessages();
        try
        {
            var result = await RestApi.TestSettings();
            _settingsTestResult = string.IsNullOrEmpty(result) ? 
                (true, "Successfully connected to Github Api!") :
                (false, $"Failed to connect to Github Api - {result}");
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Exception!");
            _settingsTestResult = (false, "Failed to connect to Github Api - Exception Occured");
        }
        StateHasChanged();
    }

    private async void OpenGithubOnClickHandler()
    {
        await JSRuntime.InvokeVoidAsync("openTab", 
            ApiSettings.GetGithubUrl());
    }

    private (bool, string) _saveChangesResult = (false, "");
    private void SaveOnClickHandler()
    {
        ResetMessages();
        try
        {
            ApiSettings.CopySettings(_apiSettings);
            ApiSettings.SaveChanges();
            _saveChangesResult = (true, "Changes saved successfully!");
            Snackbar.Add("Changes saved successfully!", Severity.Success);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Failed to save github settings because of an exception.");
            _saveChangesResult = (false, "Failed to save github settings changes because of an exception. " +
                                         $"{e}");
            Snackbar.Add("Failed to save github settings!", Severity.Error);
        }
        StateHasChanged();
    }

    private void ResetMessages()
    {
        _saveChangesResult = (false, "");
        _settingsTestResult = (false, "");
    }

    private void ToggleVisibility()
    {
        _isVisible = !_isVisible;
    }

    private async Task ClearOnClickHandler()
    {
        var result = await DialogService.ShowMessageBox("Warning!",
            "Clearing the GitHub settings can cause issues with receiving new mappers. Do you want to continue?",
            yesText: "Clear", cancelText: "Cancel");
        if (result is true)
        {
            _apiSettings.Clear();
            StateHasChanged();
            Snackbar.Add("Successfully cleared!", Severity.Info);
        }
    }
}