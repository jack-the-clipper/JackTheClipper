using System;
using System.Runtime.Serialization;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Interfaces;
using JetBrains.Annotations;

namespace JackTheClipperCommon.SharedClasses
{

    /// <summary>
    /// Contains the definition of a User.
    /// It has a unique Id, a UserName, a Password and a Role.
    /// It also has a reference to its UserSettings
    /// </summary>
    [DataContract]
    public class User : BasicUserInformation, IMailNotifiable
    {
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
            private set { Role = Enum.Parse<Role>(value); }
        }

        /// <summary>
        /// If True User needs to change his PW on login.
        /// </summary>
        [DataMember(Name = "MustChangePassword")]
        public bool MustChangePassword { get; private set; }

        /// <summary>
        /// Gets the mail address.
        /// </summary>
        [NotNull]
        [DataMember(Name = "UserMail")]
        public string MailAddress { get; private set; }

        [IgnoreDataMember]
        public string UserMailAddress => MailAddress;

        /// <summary>
        /// Gets the name of the principal unit.
        /// </summary>
        [DataMember(Name = "UserPrincipalUnit")]
        public string PrincipalUnitName { get; private set; }

        /// <summary>
        /// Gets the principal unit identifier.
        /// </summary>
        [DataMember(Name = "UserPrincipalUnitId")]
        public Guid PrincipalUnitId { get; private set;  }

        /// <summary>
        /// Gets the last login time.
        /// </summary>
        [IgnoreDataMember]
        public DateTime LastLoginTime { get; private set; }

        /// <summary>
        /// Gets the settings id.
        /// </summary>
        [DataMember(Name="UserSettingsId")]
        public Guid SettingsId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="mailAddress">The mail address.</param>
        /// <param name="role">The role.</param>
        /// <param name="name">The name.</param>
        /// <param name="settingsId">The users settings id.</param>
        /// <param name="mustChangePassword">Specifies if user has to reset his password</param>
        /// <param name="lastLoginTime">The last login time.</param>
        /// <param name="isValid">A value indicating whether the user is valid and therefore may login.</param>
        /// <param name="principalUnitName">The name of the principal unit.</param>
        /// <param name="principalUnitId">The id of the principal unit.</param>
        /// <exception cref="ArgumentNullException">mailAddress is null</exception>
        public User(Guid id, string mailAddress, Role role, string name, Guid settingsId, bool mustChangePassword,
                    DateTime lastLoginTime, bool isValid, string principalUnitName, Guid principalUnitId)
        :base(id, name, isValid)
        {
            if (string.IsNullOrEmpty(mailAddress))
            {
                throw new ArgumentNullException(nameof(mailAddress));
            }

            MailAddress = mailAddress;
            Role = role;
            SettingsId = settingsId;
            MustChangePassword = mustChangePassword;
            LastLoginTime = lastLoginTime;
            PrincipalUnitName = principalUnitName;
            PrincipalUnitId = principalUnitId;
        }
    }
}