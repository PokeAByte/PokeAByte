namespace PokeAByte.Domain;

public class DriverTimeoutException : Exception
{
    public MemoryAddress MemoryAddress { get; }

    public DriverTimeoutException(MemoryAddress address, string driverName, Exception? innerException)
        : base($"A timeout occurred when reading address {address.ToHexdecimalString()}. Is {driverName} running and accessible?", innerException)
    {
        MemoryAddress = address;
    }
}
