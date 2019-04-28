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
        /// <returns>The user (if found)</returns>
        public User GetUserByCredentials(string mail, string password)
        {
            var result = GetUsersAsync(MariaDbStatements.SelectUserByCredentials,
                                  new MySqlParameter("hash", password.GetSHA256()),
                                  new MySqlParameter("mail", mail)).Result;
            return result.First();
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
        /// <returns>The user object of the new user.</returns>
        public User AddUser(string userMail, string userName, string password, Role role, Guid unit)
        {
            return AddUserAsync(userMail, userName, password, role, unit).Result;
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
                    int interval;
                    using (var cmd = new MySqlCommand(MariaDbStatements.SelectUserSettingsById, conn))
                    {
                        cmd.Parameters.AddWithValue("id", id.ToString());
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            await reader.ReadAsync();
                            notification = reader.GetInt64(0).ConvertToNotification();
                            interval = reader.IsDBNull(1) ? 60 : reader.GetInt32(1);
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

                    return new UserSettings(id, feedList, notification, interval);
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
        /// <param name="parameters">The query prameters.</param>
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
                                var userSettings = await GetUserSettingsAsync(settingsId);
                                result.Add(new User(id, mail, role, name, userSettings));
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
        /// <returns>The user object of the new user.</returns>
        private static async Task<User> AddUserAsync(string userMail, string userName, string password, Role role, Guid unit)
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
                if (source.Id != Guid.Empty)
                {
                    throw new InvalidOperationException(nameof(source));
                }

                using (var conn = new MySqlConnection(AppConfiguration.SqlServerConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(MariaDbSP.SP_UPDATE_SOURCE.ToString(), conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarChar) { Direction = ParameterDirection.InputOutput, Value = DBNull.Value });
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
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "call SP_UPDATE_USERSETTINGS(@id, @_interval, @notification);";
                        cmd.Parameters.Add(new MySqlParameter("id", MySqlDbType.VarChar) {Value = settingsId.ToString()});
                        cmd.Parameters.Add(new MySqlParameter("_interval", MySqlDbType.Int32) {Value = toSave.NotificationCheckIntervalInMinutes});
                        cmd.Parameters.Add(new MySqlParameter("notification", MySqlDbType.Bit) {Value = toSave.NotificationSettings.ToDatabaseNotification()});
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
    }
}