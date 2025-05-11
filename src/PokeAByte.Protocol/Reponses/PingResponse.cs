namespace PokeAByte.Protocol;

public struct PingResponse
{
    public static Metadata Metadata = new Metadata(Instructions.PING, 0x01);
    public byte[] GetByteArray()
    {
        var result = new byte[Metadata.HEADER_LENGTH];
        Metadata.CopyTo(result);
        return result;
    }
}