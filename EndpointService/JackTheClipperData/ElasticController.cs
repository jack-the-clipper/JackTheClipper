using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using JackTheClipperCommon;
using JackTheClipperCommon.BusinessObjects;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Extensions;
using JackTheClipperCommon.SharedClasses;
using JetBrains.Annotations;
using Nest;

namespace JackTheClipperData
{
    /// <summary>
    /// Class to control every interaction that needs to be done with the ElasticServer.
    /// </summary>
    public class ElasticController : IIndexerService
    {
        #region members
#if ST
        private static readonly object lockObj = new object();
#endif
        /// <summary>
        /// The uri of the elastic server.
        /// </summary>
        private const string ElasticUri = "http://134.255.225.176:9200";

        /// <summary>
        /// The connections settings
        /// </summary>
        private readonly ConnectionSettings settings = new ConnectionSettings(new Uri(ElasticUri));

        /// <summary>
        /// Reference to the running clean up job.
        /// </summary>
        private static readonly ElasticDuplicateRemover CleanUpJob;
        #endregion

        #region MyRegion
        /// <summary>
        /// Static ctor of <see cref="ElasticController"/>
        /// </summary>
        static ElasticController()
        {
            CleanUpJob = new ElasticDuplicateRemover();
        }
        #endregion

        #region IndexRssFeedItemThreadSafeAsync
        /// <summary>
        /// Indexes an RSS article thread safe.
        /// </summary>
        /// <param name="article">The article.</param>
        /// <param name="key">The rss key.</param>
        public async Task IndexRssFeedItemThreadSafeAsync(Article article, RssKey key)
        {
#if ST
            lock (lockObj)
#endif
            {
                try
                {
                    using (new PerfTracer(nameof(IndexRssFeedItemThreadSafeAsync)))
                    {
                        var client = new ElasticClient(settings);
                        var speedKey = new ShortArticleKey(article.IndexingSourceId, key.Updated, key.Link);
#if ST
                        if (!(IsArticleDuplicate(speedKey, client).Result))
#else
                        if (!(await IsArticleDuplicate(speedKey, client)))
#endif
                        {
#if ST
                            var result = IndexArticle(article, speedKey, client).Result;
#else
                            var result = await IndexArticle(article, speedKey, client);
#endif

                            if (result.Status == SuccessState.UnknownError)
                            {
                                throw new Exception("IndexArticle failed: " + result.UserMessage);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        #endregion

        #region IndexArticleThreadSafeAsync
        /// <summary>
        /// Indexes the web site thread safe.
        /// </summary>
        /// <param name="article">The article.</param>
        public async Task IndexArticleThreadSafeAsync(Article article) // Article is to change to Whatever gets in here.
        {
            try
            {
                using (new PerfTracer(nameof(IndexArticleThreadSafeAsync)))
                {
#if ST
                    lock (lockObj)
#endif
                    {
                        var client = new ElasticClient(settings);
#if ST
                        if (!(IsArticleDuplicate(article, client).Result))
#else
                        if (!(await IsArticleDuplicate(article, client)))
#endif
                        {
#if ST
                            var result = IndexArticle(article, client).Result;
#else
                            var result = await IndexArticle(article, client);
#endif
                            if (result.Status == SuccessState.UnknownError)
                            {
                                throw new Exception("IndexArticle failed: " + result.UserMessage);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region ClearIndex
        /// <summary>
        /// Clears all indexes
        /// Warning: This is not reversible.
        /// Proceed with Caution
        /// </summary>
        public void ClearIndex()
        {
            using (new PerfTracer(nameof(ClearIndex)))
            {
                var client = new ElasticClient(settings);
                client.DeleteIndex(AppConfiguration.ElasticTemporaryIndexName);
                client.DeleteIndex(AppConfiguration.ElasticPermanentIndexName);
                client.DeleteIndex(AppConfiguration.ElasticRssSpeedIndexName);
                client.CreateIndex(AppConfiguration.ElasticTemporaryIndexName);
                client.CreateIndex(AppConfiguration.ElasticPermanentIndexName);
                client.CreateIndex(AppConfiguration.ElasticRssSpeedIndexName);
            }
        }
        #endregion

        #region GetFeedAsync
        /// <summary>
        /// Gets the requested feed
        /// </summary>
        /// <param name="user">The User for which the feed is requested.</param>
        /// <param name="feedId">The Id to the feed which should be generated.</param>
        /// <param name="since">The date from which on articles should be considered.</param>
        /// <param name="page">The requested page.</param>
        /// <returns>List of articles within the feed</returns>
        public async Task<IReadOnlyCollection<ShortArticle>> GetFeedAsync(User user, Guid feedId, DateTime since, int page)
        {
            try
            {
                using (new PerfTracer(nameof(GetFeedAsync) + user.MailAddress))
                {
                    var itemsPerPage = user.Settings.ArticlesPerPage;
                    var client = new ElasticClient(settings);
                    var relevantFeed = new[] { user.GetFeed(feedId) };
                    var searchResult =
                        await client.SearchAsync(GetFeedSelector(relevantFeed, page * itemsPerPage, itemsPerPage, since));
                    return GetDistinctNoBlackListViolations(relevantFeed, searchResult.Documents);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<ShortArticle>();
            }
        }
        #endregion

        #region GetCompleteFeedAsync
        /// <summary>
        /// Gets the complete feed (= all relevant articles in any feed) of a given user.
        /// </summary>
        /// <param name="user">The User for which the complete feed is requested</param>
        /// <param name="since">The date from which on articles should be considered.</param>
        /// <returns>List of articles within any feed of the user.</returns>
        public async Task<IReadOnlyCollection<ShortArticle>> GetCompleteFeedAsync(User user, DateTime since)
        {
            try
            {
                using (new PerfTracer(nameof(GetCompleteFeedAsync) + user.MailAddress))
                {
                    var client = new ElasticClient(settings);
                    var relevantFeeds = user.Settings.Feeds;
                    var searchResult =
                        await client.SearchAsync(GetFeedSelector(relevantFeeds, 0, 1000, since));
                    return GetDistinctNoBlackListViolations(relevantFeeds, searchResult.Documents);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<ShortArticle>();
            }
        }
        #endregion

        #region GetArticleAsync
        /// <summary>Gets a specific Article.</summary>
        /// <param name="articleId">The Id of the article to search.</param>
        /// <returns>The article (if exists)</returns>
        public async Task<Article> GetArticleAsync(Guid articleId)
        {
            var client = new ElasticClient(settings);
            var searchResponse = await client.SearchAsync<Article>(s => s
                                                                        .Index(AppConfiguration.ElasticPermanentIndexName)
                                                                        .Size(1)
                                                                        .Query(q => q.MatchPhrase(m => m
                                                                                                       .Field(f => f.Id)
                                                                                                       .Query(articleId.ToString()))));

            var received = searchResponse.Documents.FirstOrDefault();
            return received;
        }
        #endregion

        //// Private methods

        #region IsArticleDuplicate
        /// <summary>
        /// Checks if the specified Article is a duplicate
        /// </summary>
        /// <param name="article">Article to check</param>
        /// <param name="client">The current client</param>
        /// <returns>A value indicating whether the article already exists.</returns>
        private static async Task<bool> IsArticleDuplicate(Article article, IElasticClient client)
        {
            var articleExists =
                await client.SearchAsync<Article>(s => s
                                                       .Size(1)
                                                       .Index(AppConfiguration.ElasticPermanentIndexName)
                                                       .Query(q => q.MatchPhrase(m => m
                                                                                      .Field(f => f.IndexingSourceId)
                                                                                      .Query(article.IndexingSourceId.ToString())) &&
                                                                   q.MatchPhrase(m => m
                                                                                      .Field(f => f.Title)
                                                                                      .Query(article.Title)) &&
                                                                   q.MatchPhrase(m => m
                                                                                      .Field(f => f.Text)
                                                                                      .Query(article.Text)) &&
                                                                   q.MatchPhrase(m => m
                                                                                      .Field(f => f.ImageLink)
                                                                                      .Query(article.ImageLink))));

            return articleExists.IsValid && articleExists.Documents.Any();
        }


        /// <summary>
        /// Checks if the specified Article is a duplicate
        /// </summary>
        /// <param name="rssSpeedLookupKey">Rss key to check</param>
        /// <param name="client">The current client</param>
        /// <returns>A value indicating whether the article already exists.</returns>
        private static async Task<bool> IsArticleDuplicate(ShortArticleKey rssSpeedLookupKey, IElasticClient client)
        {
            var key = rssSpeedLookupKey;
            var articleExists =
                await client.SearchAsync<ShortArticleKey>(s => s
                                                               .Size(1)
                                                               .Index(AppConfiguration.ElasticRssSpeedIndexName)
                                                               .Query(q => q.MatchPhrase(m =>
                                                                                             m.Field(f => f.Updated)
                                                                                              .Query(key.Updated.ToString())) &&
                                                                           q.MatchPhrase(m =>
                                                                                             m.Field(f => f.Link)
                                                                                              .Query(key.Link.ToString())) &&
                                                                           q.MatchPhrase(m =>
                                                                                             m.Field(f => f.SourceId)
                                                                                              .Query(key.SourceId.ToString()))));
            return articleExists.IsValid && articleExists.Documents.Any();
        }
        #endregion

        #region GetSupersetFeedQuery
        /// <summary>
        /// Gets the superset feed query
        /// </summary>
        /// <param name="descriptor">Current descriptor</param>
        /// <param name="article">The current article which should be checked for relevance.</param>
        /// <returns>The superset feed query.</returns>
        private static QueryContainer GetSupersetFeedQuery(QueryContainerDescriptor<Article> descriptor, Article article)
        {
            var superSetFeed = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().GetSuperSetFeed();
            var keywords = superSetFeed.Keywords;
            return descriptor.MatchPhrase(c => c.Field(x => x.IndexingSourceId).Query(article.IndexingSourceId.ToString())) &&
                   (descriptor.Terms(c => c
                                          .Field(x => x.Title)
                                          .Terms(keywords)) ||
                    descriptor.Terms(c => c
                                          .Field(x => x.Text)
                                          .Terms(keywords)));
        }

        #endregion

        #region IndexArticle
        /// <summary>
        /// Indexes the specified article to ElasticServer
        /// </summary>
        /// <param name="article">The article.</param>
        /// <param name="client">ElasticClient.</param>
        /// <returns>The method result.</returns>
        private static async Task<MethodResult> IndexArticle(Article article, IElasticClient client)
        {
            //First step: Index to temporary index.
            IResponse lastResponse =
                await client.IndexAsync(article,
                                        i => i.Index(AppConfiguration.ElasticTemporaryIndexName)
                                              .Refresh(Refresh.WaitFor));
            if (lastResponse.IsValid)
            {
                using (new TemporaryIndexCleanUp(article, client))
                {
                    //Second step: Is new article relevant for any feed within the system ?
                    ISearchResponse<Article> searchResult;
                    lastResponse = searchResult =
                        await client.SearchAsync<Article>(s => s
                                                               .Index(AppConfiguration.ElasticTemporaryIndexName)
                                                               .Query(q => GetSupersetFeedQuery(q, article)));
                    if (lastResponse.IsValid)
                    {
                        var articleIsRelevant = searchResult.Documents.FirstOrDefault(t => t.Id == article.Id) != null;
                        if (articleIsRelevant)
                        {
                            //Third step: Article is relevant -> transfer to permanent index.
                            lastResponse =
                                await client.IndexAsync(article, i => i
                                                                      .Index(AppConfiguration.ElasticPermanentIndexName)
                                                                      .Refresh(Refresh.True));
                        }
                    }
                }
            }

            return !lastResponse.IsValid
                ? new MethodResult(SuccessState.UnknownError, lastResponse.DebugInformation)
                : new MethodResult();
        }

        /// <summary>
        /// Indexes the specified article to ElasticServer
        /// </summary>
        /// <param name="article">The article.</param>
        /// <param name="rssLookupKey">The rss lookup key.</param>
        /// <param name="client">The ElasticClient.</param>
        /// <returns>The method result.</returns>
        private static async Task<MethodResult> IndexArticle(Article article, ShortArticleKey rssLookupKey, IElasticClient client)
        {
            var result = await IndexArticle(article, client);
            if (result.IsSucceeded())
            {
                var indexingResult =
                    await client.IndexAsync(rssLookupKey, i => i.Index(AppConfiguration.ElasticRssSpeedIndexName));
                if (!indexingResult.IsValid)
                {
                    return new MethodResult(SuccessState.UnknownError, indexingResult.DebugInformation);
                }
            }

            return result;
        }
        #endregion

        #region DeleteByIdAsync
        /// <summary>
        /// Deletes an article by its id.
        /// </summary>
        /// <param name="client">The client to use.</param>
        /// <param name="index">The index to use.</param>
        /// <param name="articleId">The article id to delete.</param>
        private static void DeleteByIdAsync(IElasticClient client, string index, string articleId)
        {
            client.DeleteByQueryAsync<Article>(descriptor =>
                                                   descriptor.Index(index)
                                                             .Refresh()
                                                             .Query(q => q.MatchPhrase(m => m
                                                                                            .Field(f => f.Id)
                                                                                            .Query(articleId))));
        }
        #endregion

        #region GetFeedSelector       
        /// <summary>
        /// Builds and returns the feed selector.
        /// </summary>
        /// <param name="feeds">The feeds.</param>
        /// <param name="from">Starting index for return value (paging).</param>
        /// <param name="size">Number of items to return (paging).</param>
        /// <param name="since">The time from which on feeds should be queried.</param>
        /// <returns>Function, representing the feed selector.</returns>
        private static Func<SearchDescriptor<Article>, ISearchRequest> GetFeedSelector(IEnumerable<Feed> feeds,
                                                                                       int from, int size, 
                                                                                       DateTime since)
        {
            return s => s
                        .Index(AppConfiguration.ElasticPermanentIndexName)
                        .From(from)
                        .Size(size)
                        .Query(q =>
                                   q.DateRange(d => d
                                                    .Field(f => f.Indexed)
                                                    .GreaterThanOrEquals(since)) &&
                                   GetFeedQuery(feeds))
                        .Sort(x => x.Descending(y => y.Published));
        }

        #endregion

        #region GetFeedQuery
        /// <summary>
        /// Gets the elastic query for the given feeds.
        /// </summary>
        /// <param name="relevantFeeds">The relevant feeds to determine their query.</param>
        /// <returns>The query for the feed.</returns>
        private static QueryContainer GetFeedQuery(IEnumerable<Feed> relevantFeeds)
        {
            QueryContainer finalContainer = null;
            foreach (var feed in relevantFeeds)
            {
                var keywordList = feed.Filter.Keywords.Select(x => x.ToLowerInvariant()).ToList();
                var basicTermsOperator = new TermsQuery
                                         {
                                             Field = "ArticleTitle",
                                             Terms = keywordList
                                         } ||
                                         new TermsQuery
                                         {
                                             Field = "ArticleLongText",
                                             Terms = keywordList
                                         };
                foreach (var guid in feed.Sources.Select(x => x.Id))
                {
                    finalContainer |= new MatchPhraseQuery
                    {
                        Field = "IndexingSourceId",
                        Query = (guid.ToString())
                    } && basicTermsOperator;
                }
            }

            return finalContainer;
        }
        #endregion

        #region GetDistinctNoBlackListViolations        
        /// <summary>
        /// Distincts the input and checks whether the input doesnt violates any black list.
        /// </summary>
        /// <param name="feeds">The relevant feeds.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if no violation occured.</returns>
        private static List<Article> GetDistinctNoBlackListViolations([NotNull]IReadOnlyCollection<Feed> feeds, [NotNull]IEnumerable<Article> value)
        {
            var checkedValues = new List<Article>();
            foreach (var articles in value.Distinct(ArticleComparer.FullArticleComparer).GroupBy(x => x.IndexingSourceId))
            {
                var belongingFeeds = feeds.Where(x => x.Sources.Any(src => src.Id == articles.Key));
                var blackList = belongingFeeds.Select(x => x.Filter.Blacklist ?? new List<string>()).SelectMany(d => d).ToList();
                if (blackList.Any())
                {
                    checkedValues.AddRange(articles.Where(x => !blackList.Any((x.Text ?? string.Empty).Contains) &&
                                                               !blackList.Any((x.Title ?? string.Empty).Contains)));
                }
                else
                {
                    checkedValues.AddRange(articles);
                }
            }

            return checkedValues;
        }
        #endregion

        #region TemporaryIndexCleanUp
        /// <summary>
        /// Class which ensures the cleanup of a temporary index operation
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        private class TemporaryIndexCleanUp : IDisposable
        {
            private readonly IElasticClient client;
            private readonly string articleId;

            /// <summary>
            /// Initializes a new instance of the <see cref="TemporaryIndexCleanUp"/> class.
            /// </summary>
            /// <param name="article">The article.</param>
            /// <param name="client">The client.</param>
            public TemporaryIndexCleanUp(ShortArticle article, IElasticClient client)
            {
                this.client = client;
                this.articleId = article.Id.ToString();
            }

            /// <summary>
            /// Undoes the temporary index operation
            /// </summary>
            public void Dispose()
            {
                DeleteByIdAsync(client, AppConfiguration.ElasticTemporaryIndexName, articleId);
            }
        }
        #endregion

        #region ElasticDuplicateRemover
        /// <summary>
        /// Removes duplicates we cant avoid (Example: Webpage: <p><p>Test</p></p> with Regex //p + asyncness /
        /// Other example: Some Rss feeds increment the "Published" time after the article has been changed, but not the text)
        /// Based upon https://www.elastic.co/de/blog/how-to-find-and-remove-duplicate-documents-in-elasticsearch
        /// </summary>
        private class ElasticDuplicateRemover
        {
            private Timer scheduler;

            /// <summary>
            /// Initializes a new instance of the <see cref="ElasticDuplicateRemover"/> class.
            /// </summary>
            public ElasticDuplicateRemover()
            {
                this.scheduler = new Timer(CleanUp, null, 0, 6 * 60 * 1000);
            }

            #region CleanUp
            /// <summary>
            /// Performs the clean up job.
            /// </summary>
            /// <param name="state">State param, never used.</param>
            private static void CleanUp(object state)
            {
                try
                {
                    using (new PerfTracer(nameof(CleanUpJob)))
                    {
                        var client = new ElasticClient(new ConnectionSettings(new Uri(ElasticUri)));
                        var comparer = ArticleComparer.FullArticleComparer;
                        var all = GetAllDocumentsInIndex(client);
                        var allDuplicates = all.GroupBy(x => comparer.GetHashCode(x)).Where(group => group.Count() > 1);
                        var deleted = 0;
                        foreach (var grouping in allDuplicates)
                        {
                            //HashCode is not enough (only a rough examination), also we didn't
                            //considered the indexing source till now.
                            var realDuplicates = grouping
                                                .GroupBy(x => new {x.Text, x.ImageLink, x.IndexingSourceId, x.Title})
                                                .Where(group => group.Count() > 1);

                            foreach (var duplicates in realDuplicates)
                            {
                                foreach (var article in duplicates.Skip(1))
                                {
                                    DeleteByIdAsync(client, AppConfiguration.ElasticPermanentIndexName,
                                                    article.Id.ToString());
                                    deleted++;
                                }
                            }
                        }

                        Console.WriteLine("CLEANUPJOB: Deleted " + deleted + " duplicates");
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine(error);
                }
            }
            #endregion

            #region GetAllDocumentsInIndex
            /// <summary>
            /// Gets all documents within elastic.
            /// Based upon http://telegraphrepaircompany.com/elasticsearch-nest-scroll-api-c/
            /// </summary>
            /// <param name="client">The client to use.</param>
            /// <returns>Enumerable of all articles within the ElasticSearch server.</returns>
            private static IEnumerable<Article> GetAllDocumentsInIndex(IElasticClient client)
            {
                var initialResponse = client.Search<Article>
                    (scr => scr.Index(AppConfiguration.ElasticPermanentIndexName)
                               .From(0)
                               .Take(1000)
                               .MatchAll()
                               .Scroll("2m"));
                var results = new List<Article>();
                if (initialResponse.Documents.Any())
                {
                    results.AddRange(initialResponse.Documents);
                }

                var scrollId = initialResponse.ScrollId;
                var dataRemaining = true;
                while (dataRemaining)
                {
                    var loopingResponse = client.Scroll<Article>("2m", scrollId);
                    if (loopingResponse.IsValid)
                    {
                        results.AddRange(loopingResponse.Documents);
                        scrollId = loopingResponse.ScrollId;
                    }

                    dataRemaining = loopingResponse.Documents.Any();
                }

                client.ClearScroll(new ClearScrollRequest(scrollId));
                return results;
            }
            #endregion
        }
        #endregion
    }
}