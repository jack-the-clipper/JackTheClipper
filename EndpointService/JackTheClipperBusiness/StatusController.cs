using System;
using System.Collections.Generic;
using System.Reflection;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;

namespace JackTheClipperBusiness
{
    /// <summary>
    /// Status controller, implements <see cref="IClipperService"/>
    /// </summary>
    public class StatusController : IClipperService
    {
        #region GetStatus
        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <returns>MethodResult indicating the status.</returns>
        public MethodResult GetStatus()
        {
            return new MethodResult(SuccessState.Successful, "Version: " + Assembly.GetCallingAssembly().GetName().Version);
        }
        #endregion

        #region GetPrincipalUnitBasicInformation
        /// <summary>
        /// Gets the principal units.
        /// </summary>
        /// <returns>List of principal units.</returns>
        public IReadOnlyList<Tuple<string, Guid>> GetPrincipalUnitBasicInformation()
        {
            return DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().GetPrincipalUnitBasicInformation();
        }
        #endregion

        #region GetPrincipalUnitChildren
        /// <summary>
        /// Gets the children of a principal unit.
        /// </summary>
        /// <param name="principalUnitId">The principal unit identifier.</param>
        /// <returns>List of children of given principal unit.</returns>
        public IReadOnlyList<Tuple<string, Guid>> GetPrincipalUnitChildren(Guid principalUnitId)
        {
            return DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().GetPrincipalUnitChildren(principalUnitId);
        }
        #endregion
    }
}