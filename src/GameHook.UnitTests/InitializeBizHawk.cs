using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;
using GameHook.Domain;

namespace GameHook.UnitTests;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class InitializeBizHawk
{
    private const int FILE_SIZE = 4 * 1024 * 1024;

    private const uint PLAYER_PARTY_LOCATION = 0x244EC;
    //Todo: allow for more platforms
    public SharedPlatformConstants.PlatformEntry GBAPlatform =
        SharedPlatformConstants
            .Information
            .First(x => 
                x.BizhawkIdentifier == "GBA");

    //Todo: add metadata 
    private MemoryMappedFile? _mappedDataFile = 
        MemoryMappedFile
            .OpenExisting("GAMEHOOK_BIZHAWK_DATA.bin", MemoryMappedFileRights.Read);
    
    public Dictionary<string, byte[]> GetBizHawkMemorySnapshot()
    {
        if (_mappedDataFile == null)
            throw new InvalidOperationException("Memory mapped file is null.");
        using var mmfAccessor = _mappedDataFile
            .CreateViewAccessor(0, FILE_SIZE, MemoryMappedFileAccess.Read);
        var data = new byte[FILE_SIZE];
        mmfAccessor.ReadArray(0, data, 0, FILE_SIZE);
        var driverResult = GBAPlatform.MemoryLayout.ToDictionary(
            x => x.BizhawkIdentifier,
            x => data[x.CustomPacketTransmitPosition
                ..(x.CustomPacketTransmitPosition + x.Length)]);
        return driverResult;
    }

    public byte[] GetPokemonFromParty(int slotNumber)
    {
        if (slotNumber is < 0 or > 5)
            throw new InvalidOperationException("Slot number cannot be less than 0 or greater than 5");
        return GetBizHawkMemorySnapshot()["EWRAM"]
                [(0x244EC + 100 * slotNumber)..(0x244EC + 100 * slotNumber + 100)];
    }
}