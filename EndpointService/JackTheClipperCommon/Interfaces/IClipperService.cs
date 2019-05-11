using System;
using System.Collections.Generic;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperCommon.Interfaces
{
    /// <summary>
    /// Interface for basic clipper services
    /// </summary>
    public interface IClipperService
    {
        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <returns>MethodResult indicating the status.</returns>
        MethodResult GetStatus();

        /// <summary>
        /// Gets the principal units.
        /// </summary>
        /// <returns>List of principal units.</returns>
        IReadOnlyList<Tuple<string, Guid>> GetPrincipalUnits();
    }
}