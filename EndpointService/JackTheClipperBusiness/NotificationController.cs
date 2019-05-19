﻿using System;
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
    public static class NotificationController
    {
        private static readonly ConcurrentDictionary<Guid, DateTime> LastCheckTime = new ConcurrentDictionary<Guid, DateTime>();

        private static readonly Timer Scheduler = new Timer(CheckForNotifications);

        /// <summary>
        /// Starts the time interval for notifications.
        /// </summary>
        public static void Start()
        {
            Scheduler.Change(0, 60 * 1000);
            CheckForNotifications(DateTime.Now);
        }

        /// <summary>
        /// Checks if new feeds available and if notification and what kind of notification is wanted.
        /// </summary>
        /// <param name="timerState">The timer state.</param>
        private static void CheckForNotifications(object timerState)
        {
            Scheduler.Change(Timeout.Infinite, Timeout.Infinite);
            {
                try
                {
                    var notifiableUserSettings = Factory.GetControllerInstance<IClipperDatabase>().GetNotifiableUserSettings();
                    Parallel.ForEach(notifiableUserSettings, async (notifiable) =>
                    {
                        if (notifiable != null && notifiable.Settings.NotificationSettings != NotificationSetting.None && 
                            notifiable.Settings.Feeds.Any())
                        {
                            var lastFetched = LastCheckTime.TryGetOrAddIfNotPresentOrNull(notifiable.UserId);
                            if (lastFetched < notifiable.LastLoginTime)
                            {
                                lastFetched = notifiable.LastLoginTime;
                            }

                            if (lastFetched.AddMinutes(notifiable.Settings.NotificationCheckIntervalInMinutes) <= DateTime.UtcNow)
                            {
                                LastCheckTime[notifiable.UserId] = DateTime.UtcNow;
                                var indexer = DatabaseAdapterFactory.GetControllerInstance<IIndexerService>();
                                var feeds = await indexer.GetCompleteFeedAsync(notifiable.Settings, lastFetched);

                                if (feeds.Any())
                                {
                                    // We need to loop though all (-> blacklists) 
                                    // Anyways one query to determine if at least something COULD be relevant is way better than x querys 
                                    // which determine nothing is relevant at all.
                                    var blackList = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>()
                                                                          .GetUnitInheritedBlackList(notifiable.UserId);
                                    feeds = notifiable.Settings.Feeds.AsParallel()
                                                      .Select(f => indexer.GetFeedAsync(f, lastFetched, 10000, 0, blackList).Result)
                                                      .SelectMany(s => s)
                                                      .Distinct(ArticleComparer.ShortArticleComparer).ToList();
                                    if (feeds.Any())
                                    {
                                        var defText = "Visit " + AppConfiguration.MailConfigurationFELoginLink + notifiable.PrincipalUnitName;
                                        if (notifiable.Settings.NotificationSettings == NotificationSetting.PdfPerMail)
                                        {
                                            var pdf = PdfGeneratorBAD.GeneratePdf(feeds, notifiable.UserName);
                                            MailController.QuerySendMailAsync(notifiable, ClipperTexts.DefaultMailSubject, defText,
                                                                              pdf, "Clipper.pdf");
                                        }
                                        else
                                        {
                                            MailController.QuerySendMailAsync(notifiable, ClipperTexts.DefaultMailSubject, defText);
                                        }
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
