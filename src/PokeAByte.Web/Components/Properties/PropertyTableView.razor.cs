using Microsoft.AspNetCore.Components;
using PokeAByte.Web.Models;

namespace PokeAByte.Web.Components.Properties;

public partial class PropertyTableView : ComponentBase
{
    [Parameter] public EditPropertyModel Context { get; set; }
    private List<string> _byteArray1 = [];
    private List<string> _byteArray2 = [];
    private int _byteArray1Break = 1;
    private int _byteArray2Break = 1;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdateByteArray();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
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
            _byteArray1Break = _byteArray1.Count <= 6 ? _byteArray1.Count : 6;
            _byteArray2Break = _byteArray2.Count <= 6 ? _byteArray2.Count : 6;
        }
    }
}