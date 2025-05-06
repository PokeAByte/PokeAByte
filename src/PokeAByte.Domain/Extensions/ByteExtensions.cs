namespace PokeAByte.Domain;

public static class ByteExtensions
{
    public static byte[] ReverseBytesIfLE(this byte[] bytes, EndianTypes endianType)
    {
        if (bytes.Length == 1 || endianType != EndianTypes.LittleEndian)
        {
            return bytes;
        }

        var workingBytes = (byte[])bytes.Clone();
        Array.Reverse(workingBytes);
        return workingBytes;
    }

    public static byte[] ReverseBytesIfBE(this byte[] bytes, EndianTypes endianType)
    {
        if (bytes.Length == 1 || endianType != EndianTypes.BigEndian)
        {
            return bytes;
        }

        var workingBytes = (byte[])bytes.Clone();
        Array.Reverse(workingBytes);
        return workingBytes;
    }

    private static byte[] PadBytes(byte[] data, int size)
    {
        byte[] paddedArray = new byte[size];

        Array.Copy(data, 0, paddedArray, size - data.Length, data.Length);

        return paddedArray;
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
