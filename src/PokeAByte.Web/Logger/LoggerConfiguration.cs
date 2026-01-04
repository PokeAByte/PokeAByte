namespace PokeAByte.Web.Logger;

public sealed class LoggerConfiguration
{
    private Dictionary<string, LogLevel> _minimumLevelOverrides = [];
    public LogLevel LogLevel { get; set; }
    public required string LogFile { get; set; }
    public int FileSizeLimit { get; set; }

    public LogLevel GetMinimumLevel(string name)
    {
        if (_minimumLevelOverrides.TryGetValue(name, out var levelOverride))
        {
            return levelOverride;
        }
        else
        {
            var partialMatch = _minimumLevelOverrides.Keys.FirstOrDefault(x => name.StartsWith(x));
            if (partialMatch != null)
            {
                return _minimumLevelOverrides[partialMatch];
            }
        }
        return LogLevel;
    }

    public void AddOverride(string name, LogLevel level)
    {
        _minimumLevelOverrides.Add(name, level);
    }
}
