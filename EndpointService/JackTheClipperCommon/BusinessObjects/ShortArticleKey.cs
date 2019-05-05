using System;
using System.Runtime.Serialization;

namespace JackTheClipperCommon.BusinessObjects
{
    /// <summary>
    /// A key which uniquely identifies an rss feed item within the system.
    /// </summary>
    /// <seealso cref="JackTheClipperCommon.BusinessObjects.RssKey" />
    [DataContract]
    public class ShortArticleKey : RssKey
    {
        /// <summary>
        /// Gets the id of the belonging source.
        /// </summary>
        [DataMember(Name = "SourceId")]
        public Guid SourceId { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="ShortArticleKey"/> class from being created.
        /// </summary>
        private ShortArticleKey()
        {
            //For serialization.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortArticleKey"/> class.
        /// </summary>
        /// <param name="sourceId">The source identifier.</param>
        /// <param name="updated">The updated time.</param>
        /// <param name="link">The link.</param>
        public ShortArticleKey(Guid sourceId, long updated, string link) : base(updated, link)
        {
            SourceId = sourceId;
        }
    }
}