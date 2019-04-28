using System;
using System.Collections.Generic;
using System.Diagnostics;
using JackTheClipperBusiness;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using Microsoft.AspNetCore.Mvc;

namespace JackTheClipperRequestHandler.Controllers
{
    [Route("clipper")]
    [ApiController]
    public class ClipperOrganizationalUnitController : Controller
    {
        [Route("getorganizationalunits")]
        [HttpGet]
        public ActionResult<IReadOnlyList<OrganizationalUnit>> GetOrganizationalUnits([FromQuery]Guid userId)
        {
            try
            {
                if (userId == Guid.Parse("10000000-0000-0000-0000-000000000000"))
                {
                    //TODO: Remove after providing new api
                    userId = Guid.Parse("aaa342bb-680c-11e9-8c47-9615dc5f263c");
                }

                var user = ClipperUserController.Get<User>(userId);
                return user.Role > Role.User
                    ? new ActionResult<IReadOnlyList<OrganizationalUnit>>(Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>().GetOrganizationalUnits(user))
                    : Forbid();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest(error.Message);
            }
        }

        //public MethodResult AddOrganizationalUnit(User user, OrganizationalUnit parent, OrganizationalUnit toAdd)
        //{
        //    throw new System.NotImplementedException();
        //}

        //public MethodResult DeleteOrganizationalUnit(User user, OrganizationalUnit toDelete)
        //{
        //    throw new System.NotImplementedException();
        //}

        [Route("getorganizationalunitsettings")]
        [HttpGet]
        public ActionResult<OrganizationalUnitSettings> GetOrganizationalUnitSettings([FromQuery]Guid userId, [FromQuery]Guid unitId)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                var unit = Factory.GetObjectInstanceById<OrganizationalUnit>(unitId);
                return user.Role > Role.User
                    ? new ActionResult<OrganizationalUnitSettings>(Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>().GetOrganizationalUnitSettings(user, unit))
                    : Forbid();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest(error.Message);
            }
        }

        //public MethodResult SaveOrganizationalUnitSettings(OrganizationalUnitSettings currentSettings)
        //{
        //    throw new System.NotImplementedException();
        //}
    }
}