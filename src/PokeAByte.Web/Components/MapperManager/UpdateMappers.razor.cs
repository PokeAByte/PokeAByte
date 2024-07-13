using GameHook.Domain.Interfaces;
using Microsoft.AspNetCore.Components;
using PokeAByte.Web.Services;

namespace PokeAByte.Web.Components.MapperManager;

public partial class UpdateMappers : ComponentBase
{
    [Inject] public MapperManagerService ManagerService { get; set; }
}