namespace JackTheClipperData.MariaDb
{
    /// <summary>
    /// Enumeration of stored procedures within maria db.
    /// </summary>
    public enum MariaDbSP
    {
        /// <summary>
        /// SP_CREATE_USER(name, mail, pwHash, role, unit, OUT newUserId)
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
        /// SP_CHANGE_MAILADDRESS(userId, newMail, out success)
        /// </summary>
        SP_CHANGE_MAILADDRESS,

        /// <summary>
        /// SP_RESET_USERPW(mail, pwHash, out success)
        /// </summary>
        SP_RESET_USERPW
    }
}