using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Properties;

namespace PokeAByte.Web.Models;

public class EditPropertyModel : PropertyModel
{
    public bool IsValueEdited { get; set; } = false;
    public Dictionary<ulong, string>? GlossaryReference { get; set; }
    private string _valueString = "";
    private bool _isInitValueSet = false;
    public string ValueString 
    { 
        get => _valueString;
        set
        {
            _isInitValueSet = true;
            if (_valueString == value)
                return;
            if (string.IsNullOrEmpty(_valueString))
                _valueString = value;

            if (ValidateValueString(value))
            {
                _valueString = value;
                
                if (_isInitValueSet) IsValueEdited = true;
            }                
        }
    }

    private bool ValidateValueString(string value)
    {
        return Type switch
        {
            "bool" or "bit" => bool.TryParse(value, out _),
            "string" => Length >= (string.IsNullOrEmpty(value) ? 0 : value.Length),
            "int" or "nibble" => CanParseInt(value),
            "uint" => uint.TryParse(value, out _),
            _ => false
        };
    }

    private bool CanParseInt(string value)
    {
        //get the reference
        if (!string.IsNullOrEmpty(Reference) && GlossaryReference is not null)
        {
            return GlossaryReference.Where(g =>
                    g.Value.Contains(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(g => g.Key)
                .ToList()
                .Count != 0;
        }
        return int.TryParse(value, out _);
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
    public void Reset()
    {
        _isInitValueSet = false;
        IsValueEdited = false;
        ValueString = Value?.ToString() ?? "";
    }
    public void UpdateFromPropertyModel(PropertyModel? model)
    {
        ValueString = model?.Value?.ToString() ?? "";
        Value = model?.Value;
        Bytes = model?.Bytes;
        IsFrozen = model?.IsFrozen;
        IsReadOnly = model?.IsReadOnly ?? false;
        ByteArray = new ByteArrayProperty(model?.Bytes);
    }
}