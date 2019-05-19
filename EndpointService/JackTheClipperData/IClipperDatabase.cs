using System;
using System.Collections.Generic;
using JackTheClipperCommon.BusinessObjects;
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
        /// Gets the user by mail address.
        /// </summary>
        /// <param name="mailAddress">The mail address.</param>
        /// <returns>The user (if found)</returns>
        User GetUserByMailAddress(string mailAddress);

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
        IReadOnlyList<Source> GetSources();

        /// <summary>
        /// Gets all notifiable user settings
        /// ("notifiable" = only those which have set <see cref="UserSettings.NotificationSettings"/> != <see cref="NotificationSetting.None"/>).
        /// </summary>
        /// <returns>List of all notifiable user settings.</returns>
        IReadOnlyCollection<NotifiableUserSettings> GetNotifiableUserSettings();

        /// <summary>
        /// Gets the available sources.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <returns>List of available sources.</returns>
        IReadOnlyList<Source> GetAvailableSources(Guid userId);

        /// <summary>
        /// Gets the available organizational units.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>List of available units.</returns>
        IReadOnlyList<OrganizationalUnit> GetOrganizationalUnits(Guid userId);

        /// <summary>
        /// Adds a user.
        /// </summary>
        /// <param name="userMail">The user mail.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="role">The role.</param>
        /// <param name="principalUnit">The principal unit.</param>
        /// <param name="initialUnit">The initial unit of the user.</param>
        /// <param name="mustChangePassword">A value indicating whether the user must change the pw.</param>
        /// <param name="valid">A value whether the user is valid or not.</param>
        /// <returns>The user object of the new user.</returns>
        MethodResult<Guid> AddUser(string userMail, string userName, string password, Role role, Guid principalUnit, Guid initialUnit, bool mustChangePassword, bool valid);

        /// <summary>
        /// Deletes the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>MethodResult, indicates success</returns>
        MethodResult DeleteUser(Guid userId);

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
        /// Updates the source.
        /// </summary>
        /// <param name="updatedSource">The updated source.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult UpdateSource(Source updatedSource);

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
        IReadOnlyList<Tuple<string, Guid>> GetPrincipalUnitBasicInformation();

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
        
        /// <summary>
        /// Adds a principal unit.
        /// </summary>
        /// <param name="unitName">Name of the unit.</param>
        /// <param name="adminMailAddress">The admin mail address.</param>
        /// <param name="adminPassword">The admin password.</param>
        /// <returns>Tuple of new values.
        /// <para>Item1 = Id of new principal unit.</para>
        /// <para>Item2 = Id of new <see cref="Role.StaffChief"/>-user.</para>
        /// </returns>
        MethodResult<Tuple<Guid, Guid>> AddPrincipalUnit(string unitName, string adminMailAddress, string adminPassword);

        /// <summary>
        /// Adds a new unit unit.
        /// </summary>
        /// <param name="unitName">Name of the unit.</param>
        /// <param name="parentUnit">The id of the parent unit.</param>
        /// <returns>The id of the new unit.</returns>
        Guid AddUnit(string unitName, Guid parentUnit);

        /// <summary>
        /// Deletes an organizational unit.
        /// </summary>
        /// <param name="unitId">The unit identifier.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult DeleteOrganizationalUnit(Guid unitId);

        /// <summary>
        /// Renames an organizational unit.
        /// </summary>
        /// <param name="toChange">The id of the unit to change.</param>
        /// <param name="name">The new name.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult RenameOrganizationalUnit(Guid toChange, string name);

        /// <summary>
        /// Changes the organizational unit settings to the given ones.
        /// </summary>
        /// <param name="changed">The changed settings to persist.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult SaveOrganizationalUnitSettings(OrganizationalUnitSettings changed);

        /// <summary>
        /// Sets the organizational units of a user.
        /// </summary>
        /// <param name="userId">The user id of the changed user.</param>
        /// <param name="units">The organizational units.</param>
        /// <returns>MethodResult indicating success.</returns>
        MethodResult SetUserOrganizationalUnits(Guid userId, IEnumerable<Guid> units);

        /// <summary>
        /// Gets the children of a principal unit.
        /// </summary>
        /// <param name="principalUnitId">The principal unit identifier.</param>
        /// <returns>List of children of given principal unit.</returns>
        IReadOnlyList<Tuple<string, Guid>> GetPrincipalUnitChildren(Guid principalUnitId);

        /// <summary>
        /// Gets all principal units.
        /// </summary>
        /// <returns>The principal units</returns>
        IReadOnlyList<OrganizationalUnit> GetPrincipalUnits();

        /// <summary>
        /// Gets the organizational unit settings.
        /// </summary>
        /// <param name="unitId">The organizational unit id.</param>
        /// <returns>The requested settings.</returns>
        OrganizationalUnitSettings GetOrganizationalUnitSettings(Guid unitId);

        /// <summary>
        /// Gets the user settings.
        /// </summary>
        /// <param name="userId">The settings id.</param>
        /// <returns>The requested settings.</returns>
        UserSettings GetUserSettingsByUserId(Guid userId);

        IReadOnlyList<BasicUserInformation> GetManageableUsers(Guid userId);

        ExtendedUser GetUserInfo(Guid requested);

        Tuple<Feed, DateTime, int> GetFeedRequestData(Guid userId, Guid feedId);

        MethodResult ModifyUser(Guid userId, string userName, Role role, bool valid, IEnumerable<Guid> userUnits);

        IEnumerable<string> GetUnitInheritedBlackList(Guid userId);

        IEnumerable<User> GetEligibleStaffChiefs(Guid affectedUnit);
    }
}
