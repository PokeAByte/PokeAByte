using System.Buffers;
using System.Collections;
using NCalc;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.PokeAByteProperties;

public partial class PokeAByteProperty : IPokeAByteProperty
{
    private static SearchValues<char> _plainAddressSearch = SearchValues.Create("1234567890 -+");
    private Expression? _addressExpression;
    private bool _hasAddressParameter;
    private EndianTypes _endian;
    private bool _isMemoryAddressSolved;

    internal bool ShouldRunReferenceTransformer
    {
        get
        {
            return Reference != null && (
                Type == PropertyType.Bool
                || Type == PropertyType.Bit
                || Type == PropertyType.Int
                || Type == PropertyType.Uint
            );
        }
    }

    public PokeAByteProperty(PropertyAttributes attributes)
    {
        Path = attributes.Path;
        Type = attributes.Type;
        _endian = attributes.EndianType;

        MemoryContainer = attributes.MemoryContainer;
        AddressString = attributes.Address;
        OriginalAddressString = attributes.Address ?? ""; ;
        Length = attributes.Length;
        Size = attributes.Size;
        Bits = attributes.Bits;
        Reference = attributes.Reference;
        Description = attributes.Description;
        Value = attributes.Value;
        StaticValue = attributes.Value;
        Bytes = [];

        ReadFunction = attributes.ReadFunction;
        WriteFunction = attributes.WriteFunction;

        AfterReadValueExpression = attributes.AfterReadValueExpression;
        AfterReadValueFunction = attributes.AfterReadValueFunction;

        BeforeWriteValueFunction = attributes.BeforeWriteValueFunction;
        if (Type == PropertyType.String && Reference == null)
        {
            Reference = "defaultCharacterMap";
        }
    }

    /// <inheritdoc />
    public string Path { get; }

    /// <inheritdoc />
    public PropertyType Type { get; }

    public string? StaticValue { get; }

    internal ReferenceItems? GetComputedReference(IPokeAByteMapper mapper)
        => (Reference == null)
            ? null
            : mapper.References[Reference];

    /// <inheritdoc />
    public bool IsFrozen => BytesFrozen.Length != 0;

    /// <inheritdoc />
    public bool IsReadOnly => AddressString == null;

    public FieldChanges FieldsChanged { get; set; } = FieldChanges.None;

    protected object? ToValue(in byte[] data, IPokeAByteMapper mapper)
    {
        switch (Type)
        {
            case PropertyType.BinaryCodedDecimal:
                return PropertyLogic.GetBCDValue(data);
            case PropertyType.BitArray:
                return PropertyLogic.GetBitArrayValue(in data);
            case PropertyType.Bool:
            case PropertyType.Bit:
                return data[0] != 0x00;
            case PropertyType.Int:
                return PropertyLogic.GetIntValue(in data, _endian);
            case PropertyType.String:
                var computedReference = GetComputedReference(mapper);
                return PropertyLogic.GetStringValue(in data, Size, computedReference, _endian);
            case PropertyType.Uint:
                return PropertyLogic.GetUIntValue(in data, _endian);
        }
        throw new NotImplementedException();
    }

    public byte[] BytesFromValue(string value, IPokeAByteMapper mapper)
    {
        switch (Type)
        {
            case PropertyType.BinaryCodedDecimal:
                throw new NotImplementedException();
            case PropertyType.BitArray:
                throw new NotImplementedException();
            case PropertyType.Bool:
            case PropertyType.Bit:
                return bool.Parse(value) ? [0x01] : [0x00];
            case PropertyType.Int:
                {
                    var integerValue = int.Parse(value);
                    var bytes = BitConverter.GetBytes(integerValue).Take(Length).ToArray();
                    return bytes.ReverseBytesIfLE(_endian);
                }
            case PropertyType.String:
                {
                    var computedReference = GetComputedReference(mapper);
                    if (computedReference == null) throw new Exception("ReferenceObject is NULL.");

                    var uints = value
                        .Select(x => computedReference.Values.FirstOrDefault(y => x.ToString() == y?.Value?.ToString()))
                        .ToList();

                    if (uints.Count + 1 > Length)
                    {
                        uints = uints.Take(Length).ToList();
                    }

                    var nullTerminationKey = computedReference.Values.First(x => x.Value == null);
                    uints.Add(nullTerminationKey);
                    return uints
                        .Select(x => x?.Value == null
                            ? nullTerminationKey.Key
                            : x.Key
                        ).SelectMany(x => Size is null
                            ? BitConverter.GetBytes(x).Take(new Range(0, 1))
                            : BitConverter.GetBytes(x).Take(Size.Value)
                        )
                        .ToArray();
                }
            case PropertyType.Uint:
                {
                    int.TryParse(value, out var integerValue);
                    byte[] bytes = BitConverter.GetBytes(integerValue)[..Length];
                    return bytes.ReverseBytesIfLE(_endian);
                }
        }
        throw new NotImplementedException();
    }

