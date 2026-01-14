using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models.Mappers;

/// <summary>
/// Interface for interacting with mapper files.
/// </summary>
public interface IMapperService
{
    /// <summary>
    /// Archive the specified mappers.
    /// </summary>
    /// <param name="paths"> The paths (relative to the mapper directory) to archive. </param>
    /// <returns> True if the archive was succesfully created. </returns>
    bool Archive(IEnumerable<string> paths);
    
    /// <summary>
    /// Backup the specified mappers.
    /// </summary>
    /// <param name="paths"> The paths (relative to the mapper directory) to back up. </param>
    /// <returns> True if the backup was succesfully created. </returns>
    Task<bool> Backup(IEnumerable<string> paths);

    /// <summary>
    /// Deletes target archive.
    /// </summary>
    /// <param name="archivePath"> 
    /// The path to the archive, relative to the general archive directory. 
    /// See also <see cref="ArchivedMapperFile.Path"/>.
    /// </param>
    void DeleteArchive(string archivePath);

    /// <summary>
    /// Download the specified mappers from GitHub into the local mapper folder.
    /// </summary>
    /// <param name="mapperPaths"> 
    /// The mapper paths to download. See also <see cref="RemoteMapperFile.Path"/> and <see cref="ListRemote"/>.
    /// </param>
    /// <returns> True if the download was succesful, otherwise false. </returns>
    Task<bool> DownloadAsync(IEnumerable<string> mapperPaths);

    /// <summary>
    /// Get the list of currently archvied mapper files.
    /// </summary>
    /// <returns> The list of <see cref="ArchivedMapperFile"/> items. </returns>
    List<ArchivedMapperFile> ListArchived();

    /// <summary>
    /// Lists all mappers currently in the users mapper directory. 
    /// </summary>
    /// <returns> The list of <see cref="InstalledMapper"/> items. </returns>
    IEnumerable<InstalledMapper> ListInstalled();

    /// <summary>
    /// Lists all mappers currently available on the GitHub repository.
    /// </summary>
    /// <returns> The list of <see cref="RemoteMapperFile"/> items. </returns>
    IEnumerable<RemoteMapperFile> ListRemote();

    /// <summary>
    /// Load the content of the mapper XML and get the path to the mappers JavaScript file, if there is one.
    /// </summary>
    /// <param name="path"> The mapper Path, see <see cref="MapperFile.Path"/>. </param>
    /// <returns> The <see cref="MapperContent"/>. </returns>
    Task<MapperContent> LoadContentAsync(string path);

    /// <summary>
    /// Restore a mapper archive or backup.
    /// </summary>
    /// <param name="archivePath"> The path to the archive/backup folder. See <see cref="ArchivedMapperFile.Path">. </param>
    /// <returns></returns>
    bool Restore(string archivePath);

    /// <summary>
    /// Check the GitHub repository for new mapppers or updates to existing mappers.
    /// </summary>
    /// <returns> True if fetching updates succeeded, false on error. </returns>
    bool UpdateRemoteMappers();
}
