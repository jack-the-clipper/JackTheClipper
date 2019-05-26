using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JackTheClipperCommon.Enums;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// A class covering all the information this application can now about a single, generic user
    /// </summary>
    [DataContract]
    public class ExtendedUser : User
    {
        /// <summary>
        /// The <see cref="OrganizationalUnit"/>s a user belongs to
        /// </summary>
        [DataMember(Name="UserOrganizationalUnits")]
        public IReadOnlyList<OrganizationalUnit> OrganizationalUnits { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedUser"/> class
        /// </summary>
        /// <param name="id">The id of the user</param>
        /// <param name="mailAddress">The email address of the user</param>
        /// <param name="role">The role of the user</param>
        /// <param name="name">The name of the user</param>
        /// <param name="settingsId">The id of the settings that belong to this user</param>
        /// <param name="mustChangePassword">Whether the user needs to change his password</param>
        /// <param name="lastLoginTime">The time of the user's last login</param>
        /// <param name="isValid">Whether the user can use the application</param>
        /// <param name="principalUnitName">The name of the client the user belongs to</param>
        /// <param name="principalUnitId">The id of the client the user belongs to</param>
        /// <param name="organizationalUnits">The <see cref="OrganizationalUnit"/>s the user belongs to</param>
        public ExtendedUser(Guid id, string mailAddress, Role role, string name, Guid settingsId, bool mustChangePassword, DateTime lastLoginTime, bool isValid, string principalUnitName, Guid principalUnitId, IReadOnlyList<OrganizationalUnit> organizationalUnits) : base(id, mailAddress, role, name, settingsId, mustChangePassword, lastLoginTime, isValid, principalUnitName, principalUnitId)
        {
            OrganizationalUnits = organizationalUnits ?? new List<OrganizationalUnit>();
        }
    }
}