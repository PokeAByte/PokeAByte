using GameHook.Domain.Interfaces;
using GameHook.Domain.Models.Mappers;
using MudBlazor;

namespace PokeAByte.Web.Services;

public class MapperConnectionService
{
    public MapperModel? CurrentlyLoadedMapper { get; private set; }
    
    private readonly IMapperFilesystemProvider _mapperFilesystemProvider;

    public MapperConnectionService(IMapperFilesystemProvider mapperFs)
    {
        _mapperFilesystemProvider = mapperFs;
    }
    private const Color DisconnectedColor = Color.Error;
    private const Color ConnectedColor = Color.Success;
    
    public bool GetCurrentConnectionStatus() => CurrentlyLoadedMapper is not null;
    public Color GetCurrentConnectionColor() => CurrentlyLoadedMapper is not null ? 
        ConnectedColor : DisconnectedColor;
    public string GetCurrentConnectionName() => CurrentlyLoadedMapper is not null ?
        "Connected" : "Disconnected";

}