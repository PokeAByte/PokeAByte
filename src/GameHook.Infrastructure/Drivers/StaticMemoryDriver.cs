using GameHook.Domain;
using GameHook.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GameHook.Infrastructure.Drivers
{
    public class StaticMemoryDriver : IStaticMemoryDriver
    {
        public string ProperName => "StaticMemory";
        public int DelayMsBetweenReads { get; } = 25;

        private readonly ILogger<StaticMemoryDriver> _logger;
        private Dictionary<uint, byte[]> MemoryFragmentLayout { get; set; } = [];

        public StaticMemoryDriver(ILogger<StaticMemoryDriver> logger)
        {
            _logger = logger;
        }

        public Task EstablishConnection()
        {
            if (BuildEnvironment.IsDebug == false)
            {
                throw new Exception("Static Memory Driver operations are not allowed if not in DEBUG mode.");
            }

            return Task.CompletedTask;
        }

        public async Task SetMemoryFragment(string filename)
        {
            if (BuildEnvironment.IsDebug == false)
            {
                throw new Exception("Static Memory Driver operations are not allowed if not in DEBUG mode.");
            }

            _logger.LogInformation($"Setting static memory fragment to {filename}.");

            var path = Path.GetFullPath(Path.Combine(BuildEnvironment.BinaryDirectoryGameHookFilePath, "..", "..", "..", "..", "Data", filename));
            if (File.Exists(path) == false) throw new Exception($"Unable to load memory container file '{filename}'.");

            var contents = await File.ReadAllTextAsync(path);
            MemoryFragmentLayout = JsonSerializer.Deserialize<Dictionary<uint, byte[]>>(contents) ?? throw new Exception("Cannot deserialize memory fragment layout.");
        }

        public Task<Dictionary<uint, byte[]>> ReadBytes(IEnumerable<MemoryAddressBlock> blocks)
        {
            if (BuildEnvironment.IsDebug == false)
            {
                throw new Exception("Static Memory Driver operations are not allowed if not in DEBUG mode.");
            }

            return Task.FromResult(MemoryFragmentLayout);
        }
        public Task WriteBytes(uint startingMemoryAddress, byte[] values)
        {
            if (BuildEnvironment.IsDebug == false)
            {
                throw new Exception("Static Memory Driver operations are not allowed if not in DEBUG mode.");
            }

            return Task.CompletedTask;
        }
    }
}
