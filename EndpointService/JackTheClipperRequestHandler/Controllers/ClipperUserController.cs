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
    /// <summary>
    /// Controller for services that any user can access
    /// </summary>
    [Route("clipper")]
    [ApiController]
    public class ClipperUserController : Controller
    {
        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="userMailOrName">user name or email by which the user to authenticate can be identified</param>
        /// <param name="userPassword">The password trying to authenticate the user</param>
        /// <param name="principalUnit">The id of the principal unit the user should belong to.
        /// This is needed since a username need only be unique within one principal unit</param>
        /// <returns>The authenticated user or <see cref="BadRequestResult"/> if the authentication failed</returns>
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

        /// <summary>
        /// Returns the feeds a user has
        /// </summary>
        /// <param name="userId">The user whose feeds are queried</param>
        /// <returns>List of <see cref="Feed"/>s belonging to the user</returns>
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

        /// <summary>
        /// Gets the articles belonging to a feed of an user
        /// </summary>
        /// <param name="userId">The user wanting to see the articles</param>
        /// <param name="feedId">The feed that is requested</param>
        /// <param name="page">The index of the page that should be returned. The size of the page is set
        /// in the user's <see cref="UserSettings.ArticlesPerPage"/></param>
        /// <param name="showArchived">Whether to consider all articles matching the feed criteria or only
        /// those indexed after the user's last login</param>
        /// <returns>List of <see cref="ShortArticle"/> matching the supplied feeds criteria. The size of the
        /// list is less than or equal to <see cref="UserSettings.ArticlesPerPage"/></returns>
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

        /// <summary>
        /// Gets a single article
        /// </summary>
        /// <param name="userId">The user requesting the article</param>
        /// <param name="articleId">The id of the requested article</param>
        /// <returns>The requested article, it is a <see cref="Article"/> unlike <see cref="GetFeed"/> which
        /// only returns <see cref="ShortArticle"/>s</returns>
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

        /// <summary>
        /// Gets the UserSettings of the requesting user
        /// </summary>
        /// <param name="userId">The id of the user requesting his settings</param>
        /// <returns><see cref="UserSettings"/> belonging to the user</returns>
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

        /// <summary>
        /// Resets the password of the user
        /// </summary>
        /// This implies that the user forgot his password
        /// <param name="userMail">The mail belonging to the user whose password will be reset</param>
        /// <returns><see cref="OkResult"/> if successful, <see cref="BadRequestResult"/> otherwise</returns>
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

        /// <summary>
        /// Changes the password of a user.
        /// </summary>
        /// Compared to <see cref="ResetPassword"/> this implies that the user still knows his password
        /// and simply wants to change it
        /// <param name="userId">The id of the user wanting to change his password </param>
        /// <param name="newPassword">The new password the user wants</param>
        /// <returns><see cref="OkResult"/> if successful, <see cref="BadRequestResult"/> otherwise</returns>
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

        /// <summary>
        /// Changes the mail address of a user.
        /// </summary>
        /// <param name="userId">The id of the user wanting to change his password </param>
        /// <param name="newUserMail">The new mail address the user wants</param>
        /// <returns><see cref="OkResult"/> if successful, <see cref="BadRequestResult"/> otherwise</returns>
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


        /// <summary>
        /// The <see cref="Source"/>s available to an user
        /// </summary>
        /// <param name="userId">The user requesting the sources available to him</param>
        /// <returns>List of all <see cref="Source"/>s available to the user</returns>
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

        /// <summary>
        /// Registers a user
        /// </summary>
        /// Compared to <see cref="ClipperStaffChiefController.AdministrativelyAddUser"/> this implies
        /// that the user is registering himself. Which is why he cannot use the application right away
        /// <param name="toAdd">The user to register</param>
        /// <param name="password">The password of the user</param>
        /// <param name="selectedUnit">The id of the <see cref="OrganizationalUnit"/> the user wants to belong
        /// to</param>
        /// <returns>MethodResult indicating success</returns>
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

        /// <summary>
        /// Gets the corresponding object by its id.
        /// </summary>
        /// <typeparam name="T">The type of the requested object.</typeparam>
        /// <param name="id">The id of the requested object.</param>
        /// <returns>The requested object (if exists).</returns>
        internal static T Get<T>(Guid id) where T : class
        {
            return GetObjectInstanceById<T>(id);
        }
    }
}
