using System;
using System.Diagnostics;
using JackTheClipperBusiness.CrawlerManagement;
using JackTheClipperBusiness.Notification;
using JackTheClipperBusiness.OrganizationalUnitManagement;
using JackTheClipperBusiness.UserManagement;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;
using JetBrains.Annotations;

namespace JackTheClipperBusiness
{
    /// <summary>
    /// Factory to get different Types of Objects 
    /// </summary>
    public static class Factory
    {
        #region GetControllerInstance
        /// <summary>
        /// Returns an instance of the requested interface.
        /// </summary>
        /// <typeparam name="T">The type of the requested interface.</typeparam>
        /// <returns>An instance of the requested interface.</returns>
        [NotNull, Pure, DebuggerStepThrough]
        public static T GetControllerInstance<T>()
        {
            if (typeof(T) == typeof(IClipperUserAPI) || 
                typeof(IClipperSystemAdministratorAPI) == typeof(T) || 
                typeof(IClipperStaffChiefAPI) == typeof(T))
            {
                return (T)(object)new UserController();
            }

            if (typeof(T) == typeof(IClipperDatabase))
            {
                return (T)DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>();
            }

            if (typeof(T) == typeof(IClipperService))
            {
                return (T)(object)new StatusController();
            }

            if (typeof(T) == typeof(IClipperOrganizationalUnitAPI))
            {
                return (T)(object)new OrganizationalUnitController();
            }

            if (typeof(T) == typeof(ICrawlerController))
            {
                return (T) (object) CrawlerController.GetCrawlerController();
            }

            if (typeof(T) == typeof(INotificationController))
            {
                return (T)(object)new NotificationControllerWrapper();
            }

            throw new ArgumentOutOfRangeException(nameof(T));
        }
        #endregion

        #region GetObjectInstanceById
        /// <summary>
        /// Gets the corresponding object by its id.
        /// </summary>
        /// <typeparam name="T">The type of the requested object.</typeparam>
        /// <param name="id">The id of the requested object.</param>
        /// <returns>The requested object (if exists).</returns>
        public static T GetObjectInstanceById<T>(Guid id) where T : class
        {
            var adapter = GetControllerInstance<IClipperDatabase>();
            object result = null;
            if (typeof(T) == typeof(User))
            {
                result = adapter.GetUserById(id);
            }

            if (typeof(T) == typeof(OrganizationalUnit))
            {
                result = adapter.GetOrganizationalUnitById(id);
            }


            return result as T;
        }
        #endregion
    }
}