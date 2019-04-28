using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;
using JetBrains.Annotations;

namespace JackTheClipperBusiness.CrawlerManagement
{
    /// <summary>
    /// Controls the interaction between ElasticSearch Server and Crawler to index Articles.
    /// Additional it has Methods to Start, Stop and Restart the Crawler.
    /// </summary>
    internal class CrawlerControllerBAD : ICrawlerObserver, ICrawlerController
    {
        private static readonly CrawlerControllerBAD ControllerBad = new CrawlerControllerBAD();
        private static readonly object LockObj = new object();

        [NotNull]
        private readonly List<Crawler> activeCrawlers = new List<Crawler>();

        private CrawlerControllerBAD()
        {
        }

        public static CrawlerControllerBAD GetCrawlerController()
        {
            return ControllerBad;
        }

        /// <summary>
        /// Notifies the observer that a new Rss feed item has been detected by a crawler.
        /// Note: Must be thread safe
        /// </summary>
        /// <param name="source">The source to index</param>
        /// <param name="feedId">The feed id.</param>
        public void NotifyNewRssFeedFoundThreadSave(SyndicationItem source, Guid feedId)
        {
            DatabaseAdapterFactory.GetControllerInstance<IIndexerService>().IndexRssFeedItemThreadSafe(source, feedId);
        }

        /// <summary>
        /// Notifies the observer that new web page content has been detected by a crawler.
        /// Note: Must be thread safe
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="content">The content</param>
        /// <param name="feedId">The feed id.</param>
        /// <returns>Method result, indicating success.</returns>
        public void NotifyNewWebPageContentFoundThreadSafe(Source source, string content, Guid feedId)
        {
            throw new NotImplementedException();
        }

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

        public void ClearAllIndexes()
        {
            DatabaseAdapterFactory.GetControllerInstance<IIndexerService>().ClearIndex();
        }

        private void Start()
        {
            if (!this.activeCrawlers.Any())
            {
                var sources = Factory.GetControllerInstance<IClipperDatabase>().GetSources();
                foreach (var source in sources)
                {
                    this.activeCrawlers.Add(new Crawler(this, source));
                }
            }

            foreach (var activeCrawler in this.activeCrawlers)
            {
                activeCrawler.Start();
            }
        }

        private void Stop()
        {
            foreach (var activeCrawler in this.activeCrawlers)
            {
                activeCrawler.Stop();
            }

            this.activeCrawlers.Clear();
        }
    }
}