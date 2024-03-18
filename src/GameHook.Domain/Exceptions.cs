namespace GameHook.Domain
{
    public class VisibleException : Exception
    {
        public VisibleException(string message, Exception? innerException = null) : base(message, innerException)
        {
        }
    }

    public class MapperException : VisibleException
    {
        public MapperException(string message, Exception? innerException = null) : base(message, innerException) { }
    }

    public class MapperInitException : VisibleException
    {
        public MapperInitException(string message, Exception? innerException = null) : base(message, innerException) { }
    }

    public class DriverTimeoutException : Exception
    {
        public MemoryAddress MemoryAddress { get; }

        public DriverTimeoutException(MemoryAddress address, string driverName, Exception? innerException)
            : base($"A timeout occurred when reading address {address.ToHexdecimalString()}. Is {driverName} running and accessible?", innerException)
        {
            MemoryAddress = address;
        }
    }

    public class PropertyProcessException : Exception
    {
        public PropertyProcessException(string message, Exception? innerException) : base(message, innerException) { }
    }
}
