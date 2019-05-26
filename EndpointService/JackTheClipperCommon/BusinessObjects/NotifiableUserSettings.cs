using System;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperCommon.BusinessObjects
{
    /// <summary>
    /// Implements <see cref="IMailNotifiable"/> for users
    /// </summary>
    public class NotifiableUserSettings : IMailNotifiable
    {
        /// <summary>
        /// The Id of the user this settings belong to
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// The mail address of the notifiable object
        /// </summary>
        public string UserMailAddress { get; private set; }

        /// <summary>
        /// The name of the notifiable object
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// The last time the user logged in
        /// </summary>
        public DateTime LastLoginTime { get; private set; }

        /// <summary>
        /// The other settings the user has
        /// </summary>
        public UserSettings Settings { get; private set; }

        /// <summary>
        /// The name of the client the user belongs to
        /// </summary>
        public string PrincipalUnitName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifiableUserSettings"/> class.
        /// </summary>
        /// <param name="userId">The id of the user this settings should belong to</param>
        /// <param name="userMailAddress">The mail address of the user</param>
        /// <param name="userName">The username of the user</param>
        /// <param name="settings">The other settings</param>
        /// <param name="lastLoginTime">The last time the user logged in</param>
        /// <param name="principalUnitName">The name of the principal unit the user belongs to</param>
        public NotifiableUserSettings(Guid userId, string userMailAddress, string userName, UserSettings settings, DateTime lastLoginTime, string principalUnitName)
        {
            this.UserId = userId;
            UserMailAddress = userMailAddress;
            UserName = userName;
            Settings = settings;
            LastLoginTime = lastLoginTime;
            PrincipalUnitName = principalUnitName;
        }
    }
}