using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Infrastructure.Drivers;
using PokeAByte.Infrastructure.Drivers.Bizhawk;
using PokeAByte.Infrastructure.Drivers.UdpPolling;

namespace PokeAByte.Web.Services.Drivers;

public class DriverService
{
    public static readonly int MaxAttempts = 25;
    private const int MaxPauseMs = 50;
    private int _currentAttempt = 0;
    private readonly AppSettings _appSettings;
    private readonly IBizhawkMemoryMapDriver _bizhawk;
    private readonly IRetroArchUdpPollingDriver _retroArch;
    private readonly IStaticMemoryDriver _staticMemory;

    public IBizhawkMemoryMapDriver Bizhawk => _bizhawk;
    public IRetroArchUdpPollingDriver RetroArch => _retroArch;
    public IStaticMemoryDriver StaticMemory => _staticMemory;


    public DriverService(
        AppSettings appSettings,
        IBizhawkMemoryMapDriver bizhawk,
        IRetroArchUdpPollingDriver retroArch,
        IStaticMemoryDriver staticMemory)
    {
        _appSettings = appSettings;
        _bizhawk = bizhawk;
        _retroArch = retroArch;
        _staticMemory = staticMemory;
    }

    public async Task<string> TestDrivers(Action<int>? callback)
    {
        _currentAttempt = 0;
        //Test the drivers
        while (_currentAttempt < MaxAttempts)
        {
            if (await BizhawkMemoryMapDriver.Probe(_appSettings)) {
                await _bizhawk.EstablishConnection();
                return DriverModels.Bizhawk;
            } else if (await RetroArchUdpDriver.Probe(_appSettings)) {
                await _retroArch.EstablishConnection();
                return DriverModels.RetroArch;
            } else if (await StaticMemoryDriver.Probe(_appSettings)) {
                return DriverModels.StaticMemory;
            }
            _currentAttempt += 1;
            callback?.Invoke(_currentAttempt);
            await Task.Delay(MaxPauseMs);
        }
        return string.Empty;
    }
}