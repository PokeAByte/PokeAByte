namespace PokeAByte.Domain;

public class PropertyProcessException : PokeAByteException
{
    public PropertyProcessException(string message, Exception? innerException) : base(message, innerException) { }
}
