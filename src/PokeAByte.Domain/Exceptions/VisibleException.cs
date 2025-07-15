namespace PokeAByte.Domain;

public class VisibleException : Exception
{
    public VisibleException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when the mapper XML failed to load for some reason.
/// </summary>
public class MapperException : Exception
{
    public MapperException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
