using GameHook.Domain;
using GameHook.Domain.Interfaces;
using System.IO.MemoryMappedFiles;
using System.Text;

#pragma warning disable CA1416 // Validate platform compatibility
namespace GameHook.Infrastructure.Drivers
{
    public class BizhawkMemoryMapDriver : IGameHookDriver, IBizhawkMemoryMapDriver
    {
        public string ProperName => "Bizhawk";
        public int DelayMsBetweenReads { get; }

        private int IntegrationVersion;
        private string SystemName = string.Empty;

        private const int METADATA_LENGTH = 32;
        private const int DATA_Length = 4 * 1024 * 1024;

        public BizhawkMemoryMapDriver(AppSettings appSettings)
        {
            DelayMsBetweenReads = appSettings.BIZHAWK_DELAY_MS_BETWEEN_READS;
        }

        string GetStringFromBytes(byte[] data)
        {
            return Encoding.UTF8.GetString(data).TrimEnd('\0');
        }

        byte[] GetFromMemoryMappedFile(string filename, int fileSize)
        {
            try
            {
                using var mmfData = MemoryMappedFile.OpenExisting(filename, MemoryMappedFileRights.Read);
                using var mmfAccessor = mmfData.CreateViewAccessor(0, fileSize, MemoryMappedFileAccess.Read);

                byte[] data = new byte[fileSize];
                mmfAccessor.ReadArray(0, data, 0, fileSize);

                return data;
            }
            catch (FileNotFoundException ex)
            {
                throw new VisibleException("Can't establish a communication with BizHawk. Is Bizhawk open? Is the GameHook integration tool running?", ex);
            }
            catch
            {
                throw;
            }
        }

        public Task EstablishConnection()
        {
            var metadata = GetFromMemoryMappedFile("GAMEHOOK_BIZHAWK.bin", METADATA_LENGTH);

            IntegrationVersion = metadata[1];

            if (IntegrationVersion != SharedPlatformConstants.BIZHAWK_INTEGRATION_VERSION)
            {
                throw new VisibleException("BizHawk's Game Hook integration is out of date! Please update it.");
            }

            SystemName = GetStringFromBytes(metadata[2..31]);

            if (string.IsNullOrEmpty(SystemName))
            {
                throw new VisibleException("BizHawk connection is established, but does not have a game running.");
            }

            return Task.CompletedTask;
        }

        public Task<Dictionary<uint, byte[]>> ReadBytes(IEnumerable<MemoryAddressBlock> blocks)
        {
            var platform = SharedPlatformConstants.Information.SingleOrDefault(x => x.BizhawkIdentifier == SystemName) ?? throw new Exception($"System {SystemName} is not yet supported.");

            var data = GetFromMemoryMappedFile("GAMEHOOK_BIZHAWK_DATA.bin", DATA_Length);

            return Task.FromResult(platform.MemoryLayout.ToDictionary(
                x => x.PhysicalStartingAddress,
                x => data[x.CustomPacketTransmitPosition..(x.CustomPacketTransmitPosition + x.Length)]
            ));
        }

        public Task WriteBytes(uint startingMemoryAddress, byte[] values)
        {
            throw new NotImplementedException();
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility