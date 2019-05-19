using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JackTheClipperCommon.Enums;

namespace JackTheClipperCommon.SharedClasses
{
    [DataContract]
    public class ExtendedUser : User
    {
        [DataMember(Name="UserOrganizationalUnits")]
        public IReadOnlyList<OrganizationalUnit> OrganizationalUnits { get; private set; }

        public ExtendedUser(Guid id, string mailAddress, Role role, string name, Guid settingsId, bool mustChangePassword, DateTime lastLoginTime, bool isValid, string principalUnitName, Guid principalUnitId, IReadOnlyList<OrganizationalUnit> organizationalUnits) : base(id, mailAddress, role, name, settingsId, mustChangePassword, lastLoginTime, isValid, principalUnitName, principalUnitId)
        {
            OrganizationalUnits = organizationalUnits ?? new List<OrganizationalUnit>();
        }
    }
}