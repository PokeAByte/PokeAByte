namespace PokeAByte.Domain;

public class VisibleException : Exception
{
    public VisibleException(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
