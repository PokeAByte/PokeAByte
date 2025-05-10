using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using SharedPlatformConstants = PokeAByte.Domain.Models.SharedPlatformConstants;

namespace PokeAByte.Infrastructure.Drivers.Bizhawk;

public class BizhawkMemoryMapDriver : IPokeAByteDriver, IBizhawkMemoryMapDriver
{
    public string ProperName => "Bizhawk";
    public int DelayMsBetweenReads { get; }
    private int IntegrationVersion;
    private string SystemName = string.Empty;
    private const int METADATA_LENGTH = SharedPlatformConstants.BIZHAWK_METADATA_PACKET_SIZE;
    private const int DATA_LENGTH = SharedPlatformConstants.BIZHAWK_DATA_PACKET_SIZE;
    MemoryMappedViewAccessor? _memoryAccessor;
    private SharedPlatformConstants.PlatformEntry? _platform;

    public BizhawkMemoryMapDriver(AppSettings appSettings)
    {
        DelayMsBetweenReads = appSettings.BIZHAWK_DELAY_MS_BETWEEN_READS;
    }

    string GetStringFromBytes(ReadOnlySpan<byte> data)
    {
        return Encoding.UTF8.GetString(data.TrimEnd((byte)'\0'));
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
                    : MemoryMappedFile.CreateFromFile("/dev/shm/POKEABYTE_BIZHAWK_DATA.bin", FileMode.Open, null, DATA_LENGTH, MemoryMappedFileAccess.Read);
                _memoryAccessor = mmfData.CreateViewAccessor(0, DATA_LENGTH, MemoryMappedFileAccess.Read);
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

        SystemName = GetStringFromBytes(metadata.AsSpan()[2..30]);
        if (string.IsNullOrEmpty(SystemName))
        {
            throw new VisibleException("BizHawk connection is established, but does not have a game running.");
        }
        _platform = SharedPlatformConstants.Information.SingleOrDefault(x => x.BizhawkIdentifier == SystemName)
            ?? throw new Exception($"System {SystemName} is not yet supported.");
        return Task.CompletedTask;
    }

    public Task Disconnect()
    {
        // Dispose of previously used accessor:
        _memoryAccessor?.Dispose();
        _memoryAccessor = null;
        return Task.CompletedTask;
    }

    public ValueTask ReadBytes(BlockData[] transferBlocks)
    {
        if (_platform == null)
        {
            throw new Exception($"System {SystemName} is not yet supported.");
        }
        for (int i = 0; i < transferBlocks.Length; i++)
        {
            var transferBlock = transferBlocks[i];
            var block = _platform.MemoryLayout
                .Where(
                    x => transferBlock.Start >= x.PhysicalStartingAddress
                        && transferBlock.Start + transferBlock.Data.Length <= x.PhysicalEndingAddress
                )
                .First();
            var offset = transferBlock.Start - block.PhysicalStartingAddress;
            ReadBizhawkData(block.CustomPacketTransmitPosition + (int)offset, transferBlock.Data.AsSpan());
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask WriteBytes(uint startingMemoryAddress, byte[] values, string? path = null)
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


        var memoryContract = new MemoryContract
        {
            BizHawkIdentifier = bizhawkMemory.BizhawkIdentifier,
            Data = values,
            DataLength = values.Length,
            MemoryAddressStart = (long)startingMemoryAddress - bizhawkMemory.PhysicalStartingAddress
        };
        BizhawkNamedPipesClient.WriteToBizhawk(memoryContract);
        return ValueTask.CompletedTask;
    }

    public static Task<bool> Probe(AppSettings appSettings)
    {
        try
        {
            using MemoryMappedFile mmfData = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? MemoryMappedFile.OpenExisting("POKEABYTE_BIZHAWK.bin", MemoryMappedFileRights.Read)
                : MemoryMappedFile.CreateFromFile("/dev/shm/POKEABYTE_BIZHAWK.bin", FileMode.Open, null, METADATA_LENGTH, MemoryMappedFileAccess.Read);
            using var accessor = mmfData.CreateViewAccessor(0, METADATA_LENGTH, MemoryMappedFileAccess.Read);
            byte[] metaData = new byte[METADATA_LENGTH];
            accessor.ReadArray(0, metaData, 0, METADATA_LENGTH);
            return Task.FromResult(metaData[1] == SharedPlatformConstants.BIZHAWK_INTEGRATION_VERSION);
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }
    }
}
