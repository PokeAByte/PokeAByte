using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using PokeAByte.Domain;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Infrastructure.Drivers.UdpPolling
{
    public class RetroArchUdpPollingDriver : IPokeAByteDriver, IRetroArchUdpPollingDriver
    {
        public string ProperName { get; } = "RetroArch";
        public int DelayMsBetweenReads { get; }
        private ILogger<RetroArchUdpPollingDriver> Logger { get; }
        private readonly AppSettings _appSettings;
        //private UdpClient UdpClient { get; set; }
        private UdpClientWrapper _udpClientWrapper;
        private Dictionary<string, ReceivedPacket> Responses { get; set; } = [];

        private CancellationTokenSource _connectionCts = new();
        //private bool _isConnected = false;

        /*void CreateUdpClient()
        {
            // Dispose of the existing UDP client if it exists.
            UdpClient?.Dispose();

            // Create a new one.
            UdpClient = new UdpClient();
            UdpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            UdpClient.Connect(IPAddress.Parse(_appSettings.RETROARCH_LISTEN_IP_ADDRESS), _appSettings.RETROARCH_LISTEN_PORT);
        }*/

        public RetroArchUdpPollingDriver(ILogger<RetroArchUdpPollingDriver> logger, AppSettings appSettings)
        {
            Logger = logger;
            _appSettings = appSettings;

            //CreateUdpClient();
            _udpClientWrapper = new UdpClientWrapper(
                _appSettings.RETROARCH_LISTEN_IP_ADDRESS, 
                _appSettings.RETROARCH_LISTEN_PORT);
            //UdpClient = UdpClient ?? throw new Exception("Unable to load UDP client.");

            DelayMsBetweenReads = appSettings.RETROARCH_DELAY_MS_BETWEEN_READS;
            Task.Run(async () =>
            {
                await ConnectAsync(_connectionCts.Token);
            });
            // Wait for messages from the UdpClient.
            /*Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (UdpClient == null || UdpClient.Client.Connected == false)
                        {
                            Logger.LogDebug("UdpClient is not connected -- reestablishing connection.");
                            _isConnected = false;
                            CreateUdpClient();
                        }

                        if (UdpClient == null)
                        {
                            throw new Exception("UdpClient is still NULL when waiting for messages.");
                        }

                        var buffer = await UdpClient.ReceiveAsync();
                        ReceivePacket(buffer.Buffer);
                    }
                    catch
                    {
                        _isConnected = false;
                        // Automatically swallow exceptions here because
                        // they're not useful even if there's an error.

                        // We don't want to spam the user with errors.
                    }
                }
            });*/
        }

        private static string ToRetroArchHexdecimalString(uint value)
        {
            // TODO: This is somewhat of a hack because
            // RetroArch returns the request 00 as 0.
            return value <= 9 ? $"{value}" : $"{value:X2}".ToLower();
        }

        public async Task WriteBytes(uint memoryAddress, byte[] values)
        {
            var bytes = string.Join(' ', values.Select(x => x.ToHexdecimalString()));
            await _udpClientWrapper
                .SendPacketAsync("WRITE_CORE_MEMORY", 
                    $"{ToRetroArchHexdecimalString(memoryAddress)} {bytes}");
        }

        /*private async Task SendPacket(string command, string argument)
        {
            // We require to store the command to watch for
            // the response.

            // command    READ_CORE_MEMORY d158
            // argument   11

            var outgoingPayload = $"{command} {argument}";
            var datagram = Encoding.ASCII.GetBytes(outgoingPayload);

            if (UdpClient == null)
            {
                CreateUdpClient();
            }

            if (UdpClient == null)
            {
                throw new Exception($"Unable to create UdpClient to SendPacket({command} {argument})");
            }

            _ = await UdpClient.SendAsync(datagram, datagram.Length);
            Logger.LogTrace($"[Outgoing Packet] {outgoingPayload}");
        }*/

        private async Task<byte[]> ReadMemoryAddress(uint memoryAddress, uint length)
        {
            var command = $"READ_CORE_MEMORY {ToRetroArchHexdecimalString(memoryAddress)}";
            await _udpClientWrapper.SendPacketAsync(command, $"{length}");

            var responsesKey = $"{command} {length}";
            ReceivedPacket? readCoreMemoryResult = null;

            SpinWait.SpinUntil(() =>
            {
                Responses.TryGetValue(responsesKey, out var result);
                readCoreMemoryResult = result;

                return readCoreMemoryResult != null;
            }, TimeSpan.FromMilliseconds(_appSettings.RETROARCH_READ_PACKET_TIMEOUT_MS));

            if (readCoreMemoryResult == null)
            {
                Logger.LogDebug($"A timeout occurred when waiting for ReadMemoryAddress reply from RetroArch. ({responsesKey})");

                throw new DriverTimeoutException(memoryAddress, "RetroArch", null);
            }

            return readCoreMemoryResult.Value;
        }

        /*private void ReceivePacket(byte[] receiveBytes)
        {
            string receiveString = Encoding.ASCII.GetString(receiveBytes).Replace("\n", string.Empty);
            Logger.LogTrace($"[Incoming Packet] {receiveString}");

            var splitString = receiveString.Split(' ');
            var command = splitString[0];
            var memoryAddressString = splitString[1];
            var valueStringArray = splitString[2..];

            if (valueStringArray[0] == "-1")
            {
                throw new Exception(receiveString);
            }

            var memoryAddress = Convert.ToUInt32(memoryAddressString, 16);
            var value = valueStringArray.Select(x => Convert.ToByte(x, 16)).ToArray();

            var receiveKey = $"{command} {memoryAddressString} {valueStringArray.Length}";

            Responses[receiveKey] = new ReceivedPacket(command, memoryAddress, value);
            Logger.LogDebug($"[Incoming Packet] Set response {receiveKey}");
        }*/

        public async Task<bool> TestConnection()
        {
            try
            {
                MemoryAddressBlock block = new("test", 0, 1);
                List<MemoryAddressBlock> blocks = [block];
                var result = await ReadBytes(blocks);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "TestConnection");
                return false;
            }
        }

        public async Task<Dictionary<uint, byte[]>> ReadBytes(IEnumerable<MemoryAddressBlock> blocks)
        {
            var results = await Task.WhenAll(blocks.Select(async x =>
            {
                // We add one here because otherwise we have an off-by-one error.

                // Example: 0xAFFF - 0xA000 is 4095 in decimal.
                // We want to actually return 4096 bytes -- we want to include 0xAFFF.
                // So we add +1 to the result.

                var data = await ReadMemoryAddress(x.StartingAddress, (x.EndingAddress - x.StartingAddress) + 1);
                return new KeyValuePair<uint, byte[]>(x.StartingAddress, data);
            }));

            return results.ToDictionary(x => x.Key, x => x.Value);
        }

        private async Task ConnectAsync(CancellationToken cancellationToken)
        {
            _udpClientWrapper.Connect();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _udpClientWrapper.ReceivePacketAsync(Responses);
                }
                catch
                {
                    _udpClientWrapper.Dispose();
                    // Automatically swallow exceptions here because
                    // they're not useful even if there's an error.

                    // We don't want to spam the user with errors.
                }
            }
        }
        public Task EstablishConnection()
        {
            return Task.CompletedTask;
        }
    }
}