using System.Text.Json;
using GameHook.Domain.Interfaces;
using GameHook.Domain.Models;
using GameHook.Infrastructure.Drivers;
using GameHook.Infrastructure.Drivers.Bizhawk;
using System.Timers;

namespace PokeAByte.Web.Services;

public class DriverService
{
    public event Action? OnDriverModelChange;
    private string _driverModel = DriverModels.Bizhawk;
    public string DriverModel
    {
        get => _driverModel;
        private set
        {
            _driverModel = value;
            OnDriverModelChange?.Invoke();
        }
    }
    private System.Timers.Timer _driverConnectionTestTimer;
    private readonly ILogger<DriverService> _logger;
    private readonly IBizhawkMemoryMapDriver _bizhawk;
    private readonly IRetroArchUdpPollingDriver _retroArch;
    private readonly IStaticMemoryDriver _staticMemory;
    public DriverService(ILogger<DriverService> logger, 
        IBizhawkMemoryMapDriver bizhawk, 
        IRetroArchUdpPollingDriver retroArch, 
        IStaticMemoryDriver staticMemory)
    {
        _logger = logger;
        _bizhawk = bizhawk;
        _retroArch = retroArch;
        _staticMemory = staticMemory;
        ConfigureService();
    }
    private void ConfigureService()
    {
        var userSettings = new UserSettings();
        try
        {
            //Get settings
            if (!File.Exists(BuildEnvironment.UserSettingsJson)) return;
            var jsonStr = File.ReadAllText(BuildEnvironment.UserSettingsJson);
            if (!string.IsNullOrWhiteSpace(jsonStr))
                userSettings = JsonSerializer.Deserialize<UserSettings>(jsonStr);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load settings data.");
        }            
        DriverModel = userSettings?.DefaultDriver ?? DriverModels.Bizhawk;
        /*_driverConnectionTestTimer =
            new System.Timers.Timer(userSettings?.DriverTestTimeoutMs ?? 100);
        _driverConnectionTestTimer.Elapsed += OnTimerEvent;
        _driverConnectionTestTimer.AutoReset = true;
        _driverConnectionTestTimer.Enabled = false;*/
    }
    private async void OnTimerEvent(object? source, ElapsedEventArgs e)
    {
        _driverConnectionTestTimer.Stop();
        var result = await TestDrivers();
        _logger.LogError($"Current Connected Driver: {result}");
        _driverConnectionTestTimer.Start();
    }
    public async Task<string> TestDrivers()
    {
        //set the list of drivers
        var driverList = DriverModels.DriverList;
        //get current driver if there is one, otherwise default to bizhawk
        var currentDriver = string.IsNullOrWhiteSpace(_driverModel) ?
            DriverModels.Bizhawk : _driverModel;
        //Test the drivers
        var connects = false;
        while (!connects && driverList.Count > 0)
        {
            switch (currentDriver)
            {
                case DriverModels.Bizhawk:
                    connects = await ReadDriver(_bizhawk);
                    driverList.Remove(DriverModels.Bizhawk);
                    break;
                case DriverModels.RetroArch:
                    connects = await ReadDriver(_retroArch);
                    driverList.Remove(DriverModels.RetroArch);
                    break;
                case DriverModels.StaticMemory:
                    connects = await ReadDriver(_staticMemory);
                    driverList.Remove(DriverModels.StaticMemory);
                    break;
                default:
                    throw new InvalidOperationException($"{currentDriver} is not a valid driver!");
            }
            //Set the next driver to test
            if (!connects)
            {
                currentDriver = driverList.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(currentDriver))
                    break;
            }
        }
        //If it connects then return the driver name, otherwise return empty
        if (connects)
            return currentDriver ?? "";
        return string.Empty;
    }

    private async Task<bool> ReadDriver(IGameHookDriver driver)
    {
        return await driver.TestConnection();
    }
}