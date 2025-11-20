namespace PokeAByte.Protocol;

public struct CloseInstruction
{
    public static Metadata Metadata = new Metadata(Instructions.CLOSE, 0x01);
    private static byte[] _bytes = new byte[Metadata.HEADER_LENGTH];

    static CloseInstruction()
    {
        Metadata.CopyTo(_bytes);
    }

    public byte[] GetByteArray()
    {
        return _bytes;
    }
}
