using System;
using System.Collections.Generic;
using System.IO;
using JackTheClipperBusiness.UserManagement;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Extensions;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;

namespace JackTheClipperBusiness.OrganizationalUnitManagement
{
    /// <summary>
    /// Settings for the OragnizationalUnit
    /// </summary>
    internal class OrganizationalUnitController : IClipperOrganizationalUnitAPI
    {
        /// <summary>
        /// Gets the organizational units of the given user.
        /// </summary>
        /// <param name="user">The user to obtain its organizational units.</param>
        /// <returns>
        /// List of accessable Organizational units.
        /// </returns>
        public IReadOnlyList<OrganizationalUnit> GetOrganizationalUnits(User user)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().GetOrganizationalUnits(user.Id);
        }

        /// <summary>
        /// Gets the organizational unit settings.
        /// </summary>
        /// <param name="unitId">The organizational unit id.</param>
        /// <returns>The requested settings.</returns>
        public OrganizationalUnitSettings GetOrganizationalUnitSettings(Guid unitId)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().GetOrganizationalUnitSettings(unitId);
        }

        /// <summary>
        /// Saves the organizational unit settings.
        /// </summary>
        /// <param name="toSave">The settings to save.</param>
        /// <returns>MethodResult indicating success</returns>
        public MethodResult SaveOrganizationalUnitSettings(OrganizationalUnitSettings toSave)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().SaveOrganizationalUnitSettings(toSave);
        }
        
        /// <summary>
        /// Adds a new unit.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parentUnitId">The parent unit id.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult AddOrganizationalUnit(string name, Guid parentUnitId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidDataException("Invalid unit name");
            }

            var orga = Factory.GetControllerInstance<IClipperDatabase>().AddUnit(name, parentUnitId);
            return orga == Guid.Empty ? new MethodResult(SuccessState.UnknownError, "") : new MethodResult();
        }

        public MethodResult DeleteOrganizationalUnit(Guid unitId)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().DeleteOrganizationalUnit(unitId);
        }

        public MethodResult UpdateOrganizationalUnit(OrganizationalUnit updatedUnit)
        {
            var adapter = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>();
            if (updatedUnit.IsPrincipalUnit)
            {
                var current = adapter.GetOrganizationalUnitById(updatedUnit.Id);
                if (current.AdminMailAddress != updatedUnit.AdminMailAddress)
                {
                    var user = adapter.GetUserByMailAddress(current.AdminMailAddress);
                    var tempResult = new UserController().ChangeMailAddress(user, updatedUnit.AdminMailAddress);
                    if (!tempResult.IsSucceeded())
                    {
                        return tempResult;
                    }

                    tempResult = new UserController().ResetPassword(updatedUnit.AdminMailAddress);
                    if (!tempResult.IsSucceeded())
                    {
                        return tempResult;
                    }
                }

                if (current.Name == updatedUnit.Name)
                {
                    return new MethodResult();
                }
            }

            return adapter.RenameOrganizationalUnit(updatedUnit.Id, updatedUnit.Name);
        }

        /// <summary>
        /// Sets the organizational units of a user.
        /// </summary>
        /// <param name="user">The user to be changed.</param>
        /// <param name="units">The organizational units.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult SetUserOrganizationalUnits(User user, IReadOnlyList<Guid> units)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().SetUserOrganizationalUnits(user.Id, units);
        }
    }
}