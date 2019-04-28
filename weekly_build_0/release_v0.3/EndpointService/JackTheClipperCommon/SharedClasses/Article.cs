using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// Data structure for an Article, which contains a Guid for identification, a Title, a summary,
    /// a link, the complete Text and 2 DateTimes (published and indexed).
    /// Extends from ShortArticle
    /// </summary>
    [DataContract]
    public class Article : ShortArticle
    {
        /// <summary>
        /// Gets the text.
        /// </summary>
        [NotNull]
        [DataMember(Name = "ArticleLongText")]
        public string Text { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Article"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="title">The title.</param>
        /// <param name="shortText">The short text.</param>
        /// <param name="link">The link.</param>
        /// <param name="text">The text.</param>
        /// <param name="published">The published date.</param>
        /// <param name="indexed">The indexed date.</param>
        /// <param name="indexingSourceId">The indexing source identifier.</param>
        public Article(Guid id, string title, string shortText, string link, string text, DateTime published,
                       DateTime indexed, Guid indexingSourceId) 
            : base(id, title, shortText, link, published, indexed, indexingSourceId)
        {
            Text = text;
        }
    }
}