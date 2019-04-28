using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JackTheClipperCommon;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Extensions;
using JackTheClipperCommon.Interfaces;
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
                    var userController = Factory.GetControllerInstance<IClipperUserAPI>();
                    var users = Factory.GetControllerInstance<IClipperDatabase>().GetAllUsers();
                    Parallel.ForEach(users, (user) =>
                    {
                        if (user.Settings != null && user.Settings.NotificationSettings != NotificationSetting.None)
                        {
                            var lastFetched = LastCheckTime.TryGetOrAddIfNotPresentOrNull(user.Id);
                            if (lastFetched.AddMinutes(user.Settings.NotificationCheckIntervalInMinutes) <=
                                DateTime.UtcNow)
                            {
                                LastCheckTime[user.Id] = DateTime.UtcNow;
                                var feeds = user.Settings.Feeds.ToList().AsParallel()
                                    .SelectMany(feed => userController.GetFeed(user, feed))
                                    .Where(x => x.Indexed >= lastFetched)
                                    .Distinct(ArticleComparer.ShortArticleComparer)
                                    .OrderByDescending(x => x.Published)
                                    .ToList();

                                if (feeds.Any())
                                {
                                    var defText = "Visit " + AppConfiguration.MailConfigurationFELoginLink;
                                    if (user.Settings.NotificationSettings == NotificationSetting.PdfPerMail)
                                    {
                                        var pdf = PdfGeneratorBAD.GeneratePdf(feeds);
                                        MailControllerBAD.QuerySendMailAsync(user, ClipperTexts.DefaultMailSubject, defText, pdf);
                                    }
                                    else
                                    {
                                        MailControllerBAD.QuerySendMailAsync(user, ClipperTexts.DefaultMailSubject, defText);
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