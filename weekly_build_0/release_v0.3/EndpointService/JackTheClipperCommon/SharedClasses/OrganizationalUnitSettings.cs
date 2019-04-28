using System;
using System.Collections.Generic;
using System.Linq;
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
        [IgnoreDataMember]
        public IReadOnlyCollection<Source> AvailableSources { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationalUnitSettings"/> class.
        /// </summary>
        /// <param name="feeds">The feeds.</param>
        /// <param name="availableSources">The available sources.</param>
        public OrganizationalUnitSettings(Guid id, [NotNull] IReadOnlyCollection<Feed> feeds, IReadOnlyCollection<Source> availableSources, NotificationSetting notification, int notificationInterval) : base(id, feeds, notification, notificationInterval)
        {
            AvailableSources = availableSources;
        }

        /// <summary>
        /// Adds a source.
        /// </summary>
        /// <param name="toAdd">The source to add.</param>
        public void AddSource(Source toAdd)
        {
            if (!AvailableSources.Contains(toAdd))
            {
                var copy = AvailableSources.ToList();
                copy.Add(toAdd);
                AvailableSources = copy;
            }
        }
    }
}