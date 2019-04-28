using System;

namespace JackTheClipperCommon.Enums
{
    /// <summary>
    /// The possible roles.
    /// </summary>
    [Flags]
    public enum Role
    {
        /// <summary>
        /// User role.
        /// </summary>
        User,

        /// <summary>
        /// Staff chief role
        /// </summary>
        StaffChief,

        /// <summary>
        /// System admin role.
        /// </summary>
        SystemAdministrator
    }
}