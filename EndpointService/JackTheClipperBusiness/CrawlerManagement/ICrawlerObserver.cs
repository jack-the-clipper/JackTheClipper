using System;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using JackTheClipperCommon.BusinessObjects;
using JackTheClipperCommon.SharedClasses;
using JetBrains.Annotations;

namespace JackTheClipperBusiness.CrawlerManagement
{
    /// <summary>
    /// Interface for the CrawlerController
    /// </summary>
    internal interface ICrawlerObserver
    {
        /// <summary>
        /// Notifies the Observer that a (probably) new Rss feed item has been detected by a crawler.
        /// Note: Must be thread safe
        /// </summary>
        /// <param name="item">The item to index</param>
        /// <param name="rssKey">The belonging rss key.</param>
        /// <param name="source">The source.</param>
        /// <returns>If indexing is required: a task with the corresponding indexing job.
        /// <para>Otherwise: a completed Task</para></returns>
        [NotNull]
        Task NotifyNewRssFeedFoundThreadSave(SyndicationItem item, RssKey rssKey, Source source);

        /// <summary>
        /// Notifies the Observer that (probably) new web page content has been detected by a crawler.
        /// Note: Must be thread safe
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="content">The content</param>
        /// <param name="link">The link.</param>
        /// <param name="published">The published date.</param>
        /// <param name="source">The source.</param>
        /// <returns>If indexing is required: a task with the corresponding indexing job.
        /// <para>Otherwise: a completed Task</para></returns>
        [NotNull]
        Task NotifyNewWebPageContentFoundThreadSafe(string title, string content, string link, DateTime published, Source source);

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
        /// <returns>If indexing is required: a task with the corresponding indexing job.
        /// <para>Otherwise: a completed Task</para></returns>
        [NotNull]
        Task NotifyNewImageContentFoundThreadSafe(string title, string description, string imageLink, string link, DateTime published, Source source);
    }
}