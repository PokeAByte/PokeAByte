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
    protected EndianTypes _endian;
    private bool _isMemoryAddressSolved;

    private bool ShouldRunReferenceTransformer
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

    public PokeAByteProperty(IPokeAByteInstance instance, PropertyAttributes attributes)
    {
        Instance = instance;
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
        Bytes = null;
        BytesFrozen = null;

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

    protected IPokeAByteInstance Instance { get; }
    public string Path { get; }
    public PropertyType Type { get; }

    public string? StaticValue { get; }

    public ReferenceItems? ComputedReference
    {
        get
        {
            if (Reference == null) return null;
            return Instance.Mapper.References[Reference];
        }
    }

    public bool IsFrozen => BytesFrozen != null;
    public bool IsReadOnly => AddressString == null;


    protected object? ToValue(in byte[] data)
    {
        switch (Type)
        {
            case PropertyType.BinaryCodedDecimal:
                return PropertyLogic.GetBCDValue(in data);
            case PropertyType.BitArray:
                return PropertyLogic.GetBitArrayValue(in data);
            case PropertyType.Bool:
            case PropertyType.Bit:
                return data[0] != 0x00;
            case PropertyType.Int:
                return PropertyLogic.GetIntValue(in data, _endian);
            case PropertyType.String:
                return PropertyLogic.GetStringValue(in data, Size, ComputedReference, _endian);
            case PropertyType.Uint:
                return PropertyLogic.GetUIntValue(in data, _endian);
        }
        throw new NotImplementedException();
    }


    public byte[] BytesFromValue(string value)
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
                    if (Length == null) throw new Exception("Length is NULL.");
                    var integerValue = int.Parse(value);
                    var bytes = BitConverter.GetBytes(integerValue).Take(Length ?? 0).ToArray();
                    return bytes.ReverseBytesIfLE(_endian);
                }
            case PropertyType.String:
                {
                    if (ComputedReference == null) throw new Exception("ReferenceObject is NULL.");
                    if (Length == null) throw new Exception("Length is NULL.");

                    var uints = value
                        .Select(x => ComputedReference.Values.FirstOrDefault(y => x.ToString() == y?.Value?.ToString()))
                        .ToList();

                    if (uints.Count + 1 > Length)
                    {
                        uints = uints.Take(Length ?? 0 - 1).ToList();
                    }

                    var nullTerminationKey = ComputedReference.Values.First(x => x.Value == null);
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
                    if (Length == null) throw new Exception("Length is NULL.");

                    int.TryParse(value, out var integerValue);
                    byte[] bytes = BitConverter.GetBytes(integerValue)[..Length.Value];
                    return bytes.ReverseBytesIfLE(_endian);
                }
        }
        throw new NotImplementedException();
    }

    public byte[] BytesFromFullValue() => BytesFromValue(FullValue?.ToString() ?? "");
    public HashSet<string> FieldsChanged { get; } = [];

    public void ProcessLoop(IPokeAByteInstance instance, IMemoryManager memoryManager, bool reloadAddresses)
    {
        if (Type == PropertyType.String && Length is 1 && Value is not null)
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
        ReadOnlySpan<byte> readonlyBytes;

        if (address == null) { throw new Exception("address is NULL."); }
        if (Length == null) { throw new Exception("Length is NULL."); }

        try
        {
            readonlyBytes = memoryManager.GetReadonlyBytes(MemoryContainer, address ?? 0x00, Length ?? 0);
            if (readonlyBytes.SequenceEqual(Bytes))
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
            Address = null;
            bytes = [0];
            readonlyBytes = bytes.AsSpan();
        }

        Address = address;

        if (_bytes?.Length == bytes.Length)
        {
            readonlyBytes.CopyTo(_bytes);
            FieldsChanged.Add("bytes");
        }
        else
        {
            Bytes = bytes.ToArray();
        }

        if (bytes.Length == 0)
        {
            throw new Exception(
                $"Unable to retrieve bytes for property '{Path}' at address {Address?.ToHexdecimalString()}. A byte array length of zero was returned?"
            );
        }
        //Store the original, full value
        FullValue = ToValue(in bytes);

        bytes = BytesFromBits(bytes);
        if (address != null && BytesFrozen != null && bytes.SequenceEqual(BytesFrozen) == false)
        {
            // Bytes have changed, but property is frozen, so force the bytes back to the original value.
            // Pretend nothing has changed. :)
            _ = instance.Driver.WriteBytes((MemoryAddress)address, BytesFrozen);
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
            : ToValue(in bytes);

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
            var computedReference = ComputedReference;
            if (computedReference == null) throw new Exception("ReferenceObject is NULL.");

            value = computedReference.GetSingleOrDefaultByKey(Convert.ToUInt64(value))?.Value;
        }
        return value;
    }

    public async Task WriteValue(string value, bool? freeze)
    {
        if (Bytes == null)
        {
            throw new Exception("Bytes is NULL.");
        }

        byte[] bytes;

        if (ShouldRunReferenceTransformer)
        {
            if (ComputedReference == null) throw new Exception("Glossary is NULL.");
            bytes = BitConverter.GetBytes(ComputedReference.GetFirstByValue(value).Key);
        }
        else
        {
            bytes = BytesFromValue(value);
        }

        if (BitIndexes != null)
        {
            var inputBits = new BitArray(bytes);
            var outputBits = new BitArray(Bytes);

            for (var i = 0; i < BitIndexes.Length; i++)
            {
                outputBits[BitIndexes[i]] = inputBits[i];
            }
            outputBits.CopyTo(bytes, 0);
        }

        if (BeforeWriteValueFunction != null && Instance.GetModuleFunctionResult(BeforeWriteValueFunction, this) == false)
        {
            // They want to do it themselves entirely in Javascript.
            return;
        }

        await WriteBytes(bytes, freeze);
    }

    public async Task WriteBytes(byte[] bytesToWrite, bool? freeze)
    {
        if (Address == null) throw new Exception($"{Path} does not have an address. Cannot write data to an empty address.");
        if (Length == null) throw new Exception($"{Path}'s length is NULL, so we can't write bytes.");

        var bytes = new byte[Length ?? 1];

        // Overlay the bytes onto the buffer.
        // This ensures that we can't overflow the property.
        // It also ensures it can't underflow the property, it copies the remaining from Bytes.
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i < bytesToWrite.Length) bytes[i] = bytesToWrite[i];
            else if (Bytes != null) bytes[i] = Bytes[i];
        }

        if (WriteFunction != null && Instance.GetModuleFunctionResult(WriteFunction, this) == false)
        {
            // They want to do it themselves entirely in Javascript.
            return;
        }

        if (freeze == true)
        {
            // The property is frozen, but we want to write bytes anyway.
            // So this should replace the existing frozen bytes.
            BytesFrozen = bytes;
        }

        if (bytes.Length != Length)
        {
            throw new Exception($"Something went wrong with attempting to write bytes for {Path}. The bytes to write and the length of the property do not match. Will not proceed.");
        }

        await Instance.Driver.WriteBytes((MemoryAddress)Address, bytes);

        if (freeze == true) await FreezeProperty(bytes);
        else if (freeze == false) await UnfreezeProperty();
    }

    public async Task FreezeProperty(byte[] bytesFrozen)
    {
        BytesFrozen = bytesFrozen;
        await Instance.ClientNotifier.SendPropertiesChanged([this]);
    }

    public async Task UnfreezeProperty()
    {
        BytesFrozen = null;
        await Instance.ClientNotifier.SendPropertiesChanged([this]);
    }
}
