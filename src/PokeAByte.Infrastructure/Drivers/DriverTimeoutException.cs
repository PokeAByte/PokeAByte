namespace PokeAByte.Infrastructure;

public class DriverTimeoutException : Exception
{
    public uint MemoryAddress { get; }

    public DriverTimeoutException(uint address, string driverName, Exception? innerException)
        : base($"A timeout occurred when reading address {address:X2}. Is {driverName} running and accessible?", innerException)
    {
        MemoryAddress = address;
    }
}
