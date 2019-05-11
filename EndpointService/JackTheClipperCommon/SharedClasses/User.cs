using System;
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
        /// If True User needs to change his PW on login.
        /// </summary>
        [DataMember(Name = "MustChangePassword")]
        public bool MustChangePassword { get; set; }

        /// <summary>
        /// Gets the mail address.
        /// </summary>
        [NotNull]
        [DataMember(Name = "UserMail")]
        public string MailAddress { get; private set; }

        /// <summary>
        /// Gets the name of the principal unit.
        /// </summary>
        [DataMember(Name = "UserPrincipalUnit")]
        public string PrincipalUnitName { get; private set; }

        /// <summary>
        /// Gets the principal unit identifier.
        /// </summary>
        [IgnoreDataMember]
        public Guid PrincipalUnitId { get; private set;  }

        /// <summary>
        /// Gets the last login time.
        /// </summary>
        [IgnoreDataMember]
        public DateTime LastLoginTime { get; private set; }

        /// <summary>
        ///Gets a value indicating whether the user is valid or not.
        /// </summary>
        [IgnoreDataMember]
        public bool IsValid { get; private set; }

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
        /// <param name="settings">The users settings.</param>
        /// <param name="mustChangePassword">Specifies if user has to reset his password</param>
        /// <param name="lastLoginTime">The last login time.</param>
        /// <param name="isValid">A value indicating whether the user is valid and therefore may login.</param>
        /// <param name="principalUnitName">The name of the principal unit.</param>
        /// <param name="principalUnitId">The id of the principal unit.</param>
        /// <exception cref="ArgumentNullException">mailAddress is null</exception>
        public User(Guid id, string mailAddress, Role role, string name, UserSettings settings, bool mustChangePassword,
                    DateTime lastLoginTime, bool isValid, string principalUnitName, Guid principalUnitId)
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
            MustChangePassword = mustChangePassword;
            LastLoginTime = lastLoginTime;
            IsValid = isValid;
            PrincipalUnitName = principalUnitName;
            PrincipalUnitId = principalUnitId;
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