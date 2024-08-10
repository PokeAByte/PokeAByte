using PokeAByte.Domain;
using PokeAByte.Domain.Models;
using PokeAByte.Domain.Models.Properties;

namespace PokeAByte.Web.Models;

public class EditPropertyModel : PropertyModel
{
    public bool IsEditing { get; set; } = false;
    public Dictionary<ulong, string>? GlossaryReference { get; set; }
    private string _valueString = "";
    public string ValueString 
    { 
        get => _valueString;
        set
        {
            if (IsFrozen is true)
            {
                _valueString = Value?.ToString() ?? "";
                return;
            }
            if (_valueString == value)
                return;
            if (string.IsNullOrEmpty(_valueString))
                _valueString = value;
            
            if (ValidateValue(value))
            {
                _valueString = value; //ValueToString(value);
            }                
        }
    }

    public string ValueToString(string value)
    {
        if (string.IsNullOrEmpty(ValueString))
            return "";
        try
        {
            //see if this is a reference
            if (!string.IsNullOrEmpty(Reference))
            {
                //Get the ref key
                var key = GlossaryReference?
                    .Where(g => g.Value == value)
                    .Select(g => g.Key)
                    .FirstOrDefault();
                if (key is not null)
                {
                    value = key.ToString() ?? "";
                }
            }
            if (string.IsNullOrEmpty(value))
                return "";
            //Try to get bytes
            var bytes = BaseProperty.BytesFromValue(value);
            //Get the bits
            bytes = BaseProperty.BytesFromBits(bytes);
            //try to get calculated value
            var val = BaseProperty.CalculateObjectValue(bytes);
            //see if object is null
            if (val is null)
                return "";
            return val.ToString() ?? "";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "";
        }
    }
    public bool ValidateValueString()
    {
        if (string.IsNullOrEmpty(ValueString))
            return false;
        try
        {
            var value = ValueString;
            //see if this is a reference
            if (!string.IsNullOrEmpty(Reference))
            {
                //Get the ref key
                var key = GlossaryReference?
                    .Where(g => g.Value == value)
                    .Select(g => g.Key)
                    .FirstOrDefault();
                if (key is not null)
                {
                    value = key.ToString();
                }
            }
            if (string.IsNullOrEmpty(value))
                return false;
             //Try to get bytes
            var bytes = BaseProperty.BytesFromValue(value);
            //try to get calculated value
            var val = BaseProperty.CalculateObjectValue(bytes);
            //see if object is null
            if (val is null)
                return false;
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    public string GetReference => Reference ?? "";
    private bool ValidateValue(string value)
    {
        return Type switch
        {
            "bool" or "bit" => bool.TryParse(value, out _),
            "string" => Length >= (string.IsNullOrEmpty(value) ? 0 : value.Length),
            "int" or "nibble" => CanParseInt(value),
            "uint" => CanParseUInt(value)/*uint.TryParse(value, out _)*/,
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
    private bool CanParseUInt(string value)
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
        return uint.TryParse(value, out _);
    }
    public ByteArrayProperty ByteArray { get; set; }
    public string AddressString =>
        Address?.ToString("X") ?? "";
    public static EditPropertyModel FromPropertyModel(PropertyModel model)
    {            
        
        return new EditPropertyModel
        {
            ValueString = model.ValueAsString(),
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
            ByteArray = new ByteArrayProperty(model.Bytes),
            BaseProperty = model.BaseProperty
        };
    }
    public void Reset()
    {
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

    public void UpdateByteArray()
    {
        var bytes = BaseProperty.BytesFromValue(ValueString);
        ByteArray = new ByteArrayProperty(bytes.ToIntegerArray());
    }
    public void UpdateFromByteArray()
    {
        ByteArray.UpdateByteArray();
        var arr = ByteArray.ByteArray?.Select(Convert.ToByte).ToArray();
        try
        {

            if(arr is not null)
                ValueString = BaseProperty.ObjectFromBytes(arr)?.ToString() ?? "";
        }
        catch
        {
            ValueString = this.ValueAsString();
        }
    }
}