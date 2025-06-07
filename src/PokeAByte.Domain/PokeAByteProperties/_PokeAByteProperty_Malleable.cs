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
    private byte[]? _bytes;
    private byte[]? _bytesFrozen;

    public string? MemoryContainer
    {
        get { return _memoryContainer; }
        set
        {
            if (value == _memoryContainer) { return; }

            FieldsChanged |= FieldChanges.MemoryContainer;
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
            FieldsChanged |= FieldChanges.Address;
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

            FieldsChanged |= FieldChanges.Length;
            _length = value;
        }
    }

    public int? Size
    {
        get => _size;
        set
        {
            if (_size == value) return;

            FieldsChanged |= FieldChanges.Size;
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
                FieldsChanged |= FieldChanges.Bits;
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

            FieldsChanged |= FieldChanges.Reference;
            _reference = value;
        }
    }

    public string? Description
    {
        get => _description;
        set
        {
            if (_description == value) return;

            FieldsChanged |= FieldChanges.Description;
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

            FieldsChanged |= FieldChanges.Value;
            _value = value;
        }
    }

    public object? FullValue { get; set; }

    public byte[]? Bytes
    {
        get => _bytes;
        set
        {
            if (_bytes == null && value == null) return;
            if (_bytes != null && value != null && _bytes.SequenceEqual(value)) return;
            FieldsChanged |= FieldChanges.Bytes;
            _bytes = value;
        }
    }

    public byte[]? BytesFrozen
    {
        get => _bytesFrozen;
        set
        {
            if (_bytesFrozen != null && value != null && _bytesFrozen.SequenceEqual(value)) return;

            FieldsChanged |= FieldChanges.IsFrozen;
            _bytesFrozen = value;
        }
    }

    public string? ReadFunction { get; set; }

    public string? WriteFunction { get; set; }

    public string? AfterReadValueExpression { get; set; }

    public string? AfterReadValueFunction { get; set; }

    public string? BeforeWriteValueFunction { get; set; }
}
