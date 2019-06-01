﻿using System;
using Microsoft.Extensions.Configuration;

namespace JackTheClipperCommon.Configuration
{
    /// <summary>
    ///     Private set of the default configuration properties from appsettings.json
    /// </summary>
    public class AppConfiguration
    {
        /// <summary>
        ///     Which hosts are allowed to access this application
        /// </summary>
        public static string AllowedHosts { get; private set; }

        /// <summary>
        ///     The url for use within kestrel.
        /// </summary>
        public static string KestrelEndPointsHTTPUrl { get; private set; }

        /// <summary>
        ///     The ip address the application should bind to
        /// </summary>
        public static string SiteConfigIP { get; private set; }

        /// <summary>
        ///     The port the application should bind to
        /// </summary>
        public static int SiteConfigPort { get; private set; }

        /// <summary>
        ///     Whether the application should use Https over Http
        /// </summary>
        public static bool SiteConfigSSL { get; private set; }

        /// <summary>
        ///     The name of the user the mails are sent from
        /// </summary>
        public static string MailConfigurationFrom { get; private set; }

        /// <summary>
        ///     The password to access the mail server
        /// </summary>
        public static string MailConfigurationPassword { get; private set; }

        /// <summary>
        ///     Host of the mail server
        /// </summary>
        public static string MailConfigurationHost { get; private set; }

        /// <summary>
        ///     Port of the mail server
        /// </summary>
        public static int MailConfigurationPort { get; private set; }

        /// <summary>
        ///     Whether the mail server uses Https
        /// </summary>
        public static bool MailConfigurationSSL { get; private set; }

        /// <summary>
        ///     The login link of the frontend
        /// </summary>
        public static string MailConfigurationFELoginLink { get; private set; }

        /// <summary>
        ///     Timeout for the mail server. Default is 10000. This is in Milliseconds
        /// </summary>
        public static int MailConfigurationTimeout { get; private set; }

        /// <summary>
        ///     The url of the SQL server
        /// </summary>
        public static string SqlServerConnectionString { get; private set; }

        /// <summary>
        ///     Whether to clear existing indexes of ElasticSearch on application start.
        ///     Default is false
        /// </summary>
        public static bool ElasticClearIndex { get; private set; }

        /// <summary>
        ///     The name of the permanent index. This is where all articles are saved in
        /// </summary>
        public static string ElasticPermanentIndexName { get; private set; }

        /// <summary>
        ///     The name of the temporary index. Articles are only saved for a short amount of time in here.
        ///     They are either discarded if deemed uninteresting for all users of the application or saved to
        ///     permanent index otherwise
        /// </summary>
        public static string ElasticTemporaryIndexName { get; private set; }

        /// <summary>
        ///     The name of the index to compare rss articles. This is a different one that allows for better
        ///     performance since articles can be compared by something else than their entire content
        /// </summary>
        public static string ElasticRssSpeedIndexName { get; private set; }

        /// <summary>
        ///     Uri to communicate with the Elastic Search Server
        /// </summary>
        public static string ElasticUri { get; private set; }

        /// <summary>
        ///     Whether performance should be traced for this application.
        ///     Default is false
        /// </summary>
        public static bool PerformanceTraceActive { get; private set; }

        /// <summary>
        ///     In which intervals websites should be crawled. This is in seconds.
        ///     Minimum value is 10. Default is 120
        /// </summary>
        public static int WebsiteCrawlInterval { get; private set; }

        /// <summary>
        ///     In which intervals rss feeds should be crawled. This is in seconds.
        ///     Minimum value is 10. Default is 45
        /// </summary>
        public static int RssCrawlInterval { get; private set; }

        /// <summary>
        ///     How long the autogenerated passwords should be. Must be greater than
        ///     0. Default value is 8
        /// </summary>
        public static int PasswordLength { get; private set; }

        /// <summary>
        ///     How long the short text of a ShortArticle is. Minimum value of 3.
        ///     Default is 200
        /// </summary>
        public static int ShortTextLength { get; private set; }

