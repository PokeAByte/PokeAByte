namespace PokeAByte.Domain.Interfaces
{
    public enum MapperFilesystemTypes
    {
        Official,
        Local
    }

    public record MapperContent(string Xml, string? ScriptPath, string? ScriptRoot);

    public class MapperFilesystemDTO
    {
        public string Id { get; set; } = string.Empty;
        public MapperFilesystemTypes Type { get; set; }
        public string AbsolutePath { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public interface IMapperFilesystemProvider
    {
        IEnumerable<MapperFilesystemDTO> MapperFiles { get; }

        void CacheMapperFiles();

        string GetMapperRootDirectory(string absolutePath);
        string GetRelativePath(string absolutePath);
        Task<MapperContent> LoadContentAsync(string mapperId);
    }
}
