namespace PokeAByte.Protocol;

public struct CloseInstruction
{
    private byte[] _bytes = new byte[Metadata.HEADER_LENGTH];

    public CloseInstruction(bool toClient)
    {
        var metaData = new Metadata(Instructions.CLOSE, toClient ? (byte)0x01 : (byte)0x00);
        metaData.CopyTo(_bytes);
    }

    public byte[] GetByteArray()
    {
        return _bytes;
    }
}
