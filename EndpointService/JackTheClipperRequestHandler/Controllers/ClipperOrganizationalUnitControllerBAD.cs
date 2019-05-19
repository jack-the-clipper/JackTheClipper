using System;
using System.Collections.Generic;
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

        [Route("getorganizationalunitsettings")]
        [HttpGet]
        public ActionResult<OrganizationalUnitSettings> GetOrganizationalUnitSettings([FromQuery]Guid userId, [FromQuery]Guid unitId)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                return user.Role > Role.User
                    ? new ActionResult<OrganizationalUnitSettings>(Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>().GetOrganizationalUnitSettings(unitId))
                    : Forbid();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest(error.Message);
            }
        }

        [Route("saveorganizationalunitsettings")]
        [HttpPut]
        public ActionResult<MethodResult> SaveOrganizationalUnitSettings([FromQuery]Guid userId, [FromBody]OrganizationalUnitSettings unitSettings)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                return user.Role > Role.User
                    ? new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>().SaveOrganizationalUnitSettings(unitSettings))
                    : Forbid();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest(error.Message);
            }
        }
    }
}