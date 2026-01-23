using System.Text.Json.Serialization;

namespace PokeAByte.Domain.Models.Mappers;

public record GithubCommit([property: JsonPropertyName("sha")]string Hash);
