using System;
using System.Collections.Generic;
using System.IO;
using JackTheClipperBusiness;
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
        public ActionResult<User> TryAuthenticateUser([FromQuery]string userMailOrName, [FromQuery]string userPassword, [FromQuery]Guid principalUnit)
        {
            try
            {
                var user = GetControllerInstance<IClipperUserAPI>().TryAuthenticateUser(userMailOrName, userPassword, principalUnit);
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
                return GetControllerInstance<IClipperUserAPI>().GetFeed(userId, feedId, page, showArchived);
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
                return GetControllerInstance<IClipperUserAPI>().GetUserSettings(userId);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return null;
            }
        }

        /// <summary>
        /// Saves the user settings.
        /// </summary>
        /// <param name="settingsId">The settings identifier.</param>
        /// <param name="notificationCheckInterval">The notification check interval.</param>
        /// <param name="notificationSetting">The notification setting.</param>
        /// <param name="articlesPerPage">The articles per page.</param>
        [Route("saveusersettings")]
        [HttpPut]
        public ActionResult SaveUserSettings([FromQuery]Guid settingsId, [FromQuery]int notificationCheckInterval,
                                             [FromQuery]NotificationSetting notificationSetting, [FromQuery]int articlesPerPage)
        {
            try
            {
                GetControllerInstance<IClipperUserAPI>().SaveUserSettings(settingsId, notificationCheckInterval, notificationSetting, articlesPerPage);
                return Ok();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest();
            }
        }

        /// <summary>
        /// Adds the given feed.
        /// </summary>
        /// <param name="settingsId">The settings id.</param>
        /// <param name="feed">The feed to add.</param>
        [Route("addfeed")]
        [HttpPut]
        public ActionResult AddFeed([FromQuery]Guid settingsId, [FromBody]Feed feed)
        {
            try
            {
                GetControllerInstance<IClipperUserAPI>().AddFeed(settingsId, feed);
                return Ok();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest();
            }
        }

        /// <summary>
        /// Modifies the feed.
        /// </summary>
        /// <param name="settingsId">The settings id.</param>
        /// <param name="feed">The feed to add.</param>
        [Route("modifyfeed")]
        [HttpPut]
        public ActionResult ModifyFeed([FromQuery] Guid settingsId, [FromBody] Feed feed)
        {
            try
            {
                GetControllerInstance<IClipperUserAPI>().ModifyFeed(settingsId, feed);
                return Ok();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest();
            }
        }

        /// <summary>
        /// Deletes the feed.
        /// </summary>
        /// <param name="feedId">The feed id.</param>
        [Route("deletefeed")]
        [HttpDelete]
        public ActionResult DeleteFeed([FromQuery] Guid feedId)
        {
            try
            {
                GetControllerInstance<IClipperUserAPI>().DeleteFeed(feedId);
                return Ok();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest();
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
        public ActionResult ChangeMailAddress([FromQuery] Guid userId, [FromQuery] string newUserMail)
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
                return new ActionResult<IReadOnlyList<Source>>(GetControllerInstance<IClipperUserAPI>().GetAvailableSources(userId));
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
        public ActionResult<MethodResult> AddUser([FromBody]User toAdd, [FromQuery]string password, [FromQuery]Guid selectedUnit)
        {
            try
            {
                return new ActionResult<MethodResult>(GetControllerInstance<IClipperUserAPI>().AddUser(toAdd, password, selectedUnit));
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
