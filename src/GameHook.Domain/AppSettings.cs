using Microsoft.Extensions.Configuration;

namespace GameHook.Domain
{
    static class AppSettingsHelper
    {
        public static string GetRequiredValue(this IConfiguration configuration, string key)
        {
            var value = configuration[key] ?? throw new Exception($"Configuration '{key}' is missing from appsettings.json");
            if (string.IsNullOrWhiteSpace(value)) throw new Exception($"Configuration '{key}' is empty.");

            return value;
        }
    }

    public class AppSettings
    {
        public AppSettings(IConfiguration configuration)
        {
            Urls = configuration["Urls"] ?? string.Empty;

            RETROARCH_LISTEN_IP_ADDRESS = configuration.GetRequiredValue("RETROARCH_LISTEN_IP_ADDRESS");
            RETROARCH_LISTEN_PORT = int.Parse(configuration.GetRequiredValue("RETROARCH_LISTEN_PORT"));
            RETROARCH_READ_PACKET_TIMEOUT_MS = int.Parse(configuration.GetRequiredValue("RETROARCH_READ_PACKET_TIMEOUT_MS"));
            RETROARCH_DELAY_MS_BETWEEN_READS = int.Parse(configuration.GetRequiredValue("RETROARCH_DELAY_MS_BETWEEN_READS"));

            RETROARCH_DELAY_MS_BETWEEN_READS = int.Parse(configuration.GetRequiredValue("RETROARCH_DELAY_MS_BETWEEN_READS"));

            BIZHAWK_DELAY_MS_BETWEEN_READS = int.Parse(configuration.GetRequiredValue("BIZHAWK_DELAY_MS_BETWEEN_READS"));

            SHOW_READ_LOOP_STATISTICS = bool.Parse(configuration.GetRequiredValue("SHOW_READ_LOOP_STATISTICS"));

            if (BuildEnvironment.IsDebug && configuration["MAPPER_DIRECTORY"]?.Length > 0)
            {
                MAPPER_DIRECTORY = configuration.GetRequiredValue("MAPPER_DIRECTORY");
                MAPPER_VERSION = "LOCAL FILESYTEM";
                MAPPER_DIRECTORY_OVERWRITTEN = true;
            }
            else
            {
                MAPPER_DIRECTORY = Path.Combine(BuildEnvironment.ConfigurationDirectory, "Mappers");
                MAPPER_VERSION = configuration.GetRequiredValue("MAPPER_VERSION");
            }

            LOG_HTTP_TRAFFIC = bool.Parse(configuration.GetRequiredValue("LOG_HTTP_TRAFFIC"));

            var processPath = Path.GetDirectoryName(Environment.ProcessPath) ?? throw new Exception("Unable to determine process path.");
            var localMapperDirectory = Path.Combine(processPath, "mappers");
            if (Directory.Exists(localMapperDirectory))
            {
                MAPPER_LOCAL_DIRECTORY = localMapperDirectory;
            }
        }

        public string Urls { get; }

        public string RETROARCH_LISTEN_IP_ADDRESS { get; }
        public int RETROARCH_LISTEN_PORT { get; }
        public int RETROARCH_READ_PACKET_TIMEOUT_MS { get; }
        public int RETROARCH_DELAY_MS_BETWEEN_READS { get; }

        public int BIZHAWK_DELAY_MS_BETWEEN_READS { get; }

        public bool SHOW_READ_LOOP_STATISTICS { get; }

        public string MAPPER_VERSION { get; }

        public string MAPPER_DIRECTORY { get; }
        public bool MAPPER_DIRECTORY_OVERWRITTEN { get; } = false;

        public string? MAPPER_LOCAL_DIRECTORY { get; }

        public bool LOG_HTTP_TRAFFIC { get; }
    }
}
