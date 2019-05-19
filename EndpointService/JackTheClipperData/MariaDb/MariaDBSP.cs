namespace JackTheClipperData.MariaDb
{
    /// <summary>
    /// Enumeration of stored procedures within maria db.
    /// </summary>
    public enum MariaDbSP
    {
        /// <summary>
        /// SP_CREATE_USER(name, mail, pwHash, role, principalUnit, OUT newUserId)
        /// </summary>
        SP_CREATE_USER,

        /// <summary>
        /// SP_UPDATE_SOURCE(INOUT id,	url, name, type, regex, xpath, blacklist)
        /// </summary>
        SP_UPDATE_SOURCE,

        /// <summary>
        /// SP_LOG_USERLOGIN(userId)
        /// </summary>
        SP_LOG_USERLOGIN,

        /// <summary>
        /// SP_UPDATE_USERSETTINGS(id, _interval, notification, articlesPerPage)
        /// </summary>
        SP_UPDATE_USERSETTINGS,

        /// <summary>
        /// SP_DEL_SOURCE(id)
        /// </summary>
        SP_DEL_SOURCE,

        /// <summary>
        /// SP_DEL_USER(id)
        /// </summary>
        SP_DEL_USER,

        /// <summary>
        /// SP_UPDATE_FILTER(id, keywords, expressions, blacklist)
        /// </summary>
        SP_UPDATE_FILTER,

        /// <summary>
        /// SP_UPDATE_FEED(id, name, filterId)
        /// </summary>
        SP_UPDATE_FEED,

        /// <summary>
        /// SP_LINK_SOURCE_FEED(feedId, sourceId)
        /// </summary>
        SP_LINK_SOURCE_FEED,

        /// <summary>
        /// SP_LINK_SETTINGS_FEED(feedId, settingsId)
        /// </summary>
        SP_LINK_SETTINGS_FEED,

        /// <summary>
        /// SP_CHANGE_MAILADDRESS(userId, newMail, out success)
        /// </summary>
        SP_CHANGE_MAILADDRESS,

        /// <summary>
        /// SP_CHANGE_USERPW(mail, pwHash, mustChangePassword, out success)
        /// </summary>
        SP_CHANGE_USERPW,

        /// <summary>
        /// SP_CREATE_UNIT(name, parent, OUT newUnitId
        /// </summary>
        SP_CREATE_UNIT,

        /// <summary>
        /// SP_ADD_USER_UNIT(IN userId VARCHAR(36), IN unitId VARCHAR(36))
        /// </summary>
        SP_ADD_USER_UNIT,

        /// <summary>
        /// SP_REMOVE_USER_UNIT(IN userId VARCHAR(36), IN unitId VARCHAR(36))
        /// </summary>
        SP_REMOVE_USER_UNIT,

        /// <summary>
        /// SP_CREATE_PRINZIPALUNIT(IN name TEXT, IN mail VARCHAR(191), IN adminPwHash TEXT,
        ///                         OUT newPrincipalUnitId VARCHAR(36),
        ///                         OUT newUserId VARCHAR(36))
        /// </summary>
        SP_CREATE_PRINZIPALUNIT,

        /// <summary>
        /// SP_UPDATE_UNIT(IN unitId VARCHAR(36), IN name TEXT)
        /// </summary>
        SP_UPDATE_UNIT,

        /// <summary>
        /// SP_DELETE_UNIT(IN unitId VARCHAR(36))
        /// </summary>
        SP_DELETE_UNIT,

        /// <summary>
        /// SP_SET_UNIT_BLACKLIST(IN settingsId VARCHAR(36), IN blackList TEXT)
        /// </summary>
        SP_SET_UNIT_BLACKLIST,

        /// <summary>
        /// SP_ADD_UNIT_SOURCE(IN settingsId VARCHAR(36), IN sourceId VARCHAR(36))
        /// </summary>
        SP_ADD_UNIT_SOURCE,

        /// <summary>
        /// SP_REMOVE_UNIT_SOURCE(IN settingsId VARCHAR(36), IN sourceId VARCHAR(36))
        /// </summary>
        SP_REMOVE_UNIT_SOURCE,

        /// <summary>
        /// SP_DELETE_FEED(IN feedId VARCHAR(36))
        /// </summary>
        SP_DELETE_FEED,

        /// <summary>
        /// SP_REMOVE_SOURCE_FEED(IN feedId VARCHAR(36), IN sourceId VARCHAR(36))
        /// </summary>
        SP_REMOVE_SOURCE_FEED,

        /// <summary>
        /// SP_UPDATE_USER_AND_CLEAR_USERUNITS(IN userId VARCHAR(36), IN name TEXT, IN role BIT(4), IN valid BOOL)
        /// </summary>
        SP_UPDATE_USER_AND_CLEAR_USERUNITS
    }
}