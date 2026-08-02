namespace PokeAByte.Web;

public sealed record Error(string Code, string? Description)
{
    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Description)
            ? ""
            : $"{Description}";
    }

    internal static readonly Error None = new(string.Empty, null);
    internal static readonly Error Exception = new(nameof(Exception), "An exception occured.");

    internal static readonly Error NullValue = new(
        nameof(NullValue),
        "A null value was provided."
    );

    internal static readonly Error FailedToLoadMapper = new(
        nameof(FailedToLoadMapper),
        "Failed to load mapper."
    );

    internal static readonly Error MapperNotLoaded = new(
        nameof(MapperNotLoaded),
        "No mapper loaded."
    );

    internal static readonly Error StringIsNullOrEmpty = new(
        nameof(StringIsNullOrEmpty),
        "The input string was null or empty."
    );

    internal static readonly Error NoGlossaryItemsFound = new(
        nameof(NoGlossaryItemsFound),
        "No glossary items were found."
    );

    internal static readonly Error FailedToUpdateProperty = new(
        nameof(FailedToUpdateProperty),
        "Failed to update property."
    );
}