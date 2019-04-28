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
    }
}