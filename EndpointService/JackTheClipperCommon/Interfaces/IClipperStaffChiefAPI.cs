using System;
using System.Collections.Generic;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperCommon.Interfaces
{
    /// <summary>
    /// Interface of the API for a staff chief.
    /// </summary>
    /// <seealso cref="JackTheClipperCommon.Interfaces.IClipperOrganizationalUnitAPI" />
    public interface IClipperStaffChiefAPI
    {
        IReadOnlyList<BasicUserInformation> GetManageableUsers(Guid userId);

        ExtendedUser GetUserInfo(Guid requested);

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult DeleteUser(Guid userId);

        /// <summary>
        /// Administratively adds a user.
        /// </summary>
        /// <param name="toAdd">The user to add.</param>
        /// <param name="userUnits">The users units.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult AdministrativelyAddUser(User toAdd, IReadOnlyList<Guid> userUnits);

        /// <summary>
        /// Modifies the user.
        /// </summary>
        /// <param name="toModify">The user to modify.</param>
        /// <param name="userUnits">The users units.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult ModifyUser(User toModify, IReadOnlyList<Guid> userUnits);
    }
}