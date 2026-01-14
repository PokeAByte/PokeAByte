using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Models.Mappers;

[JsonSerializable(typeof(List<AppSettings>))]
[JsonSerializable(typeof(List<MapperFile>))]
[JsonSerializable(typeof(List<GithubCommit>))]
[JsonSerializable(typeof(List<GithubUpdate>))]
[JsonSerializable(typeof(DownloadSettings))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class DomainJson : JsonSerializerContext
{
}
