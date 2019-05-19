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
    public class ClipperAdminController : Controller
    {
        /// <summary>
        /// Adds a new source.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="toAdd">The source to add.</param>
        /// <returns>MethodResult indicating success.</returns>
        [HttpPut]
        [Route("addsource")]
        public ActionResult<MethodResult> AddSource([FromQuery]Guid userId, [FromBody]Source toAdd)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role == Role.SystemAdministrator)
                {
                   return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperSystemAdministratorAPI>().AddSource(user, toAdd));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }

        /// <summary>
        /// Deletes the source.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="sourceId">The source identifier.</param>
        /// <returns>MethodResult indicating success.</returns>
        [HttpDelete]
        [Route("deletesource")]
        public ActionResult<MethodResult> DeleteSource([FromQuery]Guid userId, [FromQuery] Guid sourceId)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role == Role.SystemAdministrator)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperSystemAdministratorAPI>().DeleteSource(user, sourceId));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }

        /// <summary>
        /// Changes the source.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="updatedSource">The updated source.</param>
        /// <returns>MethodResult indicating success.</returns>
        [HttpPut]
        [Route("changesource")]
        public ActionResult<MethodResult> ChangeSource([FromQuery]Guid userId, [FromBody] Source updatedSource)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role == Role.SystemAdministrator)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperSystemAdministratorAPI>().ChangeSource(user, updatedSource));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }

        /// <summary>
        /// Adds a new principal unit.
        /// </summary>
        /// <param name="userId">The user identifier of the user which calls the api.</param>
        /// <param name="name">The name of the new principal unit.</param>
        /// <param name="principalUnitMail">The mail of the principal unit.</param>
        /// <returns>MethodResult indicating success.</returns>
        [HttpPut]
        [Route("addprincipalunit")]
        public ActionResult<MethodResult> AddPrincipalUnit([FromQuery]Guid userId, [FromQuery] string name, [FromQuery] string principalUnitMail)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role == Role.SystemAdministrator)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperSystemAdministratorAPI>().AddPrincipalUnit(user, name, principalUnitMail));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }

        /// <summary>
        /// Gets all principal units.
        /// </summary>
        /// <param name="userId">The user identifier of the requesting admin.</param>
        /// <returns>List of principal units.</returns>
        [HttpPut]
        [Route("getprincipalunits")]
        public ActionResult<IReadOnlyList<OrganizationalUnit>> GetPrincipalUnits([FromQuery]Guid userId)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                return user.Role > Role.User
                    ? new ActionResult<IReadOnlyList<OrganizationalUnit>>(Factory.GetControllerInstance<IClipperSystemAdministratorAPI>().GetPrincipalUnits())
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
