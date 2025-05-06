namespace PokeAByte.Domain.Interfaces;

public interface IMapperArchiveManager
{
    public IReadOnlyDictionary<string, IReadOnlyList<ArchivedMapperDto>> GetArchivedMappers();
    public void GenerateArchivedList();
    public string ArchiveFile(string relativeFilename,
        string filepath, string? archivePath = null);
    public void ArchiveDirectory(string directoryPath,
        string? archivePath = null);
    public string BackupFile(string relativeFilename,
        string filepath,
        string? archivedPath = null);
    public void RestoreMappersFromArchive(List<ArchivedMapperDto> archivedMappers);
    public void DeleteMappersFromArchive(List<ArchivedMapperDto> archivedMappers);
}