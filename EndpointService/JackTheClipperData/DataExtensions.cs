using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using JackTheClipperCommon.Enums;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace JackTheClipperData
{
    /// <summary>
    /// Contains extension methods of the data package
    /// </summary>
    internal static class DataExtensions
    {
        /// <summary>
        /// Gets the SHA256-hash of the given string.
        /// </summary>
        /// <param name="toHash">The string to hash.</param>
        /// <returns>The hash of the given string.</returns>
        /// <exception cref="System.ArgumentNullException">toHash was null</exception>
        public static string GetSHA256(this string toHash)
        {
            if (toHash == null)
            {
                throw new ArgumentNullException(nameof(toHash));
            }

            using (var crypt = new System.Security.Cryptography.SHA256Managed())
            {
                var hash = crypt.ComputeHash(Encoding.UTF8.GetBytes(toHash));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        /// <summary>
        /// Converts the given database value to a <see cref="Role"/>.
        /// </summary>
        /// <param name="toConvert">The database value to convert.</param>
        /// <returns>The determined <see cref="Role"/>.</returns>
        public static Role ConvertToRole(this long toConvert)
        {
            switch (toConvert)
            {
                case 15:
                    return Role.SystemAdministrator;
                case 7:
                    return Role.StaffChief;
                default:
                    return Role.User;
            }
        }

        /// <summary>
        /// Converts a given <see cref="Role"/> to the corresponding database value.
        /// </summary>
        /// <param name="toConvert">The <see cref="Role"/> to convert.</param>
        /// <returns>The corresponding database value</returns>
        public static byte ToDatabaseRole(this Role toConvert)
        {
            switch (toConvert)
            {
                case Role.SystemAdministrator:
                    return 15;
                case Role.StaffChief:
                    return 7;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Converts a given <see cref="ContentType"/> to the corresponding database value.
        /// </summary>
        /// <param name="toConvert">The <see cref="ContentType"/> to convert.</param>
        /// <returns>The corresponding database value</returns>
        public static byte ToDatabaseContentType(this ContentType toConvert)
        {
            switch (toConvert)
            {
                case ContentType.Rss:
                    return 1;
                case ContentType.WebSite:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Converts the given database value to a <see cref="ContentType"/>.
        /// </summary>
        /// <param name="toConvert">The database value to convert.</param>
        /// <returns>The determined <see cref="ContentType"/>.</returns>
        public static ContentType ConvertToContent(this long toConvert)
        {
            switch(toConvert)
            {
                case 1:
                    return ContentType.Rss;
                case 2:
                    return ContentType.WebSite;
                default:
                    return ContentType.Undefined;
            }
        }

        /// <summary>
        /// Converts a given <see cref="NotificationSetting"/> to the corresponding database value.
        /// </summary>
        /// <param name="toConvert">The <see cref="NotificationSetting"/> to convert.</param>
        /// <returns>The corresponding database value</returns>
        public static long ToDatabaseNotification(this NotificationSetting toConvert)
        {
            switch (toConvert)
            {
                case NotificationSetting.PdfPerMail:
                    return 2;
                case NotificationSetting.LinkPerMail:
                    return 1;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Converts the given database value to a <see cref="NotificationSetting"/>.
        /// </summary>
        /// <param name="toConvert">The database value to convert.</param>
        /// <returns>The determined <see cref="NotificationSetting"/>.</returns>
        public static NotificationSetting ConvertToNotification(this long toConvert)
        {
            switch (toConvert)
            {
                case 2:
                    return NotificationSetting.PdfPerMail;
                case 1:
                    return NotificationSetting.LinkPerMail;
                default:
                    return NotificationSetting.None;
            }
        }

        /// <summary>
        /// Converts a given <see cref="IEnumerable{String}"/> to the corresponding database value.
        /// </summary>
        /// <param name="toConvert">The <see cref="IEnumerable{String}"/> to convert.</param>
        /// <returns>The corresponding database value</returns>
        public static string ToDatabaseList(this IEnumerable<string> toConvert)
        {
            return toConvert != null && toConvert.Any() ? JsonConvert.SerializeObject(toConvert) : null;
        }

        /// <summary>
        /// Converts the given database value to an <see cref="IReadOnlyList{String}"/>.
        /// </summary>
        /// <param name="toConvert">The database value to convert.</param>
        /// <returns>The determined <see cref="IReadOnlyList{String}"/>.</returns>
        public static IReadOnlyList<string> ConvertToStringList(this string toConvert)
        {
            return toConvert == null ? new List<string>() : JsonConvert.DeserializeObject<IReadOnlyList<string>>(toConvert);
        }

        /// <summary>
        /// Gets the string of nullable database field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="ordinal">The ordinal index of the column.</param>
        /// <returns>The determined string.</returns>
        public static string GetStringFromNullable(this DbDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        /// <summary>
        /// Appends the parameters.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        public static void AppendParameters(this MySqlCommand command, MySqlParameter[] parameters)
        {
            if (parameters != null)
            {
                Array.ForEach(parameters, x => command.Parameters.Add(x));
            }
        }

        /// <summary>
        /// Converts the given parameters to a stored procedure out param.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns>The generated out param.</returns>
        public static MySqlParameter ToStoredProcedureOutParam(this string name, MySqlDbType type)
        {
            return new MySqlParameter(name, type) {Direction = ParameterDirection.Output};
        }
    }
}