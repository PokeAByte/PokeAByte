using System.Text.Json;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Infrastructure.Drivers
{
    public class StaticMemoryDriver : IStaticMemoryDriver
    {
        public string ProperName => "StaticMemory";
        public int DelayMsBetweenReads { get; } = 25;

        private readonly ILogger<StaticMemoryDriver> _logger;
        private Dictionary<uint, byte[]> MemoryFragmentLayout { get; set; } = [];
        private static bool _isConnected = false;
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
            _isConnected = true;
            return Task.CompletedTask;
        }

        public Task Disconnect() => Task.CompletedTask;

        public Task<bool> TestConnection()
        {
            return Task.FromResult(_isConnected);
        }

        public async Task SetMemoryFragment(string filename)
        {
            if (BuildEnvironment.IsDebug == false)
            {
                throw new Exception("Static Memory Driver operations are not allowed if not in DEBUG mode.");
            }

            _logger.LogInformation($"Setting static memory fragment to {filename}.");

            var path = Path.GetFullPath(Path.Combine(BuildEnvironment.BinaryDirectoryPokeAByteFilePath, "..", "..", "..", "..", "Data", filename));
            if (File.Exists(path) == false) throw new Exception($"Unable to load memory container file '{filename}'.");

            var contents = await File.ReadAllTextAsync(path);
            MemoryFragmentLayout = JsonSerializer.Deserialize<Dictionary<uint, byte[]>>(contents) ?? throw new Exception("Cannot deserialize memory fragment layout.");
        }

        public Task<BlockData[]> ReadBytes(IList<MemoryAddressBlock> blocks)
        {
            if (BuildEnvironment.IsDebug == false)
            {
                throw new Exception("Static Memory Driver operations are not allowed if not in DEBUG mode.");
            }

            return Task.FromResult(MemoryFragmentLayout.Select(x => new BlockData(x.Key, x.Value)).ToArray());
        }
        public Task WriteBytes(uint startingMemoryAddress, byte[] values, string? path = null)
        {
            if (BuildEnvironment.IsDebug == false)
            {
                throw new Exception("Static Memory Driver operations are not allowed if not in DEBUG mode.");
            }

            return Task.CompletedTask;
        }

        public static Task<bool> Probe(AppSettings _)
        {
            return Task.FromResult(_isConnected);
        }
    }
}