    public byte[] BytesFromFullValue(IPokeAByteMapper mapper) => BytesFromValue(FullValue?.ToString() ?? "", mapper);

    public void ProcessLoop(IPokeAByteInstance instance, IMemoryManager memoryManager, bool reloadAddresses)
    {
        if (Type == PropertyType.String && Length == 1 && Value is not null)
        {
            var valString = Value.ToString();
            if (!string.IsNullOrWhiteSpace(valString))
            {
                Length = valString.Length;
            }
        }

        if (ReadFunction != null && instance.GetModuleFunctionResult(ReadFunction, this) == false)
        {
            // They want to do it themselves entirely in Javascript.
            return;
        }

        if (StaticValue != null)
        {
            Value = StaticValue;
            return;
        }

        MemoryAddress? address = Address;
        if (reloadAddresses && _hasAddressParameter)
        {
            _isMemoryAddressSolved = false;
        }
        if (_addressExpression != null && _isMemoryAddressSolved == false)
        {
            try
            {
                if (AddressMath.TrySolve(_addressExpression, instance.Variables, out var solvedAddress))
                {
                    address = solvedAddress;
                    _isMemoryAddressSolved = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        if (address == null)
        {
            // There is nothing to do for this property, as it does not have an address or bytes.
            // Hopefully a postprocessor will pick it up and set it's value!
            return;
        }

        byte[] bytes;
        try
        {
            var readonlyBytes = memoryManager.GetReadonlyBytes(MemoryContainer, address ?? 0x00, Length);
            if (readonlyBytes.SequenceEqual(_bytes.AsSpan()))
            {
                // Fast path - if the bytes match, then we can assume the property has not been
                // updated since last poll.

                // Do nothing, we don't need to calculate the new value as
                // the bytes are the same.
                Address = address;
                return;
            }
            bytes = readonlyBytes.ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new Exception(
                $"Unable to retrieve bytes for property '{Path}' at address {Address?.ToHexdecimalString()}. A byte array length of zero was returned?"
            );
        }
        Address = address;
        _bytes = [.. bytes];
        FieldsChanged |= FieldChanges.Bytes;

        // Store the original, full value
        FullValue = ToValue(in bytes, instance.Mapper);
        bytes = BytesFromBits(bytes);
        if (address != null && BytesFrozen.Length != 0)
        {
            // Bytes have changed, but property is frozen, so force the bytes back to the original value.
            // Pretend nothing has changed. :)
            _ = instance.Driver.WriteBytes(address.Value, BytesFrozen);
            return;
        }

        Value = CalculateObjectValue(instance, bytes);
    }

    public byte[] BytesFromBits(byte[] bytes)
    {
        if (BitIndexes == null) return bytes;
        int[] indexes = BitIndexes;
        var i = 0;
        var inputBits = new BitArray(bytes);
        var outputBits = new BitArray(bytes.Length * 8);

        foreach (var x in indexes)
        {
            outputBits[i] = inputBits[x];
            i += 1;
        }
        outputBits.CopyTo(bytes, 0);
        return bytes;
    }

    public object? CalculateObjectValue(IPokeAByteInstance instance, byte[] bytes)
    {
        var value = BitIndexes == null
            ? FullValue
            : ToValue(in bytes, instance.Mapper);

        if (value != null && AfterReadValueExpression != null)
        {
            value = instance.ExecuteExpression(AfterReadValueExpression, value);
        }

        if (value != null && AfterReadValueFunction != null)
        {
            value = instance.ExecuteModuleFunction(AfterReadValueFunction, this);
        }

        // Reference lookup
        if (ShouldRunReferenceTransformer)
        {
            var reference = GetComputedReference(instance.Mapper);
            if (reference == null) throw new Exception("ReferenceObject is NULL.");
            value = reference.GetSingleOrDefaultByKey(Convert.ToUInt64(value))?.Value;
        }
        return value;
    }
}
