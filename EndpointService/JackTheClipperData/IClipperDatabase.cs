using System;
using System.Collections.Generic;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperData
{
    /// <summary>
    /// Interface for the clipper database.
    /// </summary>
    public interface IClipperDatabase
    {
        /// <summary>
        /// Gets the user by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The user (if found)</returns>
        User GetUserById(Guid id);

        /// <summary>
        /// Gets the organizational unit by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The unit (if found)</returns>
        OrganizationalUnit GetOrganizationalUnitById(Guid id);

        /// <summary>
        /// Gets the user by credentials.
        /// </summary>
        /// <param name="mailOrName">The mail or name.</param>
        /// <param name="password">The password.</param>
        /// <param name="principalUnit">The principal unit.</param>
        /// <param name="updateLoginTimeStamp">A value indicating whether the login timestamp should be updated or not.</param>
        /// <returns>The user (if found)</returns>
        User GetUserByCredentials(string mailOrName, string password, Guid principalUnit, bool updateLoginTimeStamp);

        /// <summary>
        /// Gets all sources.
        /// </summary>
        /// <returns>List of all sources.</returns>
        IReadOnlyCollection<Source> GetSources();

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>List of all users.</returns>
        IReadOnlyCollection<User> GetAllUsers();

        /// <summary>
        /// Gets the available sources.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>List of available sources.</returns>
        IReadOnlyList<Source> GetAvailableSources(User user);

        /// <summary>
        /// Gets the available organizational units.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>List of available units.</returns>
        IReadOnlyList<OrganizationalUnit> GetOrganizationalUnits(User user);

        /// <summary>
        /// Adds a user.
        /// </summary>
        /// <param name="userMail">The user mail.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="role">The role.</param>
        /// <param name="principalUnit">The principal unit.</param>
        /// <param name="mustChangePassword">A value indicating whether the user must change the pw.</param>
        /// <param name="valid">A value whether the user is valid or not.</param>
        /// <returns>The user object of the new user.</returns>
        User AddUser(string userMail, string userName, string password, Role role, Guid principalUnit, bool mustChangePassword, bool valid);

        /// <summary>
        /// Adds a source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The object of the new source.</returns>
        Source AddSource(Source source);


        /// <summary>
        /// Deletes the source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>succesfull bool</returns>
        bool DeleteSource(Guid source);

        /// <summary>
        /// Resets the users password.
        /// </summary>
        /// <param name="userMail"></param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult ResetPassword(string userMail, string newPassword);

        /// <summary>
        /// Changes the users password.
        /// </summary>
        /// <param name="user">The users mail address.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult ChangePassword(User user, string newPassword);

        /// <summary>
        /// Changes the users mail address.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="newUserMail">The users new mail address.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult ChangeMailAddress(User user, string newUserMail);
      
        ///<summary>
        /// Gets the superset feed filter.
        /// This is a feed filter which contains all relevant keywords form any feed within the system.
        /// </summary>
        /// <returns>The superset feed filter.</returns>
        Filter GetSuperSetFeed();

        /// <summary>
        /// Gets the principal units.
        /// </summary>
        /// <returns>List of principal units.</returns>
        IReadOnlyList<Tuple<string, Guid>> GetPrincipalUnits();

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