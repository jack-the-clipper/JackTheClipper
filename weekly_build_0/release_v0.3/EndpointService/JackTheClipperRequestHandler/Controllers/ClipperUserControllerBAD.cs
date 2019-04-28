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
    public class ClipperUserController : Controller
    {
        //Ok
        [Route("login")]
        [HttpGet]
        public ActionResult<User> TryAuthenticateUser([FromQuery]string userMail, [FromQuery]string userPassword)
        {
            try
            {
                var user = Factory.GetControllerInstance<IClipperUserAPI>().TryAuthenticateUser(userMail, userPassword);
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
                return Factory.GetControllerInstance<IClipperUserAPI>().GetFeedDefinitions(user);
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
        public IReadOnlyCollection<ShortArticle> GetFeed([FromQuery]Guid userId, [FromQuery]Guid feedId)
        {
            try
            {
                var user = Get<User>(userId);
                return Factory.GetControllerInstance<IClipperUserAPI>().GetFeed(user, user.GetFeed(feedId));
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
                var user = Get<User>(userId);
                return Factory.GetControllerInstance<IClipperUserAPI>().GetArticle(user, new ShortArticle(articleId, null, null, null, DateTime.UtcNow, default(DateTime), Guid.Empty));
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
                return Factory.GetControllerInstance<IClipperUserAPI>().GetUserSettings(user);
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
                return Factory.GetControllerInstance<IClipperUserAPI>().SaveUserSettings(user, toSave);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return new MethodResult(SuccessState.UnknownError, "Bad things happend");
            }
        }

        //Ok
        [Route("reset")]
        [HttpGet]
        public MethodResult ResetPassword([FromQuery]string userMail)
        {
            try
            {
                return Factory.GetControllerInstance<IClipperUserAPI>().ResetPassword(userMail);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return new MethodResult(SuccessState.UnknownError, "Bad things happend");
            }
        }

        //Ok
        [Route("availablesources")]
        [HttpGet]
        public ActionResult<IReadOnlyList<Source>> GetAvailableSources([FromQuery]Guid userId)
        {
            try
            {
                var user = Get<User>(userId);
                return new ActionResult<IReadOnlyList<Source>>(Factory.GetControllerInstance<IClipperUserAPI>().GetAvailableSources(user));
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
                return new ActionResult<User>(Factory.GetControllerInstance<IClipperUserAPI>().AddUser(
                    userMail, userName, password, (Role)Enum.Parse(typeof(Role), role), unit));
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest(error.Message);
            }
        }

        internal static T Get<T>(Guid id) where T : class
        {
            return Factory.GetObjectInstanceById<T>(id);
        }
    }
}