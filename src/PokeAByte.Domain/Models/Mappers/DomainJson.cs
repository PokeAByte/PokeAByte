using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Models.Mappers;

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(List<MapperFile>))]
[JsonSerializable(typeof(GithubCommit))]
[JsonSerializable(typeof(GithubUpdate))]
[JsonSerializable(typeof(DownloadSettings))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class DomainJson : JsonSerializerContext
{
}
