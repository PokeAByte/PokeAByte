using GameHook.Domain.Models;
using GameHook.Domain.Models.Properties;

namespace PokeAByte.Web.Models;

public class EditPropertyModel : PropertyModel
{
    public bool IsValueEdited { get; set; } = false;
    private string _valueString = "";
    private bool _isInitValueSet = false;
    public string ValueString 
    { 
        get => _valueString;
        set
        {
            if (_valueString == value)
                return;
            if (string.IsNullOrEmpty(_valueString))
                _valueString = value;
            if (ValidateValueString(value))
            {
                _valueString = value;
                if (_isInitValueSet) IsValueEdited = true;
            }
            _isInitValueSet = true;
        }
    }

    private bool ValidateValueString(string value)
    {
        return Type switch
        {
            "bool" or "bit" => bool.TryParse(value, out _),
            "string" => Length >= value.Length,
            "int" or "nibble" => int.TryParse(value, out _),
            "uint" => uint.TryParse(value, out _),
            _ => false
        };
    }
    public ByteArrayProperty ByteArray { get; set; }
    public static EditPropertyModel FromPropertyModel(PropertyModel model)
    {
        return new EditPropertyModel
        {
            ValueString = model.Value?.ToString() ?? "",
            Path = model.Path,
            Type = model.Type,
            MemoryContainer = model.MemoryContainer,
            Address = model.Address,
            Length = model.Length,
            Size = model.Size,
            Reference = model.Reference,
            Bits = model.Bits,
            Description = model.Description,
            Value = model.Value,
            Bytes = model.Bytes,
            IsFrozen = model.IsFrozen,
            IsReadOnly = model.IsReadOnly,
            ByteArray = new ByteArrayProperty(model.Bytes)
        };
    }

    public void Clear()
    {
        _valueString = Value?.ToString() ?? "";
        IsValueEdited = false;
    }
}