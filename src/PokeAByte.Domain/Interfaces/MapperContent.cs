namespace PokeAByte.Domain.Interfaces;

public enum MapperFileType : byte
{
    Official,
    Local
}

public record MapperContent(string Path, string Xml, string? ScriptPath, string? ScriptRoot);
