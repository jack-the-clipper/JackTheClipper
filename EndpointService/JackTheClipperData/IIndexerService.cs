using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JackTheClipperCommon.BusinessObjects;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperData
{
    /// <summary>
    /// Interface for any indexer service 
    /// </summary>
    public interface IIndexerService
    {
        /// <summary>
        /// Clears all indexes
        /// Warning: This is not reversible.
        /// Proceed with Caution
        /// </summary>
        void ClearIndex();

        /// <summary>
        /// Indexes an RSS article thread safe.
        /// </summary>
        /// <param name="article">The article.</param>
        /// <param name="key">The rss key.</param>
        Task IndexRssFeedItemThreadSafeAsync(Article article, RssKey key);

        /// <summary>
        /// Indexes a web site article thread safe.
        /// </summary>
        /// <param name="article">The article.</param>
        Task IndexArticleThreadSafeAsync(Article article);

        /// <summary>
        /// Gets the requested feed
        /// </summary>
        /// <param name="user">The User for which the feed is requested.</param>
        /// <param name="feedId">The Id to the feed which should be generated.</param>
        /// <param name="since">The date from which on articles should be considered.</param>
        /// <param name="page">The requested page.</param>
        /// <returns>List of articles within the feed</returns>
        Task<IReadOnlyCollection<ShortArticle>> GetFeedAsync(User user, Guid feedId, DateTime since, int page);

        /// <summary>
        /// Gets the complete feed (= all relevant articles in any feed) of a given user.
        /// </summary>
        /// <param name="user">The User for which the complete feed is requested</param>
        /// <param name="since">The date from which on articles should be considered.</param>
        /// <returns>List of articles within any feed of the user.</returns>
        Task<IReadOnlyCollection<ShortArticle>> GetCompleteFeedAsync(User user, DateTime since);

        /// <summary>Gets a specific Article.</summary>
        /// <param name="articleId">The Id of the article to search.</param>
        /// <returns>The article (if exists)</returns>
        Task<Article> GetArticleAsync(Guid articleId);
    }
}
