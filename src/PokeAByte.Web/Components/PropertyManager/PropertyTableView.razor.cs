using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using PokeAByte.Web.Models;

namespace PokeAByte.Web.Components.PropertyManager;

public partial class PropertyTableView : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public ISnackbar Snackbar { get; set; }
    [Inject] public ILogger<PropertyTableView> Logger { get; set; }
    [Parameter] public EditPropertyModel Context { get; set; }
    public required PropertyValueEditor PropertyValueEditor { get; set; }

    private List<string> _byteArray1 = [];
    private List<string> _byteArray2 = [];
    private int _byteArray1Break = 2;
    private int _byteArray2Break = 2;
    private int _valueWidth = 150;
    private readonly int _nameWidth = 95;
    private readonly int _copyWidth = 35;
    private int _tableWidth = 250 + 95 + 35;
    private readonly string _copyBtnViewBox = "0 0 30 30"; 
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateByteArray();
        //Get the lengths of values
        //value, path, address, reference, bytes
        var lens = new List<int>()
        {
            Context.ValueString.Length,
            Context.Path.Length,
            Context.AddressString.Length,
            Context.Reference?.Length ?? 0,
            Context.ByteArray.ToString().Length,
        };
        var max = lens.Max();
        _valueWidth = max * 14 > 250 ? max * 14 : 250;
        _tableWidth = _valueWidth + _nameWidth + _copyWidth;
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        UpdateByteArray();
    }

    private void UpdateByteArray()
    {
        if (Context.ByteArray.EditableArray.Count > 0)
        {
            var c = Context.ByteArray.EditableArray.Count;
            if (c == 1)
            {
                _byteArray1 = Context.ByteArray.EditableArray;
                _byteArray2 = [];
                return;
            }
            _byteArray1 = Context.ByteArray.EditableArray.Take(c / 2).ToList();
            _byteArray2 = Context.ByteArray.EditableArray.Skip(c / 2).ToList();
            _byteArray1Break = _byteArray1.Count * 2 <= 6 ? _byteArray1.Count * 2 : 6;
            _byteArray2Break = _byteArray2.Count * 2 <= 6 ? _byteArray2.Count * 2 : 6;
        }
    }

    private async Task CopyToClipboard(string copy)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", copy);
            Snackbar.Add($"Copied {copy} to the clipboard!", 
                Severity.Info);
        }
        catch (Exception e)
        {
            var msg = "Failed to copy to clipboard!";
            Logger.LogError(e, msg);
            Snackbar.Add(msg, Severity.Error);
        }
    }

    private async Task ArrayOnKeyDownHandler(KeyboardEventArgs key)
    {
        if (key.Code is "Enter" or "NumpadEnter")
        {
            await PropertyValueEditor.Save();
        }
    }

    private void OnTextChangedHandler(string text)
    {
        //update the value
        Context.UpdateFromByteArray();
        StateHasChanged();
    }
}