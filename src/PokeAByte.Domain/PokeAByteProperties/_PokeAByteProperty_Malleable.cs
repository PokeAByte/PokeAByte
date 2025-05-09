using NCalc;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.PokeAByteProperties;

public partial class PokeAByteProperty : IPokeAByteProperty
{
    private string? _memoryContainer;
    private uint? _address;
    private string? _addressString;
    private int? _length;
    private int? _size;
    private string? _bits;
    private string? _reference;
    private string? _description;
    private object? _value;
    private object? _fullValue;
    private byte[]? _bytes;
    private byte[]? _bytesFrozen;
    private string? _readFunction;
    private string? _writeFunction;
    private string? _afterReadValueExpression;
    private string? _afterReadValueFunction;
    private string? _beforeWriteValueFunction;

    public string? MemoryContainer
    {
        get { return _memoryContainer; }
        set
        {
            if (value == _memoryContainer) { return; }

            FieldsChanged.Add("memoryContainer");
            _memoryContainer = value;
        }
    }

    public uint? Address
    {
        get { return _address; }
        set
        {
            if (value == _address) { return; }

            _address = value;
            _addressString = value.ToString();

            _isMemoryAddressSolved = true;

            FieldsChanged.Add("address");
        }
    }

    public string OriginalAddressString { get; }

    public string? AddressString
    {
        get { return _addressString; }
        set
        {
            if (value == _addressString) { return; }

            _addressString = value;
            _isMemoryAddressSolved = false;
            _hasAddressParameter = value.AsSpan().ContainsAnyExcept(_plainAddressSearch);
            _addressExpression = !string.IsNullOrEmpty(value)
                ? new Expression(value)
                : null;
        }
    }

    public int? Length
    {
        get => _length;
        set
        {
            if (_length == value) return;

            FieldsChanged.Add("length");
            _length = value;
        }
    }

    public int? Size
    {
        get => _size;
        set
        {
            if (_size == value) return;

            FieldsChanged.Add("size");
            _size = value;
        }
    }

    public string? Bits
    {
        get => _bits;
        set
        {
            if (value != _bits)
            {
                _bits = value;
                BitIndexes = value != null
                    ? PropertyLogic.ParseBits(value)
                    : null;
                FieldsChanged.Add("bits");
            }
        }
    }

    internal int[]? BitIndexes { get; private set; }

    public string? Reference
    {
        get => _reference;
        set
        {
            if (_reference == value) return;

            FieldsChanged.Add("reference");
            _reference = value;
        }
    }

    public string? Description
    {
        get => _description;
        set
        {
            if (_description == value) return;

            FieldsChanged.Add("description");
            _description = value;
        }
    }

    public object? Value
    {
        get => _value;
        set
        {
            if (_value == null && value == null) return;
            if (_value != null && _value.Equals(value)) return;

            FieldsChanged.Add("value");
            _value = value;
        }
    }

    public object? FullValue
    {
        get => _fullValue;
        set
        {
            if (_fullValue != null && _fullValue.Equals(value)) return;

            //FieldsChanged.Add("value");
            _fullValue = value;
        }
    }

    public byte[]? Bytes
    {
        get => _bytes;
        set
        {
            if (_bytes == null && value == null) return;
            if (_bytes != null && value != null && _bytes.SequenceEqual(value)) return;

            FieldsChanged.Add("bytes");
            _bytes = value;
        }
    }

    public byte[]? BytesFrozen
    {
        get => _bytesFrozen;
        set
        {
            if (_bytesFrozen != null && value != null && _bytesFrozen.SequenceEqual(value)) return;

            FieldsChanged.Add("frozen");
            _bytesFrozen = value;
        }
    }

    public string? ReadFunction
    {
        get => _readFunction;
        set
        {
            if (_readFunction == value) return;

            FieldsChanged.Add("readFunction");
            _readFunction = value;
        }
    }

    public string? WriteFunction
    {
        get => _writeFunction;
        set
        {
            if (_writeFunction == value) return;

            FieldsChanged.Add("writeFunction");
            _writeFunction = value;
        }
    }

    public string? AfterReadValueExpression
    {
        get => _afterReadValueExpression;
        set
        {
            if (_afterReadValueExpression == value) return;

            FieldsChanged.Add("afterReadValueExpression");
            _afterReadValueExpression = value;
        }
    }

    public string? AfterReadValueFunction
    {
        get => _afterReadValueFunction;
        set
        {
            if (_afterReadValueFunction == value) return;

            FieldsChanged.Add("afterReadValueFunction");
            _afterReadValueFunction = value;
        }
    }

    public string? BeforeWriteValueFunction
    {
        get => _beforeWriteValueFunction;
        set
        {
            if (_beforeWriteValueFunction == value) return;

            FieldsChanged.Add("beforeWriteValueFunction");
            _beforeWriteValueFunction = value;
        }
    }
}
