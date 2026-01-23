using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;
using PokeAByte.Infrastructure.Drivers.PokeAProtocol;
using PokeAByte.Infrastructure.Drivers.UdpPolling;

namespace PokeAByte.Web.Services.Drivers;

public interface IDriverService
{
    Task<IPokeAByteDriver?> TestDrivers();
}

public class DriverService : IDriverService, IAsyncDisposable
{
    public static readonly int MaxAttempts = 25;
    private const int MaxPauseMs = 50;
    private int _currentAttempt = 0;
    private readonly AppSettingsService _appSettingsService;
    private readonly ILogger<RetroArchUdpDriver> _driverLogger;
    private IPokeAByteDriver? _currentDriver;

    public DriverService( AppSettingsService settingsService, ILogger<RetroArchUdpDriver> driverLogger)
    {
        _appSettingsService = settingsService;
        _driverLogger = driverLogger;
    }

    private async Task Cleanup()
    {
        if (_currentDriver != null)
        {
            await _currentDriver.Disconnect();
            _currentDriver = null;
        }
    }

    private async Task<IPokeAByteDriver> GetRetroArchDriver()
    {
        await Cleanup();
        _currentDriver = new RetroArchUdpDriver(_driverLogger, _appSettingsService.Get());
        await _currentDriver.EstablishConnection();
        return _currentDriver;
    }

    private async Task<IPokeAByteDriver> GetPokeAProtocolDriver()
    {
        await Cleanup();
        _currentDriver = new PokeAProtocolDriver(_appSettingsService.Get());
        await _currentDriver.EstablishConnection();
        return _currentDriver;
    }

    public async Task<IPokeAByteDriver?> TestDrivers()
    {
        _currentAttempt = 0;
        // Test the drivers
        while (_currentAttempt < MaxAttempts)
        {
            if (await PokeAProtocolDriver.Probe(_appSettingsService.Get()))
            {
                return await GetPokeAProtocolDriver();
            }
            else if (await RetroArchUdpDriver.Probe(_appSettingsService.Get()))
            {
                return await GetRetroArchDriver();
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