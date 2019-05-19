using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JackTheClipperCommon.Enums;
using JetBrains.Annotations;

namespace JackTheClipperCommon.SharedClasses
{
    /// <inheritdoc />
    /// <summary>
    /// Contains Settings, which are specific for the Organization
    /// </summary>
    [DataContract]
    public class OrganizationalUnitSettings : UserSettings
    {
        /// <summary>
        /// Gets the generally available sources.
        /// </summary>
        [DataMember(Name = "OrganizationalUnitSources")]
        public IReadOnlyCollection<Source> AvailableSources { get; private set; }

        /// <summary>
        /// Gets the black list.
        /// </summary>
        [DataMember(Name = "OrganizationalUnitBlackList")]
        public IReadOnlyCollection<string> BlackList { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationalUnitSettings"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="feeds">The feeds.</param>
        /// <param name="availableSources">The available sources.</param>
        /// <param name="notification">The notification settings.</param>
        /// <param name="notificationInterval">The notification interval.</param>
        /// <param name="blackList">The black list.</param>
        public OrganizationalUnitSettings(Guid id, [NotNull] IReadOnlyCollection<Feed> feeds, IReadOnlyCollection<Source> availableSources, NotificationSetting notification, int notificationInterval, IReadOnlyCollection<string> blackList) : base(id, feeds, notification, notificationInterval, 20)
        {
            AvailableSources = availableSources;
            BlackList = blackList;
        }
    }
}