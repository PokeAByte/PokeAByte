namespace PokeAByte.Domain.Models;

public static class DriverModels
{
    public const string Bizhawk = "bizhawk";
    public const string RetroArch = "retroarch";
    public const string StaticMemory = "staticMemory";
    public static readonly List<string> DriverList = [Bizhawk, RetroArch, StaticMemory];
}