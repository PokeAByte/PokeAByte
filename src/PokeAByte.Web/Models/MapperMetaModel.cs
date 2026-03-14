using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Web.Models;

public record MapperMetaModel(Guid Id, string GameName, string GamePlatform, string? Version, string Path)
{
    public static MapperMetaModel FromMapperSection(MetadataSection metadata)
        => new(metadata.Id, metadata.GameName, metadata.GamePlatform, metadata.Version, metadata.Path);
}