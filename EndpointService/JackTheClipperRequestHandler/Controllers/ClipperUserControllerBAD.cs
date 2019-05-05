using System;
using System.Collections.Generic;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Extensions;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using Microsoft.AspNetCore.Mvc;
using static JackTheClipperBusiness.Factory;

namespace JackTheClipperRequestHandler.Controllers
{
    [Route("clipper")]
    [ApiController]
    public class ClipperUserController : Controller
    {
        //Ok
        [Route("login")]
        [HttpGet]
        public ActionResult<User> TryAuthenticateUser([FromQuery]string userMail, [FromQuery]string userPassword)
        {
            try
            {
                var user = GetControllerInstance<IClipperUserAPI>().TryAuthenticateUser(userMail, userPassword);
                if (user != null)
                {
                    return new ActionResult<User>(user);
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }

            return BadRequest("User not found");
        }

        //Ok
        [Route("getfeeddefinitions")]
        [HttpGet]
        public IReadOnlyCollection<Feed> GetFeedDefinitions([FromQuery]Guid userId)
        {
            try
            {
                var user = Get<User>(userId);
                return GetControllerInstance<IClipperUserAPI>().GetFeedDefinitions(user);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return null;
            }
        }

        //Ok
        [Route("getfeed")]
        [HttpGet]
        public IReadOnlyCollection<ShortArticle> GetFeed([FromQuery]Guid userId, [FromQuery]Guid feedId, [FromQuery]int page, [FromQuery]bool showArchived)
        {
            try
            {
                var user = Get<User>(userId);
                return GetControllerInstance<IClipperUserAPI>().GetFeed(user, user.GetFeed(feedId), page, showArchived);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return null;
            }
        }

        //Ok
        [Route("getarticle")]
        [HttpGet]
        public Article GetArticle([FromQuery]Guid userId, [FromQuery]Guid articleId)
        {
            try
            {
                return GetControllerInstance<IClipperUserAPI>().GetArticle(articleId);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return null;
            }
        }

        //Ok
        [Route("getusersettings")]
        [HttpGet]
        public UserSettings GetUserSettings([FromQuery]Guid userId)
        {
            try
            {
                var user = Get<User>(userId);
                return GetControllerInstance<IClipperUserAPI>().GetUserSettings(user);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return null;
            }
        }

        //Ok, but should be reviewed
        [Route("saveusersettings")]
        [HttpPut]
        public MethodResult SaveUserSettings([FromQuery]Guid userId, [FromBody]UserSettings toSave)
        {
            try
            {
                var user = Get<User>(userId);
                return GetControllerInstance<IClipperUserAPI>().SaveUserSettings(user, toSave);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return new MethodResult(SuccessState.UnknownError, "Bad things happened");
            }
        }

        //Ok
        [Route("reset")]
        [HttpPut] 
        public ActionResult ResetPassword([FromQuery]string userMail)
        {
            try
            {
               var result = GetControllerInstance<IClipperUserAPI>().ResetPassword(userMail);
               if (result.IsSucceeded())
               {
                   return Ok();
               }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }

            return BadRequest();
        }

        [Route("changepassword")]
        [HttpPut]
        public ActionResult ChangePassword([FromQuery] Guid userId, [FromQuery] string newPassword)
        {
            try
            {
                var user = GetObjectInstanceById<User>(userId);

                var result = GetControllerInstance<IClipperUserAPI>().ChangePassword(user, newPassword);
                if (result.IsSucceeded())
                {
                    return Ok();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }

            return BadRequest();
        }

        [Route("changemailaddress")]
        [HttpPut]
        public ActionResult ChangeMailAddress([FromQuery]Guid userId, [FromQuery] string newUserMail)
        {
            try
            {
                var user = Get<User>(userId);
                var result = GetControllerInstance<IClipperUserAPI>().ChangeMailAddress(user, newUserMail);
                if (result.IsSucceeded())
                {
                    return Ok();
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }

            return BadRequest();
        }

        //Ok
        [Route("availablesources")]
        [HttpGet]
        public ActionResult<IReadOnlyList<Source>> GetAvailableSources([FromQuery]Guid userId)
        {
            try
            {
                var user = Get<User>(userId);
                return new ActionResult<IReadOnlyList<Source>>(GetControllerInstance<IClipperUserAPI>().GetAvailableSources(user));
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest(error.Message);
            }
        }

        //Ok
        [HttpPut]
        [Route("register")]
        public ActionResult<User> AddUser([FromQuery]string userMail, [FromQuery]string userName,
            [FromQuery] string password, [FromQuery]string role,[FromQuery]Guid unit)
        {
            try
            {
                return new ActionResult<User>(GetControllerInstance<IClipperUserAPI>().AddUser(
                    userMail, userName, password, (Role)Enum.Parse(typeof(Role), role), unit, false, true));
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest(error.Message);
            }
        }

        internal static T Get<T>(Guid id) where T : class
        {
            return GetObjectInstanceById<T>(id);
        }
    }
}