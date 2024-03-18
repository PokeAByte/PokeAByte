using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace GameHook.WebAPI
{
    public static class EmbededResources
    {
        public static Stream appsettings_json => ApiHelper.GetEmbeddedResourceStream("GameHook.WebAPI.appsettings.json");
        public static Stream index_html => ApiHelper.GetEmbeddedResourceStream("GameHook.WebAPI.wwwroot.index.html");
        public static Stream favicon_ico => ApiHelper.GetEmbeddedResourceStream("GameHook.WebAPI.wwwroot.favicon.ico");
        public static Stream site_css => ApiHelper.GetEmbeddedResourceStream("GameHook.WebAPI.wwwroot.site.css");
        public static Stream dist_gameHookMapperClient_js => ApiHelper.GetEmbeddedResourceStream("GameHook.WebAPI.wwwroot.dist.gameHookMapperClient.js");
    }

    public static class ApiHelper
    {
        /// <summary>
        /// Takes the full name of a resource and loads it in to a stream.
        /// </summary>
        /// <param name="resourceName">Assuming an embedded resource is a file
        /// called info.png and is located in a folder called Resources, it
        /// will be compiled in to the assembly with this fully qualified
        /// name: Full.Assembly.Name.Resources.info.png. That is the string
        /// that you should pass to this method.</param>
        /// <returns></returns>
        public static Stream GetEmbeddedResourceStream(string resourceName)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null) { throw new Exception($"Unable to load embeded resource: {resourceName}"); }

            return stream;
        }

        /// <summary>
        /// Get the list of all emdedded resources in the assembly.
        /// </summary>
        /// <returns>An array of fully qualified resource names</returns>
        public static string[] GetEmbeddedResourceNames()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }

        public static string FromRouteToPath(this string route)
        {
            return route.Replace("/", ".").Replace("%2F", ".");
        }

        public static string StripEndingRoute(this string route)
        {
            if (route.EndsWith("/")) { return route.Substring(0, route.Length - 1); }
            return route;
        }

        public static ObjectResult BadRequestResult(string detail)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "BAD_REQUEST",
                Detail = detail
            };

            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 400,
            };
        }

        public static ObjectResult MapperNotLoaded()
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "MAPPER_NOT_LOADED",
                Detail = "Please load a mapper file first."
            };

            return new ObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" },
                StatusCode = 400,
            };
        }
    }
}
