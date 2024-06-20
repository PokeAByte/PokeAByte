using GameHook.Domain.Models.Properties;
using Microsoft.AspNetCore.Components;

namespace PokeAByte.Web.Components.Properties;

public partial class PropertyView : ComponentBase
{
    [Parameter] public PropertyModel Context { get; set; }
}