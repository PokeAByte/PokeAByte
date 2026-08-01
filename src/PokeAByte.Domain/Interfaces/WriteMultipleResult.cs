namespace PokeAByte.Domain.Interfaces;

public enum WriteMultipleResult
{
    Success,
    InvalidPaths,
    InvalidLengthOrAddress,
    InvalidValue,
    InvalidProperty,
}
