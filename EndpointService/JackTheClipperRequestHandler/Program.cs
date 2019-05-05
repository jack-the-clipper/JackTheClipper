using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JackTheClipperBusiness;
using JackTheClipperBusiness.CrawlerManagement;
using JackTheClipperBusiness.Notification;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using JackTheClipperCommon.Configuration;

namespace JackTheClipperRequestHandler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            var url = config.GetValue<bool>("SiteConfiguration:SSL") ? "https://" : "https://" 
                                                                                    + config.GetValue<string>("SiteConfiguration:IP") + ":" 
                                                                                    + config.GetValue<string>("SiteConfiguration:Port");
            AppConfiguration.RegisterConfig(config);
            StartInternalServices();
            return WebHost.CreateDefaultBuilder(args)
                          .UseKestrel()
                          .UseStartup<Startup>()
                          .UseUrls(url);
        }

        private static void StartInternalServices()
        {
            if (AppConfiguration.ElasticClearIndex)
            {
                Factory.GetControllerInstance<ICrawlerController>().ClearAllIndexes();
            }

            Factory.GetControllerInstance<ICrawlerController>().Restart();
            Task.Run(() =>
            {
                Thread.Sleep(5000);
                Factory.GetControllerInstance<INotificationController>().StartNotificationProcessing();
            });
        }
    }
}
