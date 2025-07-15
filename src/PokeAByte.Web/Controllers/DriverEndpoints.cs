using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain.Interfaces;

namespace PokeAByte.Web.Controllers;

public record UpdateMemoryModel(uint Address, byte[] Bytes);

public static class DriverEndpoints
{
    public static void MapDriverEndpoints(this WebApplication app)
    {
        app.MapPut(
            "/diver/memory",
            async (IStaticMemoryDriver driver, [FromBody] UpdateMemoryModel model)
                => await driver.WriteBytes(model.Address, model.Bytes)
        );
    }
}
