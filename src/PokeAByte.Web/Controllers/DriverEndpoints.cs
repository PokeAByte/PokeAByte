using Microsoft.AspNetCore.Http.HttpResults;
using PokeAByte.Web.Services.Mapper;

namespace PokeAByte.Web.Controllers;

internal static class DriverEndpoints
{
    internal static void MapDriverEndpoints(this WebApplication app)
    {
        app.MapGet("/driver/name", GetDriverName);;
    }

    /// <summary>
    /// Get the name of the emulator driver currently used.
    /// </summary>
    /// <param name="service"></param>
    /// <returns> Null if no mapper is loaded. </returns>
    public static Ok<string> GetDriverName(IInstanceService service) {
        return TypedResults.Ok(service.Instance?.Driver?.ProperName);
    }
}
