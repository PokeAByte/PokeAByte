namespace PokeAByte.Infrastructure.Drivers.UdpPolling;

public record ReceivedPacket(string Command, uint MemoryAddress, byte[] Value)
{
    public byte[] Value { get; set; } = Value;
}