using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace PokeAByte.Web.Logger;

public sealed class PokeAByteLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private LoggerConfiguration _currentConfig;
    private LogFileWriter? _fileWriter = null;
    private readonly ConcurrentDictionary<string, PokeAByteLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    public PokeAByteLoggerProvider(IOptionsMonitor<LoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _fileWriter = new LogFileWriter(_currentConfig.LogFile, _currentConfig.FileSizeLimit);        
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(
            categoryName,
            name => new PokeAByteLogger(name, _currentConfig, _fileWriter)
        );
    }

    public void Dispose()
    {
        _loggers.Clear();
        _fileWriter?.Dispose();
        _onChangeToken?.Dispose();
    }
}
