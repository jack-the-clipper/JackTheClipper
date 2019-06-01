using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JackTheClipperBusiness;
using JackTheClipperBusiness.CrawlerManagement;
using JackTheClipperBusiness.Notification;
using JackTheClipperCommon.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace JackTheClipperRequestHandler
{
    /// <summary>
    ///     Class responsible for starting the application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        ///     Configures the WebHostBuilder for this application
        /// </summary>
        /// <param name="args">Commandline parameters (if present)</param>
        /// <returns>IWebHostBuilder used to build the application</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();
            AppConfiguration.RegisterConfig(config);
            var url = AppConfiguration.SiteConfigSSL
                ? "https://"
                : "https://" + AppConfiguration.SiteConfigIP + ":"
                  + AppConfiguration.SiteConfigPort;

            StartInternalServices();
            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls(url);
        }

        /// <summary>
        ///     Starts all the services needed by the application like the <see cref="ICrawlerController" />
        ///     implementation
        /// </summary>
        private static void StartInternalServices()
        {
            if (AppConfiguration.ElasticClearIndex)
                Factory.GetControllerInstance<ICrawlerController>().ClearAllIndexes();

            Factory.GetControllerInstance<ICrawlerController>().Restart();
            Task.Run(() =>
            {
                Thread.Sleep(60*1000);
                Factory.GetControllerInstance<INotificationController>().StartNotificationProcessing();
            });
        }
    }
}