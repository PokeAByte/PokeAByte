using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using PokeAByte.Domain.Models.Properties;
using PokeAByte.Web.Models;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Components.PropertyManager;

public partial class PropertyValueEditor : ComponentBase
{
    [Inject] public required MapperClientService MapperClientService { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Parameter]
    public required EditPropertyModel EditContext { get; set; }
    [Parameter] public bool IsShortDisplay { get; set; }
    private Dictionary<ulong, string> _cachedGlossary = []; 
    private string FreezeIcon => EditContext.IsFrozen is true
        ? PokeAByteIcons.SnowflakeIcon
        : PokeAByteIcons.SnowflakeIconDisabled;

    private Action<string>? OnValueChanged { get; set; }
    public Action<IntegerValueReference?> OnIntValueChanged { get; set; }

    public readonly MudBlazor.Converter<string?, bool?> MudSwitchConverter = new MudBlazor.Converter<string?, bool?>
    {
        SetFunc = text => text?.ToLowerInvariant() == "true",
        GetFunc = val => val?.ToString().ToLowerInvariant() ?? "false",
    };

    private readonly string _copyBtnViewBox = "0 0 30 30"; 
    protected override void OnInitialized()
    {
        base.OnInitialized();
        OnValueChanged += OnValueChangedHandler;
        OnIntValueChanged += OnIntValueChangedHandler;
        if (string.IsNullOrEmpty(EditContext.Reference)) return;
        _cachedGlossary = GetGlossaryByReferenceKey(EditContext.Reference);
        EditContext.GlossaryReference = _cachedGlossary;
        if (EditContext.Type is not "string" && !string.IsNullOrEmpty(EditContext.Reference))
        {
            var foundVal = _cachedGlossary
                .FirstOrDefault(x => x.Value == EditContext.ValueString);
            if (!string.IsNullOrEmpty(foundVal.Value))
            {
                _autocompleteIntValue = new IntegerValueReference(foundVal.Key, foundVal.Value);
            }
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender) return;
        //_autocompleteIntValue
        var reference = _cachedGlossary?
            .Where(x => x.Value == EditContext.ValueString)
            .FirstOrDefault();
        if (reference.HasValue)
        {
            _autocompleteIntValue = new IntegerValueReference(reference.Value.Key, reference.Value.Value);
        }
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
    //For some reason without this handler (even if it does nothing at all) the input will not update right away.
    //It would only update when the user clicks out of the textbox then back into it... However, adding in this empty
    //handler it will update when it loses focus. I am not a fan of leaving in empty methods but if it works, it works
    private void InputFocusLostHandler(FocusEventArgs obj) {}
    public async Task Save()
    {
        var result = await MapperClientService.WritePropertyData(EditContext.Path,
            EditContext.ValueString,
            EditContext.IsFrozen ?? false);
        if (!result.IsSuccess)
        {
            Snackbar.Add(result.ToString(), Severity.Error);
        }
        else
        {
            Snackbar.Add("Saved successful!", Severity.Success);
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
    private async Task OnClickFreezeHandler()
    {
        EditContext.IsFrozen = !EditContext.IsFrozen;
        await Save();
    }
    private Task<IEnumerable<string>> SearchForReference(string arg1, CancellationToken arg2)
    {
        if (string.IsNullOrEmpty(EditContext.Reference))
            return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
        if (string.IsNullOrEmpty(arg1))
            return Task.FromResult(_cachedGlossary
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(g => g.Value));
        return Task.FromResult(_cachedGlossary
            .Where(x => 
                !string.IsNullOrWhiteSpace(x.Value) &&
                x.Value.Contains(arg1, StringComparison.InvariantCultureIgnoreCase))
            .Select(g => g.Value));
    }

    private string FormatString(string val)
    {
        if (!IsShortDisplay || val.Length <= 15) return val;
        return $"{val[..5]}...{val[^5..]}";
    }

    private async Task OnKeyDownAutoCompleteHandler(KeyboardEventArgs key, EditPropertyModel editContext)
    {
        if (key.Code is "Enter" or "NumpadEnter" && !string.IsNullOrEmpty(_autocompleteValue))
        {
            editContext.ValueString = _autocompleteValue;
            _autocompleteValue = "";
            await Save();
        }
    }
    
    private void OnInputChanged(ChangeEventArgs obj)
    {
    }

    private string _autocompleteValue = "";
    private void OnValueChangedHandler(string val)
    {
        _autocompleteValue = val;
        EditContext.ValueString = val;
    }
    private IntegerValueReference? _autocompleteIntValue;
    private string? _autocompleteIntString;
    private void OnIntValueChangedHandler(IntegerValueReference val)
    {
        _autocompleteIntValue = val;
        EditContext.ValueString = _autocompleteIntValue.value;
    }
    private Task<IEnumerable<IntegerValueReference>> SearchForIntReference(string arg1, CancellationToken arg2)
    {
        if (string.IsNullOrEmpty(EditContext.Reference))
            return Task.FromResult<IEnumerable<IntegerValueReference>>(Array.Empty<IntegerValueReference>());
        if (string.IsNullOrEmpty(arg1))
            return Task.FromResult(_cachedGlossary
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(g => new IntegerValueReference(g.Key, g.Value)));
        return Task.FromResult(_cachedGlossary
            .Where(x => 
                !string.IsNullOrWhiteSpace(x.Value) &&
                x.Value.Contains(arg1, StringComparison.InvariantCultureIgnoreCase))
            .Select(g => new IntegerValueReference(g.Key, g.Value)));
    }

    private async Task OnKeyDownIntAutoCompleteHandler(KeyboardEventArgs key, EditPropertyModel editContext)
    {
        if (key.Code is "Enter" or "NumpadEnter" && !string.IsNullOrEmpty(_autocompleteIntValue?.key.ToString()))
        {
            await Task.Delay(100);
            editContext.ValueString = _autocompleteIntValue.value;
            //_autocompleteIntValue = null;
            await Save();
        }
    }

    private void OnTextChangedHandler(string obj)
    {
        EditContext.UpdateByteArray();
        StateHasChanged();
    }
}