using System;
using JackTheClipperData.MariaDb;
using JetBrains.Annotations;

namespace JackTheClipperData
{
    /// <summary>
    /// The factory for any database adapter
    /// </summary>
    public class DatabaseAdapterFactory
    {
        /// <summary>
        /// Gets the controller instance of any database controller.
        /// </summary>
        /// <typeparam name="T">Type of database controller</typeparam>
        /// <returns>Instance which implements T</returns>
        /// <exception cref="ArgumentOutOfRangeException">T not supported</exception>
        [NotNull, Pure]
        public static T GetControllerInstance<T>()
        {
            if (typeof(T) == typeof(IClipperDatabase))
            {
                return (T) (object) new MariaDbAdapter();
            }

            if (typeof(T) == typeof(IIndexerService))
            {
                return (T) (object) new ElasticController();
            }

            throw new ArgumentOutOfRangeException(nameof(T));
        }
    }
}