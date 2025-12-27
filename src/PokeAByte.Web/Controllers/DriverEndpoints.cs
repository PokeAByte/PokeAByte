using Microsoft.AspNetCore.Mvc;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Web.Services.Mapper;

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
        app.MapGet(
            "/driver/name",
            (IInstanceService instance) => Results.Ok(instance.Instance?.Driver?.ProperName)
        );
    }
}
