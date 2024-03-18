using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GameHook.Domain
{
    public class StopwatchInstance(ILogger logger, string description) : IDisposable
    {
        private readonly ILogger _logger = logger;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly string _description = description;

        public void Dispose()
        {
            _stopwatch.Stop();

            _logger.LogInformation($"Stopwatch for {_description} took an elapsed {_stopwatch.ElapsedMilliseconds} ms.");
           
            GC.SuppressFinalize(this);
        }
    }
}
