using Microsoft.Extensions.Configuration;

namespace JackTheClipperCommon.Configuration
{
    /// <summary>
    /// Private set of the default configuration properties from appsettings.json
    /// </summary>

    public class AppConfiguration
    {
        public static string LoggingLogLevelDefault { get; private set; }
        public static string AllowedHosts { get; private set; }
        public static string KestrelEndPointsHTTPUrl { get; private set; }
        public static string SiteConfigIP { get; private set; }
        public static int SiteConfigPort { get; private set; }
        public static bool SiteConfigSSL { get; private set; }
        public static string MailConfigurationFrom { get; private set; }
        public static string MailConfigurationPassword { get; private set; }
        public static string MailConfigurationHost { get; private set; }
        public static int MailConfigurationPort { get; private set; }
        public static bool MailConfigurationSSL { get; private set; }
        public static string MailConfigurationFELoginLink { get; private set; }
        public static string SqlServerConnectionString { get; private set; }
        public static bool ElasticClearIndex { get; private set; }
        public static string ElasticPermanentIndexName { get; private set; }
        public static string ElasticTemporaryIndexName { get; private set; }
        public static string ElasticRssSpeedIndexName { get; private set; }

        /// <summary>
        /// Set the default configuration settings for the connection.
        /// </summary>
        /// <param name="config">The current config.</param>
        public static void RegisterConfig(IConfigurationRoot config)
        {
            var configuration = config;

            LoggingLogLevelDefault = configuration["Logging:LogLevel:Default"];
            AllowedHosts = configuration["AllowedHosts"];
           
            //CUSTOM PREFERENCES - Kestrel (Raspberry): SET
            KestrelEndPointsHTTPUrl = configuration["Kestrel:EndPoints:Http1:Url"];

            //CUSTOM PREFERENCES - IIS: SET
            SiteConfigIP = configuration["SiteConfiguration:IP"];
            SiteConfigPort = int.Parse(configuration["SiteConfiguration:Port"]);
            SiteConfigSSL = bool.Parse(configuration["SiteConfiguration:SSL"]);

            MailConfigurationFrom = configuration["MailConfiguration:From"];
            MailConfigurationPassword = configuration["MailConfiguration:Password"];
            MailConfigurationHost = configuration["MailConfiguration:Host"];
            MailConfigurationPort = int.Parse(configuration["MailConfiguration:Port"]);
            MailConfigurationSSL = bool.Parse(configuration["MailConfiguration:SSL"]);
            MailConfigurationFELoginLink = configuration["MailConfiguration:FrontEndLoginLink"];
            SqlServerConnectionString = configuration["ServerConfiguration:SQLConnectionString"];
            ElasticPermanentIndexName = configuration["ServerConfiguration:ElasticPermanentIndex"];
            ElasticTemporaryIndexName = configuration["ServerConfiguration:ElasticTemporaryIndex"];
            ElasticRssSpeedIndexName = configuration["ServerConfiguration:ElasticRssSpeedIndex"];
            ElasticClearIndex = bool.Parse(configuration["ServerConfiguration:ClearElasticIndexes"] ?? "false");
        }
    }
}
    