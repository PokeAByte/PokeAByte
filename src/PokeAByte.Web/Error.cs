namespace PokeAByte.Web;

public sealed record Error(string Code, string? Description)
{
    public override string ToString()
    {
        return (string.IsNullOrWhiteSpace(Description) ?
                   "" :
                   $"{Description}");
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

    public static readonly Error StringIsNullOrEmpty = new(nameof(StringIsNullOrEmpty),
        "The input string was null or empty.");
    public static readonly Error ListIsEmpty = new(nameof(ListIsEmpty),
        "The list that was being accessed is empty.");

    public static readonly Error FailedToParseValue = new(nameof(FailedToParseValue),
        "Failed to parse value.");

    public static readonly Error NoGlossaryItemsFound = new(nameof(NoGlossaryItemsFound),
        "No glossary items were found.");

    public static readonly Error FailedToUpdateProperty = new(nameof(FailedToUpdateProperty),
        "Failed to update property.");

    public static readonly Error FailedToLoadSavedMapperSettings = new(nameof(FailedToLoadSavedMapperSettings),
        "Unable to load saved mapper settings.");
    public static readonly Error FailedToFindMapper = new(nameof(FailedToFindMapper),
        "Unable to find the requested mapper.");

    public static readonly Error FailedToSaveData = new(nameof(FailedToSaveData),
        "Failed to save data.");
    public static readonly Error GeneralError = new(nameof(GeneralError), "Error!");
}