using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.Properties;

public partial class PropertyValueEditor
{
    [Inject] public MapperClientService MapperClientService { get; set; }
    [Parameter] public EditPropertyModel EditContext { get; set; }
    public MudBaseInput<string>? InputModel { get; set; }

    public readonly MudBlazor.Converter<string?, bool?> MudSwitchConverter = new MudBlazor.Converter<string?, bool?>
    {
        SetFunc = text => text?.ToLowerInvariant() == "true",
        GetFunc = val => val?.ToString().ToLowerInvariant() ?? "false",
    };

    private Dictionary<ulong, string> _cachedGlossary = [];
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (string.IsNullOrEmpty(EditContext.Reference)) return;
        _cachedGlossary = GetGlossaryByReferenceKey(EditContext.Reference);
        EditContext.GlossaryReference = _cachedGlossary;
    }

    private Dictionary<ulong, string> GetGlossaryByReferenceKey(string reference)
    {
        var glossaryResult = MapperClientService.GetGlossaryByReferenceKey(reference);
        if (!glossaryResult.IsSuccess)
            //todo log
            return [];
        var glossaryItems = glossaryResult.ResultValue;
        return glossaryItems?
            .Select(g => new KeyValuePair<ulong, string>(g.Key, g.Value?.ToString() ?? ""))
            .ToDictionary() ?? [];
    }
    
    private Task<IEnumerable<string>> SearchForReference(string arg)
    {
        if (string.IsNullOrEmpty(EditContext.Reference))
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        if (string.IsNullOrEmpty(arg))
            return Task.FromResult(_cachedGlossary.Select(g => g.Value));
        return Task.FromResult(_cachedGlossary
            .Where(x => 
                x.Value.Contains(arg, StringComparison.InvariantCultureIgnoreCase))
            .Select(g => g.Value));
    }

    //For some reason without this handler (even if it does nothing at all) the input will not update right away.
    //It would only update when the user clicks out of the textbox then back into it... However, adding in this empty
    //handler it will update when it loses focus. I am not a fan of leaving in empty methods but if it works, it works
    private void InputFocusLostHandler(FocusEventArgs obj){}
    private string _errorMessage = "";
    private string _successMessage = "";

    private async Task Save()
    {
        var result = await MapperClientService.WritePropertyData(EditContext.Path,
            EditContext.ValueString,
            EditContext.IsFrozen ?? false);
        if (!result.IsSuccess)
        {
            _errorMessage = result.ToString();
        }
        else
        {
            _successMessage = "Saved successful!";
        }
        StateHasChanged();
    }
    private async Task SaveBtnOnClickHandler(MouseEventArgs obj)
    {
        await Save();
    }

    private void ClearBtnOnClickHandler(MouseEventArgs obj)
    {
        EditContext.Reset();
        StateHasChanged();
    }

    private async Task OnKeyDownHandler(KeyboardEventArgs args)
    {
        if (args.Code is "Enter" or "NumpadEnter")
        {
            await Save();
        }
    }
}