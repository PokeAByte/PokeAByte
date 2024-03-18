using Microsoft.Extensions.Logging;

namespace GameHook.Application
{
    public record ScriptConsole
    {
        private readonly ILogger<ScriptConsole> _logger;

        public ScriptConsole(ILogger<ScriptConsole> logger)
        {
            _logger = logger;
        }

        public void Log(string message) => _logger.LogInformation(message);
        public void Trace(string message) => _logger.LogTrace(message);
        public void Debug(string message) => _logger.LogDebug(message);
        public void Info(string message) => _logger.LogInformation(message);
        public void Warn(string message) => _logger.LogWarning(message);
        public void Error(string message) => _logger.LogError(message);
    }
}
