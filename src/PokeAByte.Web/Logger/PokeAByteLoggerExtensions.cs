using Microsoft.Extensions.Logging.Configuration;

namespace PokeAByte.Web.Logger;

public static class PokeAByteLoggerExtensions
{
    public static ILoggingBuilder AddPokeAByteLogger(
        this ILoggingBuilder builder,
        Action<LoggerConfiguration> configure)
    {
        builder.AddConfiguration();
        builder.Services.AddSingleton<ILoggerProvider, PokeAByteLoggerProvider>();
        builder.Services.Configure(configure);

        return builder;
    }
}