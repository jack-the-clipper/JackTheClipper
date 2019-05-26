using System;
using System.Collections.Generic;
using JackTheClipperBusiness;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using Microsoft.AspNetCore.Mvc;

namespace JackTheClipperRequestHandler.Controllers
{
    /// <summary>
    /// Controller for services concerning <see cref="OrganizationalUnit"/>s
    /// </summary>
    [Route("clipper")]
    [ApiController]
    public class ClipperOrganizationalUnitController : Controller
    {
        /// <summary>
        /// Gets the organizational units a user is allowed to see
        /// </summary>
        /// <param name="userId">The id of the requesting user</param>
        /// <returns>List of <see cref="OrganizationalUnit"/> that the user is allowed to see</returns>
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

        /// <summary>
        /// Gets the settings of an <see cref="OrganizationalUnit"/>
        /// </summary>
        /// <param name="userId">The id of the user requesting the settings</param>
        /// <param name="unitId">The id of the unit whose settings are requested</param>
        /// <returns>The <see cref="OrganizationalUnitSettings"/> of the specified unit</returns>
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

        /// <summary>
        /// Saves the <see cref="OrganizationalUnitSettings"/> 
        /// </summary>
        /// <param name="userId">The user requesting this action</param>
        /// <param name="unitSettings">The updated version of already existing settings</param>
        /// <returns>MethodResult indicating success</returns>
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