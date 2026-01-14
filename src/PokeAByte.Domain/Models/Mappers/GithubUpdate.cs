namespace PokeAByte.Domain.Models.Mappers;

/// <summary>
/// GitHub upate information.
/// </summary>
/// <param name="Hash"> Hash of the last commit on the mapper repository. </param>
/// <param name="LastTry"> UTC timestamp of the last time the latest commit was fetched. </param>
public record GithubUpdate(string Hash, DateTimeOffset LastTry);
