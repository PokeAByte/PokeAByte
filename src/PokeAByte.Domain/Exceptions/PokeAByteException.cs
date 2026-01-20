namespace PokeAByte.Domain;

/// <summary>
/// Thrown when Poke-A-Byte logic fails for some reason.
/// </summary>
public class PokeAByteException : Exception
{
    public PokeAByteException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}