using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JackTheClipperBusiness.CrawlerManagement;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Extensions;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;
using User = JackTheClipperCommon.SharedClasses.User;
using static JackTheClipperBusiness.MailController;

namespace JackTheClipperBusiness.UserManagement
{
    /// <summary>
    /// Contains Methods to get Articles from the ElasticServer, which match the Users preferences
    /// Also contains Methods which correspond to the class name
    /// </summary>
    internal class UserController : IClipperUserAPI, IClipperSystemAdministratorAPI
    {
        /// <summary>
        /// Tries to authenticate the user.
        /// </summary>
        /// <param name="userMailOrName">The user email or name.</param>
        /// <param name="userPassword">The password of the user.</param>
        /// <returns>The <see cref="User"/>; if authenticated successfully</returns>
        public User TryAuthenticateUser(string userMailOrName, string userPassword)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().GetUserByCredentials(userMailOrName, userPassword, true);
        }

        /// <summary>
        /// Gets the feed definitions of the given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Collection of feeds for the given user.</returns>
        public IReadOnlyCollection<Feed> GetFeedDefinitions(User user)
        {
            var userSettings = GetUserSettings(user);
            return userSettings != null ? userSettings.Feeds : new List<Feed>();
        }

        /// <summary>
        /// Gets the feed data.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="feed">The feed.</param>
        /// <param name="page">The requested page.</param>
        /// <param name="showArchived">A value indicating whether the archived articles should be shown or not.</param>
        /// <returns>List of <see cref="ShortArticle"/>s within the feed.</returns>
        public IReadOnlyCollection<ShortArticle> GetFeed(User user, Feed feed, int page, bool showArchived)
        {
            var since = showArchived ? DateTime.MinValue : user.LastLoginTime;
            return DatabaseAdapterFactory.GetControllerInstance<IIndexerService>().GetFeedAsync(user, feed.Id, since, page).Result;
        }

        /// <summary>
        /// Gets a specific article.
        /// </summary>
        /// <param name="articleId">The article id.</param>
        /// <returns>The (full) <see cref="Article"/>.</returns>
        public Article GetArticle(Guid articleId)
        {
            return DatabaseAdapterFactory.GetControllerInstance<IIndexerService>().GetArticleAsync(articleId).Result;
        }

        /// <summary>
        /// Gets the user settings.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The settings of the given user.</returns>
        public UserSettings GetUserSettings(User user)
        {
            return user.Settings;
        }

        /// <summary>
        /// Saves the user settings.
        /// </summary>
        /// <param name="user">The user who requests the save.</param>
        /// <param name="toSave">The settings to save.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult SaveUserSettings(User user, UserSettings toSave)
        {
            Factory.GetControllerInstance<IClipperDatabase>().SaveUserSettings(user, toSave);
            return new MethodResult();
        }
        
        /// <summary>
        /// Attempts to reset the password.
        /// </summary>
        /// <param name="userMail">The users mail address.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult ResetPassword(string userMail)
        {

            var newPassword = PasswordGenerator.GeneratePw();
            var result = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().ResetPassword(userMail, newPassword);
            if (result.IsSucceeded())
            {
                var user = Factory.GetControllerInstance<IClipperDatabase>()
                                  .GetUserByCredentials(userMail, newPassword, false);
                QuerySendMailAsync(user, "Your password has been reset.", $"Your new password is: {newPassword}");
            }

            return result;
        }

        /// <summary>
        /// Attempts to change the password.
        /// </summary>
        /// <param name="user">The users mail address</param>
        /// <param name="newPassword">The new user password.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult ChangePassword(User user, string newPassword)
        {
            var result = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().ChangePassword(user, newPassword);
            if (result.IsSucceeded())
            {
                QuerySendMailAsync(user, "Your password has been changed.", $"Your new password is: {newPassword}");
            }

            return result;
        }

        /// <summary>
        /// Changes the users mail address.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="newUserMail">The new mail address.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult ChangeMailAddress(User user, string newUserMail)
        {
            var result = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().ChangeMailAddress(user, newUserMail);
            if (result.IsSucceeded())
            {
                QuerySendMailAsync(user, "Your mail has been changed.", $"Your new mail address is: {newUserMail}");
            }
            
            return result;
        }

        /// <summary>
        /// Gets the available sources for a given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>List of available sources.</returns>
        public IReadOnlyList<Source> GetAvailableSources(User user)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().GetAvailableSources(user);
        }

        /// <summary>
        /// Adds a user.
        /// </summary>
        /// <param name="userMail">The new users mail.</param>
        /// <param name="userName">The new users username.</param>
        /// <param name="password">The new users password.</param>
        /// <param name="role">The new users role.</param>
        /// <param name="unit">The new users unit.</param>
        /// <param name="mustChangePassword">A value indicating whether the user must change the pw.</param>
        /// <param name="valid">A value whether the user is valid or not.</param>
        /// <returns>The new users <see cref="User"/>object</returns>
        public User AddUser(string userMail, string userName, string password, Role role, Guid unit, bool mustChangePassword, bool valid)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().AddUser(userMail, userName, password, role, unit, mustChangePassword, valid);
        }


        /// <summary>
        /// Adds the given source.
        /// </summary>
        /// <param name="user">The user who requests the addition.</param>
        /// <param name="toAdd">The source to add.</param>
        /// <returns>MethodResult indicating success</returns>

        public MethodResult AddSource(User user, Source toAdd)
        {
            Factory.GetControllerInstance<IClipperDatabase>().AddSource(toAdd);
            CrawlerController.GetCrawlerController().Restart();
            return new MethodResult();
        }

        /// <summary>
        /// Deletes the given source.
        /// </summary>
        /// <param name="user">The user who requests the deletion.</param>
        /// <param name="toDelete">The source to delete.</param>
        /// <returns>MethodResult indicating success</returns>
        public MethodResult DeleteSource(User user, Guid toDelete)
        {
            Factory.GetControllerInstance<IClipperDatabase>().DeleteSource(toDelete);
            CrawlerController.GetCrawlerController().Restart();
            return new MethodResult();
        }

        /// <summary>
        /// Changes the source.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="toChange">Id of the source, which should be changed</param>
        /// <param name="newSource">The new source.</param>
        /// <returns>MethodResult</returns>
        public MethodResult ChangeSource(User user, Guid toChange, Source newSource)
        {
            var controller = Factory.GetControllerInstance<IClipperDatabase>();
            controller.AddSource(newSource);
            CrawlerController.GetCrawlerController().Restart();
            return new MethodResult();
        }
    }
}
