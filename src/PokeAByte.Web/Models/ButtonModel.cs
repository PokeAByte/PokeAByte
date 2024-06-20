using PokeAByte.Web.Services;

namespace PokeAByte.Web.Models;

public class ButtonModel
{
    public NavigationService.Pages Page { get; init; }
    public bool IsDeactivated { get; set; }
    public void SetDeactivated(bool isDeactivated) => IsDeactivated = isDeactivated;

}