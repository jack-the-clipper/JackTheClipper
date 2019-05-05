using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperCommon.Interfaces
{
    /// <summary>
    /// Interface for the user API.
    /// </summary>
    public interface IClipperUserAPI
    {
        /// <summary>
        /// Tries to authenticate the user.
        /// </summary>
        /// <param name="userMailOrName">The user email or name.</param>
        /// <param name="userPassword">The password of the user.</param>
        /// <returns>The <see cref="User"/>; if authenticated successfully</returns>
        User TryAuthenticateUser(string userMailOrName, string userPassword);

        /// <summary>
        /// Gets the feed definitions of the given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Collection of feeds for the given user.</returns>
        IReadOnlyCollection<Feed> GetFeedDefinitions(User user);

        /// <summary>
        /// Gets the feed data.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="feed">The feed.</param>
        /// <param name="page">The requested page.</param>
        /// <param name="showArchived">A value indicating whether the archived articles should be shown or not.</param>
        /// <returns>List of <see cref="ShortArticle"/>s within the feed.</returns>
        IReadOnlyCollection<ShortArticle> GetFeed(User user, Feed feed, int page, bool showArchived);

        /// <summary>
        /// Gets a specific article.
        /// </summary>
        /// <param name="articleId">The article id.</param>
        /// <returns>The (full) <see cref="Article"/>.</returns>
        Article GetArticle(Guid articleId);

        /// <summary>
        /// Gets the user settings.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The settings of the given user.</returns>
        UserSettings GetUserSettings(User user);

        /// <summary>
        /// Saves the user settings.
        /// </summary>
        /// <param name="user">The user who requests the save.</param>
        /// <param name="toSave">The settings to save.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult SaveUserSettings(User user, UserSettings toSave);

        /// <summary>
        /// Attempts to reset the password.
        /// </summary>
        /// <param name="userMail">The users mail address.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult ResetPassword(string userMail);

        /// <summary>
        /// Attempts to change the password.
        /// </summary>
        /// <param name="user">The users mail address</param>
        /// <param name="newPassword">The new user password.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult ChangePassword(User user, string newPassword);

        /// <summary>
        /// Changes the users mail address.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="newUserMail">The new mail address.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult ChangeMailAddress(User user, string newUserMail);
        
        /// <summary>
        /// Gets the available sources for a given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>List of available sources.</returns>
        IReadOnlyList<Source> GetAvailableSources(User user);

        /// <summary>
        /// Adds a user.
        /// </summary>
        /// <param name="userMail">The new users mail.</param>
        /// <param name="username">The new users username.</param>
        /// <param name="password">The new users password.</param>
        /// <param name="role">The new users role.</param>
        /// <param name="unit">The new users unit.</param>
        /// <param name="mustChangePassword">A value indicating whether the user must change the pw.</param>
        /// <param name="valid">A value whether the user is valid or not.</param>
        /// <returns>The new users <see cref="User"/>object</returns>
        User AddUser(string userMail, string username, string password, Role role, Guid unit, bool mustChangePassword, bool valid);

    }
}