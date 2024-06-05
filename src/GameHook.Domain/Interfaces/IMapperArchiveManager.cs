namespace GameHook.Domain.Interfaces;

public interface IMapperArchiveManager
{
    public IReadOnlyDictionary<string, IReadOnlyList<ArchivedMapperDto>> GetArchivedMappers();
    public void GenerateArchivedList();
    public void ArchiveFile(string relativeFilename,
        string filepath, 
        string? archivePath);
    public void ArchiveDirectory(string directoryPath);
}