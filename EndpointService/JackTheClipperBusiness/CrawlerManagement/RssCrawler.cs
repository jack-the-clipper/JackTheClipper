using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Linq;
using HtmlAgilityPack;
using JackTheClipperCommon;
using JackTheClipperCommon.BusinessObjects;
using JackTheClipperCommon.SharedClasses;
using JetBrains.Annotations;

namespace JackTheClipperBusiness.CrawlerManagement
{
    /// <summary>
    /// A crawler which observes an RSS-source.
    /// </summary>
    internal class RssCrawler : Crawler
    {
        #region Member
        /// <summary>
        /// Number of cached last rss feeds.
        /// </summary>
        private const int CacheThreshold = 5000;

        /// <summary>
        /// Cache, containing the last indexed keys
        /// </summary>
        private readonly HashSet<RssKey> lastIndexed;

        /// <summary>
        /// <see cref="WebsiteCrawler"/>, used to perform the regex and/or XPath handling
        /// </summary>
        private readonly WebsiteCrawler websiteCrawler;

        /// <summary>
        /// Gets the observation interval
        /// </summary>
        protected override int Interval => 45 * 1000;
        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="RssCrawler"/> class.
        /// </summary>
        /// <param name="observer">The Observer of the instance to create.</param>
        /// <param name="source">The source to observe.</param>
        public RssCrawler([NotNull] ICrawlerObserver observer, [NotNull] Source source) : base(observer, source)
        {
            if (!string.IsNullOrEmpty(source.Regex) || !string.IsNullOrEmpty(source.XPath))
            {
                this.websiteCrawler = new WebsiteCrawler(observer, source);
            }
            else
            {
                this.lastIndexed = new HashSet<RssKey>(5000);
            }
        }
        #endregion

        #region PerformObservation
        /// <summary>
        /// Performs the observation. Callback event of <see cref="Crawler.scheduler"/>
        /// </summary>
        /// <param name="timerState">timer state object (never used).</param>
        protected override void PerformObservation(object timerState)
        {
            using (new PerfTracer(nameof(PerformObservation) + Source.Name))
            {
                try
                {
                    using (var reader = XmlReader.Create(Source.Uri))
                    {
                        var feed = SyndicationFeed.Load(reader);
                        reader.Close();

                        var alreadyIndexed = this.lastIndexed;
                        foreach (var item in feed.Items)
                        {
                            if (item != null)
                            {
                                if (this.websiteCrawler != null)
                                {
                                    var html = new HtmlDocument();
                                    html.LoadHtml(item.Summary.Text);
                                    this.websiteCrawler.HandleDocument(html, item.Title?.Text);
                                    continue;
                                }

                                var key = GetKey(item);
                                if (!alreadyIndexed.Contains(key))
                                {
                                    alreadyIndexed.Add(key);
                                    if (alreadyIndexed.Count > CacheThreshold)
                                    {
                                        alreadyIndexed.ExceptWith(alreadyIndexed.Take(CacheThreshold / 20));
                                    }

                                    Observer.NotifyNewRssFeedFoundThreadSave(item, key, Source);
                                    var content = item.Content == null ? item.Summary?.Text : (item.Content as TextSyndicationContent)?.Text;
                                    HandleImages(content, item.Title?.Text, key);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    RequestRescheduling();
                }
            }
        }
        #endregion

        #region GetKey
        /// <summary>
        /// Gets the <see cref="RssKey"/> for the given <see cref="SyndicationItem"/>
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The key of the item.</returns>
        private static RssKey GetKey(SyndicationItem item)
        {
            var link = item.Links.FirstOrDefault();
            var linkAsString = link != null ? link.Uri.ToString() : string.Empty;
            var lastUpdatedDate = item.LastUpdatedTime.UtcTicks == 0
                ? item.PublishDate.UtcTicks
                : item.LastUpdatedTime.UtcTicks;
            return new RssKey(lastUpdatedDate, linkAsString);
        }
        #endregion
    }
}