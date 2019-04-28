using System.Collections.Generic;
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
        public IReadOnlyList<OrganizationalUnit> GetOrganizationalUnits(User user)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().GetOrganizationalUnits(user);
        }

        public MethodResult AddOrganizationalUnit(User user, OrganizationalUnit parent, OrganizationalUnit toAdd)
        {
            throw new System.NotImplementedException();
        }

        public MethodResult DeleteOrganizationalUnit(User user, OrganizationalUnit toDelete)
        {
            throw new System.NotImplementedException();
        }

        public OrganizationalUnitSettings GetOrganizationalUnitSettings(User user, OrganizationalUnit unit)
        {
            return unit.DefaultSettings;
        }

        public MethodResult SaveOrganizationalUnitSettings(OrganizationalUnitSettings toSave)
        {
            throw new System.NotImplementedException();
        }
    }
}