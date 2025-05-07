using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Infrastructure.Drivers;
using PokeAByte.Infrastructure.Drivers.Bizhawk;
using PokeAByte.Infrastructure.Drivers.UdpPolling;

namespace PokeAByte.Web.Services.Drivers;

public interface IDriverService
{
    IStaticMemoryDriver StaticMemory { get; }

    Task<IBizhawkMemoryMapDriver> GetBizhawkDriver();
    Task<IRetroArchUdpPollingDriver> GetRetroArchDriver();
    Task<IPokeAByteDriver?> TestDrivers();
}

public class DriverService : IDriverService
{
    public static readonly int MaxAttempts = 25;
    private const int MaxPauseMs = 50;
    private int _currentAttempt = 0;
    private readonly AppSettings _appSettings;
    private readonly ILogger<RetroArchUdpDriver> _driverLogger;
    private readonly IStaticMemoryDriver _staticMemory;

    public DriverService(
        AppSettings appSettings,
        ILogger<RetroArchUdpDriver> driverLogger,
        IStaticMemoryDriver staticMemory)
    {
        _appSettings = appSettings;
        _driverLogger = driverLogger;
        _staticMemory = staticMemory;
    }

    public IStaticMemoryDriver StaticMemory => _staticMemory;

    public async Task<IBizhawkMemoryMapDriver> GetBizhawkDriver()
    {
        var driver = new BizhawkMemoryMapDriver(_appSettings);
        await driver.EstablishConnection();
        return driver;
    }

    public async Task<IRetroArchUdpPollingDriver> GetRetroArchDriver()
    {
        var driver = new RetroArchUdpDriver(_driverLogger, _appSettings);
        await driver.EstablishConnection();
        return driver;
    }

    public async Task<IPokeAByteDriver?> TestDrivers()
    {
        _currentAttempt = 0;
        // Test the drivers
        while (_currentAttempt < MaxAttempts)
        {
            if (await BizhawkMemoryMapDriver.Probe(_appSettings))
            {
                return await GetBizhawkDriver();
            }
            else if (await RetroArchUdpDriver.Probe(_appSettings))
            {
                return await GetRetroArchDriver();
            }
            else if (await StaticMemoryDriver.Probe(_appSettings))
            {
                return _staticMemory;
            }
            _currentAttempt += 1;
            await Task.Delay(MaxPauseMs);
        }
        return null;
    }
}