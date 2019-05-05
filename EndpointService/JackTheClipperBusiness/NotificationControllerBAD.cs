using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JackTheClipperCommon;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Extensions;
using JackTheClipperCommon.Localization;
using JackTheClipperData;

namespace JackTheClipperBusiness
{
    /// <summary>
    /// Check if Notification is wanted/necessary
    /// </summary>
    public static class NotificationControllerBAD
    {
        private static readonly ConcurrentDictionary<Guid, DateTime> LastCheckTime = new ConcurrentDictionary<Guid, DateTime>();

        private static readonly Timer Scheduler = new Timer(CheckForNotifications);


        public static void Start()
        {
            Scheduler.Change(0, 60 * 1000);
        }

        private static void CheckForNotifications(object timerState)
        {
            Scheduler.Change(Timeout.Infinite, Timeout.Infinite);
            //using (new PerfTracer(nameof(CheckForNotifications) + DateTime.UtcNow.ToLongTimeString()))
            {
                try
                {
                    var users = Factory.GetControllerInstance<IClipperDatabase>().GetAllUsers();
                    Parallel.ForEach(users, async (user) =>
                    {
                        if (user.Settings != null && user.Settings.NotificationSettings != NotificationSetting.None)
                        {
                            var lastFetched = LastCheckTime.TryGetOrAddIfNotPresentOrNull(user.Id);
                            if (lastFetched.AddMinutes(user.Settings.NotificationCheckIntervalInMinutes) <=
                                DateTime.UtcNow)
                            {
                                LastCheckTime[user.Id] = DateTime.UtcNow;

                                var feeds =
                                    await DatabaseAdapterFactory
                                          .GetControllerInstance<IIndexerService>()
                                          .GetCompleteFeedAsync(user, lastFetched);

                                if (feeds.Any())
                                {
                                    var defText = "Visit " + AppConfiguration.MailConfigurationFELoginLink;
                                    if (user.Settings.NotificationSettings == NotificationSetting.PdfPerMail)
                                    {
                                        var pdf = PdfGeneratorBAD.GeneratePdf(feeds);
                                        MailController.QuerySendMailAsync(user, ClipperTexts.DefaultMailSubject, defText, pdf, "Clipper.pdf");
                                    }
                                    else
                                    {
                                        MailController.QuerySendMailAsync(user, ClipperTexts.DefaultMailSubject, defText);
                                    }
                                }
                            }
                        }
                    });
                }
                catch (Exception error)
                {
                    Console.WriteLine(error);
                }
                finally
                {
                    Scheduler.Change(60 * 1000, Timeout.Infinite);
                }
            }
        }
    }
}
