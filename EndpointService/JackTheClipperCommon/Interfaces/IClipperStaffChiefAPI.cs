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
        /// <summary>
        /// Gets the minimal information of the users a staffchief can manage
        /// </summary>
        /// <param name="userId">The id of the staffchief</param>
        /// <returns>A list of <see cref="BasicUserInformation"/> if the given id actually belonged to a staffchief</returns>
        IReadOnlyList<BasicUserInformation> GetManageableUsers(Guid userId);

        /// <summary>
        /// Gets all information on a user 
        /// </summary>
        /// <param name="requested">The id of the user whose information is requested</param>
        /// <returns>All the information on a user like the <see cref="OrganizationalUnit"/>s he belongs to</returns>
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