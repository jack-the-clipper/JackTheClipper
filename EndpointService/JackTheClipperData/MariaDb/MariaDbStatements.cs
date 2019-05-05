namespace JackTheClipperData.MariaDb
{
    /// <summary>
    /// The (data retrieving) sql statements for maria db.
    /// </summary>
    internal static class MariaDbStatements
    {
        public const string SelectAllUsers = "SELECT BIN_TO_UUID(a.coId), a.coName, a.coMail, a.coRole, BIN_TO_UUID(a.coSettingsId), a.coMustChangePassword, a.coValid, " +
                                             "(SELECT tbuserlogins.coLoginTime FROM tbuserlogins WHERE tbuserlogins.coUserId = a.coId ORDER BY tbuserlogins.coLoginTime DESC LIMIT 1,1) " +
                                             "FROM tbuser a;";
        public const string SelectUserByCredentials = "SELECT BIN_TO_UUID(a.coId), a.coName, a.coMail, a.coRole, BIN_TO_UUID(a.coSettingsId), a.coMustChangePassword, a.coValid, " +
                                                      "(SELECT MAX(tbuserlogins.coLoginTime) FROM tbuserlogins WHERE tbuserlogins.coUserId = a.coId) " +
                                                      "FROM tbuser a WHERE a.coPasswordHash = @hash AND a.coMail = @mail LIMIT 1;";
        public const string SelectUserById = "SELECT BIN_TO_UUID(a.coId), a.coName, a.coMail, a.coRole, BIN_TO_UUID(a.coSettingsId), a.coMustChangePassword, a.coValid, " +
                                             "(SELECT tbuserlogins.coLoginTime FROM tbuserlogins WHERE tbuserlogins.coUserId = a.coId ORDER BY tbuserlogins.coLoginTime DESC LIMIT 1,1) " +
                                             "FROM tbuser a WHERE a.coId = UUID_TO_BIN(@id) LIMIT 1;";

        public const string SelectAllSources = "SELECT BIN_TO_UUID(coId), coUrl, coName, coType, coRegex, coXpath, coBlackList FROM tbSource;";
        public const string SelectSpecificSourceById = "SELECT BIN_TO_UUID(coId), coUrl, coName, coType, coRegex, coXpath, coBlackList FROM tbSource WHERE coID = UUID_TO_BIN(@id) LIMIT 1;";
        public const string SelectFeedSources = "SELECT BIN_TO_UUID(b.coId), b.coUrl, b.coName, b.coType, b.coRegex, b.coXpath, b.coBlackList " +
                                                "FROM tbfeedsource a, tbsource b " +
                                                "WHERE a.coSourceId = b.coId " +
                                                "AND a.coFeedId = UUID_TO_BIN(@feedid);";

        public const string SelectAllOrganizationalUnits = "SELECT BIN_TO_UUID(coId), coName, BIN_TO_UUID(coParentId), BIN_TO_UUID(coSettingsId) FROM tbOrganizationalUnit";
        public const string SelectOrganizationalUnitById = SelectAllOrganizationalUnits + " WHERE coId = UUID_TO_BIN(@id)";

        public const string SelectUserSettingsFeeds = "SELECT BIN_TO_UUID(a.coFeedId), b.coName, BIN_TO_UUID(b.coFilterId), c.coBlackList, c.coKeywords, c.coExpressions " +
                                                      "FROM tbsettingsfeeds a, tbfeed b, tbfeedfilter c " +
                                                      "WHERE a.coFeedId = b.coId " +
                                                      "AND b.coFilterId = c.coId " +
                                                      "AND a.coSettingsId = UUID_TO_BIN(@settingsId);";

        public const string SelectUserSettingsById = "SELECT coNotification, coNotificationInterval, coNotificationInterval " +
                                                     "FROM tbUserSettings WHERE coid = UUID_TO_BIN(@id) LIMIT 1;";

        public const string SelectSuperSetFeed = "SELECT tbfeedfilter.coKeywords, tbfeedfilter.coBlackList " +
                                                 "FROM tbfeedfilter";
    }
}
