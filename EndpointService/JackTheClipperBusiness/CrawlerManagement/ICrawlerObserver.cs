using System;
using System.ServiceModel.Syndication;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperBusiness.CrawlerManagement
{
    /// <summary>
    /// Interface for the CrawlerController
    /// </summary>
    internal interface ICrawlerObserver
    {
        /// <summary>
        /// Notifies the observer that a new Rss feed item has been detected by a crawler.
        /// Note: Must be thread safe
        /// </summary>
        /// <param name="source">The source to index</param>
        /// <param name="feedId">The feed id.</param>
        void NotifyNewRssFeedFoundThreadSave(SyndicationItem source, Guid feedId);

        /// <summary>
        /// Notifies the observer that new web page content has been detected by a crawler.
        /// Note: Must be thread safe
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="content">The content</param>
        /// <param name="feedId">The feed id.</param>
        /// <returns>Method result, indicating success.</returns>
        void NotifyNewWebPageContentFoundThreadSafe(Source source, string content, Guid feedId);
    }
}