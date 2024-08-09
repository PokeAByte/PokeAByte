using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Domain.PokeAByteProperties
{
    public class UnsignedIntegerProperty : PokeAByteProperty, IPokeAByteProperty
    {
        public UnsignedIntegerProperty(IPokeAByteInstance instance, PropertyAttributes variables) : base(instance, variables)
        {
        }

        protected override byte[] FromValue(string value)
        {
            if (Instance.PlatformOptions == null) throw new Exception("Instance.PlatformOptions is NULL.");
            if (Length == null) throw new Exception("Length is NULL.");

            int.TryParse(value, out var integerValue);
            var bytes = BitConverter.GetBytes(integerValue).Take(Length ?? 0).ToArray();
            return bytes.ReverseBytesIfLE(Instance.PlatformOptions.EndianType);
        }

        protected override object? ToValue(byte[] data)
        {
            if (Instance == null) throw new Exception("Instance is NULL.");
            if (Instance.PlatformOptions == null) throw new Exception("Instance.PlatformOptions is NULL.");

            byte[] value = new byte[8];
            Array.Copy(data.ReverseBytesIfLE(Instance.PlatformOptions.EndianType), value, data.Length);
            return BitConverter.ToUInt32(value, 0);
        }
    }
}
