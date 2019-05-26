using System;
using System.Runtime.Serialization;
using JackTheClipperCommon.Configuration;
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
        [DataMember(Name = "ArticleLongText")]
        public string Text { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Article"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="title">The title.</param>
        /// <param name="link">The link.</param>
        /// <param name="imageLink">The image link.</param>
        /// <param name="text">The text.</param>
        /// <param name="published">The published date.</param>
        /// <param name="indexed">The indexed date.</param>
        /// <param name="indexingSourceId">The indexing source identifier.</param>
        public Article(Guid id, string title, string link, string imageLink, string text, DateTime published,
                       DateTime indexed, Guid indexingSourceId) 
            : base(id, title, Truncate(text), link, imageLink, published, indexed, indexingSourceId)
        {
            Text = text;
        }

        /// <summary>
        /// Truncates the length of <see cref="Text"/> to the length specified by <see cref="AppConfiguration.ShortTextLength"/>
        /// </summary>
        /// <param name="input">The text to truncate</param>
        /// <returns>The truncated text</returns>
        public static string Truncate(string input)
        {
            if (input != null && input.Length > AppConfiguration.ShortTextLength)
            {
                return input.Substring(0, AppConfiguration.ShortTextLength - 3) + "...";
            }

            return input;
        }
    }
}