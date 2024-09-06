namespace PokeAByte.Domain;

public class MapperInitException : VisibleException
{
    public MapperInitException(string message, Exception? innerException = null) : base(message, innerException) { }
}
