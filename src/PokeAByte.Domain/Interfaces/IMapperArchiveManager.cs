namespace PokeAByte.Domain.Interfaces;

public interface IMapperArchiveManager
{
    public void ArchiveFile(string relativeFilename, string filepath);

    public void ArchiveDirectory(string directoryPath);

    public string BackupFile(string relativeFilename, string filepath);

    public void RestoreMappersFromArchive(List<ArchivedMapperDto> archivedMappers);
}