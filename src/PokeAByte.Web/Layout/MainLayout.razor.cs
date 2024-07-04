using Microsoft.AspNetCore.Components;
using MudBlazor;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Layout;

public partial class MainLayout
{
    [Inject] private MapperClientService? MapperConnectionService { get; set; }
    
    private Color _mapperConnectedColor;
    public const string PageTitle = "Poke-A-Byte";
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _mapperConnectedColor = MapperConnectionService?.GetCurrentConnectionColor() ?? 
                                Color.Info;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        _mapperConnectedColor = MapperConnectionService?.GetCurrentConnectionColor() ?? 
                                Color.Info;
    }
}