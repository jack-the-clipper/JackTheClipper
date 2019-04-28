using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JackTheClipperCommon.Enums;
using JetBrains.Annotations;

namespace JackTheClipperCommon.SharedClasses
{

    /// <summary>
    /// Contains the definition of a User.
    /// It has a unique Id, a UserName, a Password and a Role.
    /// It also has a reference to its UserSettings
    /// </summary>
    [DataContract]
    public class User
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        [DataMember(Name = "UserId")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        [DataMember(Name = "UserName")]
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the role(s).
        /// </summary>
        [IgnoreDataMember]
        public Role Role { get; private set; }

        /// <summary>
        /// Gets the role as string.
        /// </summary>
        [DataMember(Name = "UserRole")]
        public string RoleAsString
        {
            get { return Role.ToString(); }
        }

        /// <summary>
        /// Gets the mail address.
        /// </summary>
        [NotNull]
        [IgnoreDataMember]
        public string MailAddress { get; private set; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        [IgnoreDataMember]
        public UserSettings Settings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="mailAddress">The mail address.</param>
        /// <param name="role">The role.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">mailAddress is null</exception>
        public User(Guid id, string mailAddress, Role role, string name, UserSettings settings)
        {
            if (string.IsNullOrEmpty(mailAddress))
            {
                throw new ArgumentNullException(nameof(mailAddress));
            }

            MailAddress = mailAddress;
            Role = role;
            Id = id;
            UserName = name;
            Settings = settings;
        }

        /// <summary>
        /// Gets the feed by id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The feed.</returns>
        public Feed GetFeed(Guid id)
        {
            return Settings.Feeds.FirstOrDefault(feed => feed.Id == id);
        }
    }
}