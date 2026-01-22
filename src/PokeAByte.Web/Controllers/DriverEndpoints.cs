using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Controllers;

public static class DriverEndpoints
{
    public static void MapDriverEndpoints(this WebApplication app)
    {
        app.MapGet(
            "/driver/name",
            (IInstanceService instance) => Results.Ok(instance.Instance?.Driver?.ProperName)
        );
    }
}
