namespace PokeAByte.Domain.Interfaces;

public enum MapperFilesystemTypes : byte
{
    Official,
    Local
}

public record MapperContent(string FileId, string Xml, string? ScriptPath, string? ScriptRoot);

public record MapperFileData(string Id, MapperFilesystemTypes Type, string AbsolutePath, string DisplayName);
