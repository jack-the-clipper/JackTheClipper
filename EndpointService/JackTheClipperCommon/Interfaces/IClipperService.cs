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
    }
}