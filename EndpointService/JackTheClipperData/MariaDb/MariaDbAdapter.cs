using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JackTheClipperCommon;
using JackTheClipperCommon.BusinessObjects;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Extensions;
using JackTheClipperCommon.SharedClasses;
using JetBrains.Annotations;
using MySql.Data.MySqlClient;

namespace JackTheClipperData.MariaDb
{
    /// <summary>
    /// Adapter for MariaDb/MySql which implements the <see cref="IClipperDatabase"/> interface
    /// </summary>
    /// <seealso cref="JackTheClipperData.IClipperDatabase" />
    internal class MariaDbAdapter : IClipperDatabase
    {
        //// General things: NEVER DO ANYTHING ELSE THAN READ DATE FROM THE DATABASE. ALL MODIFICATIONS MUST BE DONE BY
        //// STORED PROCEDURES

        //// This code is more or less bad, especially cause no race conditions are considered (especially during inheritance)
        //// - as this is a educational project with a very limited amount of available time no one really expects this
        //// The database model isn't really good either - it may be 'ok' in terms of theoretical informatics
        //// but in practice it is a (especially performance) night mare. A matrix table with a discriminative attribute
        //// would have been much better.

        private static readonly Guid SystemUnit = Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000");
        private OrganizationalUnitManager cachedManager;

        #region GetUserById
        /// <summary>
        /// Gets the user by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The user (if found)</returns>
        public User GetUserById(Guid id)
        {
            var result = GetUsersAsync(MariaDbStatements.SelectUserById, new MySqlParameter("id", id)).Result;
            return result.FirstOrDefault();
        }

        public User GetUserByMailAddress(string mailAddress)
        {
            var result = GetUsersAsync(MariaDbStatements.SelectUserByMail, new MySqlParameter("mail", mailAddress)).Result;
            return result.FirstOrDefault();
        }

        #endregion

        #region GetOrganizationalUnitById
        /// <summary>
        /// Gets the organizational unit by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The unit (if found)</returns>
        public OrganizationalUnit GetOrganizationalUnitById(Guid id)
        {
            return new OrganizationalUnitManager(ref this.cachedManager).GetOrganizationalUnit(id);
        }
        #endregion

        #region GetUserByCredentials

