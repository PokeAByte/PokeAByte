using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Models;

namespace PokeAByte.Web.Services.Drivers;

public class DriverService
{
    private string _driverModel = DriverModels.Bizhawk;
    public static readonly int MaxAttempts = 25;
    private const int MaxPauseMs = 50;
    private int _currentAttempt = 0;

    private readonly IBizhawkMemoryMapDriver _bizhawk;
    private readonly IRetroArchUdpPollingDriver _retroArch;
    private readonly IStaticMemoryDriver _staticMemory;
    public DriverService(
        IBizhawkMemoryMapDriver bizhawk,
        IRetroArchUdpPollingDriver retroArch,
        IStaticMemoryDriver staticMemory)
    {
        _bizhawk = bizhawk;
        _retroArch = retroArch;
        _staticMemory = staticMemory;
    }


    public async Task<string> TestDrivers(Action<int>? callback)
    {
        _currentAttempt = 0;
        //set the list of drivers
        var driverList = DriverModels.DriverList.Select(x => x).ToList();
        //get current driver if there is one, otherwise default to bizhawk
        var currentDriver = string.IsNullOrWhiteSpace(_driverModel) ?
            DriverModels.Bizhawk : _driverModel;
        //Test the drivers
        var connects = false;
        while (!connects && _currentAttempt < MaxAttempts)
        {
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
                        connects = false;
                        break;
                }

                //Set the next driver to test
                if (!connects)
                {
                    currentDriver = driverList.FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(currentDriver))
                        break;
                }
            }

            if (connects) break;
            _currentAttempt += 1;
            callback?.Invoke(_currentAttempt);
            driverList = DriverModels.DriverList.Select(x => x).ToList();
            await Task.Delay(MaxPauseMs);
        }

        //If it connects then return the driver name, otherwise return empty
        if (connects)
            return currentDriver ?? "";
        return string.Empty;
    }

    private async Task<bool> ReadDriver(IPokeAByteDriver driver)
    {
        return await driver.TestConnection();
    }
}