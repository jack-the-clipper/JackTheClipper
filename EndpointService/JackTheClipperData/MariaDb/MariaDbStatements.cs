namespace JackTheClipperData.MariaDb
{
    /// <summary>
    /// The (data retrieving) sql statements for maria db.
    /// </summary>
    internal static class MariaDbStatements
    {
        public const string SelectAllUsers = "SELECT BIN_TO_UUID(a.coId), a.coName, a.coMail, a.coRole, BIN_TO_UUID(a.coSettingsId), a.coMustChangePassword, a.coValid, " +
                                             "(SELECT tbuserlogins.coLoginTime FROM tbuserlogins WHERE tbuserlogins.coUserId = a.coId ORDER BY tbuserlogins.coLoginTime DESC LIMIT 1,1), " +
                                             "BIN_TO_UUID(a.coPrincipalUnit), (SELECT tborganizationalunit.coName FROM tborganizationalunit WHERE tborganizationalunit.coId = a.coPrincipalUnit) " +
                                             "FROM tbuser a ";
        public const string SelectUserByCredentials = "SELECT BIN_TO_UUID(a.coId), a.coName, a.coMail, a.coRole, BIN_TO_UUID(a.coSettingsId), a.coMustChangePassword, a.coValid, " +
                                                      "(SELECT MAX(tbuserlogins.coLoginTime) FROM tbuserlogins WHERE tbuserlogins.coUserId = a.coId), " +
                                                      "BIN_TO_UUID(a.coPrincipalUnit), (SELECT tborganizationalunit.coName FROM tborganizationalunit WHERE tborganizationalunit.coId = a.coPrincipalUnit) " +
                                                      "FROM tbuser a WHERE a.coPasswordHash = @hash AND a.coPrincipalUnit = UUID_TO_BIN(@unit) AND (a.coMail = @mail OR a.coName = @mail) LIMIT 1;";
        public const string SelectUserById = SelectAllUsers + "WHERE a.coId = UUID_TO_BIN(@id) LIMIT 1;";
        public const string SelectUserByMail = SelectAllUsers + "WHERE a.coMail = @mail LIMIT 1;";
        public const string SelectUsersOfPrincipalUnit = SelectAllUsers + "WHERE a.coPrincipalUnit = UUID_TO_BIN(@unit);";
        public const string SelectNotifiableUsers = SelectAllUsers + ", tbusersettings b " +
                                                    "WHERE a.coSettingsId = b.coId AND b.coNotification > 0;";

        public const string SelectStaffChiefsOfPrincipalUnit = SelectAllUsers +
                                                              " WHERE a.coRole = b'0111' AND a.coPrincipalUnit = UUID_TO_BIN(@principalunit)";

        public const string SelectBasicUserInfoForUsersInUnits = "SELECT DISTINCT a.coName, BIN_TO_UUID(a.coId), a.coValid FROM tbuser a, tbuserorganizationalunit b " +
                                                                 "WHERE a.coId = b.coUserId AND b.coOrganizationalUnitId IN({0});";

        public const string SelectAllSources = "SELECT BIN_TO_UUID(coId), coUrl, coName, coType, coRegex, coXpath, coBlackList FROM tbSource;";
        public const string SelectSpecificSourceById = "SELECT BIN_TO_UUID(coId), coUrl, coName, coType, coRegex, coXpath, coBlackList FROM tbSource WHERE coID = UUID_TO_BIN(@id) LIMIT 1;";
        public const string SelectFeedSources = "SELECT BIN_TO_UUID(b.coId), b.coUrl, b.coName, b.coType, b.coRegex, b.coXpath, b.coBlackList " +
                                                "FROM tbfeedsource a, tbsource b " +
                                                "WHERE a.coSourceId = b.coId " +
                                                "AND a.coFeedId = UUID_TO_BIN(@feedid);";
        public const string SelectUnitSources = "SELECT BIN_TO_UUID(b.coId), b.coUrl, b.coName, b.coType, b.coRegex, b.coXpath, b.coBlackList " +
                                                "FROM tborganizationalunitsettingssource a, tbsource b " +
                                                "WHERE a.coSourceId = b.coId AND a.coOrganizationalUnitSettingsId = UUID_TO_BIN(@unitSettingsId);";

        public const string SelectUserOrganizationalUnitIds = "SELECT BIN_TO_UUID(coOrganizationalUnitId) FROM tbuserorganizationalunit WHERE coUserId = UUID_TO_BIN(@userId);";
        public const string SelectAllOrganizationalUnits = "SELECT BIN_TO_UUID(coId), coName, BIN_TO_UUID(coParentId), BIN_TO_UUID(coSettingsId), coMail FROM tbOrganizationalUnit";
        public const string SelectOrganizationalUnitById = SelectAllOrganizationalUnits + " WHERE coId = UUID_TO_BIN(@id)";
        public const string SelectInheritedBlacklists = "SELECT c.coBlacklist FROM tbuserorganizationalunit a, tborganizationalunit b, "+
                                                        "tborganizationalunitsettings c\r\nWHERE a.coUserId = UUID_TO_BIN(@userId) "+
                                                        "AND a.coOrganizationalUnitId = b.coId AND b.coSettingsId = c.coUserSettingsId;";

        public const string SelectPrincipalUnits = "SELECT coName, BIN_TO_UUID(coId) FROM tbOrganizationalUnit " +
                                                   "WHERE coId = 0x00000000BEEFBEEFBEEF000000000000 " +
                                                   "OR coParentId = 0x00000000BEEFBEEFBEEF000000000000;";

        public const string SelectUserSettingsFeeds = "SELECT BIN_TO_UUID(a.coFeedId), b.coName, BIN_TO_UUID(b.coFilterId), c.coBlackList, c.coKeywords, c.coExpressions " +
                                                      "FROM tbsettingsfeeds a, tbfeed b, tbfeedfilter c " +
                                                      "WHERE a.coFeedId = b.coId " +
                                                      "AND b.coFilterId = c.coId " +
                                                      "AND a.coSettingsId = UUID_TO_BIN(@settingsId);";

        public const string SelectUserSettingsById = "SELECT BIN_TO_UUID(coId), coNotification, coNotificationInterval, coArticlesPerPage " +
                                                     "FROM tbUserSettings WHERE coid = UUID_TO_BIN(@id) LIMIT 1;";
        public const string SelectUserSettingsByUserId ="SELECT BIN_TO_UUID(coId), coNotification, coNotificationInterval, coArticlesPerPage " +
                                                        "FROM tbUserSettings WHERE coid = (SELECT tbuser.coSettingsId FROM tbuser WHERE tbuser.coId = UUID_TO_BIN(@userId)) LIMIT 1;";
        public const string SelectUnitSettingsByUnitId = "SELECT BIN_TO_UUID(a.coId), a.coNotification, a.coNotificationInterval, a.coArticlesPerPage, " +
                                                         "(SELECT b.coBlacklist FROM tborganizationalunitsettings b WHERE b.coUserSettingsId = a.coId) " +
                                                         "FROM tbUserSettings a, tbOrganizationalUnit c " +
                                                         "WHERE a.coId = c.coSettingsId and c.coId = UUID_TO_BIN(@id);";

        public const string SelectSuperSetFeed = "SELECT tbfeedfilter.coKeywords, tbfeedfilter.coBlackList " +
                                                 "FROM tbfeedfilter";

        public const string SelectFeedRequestData = "SELECT b.coName, BIN_TO_UUID(b.coFilterId), c.coBlackList, c.coKeywords, c.coExpressions, " +
                                                    "(SELECT tbuserlogins.coLoginTime FROM tbuserlogins WHERE tbuserlogins.coUserId = UUID_TO_BIN(@userId) ORDER BY tbuserlogins.coLoginTime DESC LIMIT 1,1), " +
                                                    "(SELECT us.coArticlesPerPage FROM tbUserSettings us WHERE us.coId = (SELECT u.coSettingsId FROM tbuser u WHERE u.coId = UUID_TO_BIN(@userId)))" +
                                                    "FROM tbsettingsfeeds a, tbfeed b, tbfeedfilter c WHERE a.coFeedId = b.coId AND b.coFilterId = c.coId AND a.coFeedId = UUID_TO_BIN(@feedId);";
    }
}
