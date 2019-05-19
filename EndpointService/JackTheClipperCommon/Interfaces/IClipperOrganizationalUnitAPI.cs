using System;
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
        /// Gets the organizational unit settings.
        /// </summary>
        /// <param name="unitId">The organizational unit id.</param>
        /// <returns>The requested settings.</returns>
        OrganizationalUnitSettings GetOrganizationalUnitSettings(Guid unitId);

        /// <summary>
        /// Saves the organizational unit settings.
        /// </summary>
        /// <param name="toSave">The settings to save.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult SaveOrganizationalUnitSettings(OrganizationalUnitSettings toSave);
        
        /// <summary>
        /// Adds a new unit.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parentUnitId">The parent unit id.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult AddOrganizationalUnit(string name, Guid parentUnitId);

        /// <summary>
        /// Deletes an organizational unit.
        /// </summary>
        /// <param name="unitId">The unit identifier.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult DeleteOrganizationalUnit(Guid unitId);

        /// <summary>
        /// Updates an existing organizational unit to the given values.
        /// </summary>
        /// <param name="updatedUnit">The updated unit.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult UpdateOrganizationalUnit(OrganizationalUnit updatedUnit);

        /// <summary>
        /// Sets the organizational units of a user.
        /// </summary>
        /// <param name="user">The user to be changed.</param>
        /// <param name="units">The organizational units.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult SetUserOrganizationalUnits(User user, IReadOnlyList<Guid> units);
    }
}