using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Web.Models;

/// <summary>
/// Mapper meta data.
/// </summary>
/// <param name="Id"> The unique identifier of the mapper. </param>
/// <param name="GameName"> The name of the game the mapper is for. </param>
/// <param name="GamePlatform"> The target platform of the mapper / game. </param>
/// <param name="Version"> The version of the mapper, if applicable. </param>
/// <param name="Path"> The path the mapper is saved under. </param>
public record MapperMetaModel(Guid Id, string GameName, string GamePlatform, string? Version, string Path)
{
    public static MapperMetaModel FromMapperSection(MetadataSection metadata)
        => new(metadata.Id, metadata.GameName, metadata.GamePlatform, metadata.Version, metadata.Path);
}