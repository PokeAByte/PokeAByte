namespace PokeAByte.Protocol;

/// <summary>
/// Signals the closing of the connection. <br />
/// If sent from the emulator, it also means the memory mapped file will be destroyed. <br />
/// The emulator may ignore the CLOSE signal from Poke-A-Byte.
/// </summary>
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
