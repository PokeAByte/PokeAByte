namespace PokeAByte.Domain;

/// <summary>
/// Thrown when Poke-A-Byte logic fails for some reason.
/// </summary>
public class PokeAByteException : Exception
{
    public PokeAByteException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }

    public PokeAByteException(string message, string secondaryMessage) : base(message)
    {
        SecondaryMessage = secondaryMessage;
    }

    public string SecondaryMessage { get; } = "";
}