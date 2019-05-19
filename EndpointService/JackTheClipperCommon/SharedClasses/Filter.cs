using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// Contains Keywords or Expressions, which should be filtered
    /// </summary>
    [DataContract]
    public class Filter
    {
        /// <summary>
        /// Gets the identifier of the filter.
        /// </summary>
        [DataMember(Name = "FilterId")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the filter keywords.
        /// </summary>
        [DataMember(Name = "FilterKeywords")]
        public IReadOnlyCollection<string> Keywords { get; private set; }

        /// <summary>
        /// Gets the filter expressions.
        /// </summary>
        [DataMember(Name = "FilterExpressions")]
        public IReadOnlyCollection<string> Expressions { get; private set; }

        /// <summary>
        /// Gets the filter blacklist.
        /// </summary>
        [DataMember(Name = "FilterBlacklist")]
        public IReadOnlyCollection<string> Blacklist { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="Filter"/> class from being created.
        /// </summary>
        private Filter()
        {
            //For serialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Filter"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="expressions">The expressions.</param>
        /// <param name="blacklist">The blacklist.</param>
        public Filter(Guid id, IReadOnlyCollection<string> keywords, IReadOnlyCollection<string> expressions,
                      IReadOnlyCollection<string> blacklist)
        {
            Id = id;
            Keywords = keywords;
            Expressions = expressions;
            Blacklist = blacklist;
        }

        #region Equals
        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="other">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public bool Equals(Filter other)
        {
            return Id.Equals(other.Id) &&
                   (Keywords ?? new List<string>()).SequenceEqual(other.Keywords ?? new List<string>()) &&
                   (Expressions ?? new List<string>()).SequenceEqual(other.Expressions ?? new List<string>()) &&
                   (Blacklist ?? new List<string>()).SequenceEqual(other.Blacklist ?? new List<string>());
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Filter)obj);
        }
        #endregion

        #region GetHashCode
        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (Keywords == null ? 0 : Keywords.GetHashCode());
                hashCode = (hashCode * 397) ^ (Expressions == null ? 0 : Expressions.GetHashCode());
                hashCode = (hashCode * 397) ^ (Blacklist == null ? 0 : Blacklist.GetHashCode());
                return hashCode;
            }
        }
        #endregion
    }
}