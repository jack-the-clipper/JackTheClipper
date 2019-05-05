using System;
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
        /// <returns>200 if succesfull, BadRequest if not</returns>
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
        /// Endpoint to Change the Source
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="tochange">The source to change.</param>
        /// <param name="newsource">New Source</param>
        /// <returns>200 if succesfull, BadRequest if not</returns>
        [HttpPut]
        [Route("changesource")]
        public ActionResult<MethodResult> ChangeSource([FromQuery]Guid userId, [FromQuery] Guid tochange, [FromBody] Source newsource)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role == Role.SystemAdministrator)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperSystemAdministratorAPI>().ChangeSource(user, tochange, newsource));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }
    }
}
