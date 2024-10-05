namespace PokeAByte.Domain;

public class MapperException : VisibleException
{
    public MapperException(string message, Exception? innerException = null) : base(message, innerException) { }
}
