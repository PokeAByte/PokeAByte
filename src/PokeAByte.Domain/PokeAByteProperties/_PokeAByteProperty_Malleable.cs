using NCalc;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.PokeAByteProperties
{
    public abstract partial class PokeAByteProperty : IPokeAByteProperty
    {
        private string? _memoryContainer { get; set; }
        private uint? _address { get; set; }
        private string? _addressString { get; set; }
        private int? _length { get; set; }
        private int? _size { get; set; }
        private string? _bits { get; set; }
        private string? _reference { get; set; }
        private string? _description { get; set; }
        private object? _value { get; set; }
        private object? _fullValue { get; set; }
        private byte[]? _bytes { get; set; }
        private byte[]? _bytesFrozen { get; set; }
        private string? _readFunction { get; set; }
        private string? _writeFunction { get; set; }
        private string? _afterReadValueExpression { get; set; }
        private string? _afterReadValueFunction { get; set; }
        private string? _beforeWriteValueFunction { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public uint? Address
        {
            get { return _address; }
            set
            {
                if (value == _address) { return; }

                _address = value;
                _addressString = value.ToString();

                IsMemoryAddressSolved = true;

                FieldsChanged.Add("address");
            }
        }

        /// <inheritdoc />
        public string OriginalAddressString { get; }

        public string? AddressString
        {
            get { return _addressString; }
            set
            {
                if (value == _addressString) { return; }

                _addressString = value;
                _hasAddressParameter = false;
                _addressExpression = !string.IsNullOrEmpty(value) 
                    ? new Expression(value)
                    : null;
                try
                {
                    if (_addressExpression!=null) {
                        _addressExpression.EvaluateParameter += OnParamEvaluation;
                    }
                    IsMemoryAddressSolved = AddressMath.TrySolve(_addressExpression, Instance.Variables, out var solvedAddress);
                    if (_addressExpression!=null) {
                        _addressExpression.EvaluateParameter -= OnParamEvaluation;
                    }
                    
                    if (IsMemoryAddressSolved == false)
                    {
                        _address = null;
                    }
                    else
                    {
                        _address = solvedAddress;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }

                FieldsChanged.Add("address");
            }
        }

        private void OnParamEvaluation(string name, ParameterArgs args)
        {
            _hasAddressParameter = true;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public string? Bits
        {
            get => _bits;
            set
            {
                if (_bits == value) return;

                FieldsChanged.Add("bits");
                _bits = value;
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
}
