using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Web.Controllers;

public record UpdateMemoryModel(uint Address, byte[] Bytes);

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("driver")]
public class DriverController(IStaticMemoryDriver pokeAByteDriver) : Controller
{
    private readonly IPokeAByteDriver _driver = pokeAByteDriver;

    [HttpPut("memory")]
    public async Task<IActionResult> WriteMemory(UpdateMemoryModel model)
    {
        await _driver.WriteBytes(model.Address, model.Bytes);
        return Ok();
    }
}