        /// <summary>
        /// Gets the user by credentials.
        /// </summary>
        /// <param name="mailOrName">The mail.</param>
        /// <param name="password">The password.</param>
        /// <param name="principalUnit">The principal unit.</param>
        /// <param name="updateLoginTimeStamp">A value indicating whether the login timestamp should be updated or not.</param>
        /// <returns>The user (if found)</returns>
        public User GetUserByCredentials(string mailOrName, string password, Guid principalUnit, bool updateLoginTimeStamp)
        {
            try
            {
                var result = GetUsersAsync(MariaDbStatements.SelectUserByCredentials,
                                           new MySqlParameter("hash", password.GetSHA256()),
                                           new MySqlParameter("mail", mailOrName),
                                           new MySqlParameter("unit", principalUnit.ToString())).Result;
                if (result != null && result.Any())
                {
                    var user = result.First();
                    if (updateLoginTimeStamp && user != null && user.IsValid)
                    {
                        UpdateLoginTimestampAsync(user.Id).GetAwaiter().GetResult();
                    }

                    return user;
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        #endregion

        #region GetSources
        /// <summary>
        /// Gets all sources.
        /// </summary>
        /// <returns>List of all sources.</returns>
        public IReadOnlyList<Source> GetSources()
        {
            return GetSourcesAsync(MariaDbStatements.SelectAllSources).Result;
        }
        #endregion

        /// <summary>
        /// Gets all notifiable user settings
        /// ("notifiable" = only those which have set <see cref="UserSettings.NotificationSettings"/> != <see cref="NotificationSetting.None"/>).
        /// </summary>
        /// <returns>List of all notifiable user settings.</returns>
        public IReadOnlyCollection<NotifiableUserSettings> GetNotifiableUserSettings()
        {
            try
            {
                async Task<NotifiableUserSettings> Function(User user)
                {
                    var settings = await GetUserSettingsAsync(false, MariaDbStatements.SelectUserSettingsById, new MySqlParameter("id", user.SettingsId));
                    return new NotifiableUserSettings(user.Id, user.UserMailAddress, user.UserName, settings, user.LastLoginTime, user.PrincipalUnitName);
                }

                var relevant = GetUsersAsync(MariaDbStatements.SelectNotifiableUsers).Result;
                var tasks = new List<Task<NotifiableUserSettings>>(relevant.Count);
                foreach (var user in relevant)
                {
                    if (user.UserMailAddress.Contains('@'))
                    {
                        tasks.Add(Function(user));
                    }
                }

                Task.WaitAll(tasks.Cast<Task>().ToArray());
                return tasks.Where(x => x != null).Select(x => x.Result).ToList();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return new List<NotifiableUserSettings>();
            }
        }

        #region GetAvailableSources

        /// <summary>
        /// Gets the available sources.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <returns>List of available sources.</returns>
        public IReadOnlyList<Source> GetAvailableSources(Guid userId)
        {
            try
            {
                var result = new HashSet<Source>();
                var units = GetOrganizationalUnits(userId);
                if (units.Any(x => x.Id == SystemUnit))
                {
                    return GetSources();
                }

                foreach (var organizationalUnit in units)
                {
                    var settings = GetOrganizationalUnitSettings(organizationalUnit.Id);
                    result.UnionWith(settings.AvailableSources);
                }

                return result.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        #endregion

        #region GetOrganizationalUnits
        /// <summary>
        /// Gets the available organizational units.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>List of available units.</returns>
        public IReadOnlyList<OrganizationalUnit> GetOrganizationalUnits(Guid userId)
        {
            try
            {
                var manager = new OrganizationalUnitManager(ref this.cachedManager);
                var userUnits = GetUserOrganizationalUnitsAsync(userId).Result;
                if (userUnits.Contains(SystemUnit))
                {
                    return manager.GetAllOrganizationalUnits().ToList();
                }

                return userUnits.Select(uu => manager.GetOrganizationalUnit(uu)).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        #endregion

        #region AddUser

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
        public MethodResult<Guid> AddUser(string userMail, string userName, string password, Role role, Guid principalUnit, Guid initialUnit, bool mustChangePassword, bool valid)
        {
            return AddUserAsync(userMail, userName, password, role, principalUnit, initialUnit, mustChangePassword, valid).Result;
        }

        #endregion

        #region DeleteUser
        /// <summary>
        /// Deletes the given user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        public MethodResult DeleteUser(Guid userId)
        {
            return DeleteUserAsync(userId).Result;
        }
        #endregion

        #region AddSource
        /// <summary>
        /// Adds a source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The object of the new source.</returns>
        public Source AddSource(Source source)
        {
            return AddSourceAsync(source).Result;
        }
        #endregion

        #region DeleteSource
        /// <summary>
        /// Deletes the source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>
        /// True if successful
        /// </returns>
        public bool DeleteSource(Guid source)
        {
            return DeleteSourceAsync(source).Result;
        }
        #endregion

        #region UpdateSource
        /// <summary>
        /// Updates the source.
        /// </summary>
        /// <param name="updatedSource">The updated source.</param>
        /// <returns>MethodResult indicating success</returns>
        public MethodResult UpdateSource(Source updatedSource)
        {
            AddSource(updatedSource);
            return new MethodResult();
        }
        #endregion

        #region SaveUserSettings
        /// <summary>
        /// Saves the user settings.
        /// </summary>
        /// <param name="settingsId">The settings identifier.</param>
        /// <param name="notificationCheckInterval">The notification check interval.</param>
        /// <param name="notificationSetting">The notification setting.</param>
        /// <param name="articlesPerPage">The articles per page.</param>
        public void SaveUserSettings(Guid settingsId, int notificationCheckInterval, NotificationSetting notificationSetting,
                                     int articlesPerPage)
        {
            SaveUserSettingsAsync(settingsId, notificationCheckInterval, notificationSetting, articlesPerPage).GetAwaiter().GetResult();
        }
        #endregion

        #region AddFeed
        /// <summary>
        /// Adds the given feed.
        /// </summary>
        /// <param name="settingsId">The settings id.</param>
        /// <param name="feed">The feed to add.</param>
        public void AddFeed(Guid settingsId, Feed feed)
        {
            AddFeedAsync(settingsId, feed).GetAwaiter().GetResult();
        }
        #endregion

        #region ModifyFeed
        /// <summary>
        /// Modifies the feed.
        /// </summary>
        /// <param name="settingsId">The settings id.</param>
        /// <param name="feed">The feed to add.</param>
        public void ModifyFeed(Guid settingsId, Feed feed)
        {
            ModifyFeedAsync(settingsId, feed).GetAwaiter().GetResult();
        }
        #endregion

        #region DeleteFeed
        /// <summary>
        /// Deletes the feed.
        /// </summary>
        /// <param name="feedId">The feed id.</param>
        public void DeleteFeed(Guid feedId)
        {
            DeleteFeedAsync(feedId).GetAwaiter().GetResult();
        }
        #endregion

        #region ResetPassword
        /// <summary>
        /// Resets the users password.
        /// </summary>
        /// <param name="userMail">The users mail address.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>MethodResult, indicating success.</returns>
        public MethodResult ResetPassword(string userMail, string newPassword)
        {
            return ChangePasswordAsync(userMail, newPassword, true).Result;
        }
        #endregion

        #region ChangePassword
        /// <summary>
        /// Changes the users password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>MethodResult, indicating success.</returns>
        public MethodResult ChangePassword(User user, string newPassword)
        {
           return ChangePasswordAsync(user.MailAddress, newPassword, false).GetAwaiter().GetResult();
        }
        #endregion

        #region ChangeMailAddress
        /// <summary>
        /// Change the users mail address.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="newUserMail">The new mail address to set.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult ChangeMailAddress(User user, string newUserMail)
        {
            return ChangeMailAddressAsync(user, newUserMail).Result;
        }
        #endregion

        #region GetSuperSetFeed
        /// <summary>
        /// Gets the superset feed filter.
        /// This is a feed filter which contains all relevant keywords form any feed within the system.
        /// </summary>
        /// <returns>The superset feed filter.</returns>
        public Filter GetSuperSetFeed()
        {
            using (new PerfTracer(nameof(GetSuperSetFeed), 20))
            {
                try
                {
                    using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                    {
                        conn.Open();
                        var superset = new HashSet<string>();
                        using (var cmd = new MySqlCommand(MariaDbStatements.SelectSuperSetFeed, conn))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader != null && reader.Read())
                                {
                                    var keyWords = reader.GetStringFromNullable(0).ConvertToStringList();
                                    var blackList = reader.GetStringFromNullable(1).ConvertToStringList();
                                    superset.UnionWith(keyWords.Except(blackList));
                                }
                            }
                        }

                        return new Filter(Guid.Empty, superset, null, null);
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine(error);
                    return null;
                }
            }
        }
        #endregion

        #region AddPrincipalUnit
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
        public MethodResult<Tuple<Guid, Guid>> AddPrincipalUnit(string unitName, string adminMailAddress, string adminPassword)
        {
            return AddPrincipalUnitAsync(unitName, adminMailAddress, adminPassword).Result;
        }
        #endregion

        #region AddOrganizationalUnit
        /// <summary>
        /// Adds a new unit unit.
        /// </summary>
        /// <param name="unitName">Name of the unit.</param>
        /// <param name="parentUnit">The id of the parent unit.</param>
        /// <returns>The id of the new unit.</returns>
        public Guid AddUnit(string unitName, Guid parentUnit)
        {
            return AddUnitAsync(unitName, parentUnit).Result;
        }
        #endregion

        #region RenameOrganizationalUnit
        /// <summary>
        /// Renames an organizational unit.
        /// </summary>
        /// <param name="toChange">The id of the unit to change.</param>
        /// <param name="name">The new name.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult RenameOrganizationalUnit(Guid toChange, string name)
        {
            return ModifyUnitAsync(toChange, name).Result;
        }

        #endregion

        #region RemoveOrganizationalUnit
        /// <summary>
        /// Deletes an organizational unit.
        /// </summary>
        /// <param name="unitId">The unit identifier.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult DeleteOrganizationalUnit(Guid unitId)
        {
            return DeleteUnitAsync(unitId).GetAwaiter().GetResult();
        }
        #endregion

        #region SaveOrganizationalUnitSettings
        /// <summary>
        /// Changes the organizational unit settings to the given ones.
        /// </summary>
        /// <param name="changed">The changed settings to persist.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult SaveOrganizationalUnitSettings(OrganizationalUnitSettings changed)
        {
            try
            {
                using (new PerfTracer(nameof(SaveOrganizationalUnitSettings)))
                {
                    var results = new List<Task<MethodResult>>();

                    void SetSources(Guid settingsId, IReadOnlyCollection<Source> availableSources)
                    {
                        var currentSources = GetSourcesAsync(MariaDbStatements.SelectUnitSources,
                                                             new MySqlParameter("unitSettingsId", settingsId.ToString()))
                            .Result;
                        var removed = currentSources.Select(x => x.Id).ToHashSet();
                        removed.ExceptWith(availableSources.Select(x => x.Id));
                        foreach (var guid in removed)
                        {
                            results.Add(EnableOrDisableSourceForUnitAsync(settingsId, guid, true));
                        }

                        var added = availableSources.Select(x => x.Id).ToHashSet();
                        added.ExceptWith(currentSources.Select(x => x.Id));
                        foreach (var guid in added)
                        {
                            results.Add(EnableOrDisableSourceForUnitAsync(settingsId, guid, false));
                        }
                    }

                    var manager = new OrganizationalUnitManager(ref this.cachedManager);
                    var allUnits = manager.GetAllOrganizationalUnits();
                    var currentUnit = allUnits.First(x => x.SettingsId == changed.Id);
                    var affectedTree = manager.GetAllOrganizationalUnitsInTree(currentUnit.Id);
                    var principalUnit = manager.GetPrincipalUnit(currentUnit.Id);
                    var relevant = GetUsersAsync(MariaDbStatements.SelectUsersOfPrincipalUnit,
                                                 new MySqlParameter("unit", principalUnit.Id)).Result;
                    foreach (var unit in affectedTree)
                    {
                        //Inheritance
                        SetSources(unit.SettingsId, changed.AvailableSources);

                        //No inheritance for settings according to agreement
                        //results.Add(SaveUserSettingsAsync(unit.SettingsId, changed.NotificationCheckIntervalInMinutes,
                        //                                  changed.NotificationSettings, changed.ArticlesPerPage));
                        results.Add(SetOrganizationalUnitBlackListAsync(unit.SettingsId, changed.BlackList));
                    }

                    foreach (var user in relevant)
                    {
                        PerformUserSourceInheritance(user.Id).GetAwaiter().GetResult();
                    }

                    Task.WaitAll(results.Cast<Task>().ToArray());
                    var firstFailed = results.FirstOrDefault(x => !x.Result.IsSucceeded());
                    return firstFailed == null ? new MethodResult() : firstFailed.Result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        #endregion

        /// <summary>
        /// Gets the children of a principal unit.
        /// </summary>
        /// <param name="principalUnitId">The principal unit identifier.</param>
        /// <returns>List of children of given principal unit.</returns>
        public IReadOnlyList<Tuple<string, Guid>> GetPrincipalUnitChildren(Guid principalUnitId)
        {
            try
            {
                var manager = new OrganizationalUnitManager(ref this.cachedManager);
                var allInTree = manager.GetAllOrganizationalUnitsInTree(principalUnitId)
                    .Select(delegate (OrganizationalUnit x)
                    {
                        var temp = manager.GetOrganizationalUnit(x.Id);
                        return new Tuple<string, Guid>(temp.Name, temp.Id);
                    }).ToList();

                return allInTree;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Gets all principal units.
        /// </summary>
        /// <returns>The principal units</returns>
        public IReadOnlyList<OrganizationalUnit> GetPrincipalUnits()
        {
            try
            {
                var manager = new OrganizationalUnitManager(ref this.cachedManager);
                return manager.GetAllOrganizationalUnits().Where(x => x.IsPrincipalUnit).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Gets the organizational unit settings.
        /// </summary>
        /// <param name="unitId">The organizational unit id.</param>
        /// <returns>The requested settings.</returns>
        public OrganizationalUnitSettings GetOrganizationalUnitSettings(Guid unitId)
        {
            return (OrganizationalUnitSettings) GetUserSettingsAsync(true,
                                                                     MariaDbStatements.SelectUnitSettingsByUnitId,
                                                                     new MySqlParameter("id", unitId)).Result;
        }

        /// <summary>
        /// Gets the user settings.
        /// </summary>
        /// <param name="userId">The settings id.</param>
        /// <returns>The requested settings.</returns>
        public UserSettings GetUserSettingsByUserId(Guid userId)
        {
            return GetUserSettingsAsync(false, MariaDbStatements.SelectUserSettingsByUserId, new MySqlParameter("userId", userId)).Result;
        }

        /// <summary>
        /// Returns the basic information of the users the supplied user can manage
        /// </summary>
        /// <param name="userId">The id of the user requesting the information</param>
        /// <returns>List of basic information like the id and username of the manageable users</returns>
        public IReadOnlyList<BasicUserInformation> GetManageableUsers(Guid userId)
        {
            try
            {
                var userUnits = GetOrganizationalUnits(userId);
                var manageableUnits = new StringBuilder();
                foreach (var unit in userUnits)
                {
                    manageableUnits.Append(",")
                        .Append(GuidToDatabaseUuid(unit.Id));
                    foreach (var child in unit.GetAllChildren())
                    {
                        manageableUnits.Append(",")
                            .Append(GuidToDatabaseUuid(child.Id));
                    }
                }

                manageableUnits.Remove(0, 1);
                return GetBasicUserInfo(string.Format(MariaDbStatements.SelectBasicUserInfoForUsersInUnits, manageableUnits)).Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Gets all the information on a requested user
        /// </summary>
        /// <param name="requested">The id of the user whose information is requested</param>
        /// <returns>An <see cref="ExtendedUser"/> containing all the information</returns>
        public ExtendedUser GetUserInfo(Guid requested)
        {
            try
            {
                var u = GetUserById(requested);
                var userUnits = GetOrganizationalUnits(requested);
                return new ExtendedUser(u.Id, u.MailAddress, u.Role, u.UserName, u.SettingsId, u.MustChangePassword, u.LastLoginTime, u.IsValid, u.PrincipalUnitName, u.PrincipalUnitId, userUnits);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Prepares a request to see the articles of the given feed
        /// </summary>
        /// <param name="userId">The user owning the feed</param>
        /// <param name="feedId">The id of the feed</param>
        /// <returns>A tuple containing the fully instantiated <see cref="Feed"/>, the
        /// last login time of the user and how many articles he wants to see on a single
        /// page</returns>
        public Tuple<Feed, DateTime, int> GetFeedRequestData(Guid userId, Guid feedId)
        {
            return GetFeedRequestDataAsync(userId, feedId).Result;
        }

        /// <summary>
        /// Modifies a user
        /// </summary>
        /// <param name="userId">The id of the user to modify</param>
        /// <param name="userName">The new username of the user</param>
        /// <param name="role">The new role of the user</param>
        /// <param name="valid">Whether the user is allowed to use the application</param>
        /// <param name="userUnits">The <see cref="OrganizationalUnit"/>s the user should belong to</param>
        /// <returns>A <see cref="MethodResult"/> indicating success</returns>
        public MethodResult ModifyUser(Guid userId, string userName, Role role, bool valid, IEnumerable<Guid> userUnits)
        {
            return ModifyUserAsync(userId, userName, role, valid, userUnits).Result;
        }

        /// <summary>
        /// Gets the blacklist that a user has by belonging to certain <see cref="OrganizationalUnit"/>s
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>List of all blacklisted keywords for this user</returns>
        public IEnumerable<string> GetUnitInheritedBlackList(Guid userId)
        {
            return GetUnitInheritedBlackListAsync(userId).Result;
        }

        /// <summary>
        /// Gets the user that are allowed to manage the unit
        /// </summary>
        /// <param name="affectedUnit">The id of the unit</param>
        /// <returns>List of users that can manage the unit</returns>
        public IEnumerable<User> GetEligibleStaffChiefs(Guid affectedUnit)
        {
            var result = new List<User>();
            try
            {
                var manager = new OrganizationalUnitManager(ref cachedManager);
                var principalUnit = manager.GetPrincipalUnit(affectedUnit);
                var possible = GetUsersAsync(MariaDbStatements.SelectStaffChiefsOfPrincipalUnit,
                    new MySqlParameter("principalunit", principalUnit.Id.ToString())).Result;
                foreach (var user in possible)
                {
                    var userUnits = GetUserOrganizationalUnitsAsync(user.Id).Result;
                    foreach (var unit in userUnits)
                    {
                        if (manager.GetAllOrganizationalUnitsInTree(unit).Any(x => x.Id == affectedUnit))
                        {
                            result.Add(user);
                            break;
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
            }

            return result;
        }

        #region GetPrincipalUnitBasicInformation
        /// <summary>
        /// Gets the principal units.
        /// </summary>
        /// <returns>List of principal units.</returns>
        public IReadOnlyList<Tuple<string, Guid>> GetPrincipalUnitBasicInformation()
        {
            return GetStringGuidTupleList(MariaDbStatements.SelectPrincipalUnits).Result;
        }
        #endregion

        #region SetUserOrganizationalUnits                

        /// <summary>
        /// Sets the organizational units of a user.
        /// </summary>
        /// <param name="userId">The user id of the changed user.</param>
        /// <param name="units">The organizational units.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult SetUserOrganizationalUnits(Guid userId, IEnumerable<Guid> units)
        {
            return SetUserOrganizationalUnitsAsync(userId, units).Result;
        }

        #endregion

        ////Private methods

        #region GetUserSettingsAsync
        /// <summary>
        /// Gets the <see cref="UserSettings"/> by id.
        /// </summary>
        /// <param name="unitSettings">A value indicating whether <see cref="OrganizationalUnitSettings"/> are requested or not.</param>
        /// <param name="stmt">The statement to execute.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The determined <see cref="UserSettings"/>.</returns>
        private static async Task<UserSettings> GetUserSettingsAsync(bool unitSettings, string stmt, params MySqlParameter[] parameters)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    NotificationSetting notification;
                    int interval, articlesPerPage;
                    IReadOnlyList<string> unitBlackList = new List<string>();
                    Guid id;
                    using (var cmd = new MySqlCommand(stmt, conn))
                    {
                        cmd.AppendParameters(parameters);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            await reader.ReadAsync();
                            id = Guid.Parse(reader.GetString(0));
                            notification = reader.GetInt64(1).ConvertToNotification();
                            interval = reader.IsDBNull(2) ? 60 : reader.GetInt32(2);
                            articlesPerPage = reader.IsDBNull(3) ? 20 : reader.GetInt32(3);
                            if (unitSettings && !reader.IsDBNull(4))
                            {
                                unitBlackList = reader.GetString(4).ConvertToStringList();
                            }
                        }
                    }

                    var feedList = new List<Feed>();
                    using (var cmd = new MySqlCommand(MariaDbStatements.SelectUserSettingsFeeds, conn))
                    {
                        cmd.Parameters.AddWithValue("settingsId", id.ToString());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var feedId = Guid.Parse(reader.GetString(0));
                                var name = reader.GetString(1);
                                var filter = new Filter(Guid.Parse(reader.GetString(2)),
                                                        reader.GetStringFromNullable(4).ConvertToStringList(),
                                                        reader.GetStringFromNullable(5).ConvertToStringList(),
                                                        reader.GetStringFromNullable(3).ConvertToStringList());
                                var sources = await GetSourcesAsync(MariaDbStatements.SelectFeedSources,
                                                                    new MySqlParameter("feedid", feedId.ToString()));
                                feedList.Add(new Feed(feedId, sources, filter, name));
                            }
                        }
                    }

                    return !unitSettings
                        ? new UserSettings(id, feedList, notification, interval, articlesPerPage)
                        : new OrganizationalUnitSettings(id, feedList, await GetSourcesAsync(MariaDbStatements.SelectUnitSources, new MySqlParameter("unitSettingsId", id.ToString())), 
                                                         notification, interval, unitBlackList);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        #endregion

        #region GetSourcesAsync
        /// <summary>
        /// Gets all sources by executing the given command
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>List of sources.</returns>
        private static async Task<List<Source>> GetSourcesAsync(string command, params MySqlParameter[] parameters)
        {
            var result = new List<Source>();
            try
            {
                //using (new PerfTracer(nameof(GetSourcesAsync)))
                {
                    using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                    {
                        await conn.OpenAsync();
                        using (var cmd = new MySqlCommand(command, conn))
                        {
                            cmd.AppendParameters(parameters);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (reader.HasRows && await reader.ReadAsync())
                                {
                                    var id = Guid.Parse(reader.GetString(0));
                                    var url = reader.GetString(1);
                                    var name = reader.GetString(2);
                                    var contentType = reader.GetInt64(3).ConvertToContent();
                                    var regex = reader.IsDBNull(4) ? null : reader.GetString(4);
                                    var xpath = reader.IsDBNull(5) ? null : reader.GetString(5);
                                    var blacklist = reader.IsDBNull(6) ? null : reader.GetString(6).ConvertToStringList();
                                    result.Add(new Source(id, url, name, contentType, regex, xpath, blacklist));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }
        #endregion

        #region GetUsersAsync
        /// <summary>
        /// Gets all users by executing the given command
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>List of returned users.</returns>
        private static async Task<IReadOnlyList<User>> GetUsersAsync(string command, params MySqlParameter[] parameters)
        {
            var result = new List<User>();
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(command, conn))
                    {
                        cmd.AppendParameters(parameters);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.HasRows && await reader.ReadAsync())
                            {
                                var id = Guid.Parse(reader.GetString(0));
                                var name = reader.GetString(1);
                                var mail = reader.GetString(2);
                                var role = reader.GetInt64(3).ConvertToRole();
                                var settingsId = Guid.Parse(reader.GetString(4));
                                var mustChangePassword = reader.GetBoolean(5);
                                var valid = reader.GetBoolean(6);
                                var lastLogin = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7);
                                var principalUnitId = Guid.Parse(reader.GetString(8));
                                var principalUnitName = reader.GetString(9);
                                result.Add(new User(id, mail, role, name, settingsId, mustChangePassword, lastLogin,
                                                    valid, principalUnitName, principalUnitId));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }
        #endregion

        #region UpdateLoginTimestampAsync
        /// <summary>
        /// Updates the login time stamp.
        /// </summary>
        /// <param name="userId">The user id.</param>
        private static async Task UpdateLoginTimestampAsync(Guid userId)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = MariaDbSP.SP_LOG_USERLOGIN.ToString();
                        cmd.Parameters.Add(new MySqlParameter("userId", MySqlDbType.VarChar) { Value = userId.ToString() });
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region AddUserAsync
        /// <summary>
        /// Adds a user.
        /// </summary>
        /// <param name="userMail">The user mail.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="role">The role.</param>
        /// <param name="principalUnit">The principalUnit.</param>
        /// <param name="mustChangePassword">A value indicating whether the user must change the pw.</param>
        /// <param name="valid">A value whether the user is valid or not.</param>
        /// <returns>The user object of the new user.</returns>
        private async Task<MethodResult<Guid>> AddUserAsync(string userMail, string userName, string password, 
                                                                   Role role, Guid principalUnit, Guid initialUnit, 
                                                                   bool mustChangePassword, bool valid)
        {
            using(new PerfTracer(nameof(AddUserAsync)))
            try
            {
                if (principalUnit == SystemUnit || initialUnit == SystemUnit)
                {
                    throw new InvalidOperationException("No new users are possible on 'system' unit.");
                }

                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_CREATE_USER.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("name", userName);
                        cmd.Parameters.AddWithValue("mail", userMail);
                        cmd.Parameters.AddWithValue("pwHash", password.GetSHA256());
                        cmd.Parameters.AddWithValue("role", role.ToDatabaseRole());
                        cmd.Parameters.AddWithValue("principalUnit", principalUnit.ToString());
                        cmd.Parameters.AddWithValue("userUnit", initialUnit.ToString());
                        cmd.Parameters.AddWithValue("mustChangePassword", mustChangePassword);
                        cmd.Parameters.AddWithValue("valid", valid);
                        cmd.Parameters.Add("newUserId".ToStoredProcedureOutParam(MySqlDbType.VarChar));
                        await cmd.ExecuteNonQueryAsync();

                        var newUserId = Guid.Parse(cmd.Parameters["newUserId"].Value.ToString());
                        var user = GetUserById(newUserId);
                        var unitSettings = GetOrganizationalUnitSettings(initialUnit);

                        //Not possible according to agreement
                        //await SaveUserSettingsAsync(user.SettingsId, unitSettings.NotificationCheckIntervalInMinutes,
                        //                            unitSettings.NotificationSettings, unitSettings.ArticlesPerPage);
                        var tasks = new List<Task>();
                        foreach (var feed in unitSettings.Feeds)
                        {
                            var copy = new Feed(Guid.Empty, feed.Sources,
                                                new Filter(Guid.Empty, feed.Filter.Keywords, feed.Filter.Expressions,
                                                           feed.Filter.Blacklist), feed.Name);
                            tasks.Add(AddFeedAsync(user.SettingsId, copy));
                        }

                        Task.WaitAll(tasks.ToArray());
                        return new MethodResult<Guid>(newUserId);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new MethodResult<Guid>(SuccessState.UnknownError, e.Message, Guid.Empty);
            }
        }
        #endregion

        #region AddSourceAsync
        /// <summary>
        /// Adds a source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The object of the new source.</returns>
        private static async Task<Source> AddSourceAsync(Source source)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_UPDATE_SOURCE.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarChar) { Direction = ParameterDirection.InputOutput, Value = source.Id == Guid.Empty ? (object)DBNull.Value : source.Id.ToString() });
                        cmd.Parameters.Add(new MySqlParameter("url", MySqlDbType.Text) { Value = source.Uri });
                        cmd.Parameters.Add(new MySqlParameter("name", MySqlDbType.Text) { Value = source.Name });
                        cmd.Parameters.Add(new MySqlParameter("type", MySqlDbType.Bit) { Value = source.Type.ToDatabaseContentType() });
                        cmd.Parameters.Add(new MySqlParameter("regex", MySqlDbType.Text) { Value = source.Regex });
                        cmd.Parameters.Add(new MySqlParameter("xpath", MySqlDbType.Text) { Value = source.XPath });
                        cmd.Parameters.Add(new MySqlParameter("blacklist", MySqlDbType.Text) { Value = source.BlackList.ToDatabaseList() });
                        await cmd.ExecuteNonQueryAsync();

                        var newSourceId = Guid.Parse(cmd.Parameters["id"].Value.ToString());
                        var sources = await GetSourcesAsync(MariaDbStatements.SelectSpecificSourceById, new MySqlParameter("id", newSourceId));
                        return sources.First();
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return null;
            }
        }
        #endregion

        #region AddPrincipalUnitAsync
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
        private async Task<MethodResult<Tuple<Guid, Guid>>> AddPrincipalUnitAsync(string unitName, string adminMailAddress, string adminPassword)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_CREATE_PRINZIPALUNIT.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("name", MySqlDbType.Text) { Value = unitName });
                        cmd.Parameters.Add(new MySqlParameter("mail", MySqlDbType.VarChar) { Value = adminMailAddress });
                        cmd.Parameters.Add(new MySqlParameter("adminPwHash", MySqlDbType.VarChar) { Value = adminPassword.GetSHA256() });
                        cmd.Parameters.Add(new MySqlParameter("newPrincipalUnitId", MySqlDbType.VarChar) { Direction = ParameterDirection.Output });
                        cmd.Parameters.Add(new MySqlParameter("newUserId", MySqlDbType.VarChar) { Direction = ParameterDirection.Output });
                        await cmd.ExecuteNonQueryAsync();

                        this.cachedManager = null;
                        var principalUnitId = Guid.Parse(cmd.Parameters["newPrincipalUnitId"].Value.ToString());
                        var newUserId = Guid.Parse(cmd.Parameters["newUserId"].Value.ToString());
                        return new MethodResult<Tuple<Guid, Guid>>(new Tuple<Guid, Guid>(principalUnitId, newUserId));
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return new MethodResult<Tuple<Guid, Guid>>(SuccessState.UnknownError, error.Message, null);
            }
        }
        #endregion

        #region AddUnitAsync
        /// <summary>
        /// Adds a new unit asynchronously.
        /// </summary>
        /// <param name="unitName">Name of the unit.</param>
        /// <param name="parentUnit">The parent of the new unit.</param>
        private async Task<Guid> AddUnitAsync(string unitName, Guid parentUnit)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_CREATE_UNIT.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("name", MySqlDbType.Text) { Value = unitName });
                        cmd.Parameters.Add(new MySqlParameter("parent", MySqlDbType.VarChar) { Value = parentUnit.ToString() });
                        cmd.Parameters.Add(new MySqlParameter("newUnitId", MySqlDbType.VarChar) { Direction = ParameterDirection.Output });
                        await cmd.ExecuteNonQueryAsync();
                        this.cachedManager = null;
                        var unitId = Guid.Parse(cmd.Parameters["newUnitId"].Value.ToString());
                        return unitId;
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                throw;
            }
        }
        #endregion

        #region ModifyUnitAsync
        /// <summary>
        /// Renames an organizational unit.
        /// </summary>
        /// <param name="toChange">The id of the unit to change.</param>
        /// <param name="name">The new name.</param>
        /// <returns>MethodResult indicating success.</returns>
        private static async Task<MethodResult> ModifyUnitAsync(Guid toChange, string name)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_UPDATE_UNIT.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("name", MySqlDbType.Text) { Value = name });
                        cmd.Parameters.Add(new MySqlParameter("unitId", MySqlDbType.VarChar) { Value = toChange.ToString() });
                        await cmd.ExecuteNonQueryAsync();
                        return new MethodResult();
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return new MethodResult(SuccessState.UnknownError, error.Message);
            }
        }
        #endregion

        #region DeleteUnitAsync
        /// <summary>
        /// Deletes the given unit asynchronously.
        /// </summary>
        /// <param name="unitId">The unit identifier.</param>
        private async Task<MethodResult> DeleteUnitAsync(Guid unitId)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_DELETE_UNIT.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("unitId", MySqlDbType.VarChar) { Value = unitId.ToString() });
                        await cmd.ExecuteNonQueryAsync();
                        this.cachedManager = null;
                        return new MethodResult();
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return new MethodResult(SuccessState.UnknownError, error.Message);
            }
        }
        #endregion

        #region EnableOrDisableSourceForUnitAsync        
        /// <summary>
        /// Enables the or disables a source for a unit asynchronously.
        /// </summary>
        /// <param name="unitSettingsId">The unit settings identifier.</param>
        /// <param name="sourceId">The source identifier.</param>
        /// <param name="disable">A value indicating whether the source should be disabled (=true) or enabled.</param>
        private static async Task<MethodResult> EnableOrDisableSourceForUnitAsync(Guid unitSettingsId, Guid sourceId, bool disable)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    var stmt = (disable ? MariaDbSP.SP_REMOVE_UNIT_SOURCE : MariaDbSP.SP_ADD_UNIT_SOURCE).ToString();
                    using (var cmd = new MySqlCommand(stmt, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("settingsId", MySqlDbType.VarChar) { Value = unitSettingsId.ToString() });
                        cmd.Parameters.Add(new MySqlParameter("sourceId", MySqlDbType.VarChar) { Value = sourceId.ToString() });
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return new MethodResult(SuccessState.UnknownError, error.Message);
            }

            return new MethodResult();
        }
        #endregion

        #region DeleteSourceAsync
        /// <summary>
        /// Deletes the source asynchronous.
        /// </summary>
        /// <param name="sourceId">The source id.</param>
        /// <returns>True if successful false otherwise</returns>
        private static async Task<bool> DeleteSourceAsync(Guid sourceId)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_DEL_SOURCE.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarChar) { Value = sourceId.ToString() });
                        await cmd.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return false;
            }
        }
        #endregion

