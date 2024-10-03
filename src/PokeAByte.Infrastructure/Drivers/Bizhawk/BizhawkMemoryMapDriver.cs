using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using SharedPlatformConstants = PokeAByte.Domain.Models.SharedPlatformConstants;

namespace PokeAByte.Infrastructure.Drivers.Bizhawk
{
    public class BizhawkMemoryMapDriver : IPokeAByteDriver, IBizhawkMemoryMapDriver
    {
        public string ProperName => "Bizhawk";
        public int DelayMsBetweenReads { get; }
        private int IntegrationVersion;
        private string SystemName = string.Empty;
        private const int METADATA_LENGTH = SharedPlatformConstants.BIZHAWK_METADATA_PACKET_SIZE;
        private const int DATA_Length = SharedPlatformConstants.BIZHAWK_DATA_PACKET_SIZE;
        MemoryMappedViewAccessor? _memoryAccessor;
        private byte[] _readBuffer = [];
        private SharedPlatformConstants.PlatformEntry? _platform;

        public BizhawkMemoryMapDriver(AppSettings appSettings)
        {
            DelayMsBetweenReads = appSettings.BIZHAWK_DELAY_MS_BETWEEN_READS;
        }

        string GetStringFromBytes(byte[] data)
        {
            return Encoding.UTF8.GetString(data).TrimEnd('\0');
        }

        private void ReadMetaData(byte[] output)
        {
            try
            {
                using MemoryMappedFile mmfData = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? MemoryMappedFile.OpenExisting("POKEABYTE_BIZHAWK.bin", MemoryMappedFileRights.Read)
                    : MemoryMappedFile.CreateFromFile("/dev/shm/POKEABYTE_BIZHAWK.bin", FileMode.Open, null, METADATA_LENGTH, MemoryMappedFileAccess.Read);
                using var accessor = mmfData.CreateViewAccessor(0, METADATA_LENGTH, MemoryMappedFileAccess.Read);
                accessor.ReadArray(0, output, 0, METADATA_LENGTH);
            }
            catch (FileNotFoundException ex)
            {
                throw new VisibleException("Can't establish a communication with BizHawk. Is Bizhawk open? Is the PokeAByte integration tool running?", ex);
            }
        }

    private bool ReadBizhawkData(int start, Span<byte> span)
    {
        try
        {
            if (_memoryAccessor == null)
            {
                using MemoryMappedFile mmfData = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? MemoryMappedFile.OpenExisting("POKEABYTE_BIZHAWK_DATA.bin", MemoryMappedFileRights.Read)
                    : MemoryMappedFile.CreateFromFile("/dev/shm/POKEABYTE_BIZHAWK_DATA.bin", FileMode.Open, null, DATA_Length, MemoryMappedFileAccess.Read);
                _memoryAccessor = mmfData.CreateViewAccessor(0, DATA_Length, MemoryMappedFileAccess.Read);
            }
			_memoryAccessor.SafeMemoryMappedViewHandle.ReadSpan((ulong)start, span);
            return true;
        }
        catch (FileNotFoundException ex)
        {
            throw new Exception("Can't establish a communication with BizHawk. Is Bizhawk open? Is the PokeAByte integration tool running?", ex);
        }
    }

        public Task EstablishConnection()
        {
            byte[] metadata = new byte[METADATA_LENGTH];
            ReadMetaData(metadata);

            IntegrationVersion = metadata[1];

            if (IntegrationVersion != SharedPlatformConstants.BIZHAWK_INTEGRATION_VERSION)
            {
                throw new VisibleException("BizHawk's PokeAByte integration is out of date! Please update it.");
            }

            SystemName = GetStringFromBytes(metadata[2..30]);
            if (string.IsNullOrEmpty(SystemName))
            {
                throw new VisibleException("BizHawk connection is established, but does not have a game running.");
            }
            return Task.CompletedTask;
        }

        public Task Disconnect()
        {
            // Dispose of previously used accessor:
            _memoryAccessor?.Dispose();
            _memoryAccessor = null;
            return Task.CompletedTask;
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                await EstablishConnection();
                _platform = SharedPlatformConstants.Information.SingleOrDefault(x => x.BizhawkIdentifier == SystemName) ?? throw new Exception($"System {SystemName} is not yet supported.");
                _readBuffer = new byte[DATA_Length];
                var data = new byte[1];
                ReadBizhawkData(0, data.AsSpan());
                return data.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<BlockData[]> ReadBytes(IList<MemoryAddressBlock> _)
        {
            if (_platform == null)
            {
                throw new Exception($"System {SystemName} is not yet supported.");
            }
            var result = new BlockData[_platform.MemoryLayout.Length];
            for (int i = 0; i < result.Length; i++)
            {
                var block = _platform.MemoryLayout[i];
                var data = new byte[block.Length];
                ReadBizhawkData(block.CustomPacketTransmitPosition, data.AsSpan());
                result[i] = new BlockData(
                    block.PhysicalStartingAddress,
                    data
                );
            }
            return Task.FromResult(result);
        }

        public Task WriteBytes(uint startingMemoryAddress, byte[] values, string? path = null)
        {
            if (_platform == null)
            {
                throw new Exception($"System {SystemName} is not yet supported.");
            }
            //Get memory location
            //var memoryLocation = startingMemoryAddress & 0xF000000;
            var bizhawkMemory = _platform
                .MemoryLayout
                .FirstOrDefault(x =>
                    x.PhysicalStartingAddress <= startingMemoryAddress &&
                    startingMemoryAddress <= x.PhysicalStartingAddress + (uint)x.Length);
            if (bizhawkMemory is null || string.IsNullOrEmpty(bizhawkMemory.BizhawkIdentifier))
                throw new InvalidOperationException(
                    $"Could not find the BizHawk identifier for memory address {startingMemoryAddress}");


            var memoryContract = new MemoryContract<byte[]>
            {
                BizHawkIdentifier = bizhawkMemory.BizhawkIdentifier,
                Data = values,
                DataLength = values.Length,
                MemoryAddressStart = (long)startingMemoryAddress - bizhawkMemory.PhysicalStartingAddress
            };
            BizhawkNamedPipesClient.WriteToBizhawk(memoryContract);
            return Task.CompletedTask;
        }
    }
}
