using System;
using System.Collections.Generic;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperCommon.Interfaces
{
    /// <summary>
    /// Interface of the system administrator api
    /// </summary>
    public interface IClipperSystemAdministratorAPI
    {
        /// <summary>
        /// Adds the given source.
        /// </summary>
        /// <param name="user">The user who requests the addition.</param>
        /// <param name="toAdd">The source to add.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult AddSource(User user, Source toAdd);

        /// <summary>
        /// Deletes the given source.
        /// </summary>
        /// <param name="user">The user who requests the deletion.</param>
        /// <param name="toAdd">The source to add.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult DeleteSource(User user, Guid toAdd);

        /// <summary>
        /// Changes the source.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="updatedSource">The updated source.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult ChangeSource(User user, Source updatedSource);

        /// <summary>
        /// Adds the client.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="name">The name.</param>
        /// <param name="pbMail">The pb mail.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult AddPrincipalUnit(User user, string name, string pbMail);


        /// <summary>
        /// Gets all principal units.
        /// </summary>
        /// <returns>The principal units</returns>
        IReadOnlyList<OrganizationalUnit> GetPrincipalUnits();
    }
}
