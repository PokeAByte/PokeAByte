using GameHook.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace PokeAByte.Web.Controllers
{
    public record UpdateMemoryModel(uint Address, byte[] Bytes);

    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("driver")]
    public class DriverController(IGameHookDriver gameHookDriver, IGameHookInstance instance) : Controller
    {
        private readonly IGameHookDriver _driver = gameHookDriver;
        private readonly IGameHookInstance _instance = instance;

        [HttpPut("memory")]
        public async Task<IActionResult> WriteMemory(UpdateMemoryModel model)
        {
            await _driver.WriteBytes(model.Address, model.Bytes);

            return Ok();
        }
    }
}
