namespace PokeAByte.Domain;

public class PropertyProcessException : Exception
{
    public PropertyProcessException(string message, Exception? innerException) : base(message, innerException) { }
}
