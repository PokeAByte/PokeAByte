namespace PokeAByte.Domain.Interfaces;

public interface IMapperArchiveManager
{
    public IReadOnlyDictionary<string, IReadOnlyList<ArchivedMapperDto>> GetArchivedMappers();

    public void GenerateArchivedList();

    public string ArchiveFile(string relativeFilename, string filepath);

    public void ArchiveDirectory(string directoryPath);

    public string BackupFile(string relativeFilename, string filepath);

    public void RestoreMappersFromArchive(List<ArchivedMapperDto> archivedMappers);

    public void DeleteMappersFromArchive(List<ArchivedMapperDto> archivedMappers);
}