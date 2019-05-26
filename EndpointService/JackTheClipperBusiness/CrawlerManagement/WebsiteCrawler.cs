using System;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using JackTheClipperCommon;
using JackTheClipperCommon.BusinessObjects;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.SharedClasses;
using JetBrains.Annotations;

namespace JackTheClipperBusiness.CrawlerManagement
{
    /// <summary>
    /// A crawler which observes a website.
    /// </summary>
    internal class WebsiteCrawler : Crawler
    {
        #region Member
        /// <summary>
        /// The regex to be applied.
        /// </summary>
        private readonly Regex regex;

        /// <summary>
        /// The last fetched content (caching mechanisam/duplicate check)
        /// </summary>
        private string lastFetchedContent;

        /// <summary>
        /// Gets the observation interval
        /// </summary>
        protected override int Interval => AppConfiguration.WebsiteCrawlInterval * 1000;
        #endregion

        #region ctor
        /// <summary>
        /// Static constructor of <see cref="WebsiteCrawler"/>
        /// </summary>
        static WebsiteCrawler()
        {
            Regex.CacheSize = 1000;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler"/> class.
        /// </summary>
        /// <param name="observer">The Observer of the instance to create.</param>
        /// <param name="source">The source to observe.</param>
        public WebsiteCrawler([NotNull] ICrawlerObserver observer, [NotNull] Source source) 
            : base(observer, source)
        {
            if (!string.IsNullOrEmpty(source.Regex))
            {
                this.regex = new Regex(source.Regex, RegexOptions.Compiled);
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
            try
            {
                using (new PerfTracer(nameof(PerformObservation) + Source.Name))
                {
                    var web = new HtmlWeb
                    {
                        UsingCache = false,
                        UseCookies = true,
                        UserAgent = AppConfiguration.UserAgent
                    };
                    var doc = web.Load(Source.Uri);
                    if (web.StatusCode == HttpStatusCode.OK)
                    {
                        HandleDocument(doc);
                    }
                    else
                    {
                        Console.WriteLine(web.ResponseUri + " SC: " + web.StatusCode);
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
        #endregion

        #region HandleDocument
        /// <summary>
        /// Handles a given document (Xpath and regex)
        /// </summary>
        /// <param name="doc">The doc to handle.</param>
        /// <param name="title">The title (optional).</param>
        /// <param name="rssKey">The rss key (optional).</param>
        internal void HandleDocument(HtmlDocument doc, string title = null, RssKey rssKey = null)
        {
            if (string.Equals(doc.Text, this.lastFetchedContent, StringComparison.Ordinal))
            {
                return;
            }

            this.lastFetchedContent = doc.Text;

            //First possibility (makes nearly no sense): No filer -> direct index
            var link = rssKey != null ? rssKey.Link : Source.Uri;
            var published = rssKey != null ? new DateTime(rssKey.Updated) : DateTime.UtcNow;
            var noXPath = string.IsNullOrEmpty(Source.XPath);
            var noRegex = this.regex == null;
            if (noRegex && noXPath)
            {
                title = title ?? GetTitle(doc);
                Observer.NotifyNewWebPageContentFoundThreadSafe(title, doc.GetTextFromHtml(), link, published, Source);
                HandleImages(doc, title, rssKey);
                return;
            }

            if (noXPath)
            {
                title = title ?? GetTitle(doc);
                HandleRegex(doc.Text, title, link, published);
            }
            else
            {
                var relevantNodes = doc.DocumentNode.SelectNodes(Source.XPath);
                foreach (var node in relevantNodes)
                {
                    if (noRegex)
                    {
                        title = title ?? GetTitle(doc);
                        Observer.NotifyNewWebPageContentFoundThreadSafe(title,
                                                                             node.InnerHtml.GetTextFromHtml(),
                                                                             link, published, Source);
                        HandleImages(node.InnerHtml, title, rssKey);
                    }
                    else
                    {
                        title = title ?? GetTitle(doc);
                        HandleRegex(node.InnerHtml, title, link, published);
                    }
                }
            }
        }
        #endregion

        #region HandleRegex
        /// <summary>
        /// Performs regex handling.
        /// </summary>
        /// <param name="content">The content which should be observed.</param>
        /// <param name="title">The title.</param>
        /// <param name="link">The link.</param>
        /// <param name="published">the published date.</param>
        private void HandleRegex(string content, string title, string link, DateTime published)
        {
            var matches = this.regex.Matches(content);
            foreach (Match match in matches)
            {
                if (match != null && !string.IsNullOrEmpty(match.Value))
                {
                    Observer.NotifyNewWebPageContentFoundThreadSafe(title, match.Value.GetTextFromHtml(),
                                                                    link, published, Source);
                    HandleImages(match.Value, title, new RssKey(published.Ticks, link));
                }
            }
        }
        #endregion

        #region GetTitle
        /// <summary>
        /// Gets the title from the given document.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private string GetTitle(HtmlDocument doc)
        {
            var titleNode = doc.DocumentNode.SelectSingleNode("//head/title");
            return titleNode != null ? WebUtility.HtmlDecode(titleNode.InnerText) : new Uri(Source.Uri).Host;
        }
        #endregion
    }
}