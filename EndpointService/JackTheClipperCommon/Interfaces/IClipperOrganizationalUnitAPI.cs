using System.Collections.Generic;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperCommon.Interfaces
{
    /// <summary>
    /// Interface for the Organizational Unit API
    /// </summary>
    public interface IClipperOrganizationalUnitAPI
    {
        /// <summary>
        /// Gets the organizational units of the given user.
        /// </summary>
        /// <param name="user">The user to obtain its organizational units.</param>
        /// <returns>List of accessable Organizational units.</returns>
        IReadOnlyList<OrganizationalUnit> GetOrganizationalUnits(User user);

        /// <summary>
        /// Adds a given organizational unit.
        /// </summary>
        /// <param name="user">The user who requests the addition.</param>
        /// <param name="parent">The parent of the new organizational unit.</param>
        /// <param name="toAdd">To unit add.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult AddOrganizationalUnit(User user, OrganizationalUnit parent, OrganizationalUnit toAdd);

        /// <summary>
        /// Deletes the organizational unit.
        /// </summary>
        /// <param name="user">The user who requests the deletion.</param>
        /// <param name="toDelete">The unit to delete.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult DeleteOrganizationalUnit(User user, OrganizationalUnit toDelete);

        /// <summary>
        /// Gets the organizational unit settings.
        /// </summary>
        /// <param name="user">The user who requests the settings.</param>
        /// <param name="unit">The unit.</param>
        /// <returns>The requested settings.</returns>
        OrganizationalUnitSettings GetOrganizationalUnitSettings(User user, OrganizationalUnit unit);

        /// <summary>
        /// Saves the organizational unit settings.
        /// </summary>
        /// <param name="toSave">The settings to save.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult SaveOrganizationalUnitSettings(OrganizationalUnitSettings toSave);
    }
}