        /// <summary>
        ///     Which user agent to use when crawling website. Some sites deliver
        ///     their content based on the user agent. Defaults to Mozilla/5.0
        /// </summary>
        public static string UserAgent { get; private set; }

        /// <summary>
        /// Gets or sets the maximum degree of parallelism for notification job processing.
        /// Default: CPU-cores * 0.75
        /// Cant exceed <code>Environment.ProcessorCount * 2</code>
        /// </summary>
        public static int MaxNotificationJobDegreeOfParallelism { get; set; }

        /// <summary>
        /// Gets or sets the amount of last RSS last items which should be cached.
        /// Default: 5000
        /// </summary>
        public static int RssLastItemCacheSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the superset feed should be updated periodically by a cron job
        /// (this should improve performance in most scenarios).
        /// </summary>
        public static bool UseSuperSetFeedCronJob { get; set; }

        /// <summary>
        /// Gets or sets the super set feed cron job interval. Only takes effect if <see cref="UseSuperSetFeedCronJob"/> is set to <code>true</code>.
        /// </summary>
        public static int SuperSetFeedCronJobInterval { get; set; }

        /// <summary>
        ///     Set the default configuration settings for the connection.
        /// </summary>
        /// <param name="config">The current config.</param>
        public static void RegisterConfig(IConfigurationRoot config)
        {
            var configuration = config;

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
            MailConfigurationTimeout = int.Parse(configuration["MailConfiguration:Timeout"] ?? "10000");

            SqlServerConnectionString = configuration["ServerConfiguration:SQLConnectionString"];
            ElasticPermanentIndexName = configuration["ServerConfiguration:ElasticPermanentIndex"];
            ElasticTemporaryIndexName = configuration["ServerConfiguration:ElasticTemporaryIndex"];
            ElasticRssSpeedIndexName = configuration["ServerConfiguration:ElasticRssSpeedIndex"];
            ElasticUri = configuration["ServerConfiguration:ElasticUri"];
            ElasticClearIndex = bool.Parse(configuration["ServerConfiguration:ClearElasticIndexes"] ?? "false");
            PerformanceTraceActive = bool.Parse(configuration["ServerConfiguration:PerfTrace"] ?? "false");
            MaxNotificationJobDegreeOfParallelism =
                int.Parse(configuration["ServerConfiguration:MaxNotificationJobDegreeOfParallelism"] ??
                          Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75)).ToString());
            MaxNotificationJobDegreeOfParallelism = Math.Min(Environment.ProcessorCount * 2, MaxNotificationJobDegreeOfParallelism);
            RssLastItemCacheSize = int.Parse(configuration["ServerConfiguration:RssLastItemCacheSize"] ?? "5000");
            UseSuperSetFeedCronJob = bool.Parse(configuration["ServerConfiguration:UseSuperSetFeedCronJob"] ?? "true");

            var tempInterval = int.Parse(configuration["CrawlingConfiguration:WebsiteInterval"] ?? "120");
            WebsiteCrawlInterval = tempInterval >= 10 ? tempInterval : 10;
            tempInterval = int.Parse(configuration["CrawlingConfiguration:RssInterval"] ?? "45");
            RssCrawlInterval = tempInterval >= 10 ? tempInterval : 10;
            UserAgent = configuration["CrawlingConfiguration:UserAgent"] ?? "Mozilla/5.0";
            SuperSetFeedCronJobInterval = int.Parse(configuration["ServerConfiguration:SuperSetFeedCronJobInterval"] ?? Math.Min(WebsiteCrawlInterval, RssCrawlInterval).ToString());

            var tempLength = int.Parse(configuration["CrawlingConfiguration:ShortTextLength"] ?? "200");
            ShortTextLength = tempLength > 3 ? tempLength : 3;
            var tempPwLength = int.Parse(configuration["PasswordConfiguration:Length"] ?? "8");
            PasswordLength = tempPwLength > 0 ? tempPwLength : 8;
        }
    }
}