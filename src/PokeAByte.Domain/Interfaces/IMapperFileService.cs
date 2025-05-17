namespace PokeAByte.Domain.Interfaces;

public interface IMapperFileService
{
    void ArchiveDirectory(string directoryPath);
    void ArchiveFile(string relativeFilename, string filepath);
    string BackupFile(string relativeFilename, string filepath);
    void DeleteMappersFromArchive(IEnumerable<ArchivedMapperDto> archivedMappers);
    List<ArchivedMapperDto> ListArchived();
    List<MapperFileData> ListInstalled();
    Task<MapperContent> LoadContentAsync(string mapperId);
    void Refresh();
    void RestoreMappersFromArchive(List<ArchivedMapperDto> archivedMappers);
}