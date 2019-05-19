using System;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperCommon.BusinessObjects
{
    public class NotifiableUserSettings : IMailNotifiable
    {
        public Guid UserId { get; private set; }

        public string UserMailAddress { get; private set; }

        public string UserName { get; private set; }

        public DateTime LastLoginTime { get; private set; }

        public UserSettings Settings { get; private set; }

        public string PrincipalUnitName { get; private set; }

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