namespace JackTheClipperData.MariaDb
{
    /// <summary>
    /// The (data retrieving) sql statements for maria db.
    /// </summary>
    internal static class MariaDbStatements
    {
        public const string SelectAllUsers = "select BIN_TO_UUID(coId), coName, coMail, coRole, BIN_TO_UUID(coSettingsId) from tbuser;";
        public const string SelectUserByCredentials = "select BIN_TO_UUID(coId), coName, coMail, coRole, BIN_TO_UUID(coSettingsId) from tbuser where tbuser.coPasswordHash = @hash and tbuser.coMail = @mail LIMIT 1;";
        public const string SelectUserById = "select BIN_TO_UUID(coId), coName, coMail, coRole, BIN_TO_UUID(coSettingsId) from tbuser where tbuser.coId = UUID_TO_BIN(@id) LIMIT 1;";

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

        public const string SelectUserSettingsById = "SELECT coNotification, coNotificationInterval " +
                                                     "FROM tbUserSettings WHERE coid = UUID_TO_BIN(@id) LIMIT 1;";
    }
}
