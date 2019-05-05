using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JackTheClipperCommon;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.SharedClasses;
using MySql.Data.MySqlClient;

namespace JackTheClipperData.MariaDb
{
    /// <summary>
    /// Adapter for MariaDb/MySql which implements the <see cref="IClipperDatabase"/> interface
    /// </summary>
    /// <seealso cref="JackTheClipperData.IClipperDatabase" />
    public class MariaDbAdapter : IClipperDatabase
    {
        //// General things: NEVER DO ANYTHING ELSE THAN READ DATE FROM THE DATABASE. ALL MODIFICATIONS MUST BE DONE BY
        //// STORED PROCEDURES

        private static readonly Guid SystemUnit = Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000");
        private static Tuple<DateTime, Filter> superSetFeedCache;
        private static readonly object LockObj = new object();

        #region GetUserById
        /// <summary>
        /// Gets the user by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The user (if found)</returns>
        public User GetUserById(Guid id)
        {
            var result = GetUsersAsync(MariaDbStatements.SelectUserById, new MySqlParameter("id", id)).Result;
            return result.First();
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
            var result = GetOrganizationalUnitsAsync(MariaDbStatements.SelectOrganizationalUnitById,
                                                new MySqlParameter("id", id)).Result;
            return result[id];
        }
        #endregion

        #region GetUserByCredentials
        /// <summary>
        /// Gets the user by credentials.
        /// </summary>
        /// <param name="mail">The mail.</param>
        /// <param name="password">The password.</param>
        /// <param name="updateLoginTimeStamp">A value indicating whether the login timestamp should be updated or not.</param>
        /// <returns>The user (if found)</returns>
        public User GetUserByCredentials(string mail, string password, bool updateLoginTimeStamp)
        {
            var result = GetUsersAsync(MariaDbStatements.SelectUserByCredentials,
                                  new MySqlParameter("hash", password.GetSHA256()),
                                  new MySqlParameter("mail", mail)).Result;
            var user = result.First();
            if (updateLoginTimeStamp && user != null && user.IsValid)
            {
                UpdateLoginTimestampAsync(user.Id).GetAwaiter().GetResult();
            }

            return user;
        }
        #endregion

        #region GetSources
        /// <summary>
        /// Gets all sources.
        /// </summary>
        /// <returns>List of all sources.</returns>
        public IReadOnlyCollection<Source> GetSources()
        {
            return GetSourcesAsync(MariaDbStatements.SelectAllSources).Result;
        }
        #endregion

        #region GetAllUsers
        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns>List of all users.</returns>
        public IReadOnlyCollection<User> GetAllUsers()
        {
            return GetUsersAsync(MariaDbStatements.SelectAllUsers).Result;
        }
        #endregion

        #region GetAvailableSources
        /// <summary>
        /// Gets the available sources.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>List of available sources.</returns>
        public IReadOnlyList<Source> GetAvailableSources(User user)
        {
            //TODO: At the moment we cant make sources availaible
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (user.Role == Role.SystemAdministrator || true)
            {
                //No right checks
                return GetSourcesAsync(MariaDbStatements.SelectAllSources).Result;
            }
            else
            {
                //TODO: only those for which the user is allowed to see
                throw new NotImplementedException();
            }
        }
        #endregion

