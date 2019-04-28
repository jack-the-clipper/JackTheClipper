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

        [HttpDelete]
        [Route("deletesource")]
        public MethodResult DeleteSource([FromQuery]Guid userId, [FromBody]Source toAdd)
        {
            throw new NotImplementedException();
        }
    }
}