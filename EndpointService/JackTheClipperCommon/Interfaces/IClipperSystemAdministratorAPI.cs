using System;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperCommon.Interfaces
{
    /// <summary>
    /// Interface of the system administrator api
    /// </summary>
    public interface IClipperSystemAdministratorAPI
    {
        /// <summary>
        /// Adds the given source.
        /// </summary>
        /// <param name="user">The user who requests the addition.</param>
        /// <param name="toAdd">The source to add.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult AddSource(User user, Source toAdd);

        /// <summary>
        /// Deletes the given source.
        /// </summary>
        /// <param name="user">The user who requests the deletion.</param>
        /// <param name="toAdd">The source to add.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult DeleteSource(User user, Guid toAdd);


        /// <summary>
        /// Changes the source.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="toChange">Id of the source, which should be changed</param>
        /// <param name="newSource">The new source.</param>
        /// <returns>MethodResult indicating success</returns>
        MethodResult ChangeSource(User user, Guid toChange, Source newSource);
    }
}