        #region DeleteUserAsync
        /// <summary>
        /// Deletes a user asynchronous.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>MethodResult indicating success</returns>
        private static async Task<MethodResult> DeleteUserAsync(Guid userId)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_DEL_USER.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarChar) { Value = userId.ToString() });
                        await cmd.ExecuteNonQueryAsync();
                        return new MethodResult();
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return new MethodResult(SuccessState.UnknownError, error.Message);
            }
        }
        #endregion

        #region SaveUserSettings (Will be splitted up)
        /// <summary>
        /// Saves the user settings asynchronously.
        /// </summary>
        /// <param name="settingsId">The settings identifier.</param>
        /// <param name="notificationCheckInterval">The notification check interval.</param>
        /// <param name="notificationSetting">The notification setting.</param>
        /// <param name="articlesPerPage">The articles per page.</param>
        /// <returns></returns>
        private static async Task<MethodResult> SaveUserSettingsAsync(Guid settingsId, int notificationCheckInterval,
                                                        NotificationSetting notificationSetting, int articlesPerPage)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_UPDATE_USERSETTINGS.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarChar) {Value = settingsId.ToString()});
                        cmd.Parameters.Add(new MySqlParameter("_interval", MySqlDbType.Int32) {Value = notificationCheckInterval});
                        cmd.Parameters.Add(new MySqlParameter("notification", MySqlDbType.Bit) {Value = notificationSetting.ToDatabaseNotification()});
                        cmd.Parameters.Add(new MySqlParameter("articlesPerPage", MySqlDbType.Int32) {Value = articlesPerPage == 0 ? (object) DBNull.Value : articlesPerPage});
                        await cmd.ExecuteNonQueryAsync();
                        return new MethodResult();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new MethodResult(SuccessState.UnknownError, e.Message);
            }
        }
        #endregion

        #region SetOrganizationalUnitBlackListAsync
        /// <summary>
        /// Sets the organizational unit black list asynchronously.
        /// </summary>
        /// <param name="settingsId">The settings identifier.</param>
        /// <param name="blackList">The black list.</param>
        private static async Task<MethodResult> SetOrganizationalUnitBlackListAsync(Guid settingsId, IEnumerable<string> blackList)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_SET_UNIT_BLACKLIST.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("settingsId", MySqlDbType.VarChar) {Value = settingsId.ToString()});
                        cmd.Parameters.Add(new MySqlParameter("blackList", MySqlDbType.Text) {Value = blackList.ToDatabaseList()});
                        await cmd.ExecuteNonQueryAsync();
                        return new MethodResult();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new MethodResult(SuccessState.UnknownError, e.Message);
            }
        }
        #endregion

        #region AddFeedAsync

        /// <summary>
        /// Adds the feed asynchronously.
        /// </summary>
        /// <param name="settingsId">The settings identifier.</param>
        /// <param name="feed">The feed.</param>
        private async Task AddFeedAsync(Guid settingsId, Feed feed)
        {
            using (new PerfTracer(nameof(AddFeedAsync)))
            {
                try
                {
                    var filterId = await CreateOrUpdateFilter(feed.Filter);
                    var feedId = await CreateOrUpdateFeed(Guid.Empty, feed.Name, filterId);
                    if (feed.Sources != null)
                    {
                        foreach (var source in feed.Sources)
                        {
                           await LinkSourceToFeed(feedId, source.Id);
                        }
                    }

                    using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                    {
                        await conn.OpenAsync();
                        using (var cmd = new MySqlCommand(MariaDbSP.SP_LINK_SETTINGS_FEED.ToString(), conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new MySqlParameter("feedId", MySqlDbType.VarChar)
                                                   {Value = feedId.ToString()});
                            cmd.Parameters.Add(new MySqlParameter("settingsId", MySqlDbType.VarChar)
                                                   {Value = settingsId.ToString()});
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    var manager = new OrganizationalUnitManager(ref this.cachedManager);
                    var changedUnit = manager.GetAllOrganizationalUnits()
                                             .FirstOrDefault(x => x.SettingsId == settingsId);
                    if (changedUnit != null)
                    {
                        //Inherit to users
                        var affectedTree = manager.GetAllOrganizationalUnitsInTree(changedUnit.Id);
                        var principalUnit = manager.GetPrincipalUnit(changedUnit.Id);
                        var relevant = GetUsersAsync(MariaDbStatements.SelectUsersOfPrincipalUnit,
                                                     new MySqlParameter("unit", principalUnit.Id)).Result;
                        foreach (var user in relevant)
                        {
                            var units = await GetUserOrganizationalUnitsAsync(user.Id);
                            foreach (var unit in units)
                            {
                                var userTree = manager.GetAllOrganizationalUnitsInTree(unit);
                                if (userTree.Any(x => affectedTree.Contains(x)) &&
                                    !userTree.Any(x => x.Id == changedUnit.ParentId))
                                {
                                    var newFeed = new Feed(Guid.Empty, feed.Sources,
                                                           new Filter(Guid.Empty, feed.Filter.Keywords,
                                                                      feed.Filter.Expressions, feed.Filter.Blacklist),
                                                           feed.Name);
                                    await AddFeedAsync(user.SettingsId, newFeed);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        #endregion

        #region ModifyFeedAsync
        /// <summary>
        /// Modifies the feed asynchronously.
        /// </summary>
        /// <param name="settingsId">The settings identifier.</param>
        /// <param name="feed">The feed.</param>
        private static async Task ModifyFeedAsync(Guid settingsId, Feed feed)
        {
            var allTasks = new List<Task>();
            try
            {
                var settings = await GetUserSettingsAsync(false, MariaDbStatements.SelectUserSettingsById, new MySqlParameter("id", settingsId));
                var actualFeed = settings.Feeds.FirstOrDefault(x => x.Id == feed.Id);
                if (actualFeed == null)
                {
                    throw new InvalidOperationException();
                }

                if (!actualFeed.Filter.Equals(feed.Filter))
                {
                    allTasks.Add(CreateOrUpdateFilter(feed.Filter));
                }

                if (actualFeed.Name != feed.Name)
                {
                    allTasks.Add(CreateOrUpdateFeed(feed.Id, feed.Name, feed.Filter.Id));
                }

                var existingSources = actualFeed.Sources.Select(x => x.Id).ToHashSet();
                var newSources = feed.Sources.Select(x => x.Id).ToHashSet();
                if (!existingSources.SetEquals(newSources))
                {
                    var added = newSources.ToHashSet();
                    added.ExceptWith(existingSources);
                    foreach (var sourceId in added)
                    {
                        allTasks.Add(LinkSourceToFeed(feed.Id, sourceId));
                    }

                    var removed = existingSources;
                    removed.ExceptWith(newSources);
                    foreach (var sourceId in removed)
                    {
                        allTasks.Add(UnlinkSourceFromFeed(feed.Id, sourceId));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Task.WaitAll(allTasks.ToArray());
        }
        #endregion

        #region DeleteFeedAsync
        /// <summary>
        /// Deletes the feed asynchronously.
        /// </summary>
        /// <param name="feedId">The feed identifier.</param>
        private static async Task DeleteFeedAsync(Guid feedId)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_DELETE_FEED.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("feedId", MySqlDbType.VarChar) { Value = feedId.ToString() });
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region LinkSourceToFeed
        /// <summary>
        /// Links a source to a feed.
        /// </summary>
        /// <param name="feedId">The feed identifier.</param>
        /// <param name="sourceId">The source identifier.</param>
        private static async Task LinkSourceToFeed(Guid feedId, Guid sourceId)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_LINK_SOURCE_FEED.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("feedId", MySqlDbType.VarChar) { Value = feedId.ToString() });
                        cmd.Parameters.Add(new MySqlParameter("sourceId", MySqlDbType.VarChar) { Value = sourceId.ToString() });
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        #endregion

        #region UnlinkSourceFromFeed
        /// <summary>
        /// Unlinks a source from a feed.
        /// </summary>
        /// <param name="feedId">The feed identifier.</param>
        /// <param name="sourceId">The source identifier.</param>
        private static async Task UnlinkSourceFromFeed(Guid feedId, Guid sourceId)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_REMOVE_SOURCE_FEED.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("feedId", MySqlDbType.VarChar) { Value = feedId.ToString() });
                        cmd.Parameters.Add(new MySqlParameter("sourceId", MySqlDbType.VarChar) { Value = sourceId.ToString() });
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        #endregion

        #region CreateOrUpdateFeed
        /// <summary>
        /// Creates the or updates a feed.
        /// </summary>
        /// <param name="feedId">The feed identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="filterId">The filter identifier.</param>
        private static async Task<Guid> CreateOrUpdateFeed(Guid feedId, string name, Guid filterId)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_UPDATE_FEED.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarChar) { Direction = ParameterDirection.InputOutput, Value = feedId == Guid.Empty ? (object)DBNull.Value : feedId.ToString() });
                        cmd.Parameters.Add(new MySqlParameter("name", MySqlDbType.Text) { Value = name });
                        cmd.Parameters.Add(new MySqlParameter("filterId", MySqlDbType.VarChar) { Value = filterId.ToString() });
                        await cmd.ExecuteNonQueryAsync();
                        return Guid.Parse(cmd.Parameters["id"].Value.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        #endregion

        #region CreateOrUpdateFilter
        /// <summary>
        /// Creates the or updates the filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        private static async Task<Guid> CreateOrUpdateFilter(Filter filter)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_UPDATE_FILTER.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarChar)
                        {
                            Direction = ParameterDirection.InputOutput,
                            Value = filter.Id == Guid.Empty ? (object)DBNull.Value : filter.Id.ToString()
                        });
                        cmd.Parameters.Add(new MySqlParameter("keywords", MySqlDbType.Text) { Value = filter.Keywords.ToDatabaseList() });
                        cmd.Parameters.Add(new MySqlParameter("expressions", MySqlDbType.Text) { Value = filter.Expressions.ToDatabaseList() });
                        cmd.Parameters.Add(new MySqlParameter("blacklist", MySqlDbType.Text) { Value = filter.Blacklist.ToDatabaseList() });
                        await cmd.ExecuteNonQueryAsync();
                        return Guid.Parse(cmd.Parameters["id"].Value.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        #endregion

        #region ChangePassword
        /// <summary>
        /// Changes the users password.
        /// </summary>
        /// <param name="userMail">The users mail address.</param>
        /// <param name="password">The new password to set.</param>
        /// <param name="mustChangePassword"></param>
        /// <returns>MethodResult, indicating success.</returns>
        private static async Task<MethodResult> ChangePasswordAsync(string userMail, string password, bool mustChangePassword)
        {
            using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(MariaDbSP.SP_CHANGE_USERPW.ToString(), conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("mail", userMail);
                    cmd.Parameters.AddWithValue("pwHash", password.GetSHA256());
                    cmd.Parameters.AddWithValue("mustChangePassword", mustChangePassword);
                    cmd.Parameters.Add("success".ToStoredProcedureOutParam(MySqlDbType.Bool));
                    await cmd.ExecuteNonQueryAsync();
                    return bool.Parse(cmd.Parameters["success"].Value.ToString())
                        ? new MethodResult()
                        : new MethodResult(SuccessState.UnknownError, "Ooops. Something went wrong!");
                }
            }
        }
        #endregion

        #region ChangeMailAddress
        /// <summary>
        /// Change the users mail address.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="newUserMail">The new mail address to set.</param>
        /// <returns>MethodResult indicating success.</returns>
        private static async Task<MethodResult> ChangeMailAddressAsync(User user, string newUserMail)
        {
            using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(MariaDbSP.SP_CHANGE_MAILADDRESS.ToString(), conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("userId", user.Id.ToString());
                    cmd.Parameters.AddWithValue("newMail", newUserMail);
                    cmd.Parameters.Add("success".ToStoredProcedureOutParam(MySqlDbType.Bool));
                    await cmd.ExecuteNonQueryAsync();

                    var result = bool.Parse(cmd.Parameters["success"].Value.ToString());
                    if (result)
                    {
                        return new MethodResult(SuccessState.Successful, "Your Mail got changed.");
                    }

                    return new MethodResult(SuccessState.UnknownError, "Ooops. Something went wrong!");
                }
            }
        }
        #endregion

        #region GetSuperSetFeedAsync
        /// <summary>
        /// Gets the superset feed.
        /// </summary>
        /// <returns>The superset feed.</returns>
        private static async Task<Filter> GetSuperSetFeedAsync()
        {
            using (new PerfTracer(nameof(GetSuperSetFeedAsync)))
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    var superset = new HashSet<string>();
                    using (var cmd = new MySqlCommand(MariaDbStatements.SelectSuperSetFeed, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var keyWords = reader.GetStringFromNullable(0).ConvertToStringList();
                                var blackList = reader.GetStringFromNullable(1).ConvertToStringList();
                                superset.UnionWith(keyWords.Except(blackList));
                            }
                        }
                    }

                    return new Filter(Guid.Empty, superset, new List<string>(), new List<string>());
                }
            }
        }
        #endregion

        #region GetPrincipalUnitBasicInformation        
        /// <summary>
        /// Gets a list of <see cref="Tuple{String, Guid}"/>s from the database.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Obtained list of <see cref="Tuple{String, Guid}"/>s.</returns>
        private static async Task<IReadOnlyList<Tuple<string, Guid>>> GetStringGuidTupleList(string command, params MySqlParameter[] parameters)
        {
            var result = new List<Tuple<string, Guid>>();
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(command, conn))
                    {
                        cmd.AppendParameters(parameters);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.HasRows && await reader.ReadAsync())
                            {
                                result.Add(new Tuple<string, Guid>(reader.GetString(0), Guid.Parse(reader.GetString(1))));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Returns the basic information of users by executing the command
        /// </summary>
        /// <param name="command">SQL query to execute</param>
        /// <returns>List of basic information like the id and username of the manageable users</returns>
        private static async Task<IReadOnlyList<BasicUserInformation>> GetBasicUserInfo(string command)
        {
            var result = new List<BasicUserInformation>();
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(command, conn))
                    {
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.HasRows && await reader.ReadAsync())
                            {
                                result.Add(new BasicUserInformation(Guid.Parse(reader.GetString(1)), reader.GetString(0), reader.GetBoolean(2)));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            result.Sort((x, y) => string.Compare(x.UserName, y.UserName, StringComparison.CurrentCulture));
            return result;
        }

        #region GetUserOrganizationalUnitsAsync
        private static async Task<IReadOnlyList<Guid>> GetUserOrganizationalUnitsAsync(Guid userId)
        {
            var result = new List<Guid>();
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbStatements.SelectUserOrganizationalUnitIds, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", userId.ToString());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync() && Guid.TryParse(reader.GetString(0), out var id))
                            {
                                result.Add(id);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Performs the user source inheritance.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        private async Task PerformUserSourceInheritance(Guid userId)
        {
            var allowedSources = GetAvailableSources(userId);
            var settings = await GetUserSettingsAsync(false, MariaDbStatements.SelectUserSettingsByUserId,
                                                new MySqlParameter("userId", userId));
            foreach (var feed in settings.Feeds)
            {
                var noLongerAllowed = feed.Sources.Except(allowedSources).ToList();
                if (noLongerAllowed.Any())
                {
                    var newFeed = new Feed(feed.Id, noLongerAllowed, feed.Filter, feed.Name);
                    await ModifyFeedAsync(settings.Id, newFeed);
                }
            }
        }

        /// <summary>
        /// Prepares a request to see the articles of the given feed
        /// </summary>
        /// <param name="userId">The user owning the feed</param>
        /// <param name="feedId">The id of the feed</param>
        /// <returns>A tuple containing the fully instantiated <see cref="Feed"/>, the
        /// last login time of the user and how many articles he wants to see on a single
        /// page</returns>
        private static async Task<Tuple<Feed, DateTime, int>> GetFeedRequestDataAsync(Guid userId, Guid feedId)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbStatements.SelectFeedRequestData, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", userId.ToString());
                        cmd.Parameters.AddWithValue("feedId", feedId.ToString());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            await reader.ReadAsync();
                            var name = reader.GetString(0);
                            var filter = new Filter(Guid.Parse(reader.GetString(1)),
                                                    reader.GetStringFromNullable(3).ConvertToStringList(),
                                                    reader.GetStringFromNullable(4).ConvertToStringList(),
                                                    reader.GetStringFromNullable(2).ConvertToStringList());
                            var lastLogin = reader.GetDateTime(5);
                            var articlesPerPage = reader.IsDBNull(6) ? 20 : reader.GetInt32(6);
                            var sources = await GetSourcesAsync(MariaDbStatements.SelectFeedSources, new MySqlParameter("feedid", feedId.ToString()));
                            return new Tuple<Feed, DateTime, int>(new Feed(feedId, sources, filter, name), lastLogin, articlesPerPage);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Sets the organizational units of a user.
        /// </summary>
        /// <param name="userId">The user id of the changed user.</param>
        /// <param name="units">The organizational units the user should now belong to.</param>
        /// <returns><see cref="MethodResult"/> indicating success.</returns>
        private static async Task<MethodResult> SetUserOrganizationalUnitsAsync(Guid userId, IEnumerable<Guid> units)
        {
            try
            {
                var current = await GetUserOrganizationalUnitsAsync(userId);
                var added = units.Except(current);
                var deleted = current.Except(units);
                if (added.Any() || deleted.Any())
                {
                    using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                    {
                        await conn.OpenAsync();
                        var user = userId.ToString();
                        foreach (var unit in added)
                        {
                            using (var cmd = new MySqlCommand(MariaDbSP.SP_ADD_USER_UNIT.ToString(), conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new MySqlParameter("userId", MySqlDbType.VarChar) { Value =  user });
                                cmd.Parameters.Add(new MySqlParameter("unitId", MySqlDbType.VarChar) { Value = unit.ToString() });
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }


                        foreach (var unit in deleted)
                        {
                            using (var cmd = new MySqlCommand(MariaDbSP.SP_REMOVE_USER_UNIT.ToString(), conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new MySqlParameter("userId", MySqlDbType.VarChar) { Value = user });
                                cmd.Parameters.Add(new MySqlParameter("unitId", MySqlDbType.VarChar) { Value = unit.ToString() });
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return new MethodResult(SuccessState.UnknownError, error.Message);
            }

            return new MethodResult();
        }

        /// <summary>
        /// Modifies a user
        /// </summary>
        /// <param name="userId">The id of the user to modify</param>
        /// <param name="userName">The new username of the user</param>
        /// <param name="role">The new role of the user</param>
        /// <param name="valid">Whether the user is allowed to use the application</param>
        /// <param name="userUnits">The <see cref="OrganizationalUnit"/>s the user should belong to</param>
        /// <returns>A <see cref="MethodResult"/> indicating success</returns>
        private async Task<MethodResult> ModifyUserAsync(Guid userId, string userName, Role role, bool valid, IEnumerable<Guid> userUnits)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_UPDATE_USER_AND_CLEAR_USERUNITS.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("name", userName);
                        cmd.Parameters.Add(new MySqlParameter("role", MySqlDbType.Bit) { Value = role.ToDatabaseRole() });
                        cmd.Parameters.AddWithValue("userId", userId.ToString());
                        cmd.Parameters.AddWithValue("valid", valid);
                        await cmd.ExecuteNonQueryAsync();
                        conn.Close();
                        return await SetUserOrganizationalUnitsAsync(userId, userUnits);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new MethodResult(SuccessState.UnknownError, e.Message);
            }
        }

        /// <summary>
        /// Gets the blacklist that a user has by belonging to certain <see cref="OrganizationalUnit"/>s
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>List of all blacklisted keywords for this user</returns>
        public async Task<IEnumerable<string>> GetUnitInheritedBlackListAsync(Guid userId)
        {
            var result = new HashSet<string>();
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbStatements.SelectInheritedBlacklists, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", userId.ToString());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.HasRows && await reader.ReadAsync())
                            {
                                var blackList = reader.GetStringFromNullable(0).ConvertToStringList();
                                result.UnionWith(blackList);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }

        /// <summary>
        /// Maps a <see cref="Guid"/> to a value that MariaDB can understand
        /// </summary>
        /// <param name="guid">The guid to map</param>
        /// <returns>The mapped version of the supplied guid</returns>
        private static string GuidToDatabaseUuid(Guid guid)
        {
            return $"UUID_TO_BIN('{guid}')";
        }

        /// <summary>
        /// This class is inefficient, and should maybe be replaced by a better implementation (far in the future).
        /// Also it is not clear if this becomes a performance issue at all.
        /// </summary>
        private class OrganizationalUnitManager
        {
            [NotNull]
            private IReadOnlyDictionary<Guid, OrganizationalUnit> allUnits;

            /// <summary>
            /// Initializes the <see cref="MariaDbAdapter.cachedManager"/>
            /// </summary>
            /// <param name="cachedManager">The <see cref="OrganizationalUnitManager"/> to initialize</param>
            public OrganizationalUnitManager(ref OrganizationalUnitManager cachedManager)
            {
                if (cachedManager == null)
                {
                    this.allUnits = GetOrganizationalUnitsAsync(MariaDbStatements.SelectAllOrganizationalUnits).Result;
                    foreach (var pair in allUnits)
                    {
                        pair.Value.Children = allUnits.Values
                                                      .Where(o => o != null && o.ParentId == pair.Value.Id &&
                                                                  o.ParentId != o.Id)
                                                      .ToList();
                    }

                    cachedManager = this;
                }
                else
                {
                    this.allUnits = cachedManager.allUnits;
                }
            }

            /// <summary>
            /// Gets a <see cref="OrganizationalUnit"/> by its id
            /// </summary>
            /// <param name="unitId">The id of the unit</param>
            /// <returns><see cref="OrganizationalUnit"/> that matches the supplied Guid</returns>
            public OrganizationalUnit GetOrganizationalUnit(Guid unitId)
            {
                return this.allUnits[unitId];
            }

            /// <summary>
            /// Gets all organizational units
            /// </summary>
            /// <returns>List with all organizational units</returns>
            [NotNull]
            public IEnumerable<OrganizationalUnit> GetAllOrganizationalUnits()
            {
                return this.allUnits.Values;
            }

            /// <summary>
            /// Gets all <see cref="OrganizationalUnit"/>s that are lower in the hierarchy than the supplied unit
            /// </summary>
            /// <param name="startUnit">The id of the unit to start on</param>
            /// <returns>List of all units below the supplied one</returns>
            public IReadOnlyList<OrganizationalUnit> GetAllOrganizationalUnitsInTree(Guid startUnit)
            {
                var result = new List<OrganizationalUnit>();
                OrganizationalUnit unit;
                if (this.allUnits.TryGetValue(startUnit, out unit) && unit != null)
                {
                    result.Add(unit);
                    result.AddRange(unit.GetAllChildren());
                }

                return result;
            }

            /// <summary>
            /// Gets the client unit the unit belongs to
            /// </summary>
            /// <param name="startUnit">The id of the unit whose client is searched</param>
            /// <returns>The client unit the supplied unit belongs to</returns>
            public OrganizationalUnit GetPrincipalUnit(Guid startUnit)
            {
                OrganizationalUnit unit;
                if (this.allUnits.TryGetValue(startUnit, out unit) && unit != null)
                {
                    if (unit.IsPrincipalUnit || unit.ParentId == unit.Id)
                    {
                        return unit;
                    }

                    return GetPrincipalUnit(unit.ParentId);
                }

                return unit;
            }

            #region GetOrganizationalUnitsAsync

            /// <summary>
            /// Gets all units by executing the given command
            /// </summary>
            /// <param name="command">The command to execute.</param>
            /// <param name="parameters">The query parameters.</param>
            /// <returns>List of returned units.</returns>
            private static async Task<IReadOnlyDictionary<Guid, OrganizationalUnit>> GetOrganizationalUnitsAsync(
                string command, params MySqlParameter[] parameters)
            {
                var result = new Dictionary<Guid, OrganizationalUnit>();
                try
                {
                    using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                    {
                        await conn.OpenAsync();
                        using (var cmd = new MySqlCommand(command, conn))
                        {
                            cmd.AppendParameters(parameters);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                while (reader.HasRows && await reader.ReadAsync())
                                {
                                    var id = Guid.Parse(reader.GetString(0));
                                    var name = reader.GetString(1);
                                    var parentId = Guid.Parse(reader.GetString(2));
                                    var settingsId = Guid.Parse(reader.GetString(3));
                                    var mail = reader.IsDBNull(4) ? null : reader.GetString(4);
                                    result.Add(id, new OrganizationalUnit(id, name, parentId == SystemUnit, mail, parentId, settingsId));
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return result;
            }

            #endregion
        }
    }
}
