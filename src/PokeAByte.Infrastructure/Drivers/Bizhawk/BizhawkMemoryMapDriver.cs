﻿using System.IO.MemoryMappedFiles;
using System.Text;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using SharedPlatformConstants = PokeAByte.Domain.Models.SharedPlatformConstants;

#pragma warning disable CA1416 // Validate platform compatibility
namespace PokeAByte.Infrastructure.Drivers.Bizhawk
{
    public class BizhawkMemoryMapDriver : IPokeAByteDriver, IBizhawkMemoryMapDriver
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
                throw new VisibleException("Can't establish a communication with BizHawk. Is Bizhawk open? Is the PokeAByte integration tool running?", ex);
            }
            catch
            {
                throw;
            }
        }

        public Task EstablishConnection()
        {
            var metadata = GetFromMemoryMappedFile("POKEABYTE_BIZHAWK.bin", METADATA_LENGTH);

            IntegrationVersion = metadata[1];

            if (IntegrationVersion != SharedPlatformConstants.BIZHAWK_INTEGRATION_VERSION)
            {
                throw new VisibleException("BizHawk's PokeAByte integration is out of date! Please update it.");
            }

            SystemName = GetStringFromBytes(metadata[2..31]);

            if (string.IsNullOrEmpty(SystemName))
            {
                throw new VisibleException("BizHawk connection is established, but does not have a game running.");
            }

            return Task.CompletedTask;
        }

        public Task Disconnect() => Task.CompletedTask;

        public Task<bool> TestConnection()
        {
            try
            {
                EstablishConnection();
                var platform = SharedPlatformConstants.Information.SingleOrDefault(x => x.BizhawkIdentifier == SystemName) ?? throw new Exception($"System {SystemName} is not yet supported.");

                var data = GetFromMemoryMappedFile("POKEABYTE_BIZHAWK_DATA.bin", DATA_Length);

                return Task.FromResult(data.Length > 0);
            }
            catch (Exception e)
            {
                return Task.FromResult(false);
            }
        }

        public Task<BlockData[]> ReadBytes(IList<MemoryAddressBlock> _)
        {
            var platform = SharedPlatformConstants.Information.SingleOrDefault(x => x.BizhawkIdentifier == SystemName) ?? throw new Exception($"System {SystemName} is not yet supported.");

            var data = GetFromMemoryMappedFile("POKEABYTE_BIZHAWK_DATA.bin", DATA_Length);
            var result = new BlockData[platform.MemoryLayout.Length];
            for(int i = 0; i < result.Length; i++) {
                var block = platform.MemoryLayout[i];
                result[i] = new BlockData(
                    block.PhysicalStartingAddress, 
                    data[block.CustomPacketTransmitPosition..(block.CustomPacketTransmitPosition + block.Length)]
                );
            }
            return  Task.FromResult(result);
        }

        public Task WriteBytes(uint startingMemoryAddress, byte[] values, string? path = null)
        {           
            var platform = SharedPlatformConstants
                .Information
                .SingleOrDefault(x => x.BizhawkIdentifier == SystemName) ?? 
                           throw new Exception($"System {SystemName} is not yet supported.");
            //Get memory location
            //var memoryLocation = startingMemoryAddress & 0xF000000;
            var bizhawkMemory = platform
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
#pragma warning restore CA1416 // Validate platform compatibility