        #region GetOrganizationalUnits
        /// <summary>
        /// Gets the available organizational units.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>List of available units.</returns>
        public IReadOnlyList<OrganizationalUnit> GetOrganizationalUnits(User user)
        {
            //TODO: coorrect impl.
            if (user.Role.Equals(Role.SystemAdministrator) || true)
            {
                return GetOrganizationalUnitsAsync(MariaDbStatements.SelectAllOrganizationalUnits, new MySqlParameter[0]).Result.Values.ToList();
            }

            throw new NotImplementedException();
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
        /// <param name="unit">The unit.</param>
        /// <param name="mustChangePassword">A value indicating whether the user must change the pw.</param>
        /// <param name="valid">A value whether the user is valid or not.</param>
        /// <returns>The user object of the new user.</returns>
        public User AddUser(string userMail, string userName, string password, Role role, Guid unit, bool mustChangePassword, bool valid)
        {
            return AddUserAsync(userMail, userName, password, role, unit, mustChangePassword, valid).Result;
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

        #region SaveUserSettings (will be splitted up)
        /// <summary>
        /// Saves the user settings.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="toSave">The settings to save.</param>
        /// <returns>The saved user settings object.</returns>
        [Obsolete]
        public UserSettings SaveUserSettings(User user, UserSettings toSave)
        {
            return SaveUserSettings(toSave, user.Settings.Id).Result;
        }
        #endregion
        
        #region ResetPassword
        /// <summary>
        /// Reset the user password.
        /// </summary>
        /// <param name="userMail">The users mail address.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>The new random generated password</returns>
        public MethodResult ResetPassword(string userMail, string newPassword)
        {
            return ResetPasswordAsync(userMail, newPassword).Result;
        }
        #endregion

        #region ChangePassword
        /// <summary>
        /// Change the user password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>The new password which was set by the user.</returns>
        public MethodResult ChangePassword(User user, string newPassword)
        {
           return ResetPassword(user.MailAddress, newPassword);
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
            var lastResult = superSetFeedCache;
            if(lastResult != null)
            {
                if(DateTime.UtcNow < lastResult.Item1)
                {
                    return lastResult.Item2;
                }
            }

            lock (LockObj)
            {
                lastResult = superSetFeedCache;
                if (lastResult != null)
                {
                    if (DateTime.UtcNow < lastResult.Item1)
                    {
                        return lastResult.Item2;
                    }
                }

                var result = GetSuperSetFeedAsync().Result;
                superSetFeedCache = new Tuple<DateTime, Filter>(DateTime.UtcNow.AddSeconds(10), result);
                return result;
            }
        }
        #endregion

        ////Private methods

        #region GetUserSettingsAsync
        /// <summary>
        /// Gets the <see cref="UserSettings"/> by id.
        /// </summary>
        /// <param name="id">The id of the user settings.</param>
        /// <returns>The determined <see cref="UserSettings"/>.</returns>
        private static async Task<UserSettings> GetUserSettingsAsync(Guid id)
        {
            try
            {
                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    NotificationSetting notification;
                    int interval, articlesPerPage;
                    using (var cmd = new MySqlCommand(MariaDbStatements.SelectUserSettingsById, conn))
                    {
                        cmd.Parameters.AddWithValue("id", id.ToString());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            await reader.ReadAsync();
                            notification = reader.GetInt64(0).ConvertToNotification();
                            interval = reader.IsDBNull(1) ? 60 : reader.GetInt32(1);
                            articlesPerPage = reader.IsDBNull(2) ? 20 : reader.GetInt32(2);
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

                    return new UserSettings(id, feedList, notification, interval, articlesPerPage);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        #endregion

        #region GetOrganizationalUnitSettingsAsync
        /// <summary>
        /// Gets the <see cref="UserSettings"/> by id.
        /// </summary>
        /// <param name="id">The id of the user settings.</param>
        /// <returns>The determined <see cref="UserSettings"/>.</returns>
        private static async Task<OrganizationalUnitSettings> GetOrganizationalUnitSettingsAsync(Guid id)
        {
            var baseSettings = await GetUserSettingsAsync(id);

            //TODO: Determine available sources
            return new OrganizationalUnitSettings(id, baseSettings.Feeds, new List<Source>(), 
                                                  baseSettings.NotificationSettings,
                                                  baseSettings.NotificationCheckIntervalInMinutes);
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
                using (new PerfTracer(nameof(GetSourcesAsync)))
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
                                var valid = true || reader.GetBoolean(6); //TODO: enable later
                                var lastLogin = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7);
                                var userSettings = await GetUserSettingsAsync(settingsId);
                                result.Add(new User(id, mail, role, name, userSettings, mustChangePassword, lastLogin, valid));
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

        #region GetOrganizationalUnitsAsync
        /// <summary>
        /// Gets all units by executing the given command
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">The query parameters.</param>
        /// <returns>List of returned units.</returns>
        private static async Task<IReadOnlyDictionary<Guid, OrganizationalUnit>> GetOrganizationalUnitsAsync(string command, params MySqlParameter[] parameters)
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
                                var settings = await GetOrganizationalUnitSettingsAsync(settingsId);
                                result.Add(id, new OrganizationalUnit(id, name, parentId == SystemUnit, settings));
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

        #region AddUserAsync
        /// <summary>
        /// Adds a user.
        /// </summary>
        /// <param name="userMail">The user mail.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="role">The role.</param>
        /// <param name="unit">The unit.</param>
        /// <param name="mustChangePassword">A value indicating whether the user must change the pw.</param>
        /// <param name="valid">A value whether the user is valid or not.</param>
        /// <returns>The user object of the new user.</returns>
        private static async Task<User> AddUserAsync(string userMail, string userName, string password, Role role, Guid unit, bool mustChangePassword, bool valid)
        {
            try
            {
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
                        cmd.Parameters.AddWithValue("unit", unit.ToString());
                        cmd.Parameters.AddWithValue("mustChangePassword", mustChangePassword);
                        cmd.Parameters.AddWithValue("valid", valid);
                        cmd.Parameters.Add("newUserId".ToStoredProcedureOutParam(MySqlDbType.VarChar));
                        await cmd.ExecuteNonQueryAsync();

                        var newUserId = Guid.Parse(cmd.Parameters["newUserId"].Value.ToString());
                        var innerResult = await GetUsersAsync(MariaDbStatements.SelectUserById, new MySqlParameter("id", newUserId));
                        return innerResult.First();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
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

        #region SaveUserSettings (Will be splitted up)
        /// <summary>
        /// Saves the given user settings.
        /// </summary>
        /// <param name="toSave">The settings to save.</param>
        /// <param name="settingsId">The id of the settings (relict)</param>
        /// <returns>The saved user settings.</returns>
        [Obsolete]
        private static async Task<UserSettings> SaveUserSettings(UserSettings toSave, Guid settingsId)
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
                        cmd.Parameters.Add(new MySqlParameter("_interval", MySqlDbType.Int32) {Value = toSave.NotificationCheckIntervalInMinutes});
                        cmd.Parameters.Add(new MySqlParameter("notification", MySqlDbType.Bit) {Value = toSave.NotificationSettings.ToDatabaseNotification()});
                        cmd.Parameters.Add(new MySqlParameter("articlesPerPage", MySqlDbType.Int32) { Value = toSave.ArticlesPerPage == 0 ? (object)DBNull.Value : toSave.ArticlesPerPage });
                        await cmd.ExecuteNonQueryAsync();
                    }

                    var feeds = toSave.Feeds;
                    foreach (var feed in feeds)
                    {
                        Guid feedId;
                        Guid filterId;
                        using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "SP_UPDATE_FILTER";
                            cmd.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarChar)
                            {
                                Direction = ParameterDirection.InputOutput,
                                Value = feed.Filter.Id == Guid.Empty ? (object) DBNull.Value : feed.Filter.Id.ToString()
                            });
                            cmd.Parameters.Add(new MySqlParameter("keywords", MySqlDbType.Text) {Value = feed.Filter.Keywords.ToDatabaseList()});
                            cmd.Parameters.Add(new MySqlParameter("expressions", MySqlDbType.Text){Value = feed.Filter.Expressions.ToDatabaseList()});
                            cmd.Parameters.Add(new MySqlParameter("blacklist", MySqlDbType.Text){Value = feed.Filter.Blacklist.ToDatabaseList()});
                            await cmd.ExecuteNonQueryAsync();
                            filterId = Guid.Parse(cmd.Parameters["id"].Value.ToString());
                        }

                        using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "SP_UPDATE_FEED";
                            cmd.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarChar){Direction = ParameterDirection.InputOutput, Value = feed.Id == Guid.Empty ? (object) DBNull.Value : feed.Id.ToString() });
                            cmd.Parameters.Add(new MySqlParameter("name", MySqlDbType.Text) {Value = feed.Name});
                            cmd.Parameters.Add(new MySqlParameter("filterId", MySqlDbType.VarChar){Value = filterId.ToString()});
                            await cmd.ExecuteNonQueryAsync();
                            feedId = Guid.Parse(cmd.Parameters["id"].Value.ToString());
                        }

                        foreach (var source in feed.Sources)
                        {
                            using (var cmd = new MySqlCommand())
                            {
                                cmd.Connection = conn;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = "SP_LINK_SOURCE_FEED";
                                cmd.Parameters.Add(new MySqlParameter("feedId", MySqlDbType.VarChar){Value = feedId.ToString()});
                                cmd.Parameters.Add(new MySqlParameter("sourceId", MySqlDbType.VarChar){Value = source.Id.ToString()});
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "SP_LINK_SETTINGS_FEED";
                            cmd.Parameters.Add(new MySqlParameter("feedId", MySqlDbType.VarChar){Value = feedId.ToString()});
                            cmd.Parameters.Add(new MySqlParameter("settingsId", MySqlDbType.VarChar){Value = settingsId.ToString()});
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return await GetUserSettingsAsync(settingsId);
        }
        #endregion

        #region ResetPassword
        /// <summary>
        /// Reset the user password.
        /// </summary>
        /// <param name="userMail">The users mail address.</param>
        /// <param name="password">The new password to set.</param>
        /// <returns>The new random generated password</returns>
        private static async Task<MethodResult> ResetPasswordAsync(string userMail, string password)
        {
            using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(MariaDbSP.SP_RESET_USERPW.ToString(), conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("mail", userMail);
                    cmd.Parameters.AddWithValue("pwHash", password.GetSHA256());
                    cmd.Parameters.Add("success".ToStoredProcedureOutParam(MySqlDbType.Bool));
                    await cmd.ExecuteNonQueryAsync();

                    var newPassword = bool.Parse(cmd.Parameters["success"].Value.ToString());
                    if (newPassword)
                    {
                        return new MethodResult(SuccessState.Successful, "Password got reset.");
                    }
                    return new MethodResult(SuccessState.UnknownError, "Ooops. Something went wrong!");
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

                    return new Filter(Guid.Empty, superset, null, null);
                }
            }
        }
        #endregion
    }

}