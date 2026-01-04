namespace PokeAByte.Web.Middleware;

public class RequestLogMiddleware : IMiddleware {
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<RestAPI>>();
            logger.LogWarning($"Endpoint {context.GetEndpoint()?.DisplayName} encountered an exception: {ex}");
            await Results.InternalServerError("Request failed due to an exception: " + ex.Message).ExecuteAsync(context);
        }
    }
}
