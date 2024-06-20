namespace PokeAByte.Web;

public sealed record Error(string Code, string? Description)
{
    public override string ToString()
    {
        return Code +
               (string.IsNullOrWhiteSpace(Description) ? 
                   "" : 
                   $": {Description}");
    }

    public static readonly Error None = new(string.Empty, null);
    public static readonly Error Exception = new(nameof(Exception),
        "An exception occured.");
    public static readonly Error NullValue = new(nameof(NullValue),
        "A null value was provided.");

    public static readonly Error FailedToLoadMapper = new(nameof(FailedToLoadMapper),
        "Failed to load mapper.");

    public static readonly Error ClientInstanceNotInitialized = new(nameof(ClientInstanceNotInitialized),
        "The mapper client instance has not been initialized.");

    public static readonly Error NoMapperPropertiesFound = new(nameof(NoMapperPropertiesFound),
        "No properties were found for mapper.");

    public static readonly Error MapperNotLoaded = new(nameof(MapperNotLoaded),
        "No mapper loaded.");

    public static readonly Error FailedToLoadMetaData = new(nameof(FailedToLoadMetaData),
        "Failed to load mapper meta data.");

}