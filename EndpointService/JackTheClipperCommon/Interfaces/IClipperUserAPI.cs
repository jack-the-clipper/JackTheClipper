using System;
using System.Collections.Generic;
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
        /// <param name="principalUnit">The principal unit.</param>
        /// <returns>The user (if existing).</returns>
        User TryAuthenticateUser(string userMailOrName, string userPassword, Guid principalUnit);

        /// <summary>
        /// Gets the feed definitions of the given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Collection of feeds for the given user.</returns>
        IReadOnlyCollection<Feed> GetFeedDefinitions(User user);

        /// <summary>
        /// Gets the feed data.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="feedId">The feed id.</param>
        /// <param name="page">The requested page.</param>
        /// <param name="showArchived">A value indicating whether the archived articles should be shown or not.</param>
        /// <returns>List of <see cref="ShortArticle"/>s within the feed.</returns>
        IReadOnlyCollection<ShortArticle> GetFeed(Guid userId, Guid feedId, int page, bool showArchived);

        /// <summary>
        /// Gets a specific article.
        /// </summary>
        /// <param name="articleId">The article id.</param>
        /// <returns>The (full) <see cref="Article"/>.</returns>
        Article GetArticle(Guid articleId);

        /// <summary>
        /// Gets the user settings.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The settings of the given user.</returns>
        UserSettings GetUserSettings(Guid userId);

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
        IReadOnlyList<Source> GetAvailableSources(Guid user);

        /// <summary>
        /// Adds a user.
        /// </summary>
        /// <param name="toAdd">The new user to add.</param>
        /// <param name="password">The password.</param>
        /// <param name="selectedUnit">The selected requested unit.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult AddUser(User toAdd, string password, Guid selectedUnit);

        /// <summary>
        /// Saves the user settings.
        /// </summary>
        /// <param name="settingsId">The settings identifier.</param>
        /// <param name="notificationCheckInterval">The notification check interval.</param>
        /// <param name="notificationSetting">The notification setting.</param>
        /// <param name="articlesPerPage">The articles per page.</param>
        void SaveUserSettings(Guid settingsId, int notificationCheckInterval, NotificationSetting notificationSetting, int articlesPerPage);

        /// <summary>
        /// Adds the given feed.
        /// </summary>
        /// <param name="settingsId">The settings id.</param>
        /// <param name="feed">The feed to add.</param>
        void AddFeed(Guid settingsId, Feed feed);

        /// <summary>
        /// Modifies the feed.
        /// </summary>
        /// <param name="settingsId">The settings id.</param>
        /// <param name="feed">The feed to add.</param>
        void ModifyFeed(Guid settingsId, Feed feed);

        /// <summary>
        /// Deletes the feed.
        /// </summary>
        /// <param name="feedId">The feed id.</param>
        void DeleteFeed(Guid feedId);
    }
}
