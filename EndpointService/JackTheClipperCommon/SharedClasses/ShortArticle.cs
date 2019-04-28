using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// Summarized Article
    /// Contains a Guid, a Title, a summary, link and when it was published and indexed
    /// </summary>
    [DataContract]
    public class ShortArticle
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        [DataMember(Name = "ArticleId")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        [DataMember(Name ="ArticleTitle")]
        public string Title { get; private set; }

        /// <summary>
        /// Gets the short text.
        /// </summary>
        [DataMember(Name = "ArticleShortText")]
        public string ShortText { get; private set; }

        /// <summary>
        /// Gets the link.
        /// </summary>
        [DataMember(Name = "ArticleLink")]
        public string Link { get; private set; }

        /// <summary>
        /// Gets the published date.
        /// </summary>
        [DataMember(Name = "ArticlePublished")]
        public DateTime Published { get; private set; }

        /// <summary>
        /// Gets the indexed date.
        /// </summary>
        [DataMember(Name = "ArticleIndexed")]
        public DateTime Indexed { get; private set; }

        /// <summary>
        /// Gets the indexing source identifier.
        /// </summary>
        [DataMember(Name = "IndexingSourceId")]
        public Guid IndexingSourceId { get; private set;  }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortArticle"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="title">The title.</param>
        /// <param name="shortText">The short text.</param>
        /// <param name="link">The link.</param>
        /// <param name="published">The published date.</param>
        /// <param name="indexed">The indexed date.</param>
        /// <param name="indexingSourceId">The indexing source identifier.</param>
        public ShortArticle(Guid id, string title, string shortText, string link, DateTime published, DateTime indexed, Guid indexingSourceId)
        {
            Id = id;
            Title = title;
            ShortText = shortText;
            Link = link;
            Published = published;
            Indexed = indexed;
            IndexingSourceId = indexingSourceId;
        }
    }
}