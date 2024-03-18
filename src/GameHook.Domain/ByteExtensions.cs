namespace GameHook.Domain
{
    public static class ByteExtensions
    {
        public static byte[] ReverseBytesIfLE(this byte[] bytes, EndianTypes? endianType)
        {
            if (endianType == null || bytes.Length == 1) { return bytes; }

            if (endianType == EndianTypes.LittleEndian)
            {
                var workingBytes = (byte[])bytes.Clone();

                Array.Reverse(workingBytes);

                return workingBytes;
            }

            return bytes;
        }

        public static byte[] ReverseBytesIfBE(this byte[] bytes, EndianTypes? endianType)
        {
            if (endianType == null || bytes.Length == 1) { return bytes; }

            if (endianType == EndianTypes.BigEndian)
            {
                var workingBytes = (byte[])bytes.Clone();

                Array.Reverse(workingBytes);

                return workingBytes;
            }

            return bytes;
        }

        private static byte[] PadBytes(byte[] data, int size)
        {
            byte[] paddedArray = new byte[size];

            Array.Copy(data, 0, paddedArray, size - data.Length, data.Length);

            return paddedArray;
        }

        public static byte get_byte(this byte[] data)
        {
            return data[0];
        }

        public static ushort get_ushort_be(this byte[] data)
        {
            if (data.Length < 2) data = PadBytes(data, 2);
            return (ushort)((data[0] << 8) | data[1]);
        }

        public static ushort get_ushort_le(this byte[] data)
        {
            if (data.Length < 2) data = PadBytes(data, 2);
            return (ushort)(data[0] | (data[1] << 8));
        }

        public static uint get_uint_le(this byte[] data)
        {
            if (data.Length < 4) data = PadBytes(data, 4);
            return (uint)(data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24));
        }

        public static uint get_uint_be(this byte[] data)
        {
            if (data.Length < 4) data = PadBytes(data, 4);
            return (uint)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);
        }

        public static ulong get_ulong_le(this byte[] data)
        {
            if (data.Length < 8) data = PadBytes(data, 8);

            ulong result = 0;
            for (int i = 0; i < 8; i++)
            {
                result |= ((ulong)data[i] << (i * 8));
            }

            return result;
        }

        public static ulong get_ulong_be(this byte[] data)
        {
            if (data.Length < 8) data = PadBytes(data, 8);

            ulong result = 0;
            for (int i = 0; i < 8; i++)
            {
                result = (result << 8) | data[i];
            }

            return result;
        }
    }
}
