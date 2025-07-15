using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Infrastructure.Drivers;
using PokeAByte.Infrastructure.Drivers.Bizhawk;
using PokeAByte.Infrastructure.Drivers.PokeAProtocol;
using PokeAByte.Infrastructure.Drivers.UdpPolling;

namespace PokeAByte.Web.Services.Drivers;

public interface IDriverService
{
    IStaticMemoryDriver StaticMemory { get; }

    Task<IPokeAByteDriver> GetBizhawkDriver();
    Task<IPokeAByteDriver> GetRetroArchDriver();
    Task<IPokeAByteDriver?> TestDrivers();
}

public class DriverService : IDriverService, IAsyncDisposable
{
    public static readonly int MaxAttempts = 25;
    private const int MaxPauseMs = 50;
    private int _currentAttempt = 0;
    private readonly AppSettings _appSettings;
    private readonly ILogger<RetroArchUdpDriver> _driverLogger;
    private readonly IStaticMemoryDriver _staticMemory;
    private IPokeAByteDriver? _currentDriver;

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

    private async Task Cleanup()
    {
        if (_currentDriver != null)
        {
            await _currentDriver.Disconnect();
            _currentDriver = null;
        }
    }

    public async Task<IPokeAByteDriver> GetBizhawkDriver()
    {
        await Cleanup();
        _currentDriver = new BizhawkMemoryMapDriver(_appSettings);
        await _currentDriver.EstablishConnection();
        return _currentDriver;
    }

    public async Task<IPokeAByteDriver> GetRetroArchDriver()
    {
        await Cleanup();
        _currentDriver = new RetroArchUdpDriver(_driverLogger, _appSettings);
        await _currentDriver.EstablishConnection();
        return _currentDriver;
    }

    public async Task<IPokeAByteDriver> GetPokeAProtocolDriver()
    {
        await Cleanup();
        _currentDriver = new PokeAProtocolDriver(_appSettings);
        await _currentDriver.EstablishConnection();
        return _currentDriver;
    }

    public async Task<IPokeAByteDriver?> TestDrivers()
    {
        _currentAttempt = 0;
        // Test the drivers
        while (_currentAttempt < MaxAttempts)
        {
            if (await PokeAProtocolDriver.Probe(_appSettings))
            {
                return await GetPokeAProtocolDriver();
            }
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

    public async ValueTask DisposeAsync()
    {
        await this.Cleanup();
        return;
    }
}