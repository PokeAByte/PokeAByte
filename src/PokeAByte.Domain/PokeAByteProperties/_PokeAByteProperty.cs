﻿using System.Collections;
using NCalc;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.PokeAByteProperties
{
    public abstract partial class PokeAByteProperty : IPokeAByteProperty
    {
        private readonly string _originalAddressString;
        private Expression? _addressExpression;
        private bool _hasAddressParameter;
        
        public PokeAByteProperty(IPokeAByteInstance instance, PropertyAttributes attributes)
        {
            Instance = instance;
            Path = attributes.Path;
            Type = attributes.Type;

            MemoryContainer = attributes.MemoryContainer;
            AddressString = attributes.Address;
            _originalAddressString = attributes.Address ?? "";
            OriginalAddressString = _originalAddressString;
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
        }

        protected IPokeAByteInstance Instance { get; }
        public string Path { get; }
        public string Type { get; }

        public string? StaticValue { get; }

        public ReferenceItems? ComputedReference
        {
            get
            {
                if (Instance.Mapper == null) throw new Exception("Instance.Mapper is NULL.");
                if (Reference == null) return null;

                return Instance.Mapper.References[Reference];
            }
        }

        private bool IsMemoryAddressSolved { get; set; }

        private bool ShouldRunReferenceTransformer
        {
            get { return (Type == "bit" || Type == "bool" || Type == "int" || Type == "uint") && Reference != null; }
        }

        public bool IsFrozen => BytesFrozen != null;
        public bool IsReadOnly => AddressString == null;

        protected abstract object? ToValue(byte[] bytes);
        protected abstract byte[] FromValue(string value);
        
        //Not sure why FromValue and ToValue are protected? I would like to be able to convert
        //values to bytes without having to handle or copy/paste for each different property type,
        //so I am going to expose it
        public byte[] BytesFromValue(string value) 
            => FromValue(value);
        public object? ObjectFromBytes(byte[] bytes)
            => ToValue(bytes);
        public byte[] BytesFromFullValue()
            => FromValue(FullValue?.ToString() ?? "");
        public HashSet<string> FieldsChanged { get; } = [];

        public void ProcessLoop(IMemoryManager memoryManager, bool reloadAddresses)
        {
            if (Instance == null) { throw new Exception("Instance is NULL."); }
            if (Instance.Mapper == null) { throw new Exception("Instance.Mapper is NULL."); }
            if (Instance.Driver == null) { throw new Exception("Instance.Driver is NULL."); }

            if (Type is "string" && Length is 1 && Value is not null)
            {
                var valString = Value.ToString();
                if (!string.IsNullOrWhiteSpace(valString))
                {
                    Length = valString.Length;
                }
            }
            
            if (Instance.GetModuleFunctionResult(ReadFunction, this) == false)
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
                IsMemoryAddressSolved = false;
            }
            if (_addressExpression != null && IsMemoryAddressSolved == false)
            {
                try
                {
                    if (AddressMath.TrySolve(_addressExpression, Instance.Variables, out var solvedAddress))
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
            object? value;
            ReadOnlySpan<byte> readonlyBytes;

            if (address == null) { throw new Exception("address is NULL."); }
            if (Length == null) { throw new Exception("Length is NULL."); }

            try
            {
                readonlyBytes = memoryManager.GetReadonlyBytes(MemoryContainer, address ?? 0x00, Length ?? 0);
                if (readonlyBytes.SequenceEqual(Bytes)) {
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
                
            if (_bytes?.Length == bytes.Length) {
                readonlyBytes.CopyTo(_bytes);
                FieldsChanged.Add("bytes");
            } else 
            {
                Bytes = bytes.ToArray();
            }

            if (bytes == null)
            {
                throw new Exception(
                    $"Unable to retrieve bytes for property '{Path}' at address {Address?.ToHexdecimalString()}. Is the address within the drivers' memory address block ranges?");
            }

            if (bytes.Length == 0)
            {
                throw new Exception(
                  $"Unable to retrieve bytes for property '{Path}' at address {Address?.ToHexdecimalString()}. A byte array length of zero was returned?");
            }
            //Store the original, full value
            FullValue = ToValue(bytes);
            
            /*if (string.IsNullOrEmpty(Bits) == false)
            {
                int[] indexes;

                if (Bits.Contains('-'))
                {
                    var parts = Bits.Split('-');

                    if (int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                    {
                        indexes = Enumerable.Range(start, end - start + 1).ToArray();
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid format for attribute Bits ({Bits}) for path {Path}.");
                    }
                }
                else if (Bits.Contains(','))
                {
                    indexes = Bits.Split(',')
                                   .Select(x => int.TryParse(x, out int num) ? num : throw new ArgumentException($"Invalid format for attribute Bits ({Bits}) for path {Path}."))
                                   .ToArray();
                }
                else
                {
                    if (int.TryParse(Bits, out int index))
                    {
                        indexes = [index];
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid format for attribute Bits ({Bits}) for path {Path}.");
                    }
                }

                var i = 0;
                var inputBits = new BitArray(bytes);
                var outputBits = new BitArray(bytes.Length * 8);

                foreach (var x in indexes)
                {
                    outputBits[i] = inputBits[x];
                    i += 1;
                }

                outputBits.CopyTo(bytes, 0);
            }*/
            bytes = BytesFromBits(bytes);
            if (address != null && BytesFrozen != null && bytes.SequenceEqual(BytesFrozen) == false)
            {
                // Bytes have changed, but property is frozen, so force the bytes back to the original value.
                // Pretend nothing has changed. :)

                _ = Instance.Driver.WriteBytes((MemoryAddress)address, BytesFrozen);

                return;
            }

            value = CalculateObjectValue(bytes);

            Value = value;
        }

        public byte[] BytesFromBits(byte[] bytes)
        {
            if (string.IsNullOrEmpty(Bits)) return bytes;
            int[] indexes;
            ReadOnlySpan<char> bitsSpan = Bits.AsSpan();
            if (bitsSpan.Contains('-'))
            {
                var dashIndex = bitsSpan.IndexOf('-');
                var part1 = bitsSpan.Slice(0, dashIndex);
                var part2 = bitsSpan.Slice(dashIndex + 1);

                if (int.TryParse(part1, out int start) && int.TryParse(part2, out int end))
                {
                    indexes = Enumerable.Range(start, end - start + 1).ToArray();
                }
                else
                {
                    throw new ArgumentException($"Invalid format for attribute Bits ({Bits}).");
                }
            }
            else if (bitsSpan.Contains(','))
            {
                indexes = new int[bitsSpan.Count(',') + 1];
                int x = 0;
                foreach (var range in bitsSpan.Split(','))
                {
                    indexes[x++] = int.TryParse(bitsSpan[range], out int number)
                        ? number
                        : throw new ArgumentException($"Invalid format for attribute Bits ({Bits}).");
                }
            }
            else
            {
                if (int.TryParse(bitsSpan, out int index))
                {
                    indexes = [index];
                }
                else
                {
                    throw new ArgumentException($"Invalid format for attribute Bits ({Bits}) for path {Path}.");
                }
            }

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
        public object? CalculateObjectValue(byte[] bytes)
        {
            var value = ToValue(bytes);

            if (value != null && AfterReadValueExpression != null)
            {
                value = Instance.ExecuteExpression(AfterReadValueExpression, value);
            }

            if (value != null && AfterReadValueFunction != null)
            {
                value = Instance.ExecuteModuleFunction(AfterReadValueFunction, this);
            }

            // Reference lookup
            if (ShouldRunReferenceTransformer)
            {
                if (ComputedReference == null) throw new Exception("ReferenceObject is NULL.");

                value = ComputedReference.GetSingleOrDefaultByKey(Convert.ToUInt64(value))?.Value;
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
                bytes = FromValue(value);
            }

            if (string.IsNullOrEmpty(Bits) == false)
            {
                int[] indexes;

                if (Bits.Contains('-'))
                {
                    var parts = Bits.Split('-');

                    if (int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                    {
                        indexes = Enumerable.Range(start, end - start + 1).ToArray();
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid format for attribute Bits ({Bits}) for path {Path}.");
                    }
                }
                else if (Bits.Contains(','))
                {
                    indexes = Bits.Split(',')
                                   .Select(x => int.TryParse(x, out int num) ? num : throw new ArgumentException($"Invalid format for attribute Bits ({Bits}) for path {Path}."))
                                   .ToArray();
                }
                else
                {
                    if (int.TryParse(Bits, out int index))
                    {
                        indexes = [index];
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid format for attribute Bits ({Bits}) for path {Path}.");
                    }
                }

                var inputBits = new BitArray(bytes);
                var outputBits = new BitArray(Bytes);

                for (var i = 0; i < indexes.Length; i++)
                {
                    outputBits[indexes[i]] = inputBits[i];
                }

                outputBits.CopyTo(bytes, 0);
            }

            if (Instance.GetModuleFunctionResult(BeforeWriteValueFunction, this) == false)
            {
                // They want to do it themselves entirely in Javascript.

                return;
            }

            await WriteBytes(bytes, freeze);
        }

        public async Task WriteBytes(byte[] bytesToWrite, bool? freeze)
        {
            if (Instance == null) throw new Exception("Instance is NULL.");
            if (Instance.Driver == null) throw new Exception("Driver is NULL.");

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

            if (Instance.GetModuleFunctionResult(WriteFunction, (IPokeAByteProperty)this) == false)
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

            var propertyArray = new IPokeAByteProperty[] { this };
            foreach (var notifier in Instance.ClientNotifiers)
            {
                await notifier.SendPropertiesChanged(propertyArray);
            }
        }

        public async Task UnfreezeProperty()
        {
            BytesFrozen = null;

            var propertyArray = new IPokeAByteProperty[] { this };
            foreach (var notifier in Instance.ClientNotifiers)
            {
                await notifier.SendPropertiesChanged(propertyArray);
            }
        }
    }
}