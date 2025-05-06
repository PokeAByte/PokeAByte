namespace PokeAByte.Domain.Models.Mappers;

public record MapperXmlFileDto
{
    public required string FilePath { get; init; }
    public required string RelativePath { get; init; }
    public required string FullPath { get; init; }

    public static MapperXmlFileDto Create(string filePath, string pathBase)
    {
        filePath = filePath.Replace('\\', '/');
        var relativePath = filePath[pathBase.Length..filePath.LastIndexOf('/')];
        var fullPath = filePath[..filePath.LastIndexOf('/')];
        /*var relativePath = filePath[..filePath
            .LastIndexOf(fileName, StringComparison.Ordinal)]
            .Trim('/')
            .Trim('\\');*/
        //relativePath = relativePath[relativePath.LastIndexOf()]

        return new MapperXmlFileDto
        {
            FilePath = filePath,
            RelativePath = relativePath,
            FullPath = fullPath
        };
    }
}