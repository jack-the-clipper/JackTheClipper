using System;
using System.Collections;
using System.Collections.Generic;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.SharedClasses;
using JetBrains.Annotations;

namespace JackTheClipperCommon.Extensions
{
    /// <summary>
    /// Class for multiple Extension Methods, which have no specified Class, to which they extend
    /// </summary>
    public static class GeneralExtensions
    {
        /// <summary>
        /// Determines whether the <see cref="MethodResult"/> is succeeded.
        /// </summary>
        /// <param name="result">The method result to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified result is succeeded; otherwise, <c>false</c>.
        /// </returns>
        [ContractAnnotation("=> true, result: notnull;")]
        public static bool IsSucceeded(this MethodResult result)
        {
            return result != null && result.Status == SuccessState.Successful;
        }

        /// <summary>
        /// Tries the get an item by the given key or adds it if no item is present or null.
        /// </summary>
        /// <typeparam name="K">Type of key.</typeparam>
        /// <typeparam name="T">Tye of value</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns>The obtained object.</returns>
        /// <exception cref="ArgumentNullException">dict is null.</exception>
        public static T TryGetOrAddIfNotPresentOrNull<K, T>(this IDictionary<K, T> dict, K key) where T : new()
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            T temp;
            if (!dict.TryGetValue(key, out temp) || temp == null)
            {
                dict[key] = temp = new T();
            }

            return temp;
        }
    }
}