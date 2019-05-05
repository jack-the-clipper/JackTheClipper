using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace JackTheClipperCommon.BusinessObjects
{
    /// <summary>
    /// A key which uniquely identifies an rss feed item within a specific source.
    /// </summary>
    /// <seealso cref="System.IEquatable{RssKey}" />
    [DataContract]
    public class RssKey : IEquatable<RssKey>
    {
        /// <summary>
        /// Gets the updated time.
        /// </summary>
        [DataMember(Name = "Updated")]
        public long Updated { get; private set; }

        /// <summary>
        /// Gets the link.
        /// </summary>
        [DataMember(Name = "Link")]
        [NotNull]
        public string Link { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RssKey"/> class.
        /// </summary>
        protected RssKey()
        {
            //For serialization.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RssKey"/> class.
        /// </summary>
        /// <param name="updated">The updated time.</param>
        /// <param name="link">The link.</param>
        /// <exception cref="ArgumentNullException">link is null</exception>
        public RssKey(long updated, string link)
        {
            Updated = updated;
            Link = link ?? throw new ArgumentNullException(nameof(link));
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(RssKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Updated == other.Updated && string.Equals(Link, other.Link);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is RssKey other && Equals(other);
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Updated.GetHashCode() * 397) ^ Link.GetHashCode();
            }
        }
    }
}