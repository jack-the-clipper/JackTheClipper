using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using JackTheClipperCommon.BusinessObjects;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;
using JetBrains.Annotations;

namespace JackTheClipperBusiness.CrawlerManagement
{
    /// <summary>
    /// Controls active crawlers and manages the interaction with the indexer.
    /// </summary>
    internal class CrawlerController : ICrawlerObserver, ICrawlerController
    {
        #region Member
        /// <summary>
        /// The only instance of the controller;
        /// </summary>
        private static readonly CrawlerController Controller = new CrawlerController();
        private static readonly object LockObj = new object();

        /// <summary>
        /// List of currently active crawlers.
        /// </summary>
        [NotNull]
        private readonly List<Crawler> activeCrawlers = new List<Crawler>();
        #endregion

        #region ctor
        /// <summary>
        /// Prevents a default instance of the <see cref="CrawlerController"/> class from being created.
        /// </summary>
        private CrawlerController()
        {
        }
        #endregion

        #region GetCrawlerController
        /// <summary>
        /// Gets an instance of a <see cref="CrawlerController"/>.
        /// </summary>
        /// <returns>Instance of <see cref="CrawlerController"/></returns>
        public static CrawlerController GetCrawlerController()
        {
            return Controller;
        }
        #endregion

        #region Restart
        /// <summary>
        /// Restarts the controller instance.
        /// </summary>
        public void Restart()
        {
            Task.Run(() =>
            {
                lock (LockObj)
                {
                    Stop();
                    Start();
                }
            });
        }
        #endregion

        #region ClearAllIndexes
        /// <summary>
        /// Clears all indexes (use with care, this is a permanent operation).
        /// </summary>
        public void ClearAllIndexes()
        {
            DatabaseAdapterFactory.GetControllerInstance<IIndexerService>().ClearIndex();
        }
        #endregion

        #region NotifyNewRssFeedFoundThreadSave
        /// <summary>
        /// Notifies the Observer that a (probably) new Rss feed item has been detected by a crawler.
        /// Note: Must be thread safe
        /// </summary>
        /// <param name="item">The item to index</param>
        /// <param name="rssKey">The belonging rss key.</param>
        /// <param name="source">The source.</param>
        public void NotifyNewRssFeedFoundThreadSave(SyndicationItem item, RssKey rssKey, Source source)
        {
            var plainText = item.Summary?.Text?.GetTextFromHtml() ?? string.Empty;
            var plainTitle = item.Title?.Text.GetTextFromHtml() ?? source.Name;
            if (DoesNotViolateBlackList(source, plainText) && DoesNotViolateBlackList(source, plainTitle))
            {
                var article = new Article(Guid.NewGuid(),
                                          plainTitle,
                                          rssKey.Link,
                                          null,
                                          plainText,
                                          new DateTime(rssKey.Updated),
                                          DateTime.UtcNow,
                                          source.Id);
                DatabaseAdapterFactory.GetControllerInstance<IIndexerService>()
                                      .IndexRssFeedItemThreadSafeAsync(article, rssKey);
            }
        }
        #endregion

        #region NotifyNewWebPageContentFoundThreadSafe
        /// <summary>
        /// Notifies the Observer that (probably) new web page content has been detected by a crawler.
        /// Note: Must be thread safe
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="content">The content</param>
        /// <param name="link">The link.</param>
        /// <param name="published">The published date.</param>
        /// <param name="source">The source.</param>
        /// <returns>Method result, indicating success.</returns>
        public void NotifyNewWebPageContentFoundThreadSafe(string title, string content, string link,
            DateTime published, Source source)
        {
            if (DoesNotViolateBlackList(source, title) && DoesNotViolateBlackList(source, content))
            {
                var article = new Article(Guid.NewGuid(),
                                          title,
                                          link,
                                          null,
                                          content,
                                          published,
                                          DateTime.UtcNow,
                                          source.Id);
                DatabaseAdapterFactory.GetControllerInstance<IIndexerService>().IndexArticleThreadSafeAsync(article);
            }
        }
        #endregion

        #region NotifyNewImageContentFoundThreadSafe
        /// <summary>
        /// Notifies the Observer that (probably) new web page content has been detected by a crawler.
        /// Note: Must be thread safe
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="description">The image description</param>
        /// <param name="link">The link.</param>
        /// <param name="imageLink">The link of the image.</param>
        /// <param name="published">The published date.</param>
        /// <param name="source">The source.</param>
        /// <returns>Method result, indicating success.</returns>
        public void NotifyNewImageContentFoundThreadSafe(string title, string description, string imageLink, string link,
            DateTime published, Source source)
        {
            if (DoesNotViolateBlackList(source, title) && DoesNotViolateBlackList(source, description))
            {
                var article = new Article(Guid.NewGuid(),
                                          title,
                                          link,
                                          imageLink,
                                          description,
                                          published,
                                          DateTime.UtcNow,
                                          source.Id);
                DatabaseAdapterFactory.GetControllerInstance<IIndexerService>().IndexArticleThreadSafeAsync(article);
            }
        }
        #endregion

        #region Start        
        /// <summary>
        /// Starts the observation. Creates all necessary crawlers.
        /// </summary>
        private void Start()
        {
            if (!this.activeCrawlers.Any())
            {
                var sources = Factory.GetControllerInstance<IClipperDatabase>().GetSources();
                foreach (var source in sources)
                {
                    switch (source.Type)
                    {
                        case ContentType.Rss:
                            this.activeCrawlers.Add(new RssCrawler(this, source));
                            break;
                        case ContentType.WebSite:
                            this.activeCrawlers.Add(new WebsiteCrawler(this, source));
                            break;
                    }
                }
            }

            foreach (var activeCrawler in this.activeCrawlers)
            {
                activeCrawler.Start();
            }
        }
        #endregion

        #region Stop        
        /// <summary>
        /// Stops all active crawlers.
        /// </summary>
        private void Stop()
        {
            foreach (var activeCrawler in this.activeCrawlers)
            {
                activeCrawler.Stop();
            }

            this.activeCrawlers.Clear();
        }
        #endregion

        #region DoesNotViolateBlackList        
        /// <summary>
        /// Checks whether the input doesnt violates the black list.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if no violation occured.</returns>
        private static bool DoesNotViolateBlackList(Source source, string value)
        {
            var blackList = source.BlackList;
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (blackList != null)
            {
                return !blackList.Any(value.Contains);
            }

            return true;
        }
        #endregion
    }
}