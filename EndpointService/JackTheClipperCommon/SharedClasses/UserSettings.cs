using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JackTheClipperCommon.Enums;
using JetBrains.Annotations;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// UserSettings are connected to a specific User and
    /// contain its Feed definition, its notificationSettings and the Notificationintervall
    /// </summary>
    [DataContract]
    public class UserSettings
    {
        /// <summary>
        /// Gets the id of the settings
        /// </summary>
        [DataMember(Name = "SettingsId")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the feeds.
        /// </summary>
        [NotNull]
        [DataMember(Name = "UserSettingsFeeds")]
        public IReadOnlyCollection<Feed> Feeds { get; private set; }

        /// <summary>
        /// Gets the notification settings.
        /// </summary>
        [IgnoreDataMember]
        public NotificationSetting NotificationSettings { get; private set; }

        /// <summary>
        /// Gets the notification settings as string.
        /// </summary>
        [DataMember(Name = "UserNotificationSetting")]
        public string NotificationSettingsAsString
        {
            get
            {
                return NotificationSettings.ToString();
            }
            private set
            {
                NotificationSettings = Enum.Parse<NotificationSetting>(value);
            }
        }

        /// <summary>
        /// Gets the notification check interval in minutes.
        /// </summary>
        [DataMember(Name = "UserNotificationCheckInterval")]
        public int NotificationCheckIntervalInMinutes { get; private set; }

        /// <summary>
        /// Gets the number of articles per page.
        /// </summary>
        [DataMember(Name = "UserNumberOfArticles")]
        public int ArticlesPerPage { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="UserSettings"/> class from being created.
        /// </summary>
        private UserSettings()
        {
            //For serialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSettings"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="feeds">The feeds.</param>
        /// <param name="notificationSettings">The notification settings.</param>
        /// <param name="notificationCheckIntervalInMinutes">The notification check interval in minutes.</param>
        /// <param name="articlesPerPage">The number of articles per page.</param>
        /// <exception cref="ArgumentNullException">feeds are null</exception>
        public UserSettings(Guid id, [NotNull] IReadOnlyCollection<Feed> feeds,
            NotificationSetting notificationSettings, int notificationCheckIntervalInMinutes, int articlesPerPage)
        {
            Id = id;
            Feeds = feeds ?? throw new ArgumentNullException(nameof(feeds));
            NotificationSettings = notificationSettings;
            NotificationCheckIntervalInMinutes = notificationCheckIntervalInMinutes;
            ArticlesPerPage = articlesPerPage;
        }
    }
}