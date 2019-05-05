using System;
using System.Threading;
using HtmlAgilityPack;
using JackTheClipperCommon.BusinessObjects;
using JackTheClipperCommon.SharedClasses;
using JetBrains.Annotations;

namespace JackTheClipperBusiness.CrawlerManagement
{
    /// <summary>
    /// The class which represents a crawler.
    /// This class observes a given source, performs basic caching and notifies the Observer in case a new article was found.
    /// </summary>
    internal abstract class Crawler
    {
        #region member
        /// <summary>
        /// The scheduler.
        /// </summary>
        private Timer scheduler;

        /// <summary>
        /// The Observer of the current instance.
        /// </summary>
        [NotNull]
        protected ICrawlerObserver Observer { get; private set; }

        /// <summary>
        /// The source which should ne observed.
        /// </summary>
        [NotNull]
        protected Source Source { get; private set; }

        /// <summary>
        /// Gets the observation interval
        /// </summary>
        protected abstract int Interval { get; }
        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler"/> class.
        /// </summary>
        /// <param name="observer">The Observer of the instance to create.</param>
        /// <param name="source">The source to observe.</param>
        protected Crawler([NotNull] ICrawlerObserver observer, [NotNull]Source source)
        {
            Observer = observer;
            Source = source;
        }
        #endregion

        //// Protected methods

        #region PerformObservation
        /// <summary>
        /// Performs the observation. Callback event of <see cref="scheduler"/>
        /// </summary>
        /// <param name="timerState">timer state object (never used).</param>
        protected abstract void PerformObservation(object timerState);
        #endregion

        #region RequestRescheduling
        /// <summary>
        /// Requests rescheduling.
        /// </summary>
        protected void RequestRescheduling()
        {
            try
            {
                var s = this.scheduler; //keep reference (we are probably multi-threaded)
                s?.Change(Interval, Timeout.Infinite);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region HandleImages        
        /// <summary>
        /// Performs image handling.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="title">The title.</param>
        /// <param name="rssKey">The RSS key.</param>
        protected void HandleImages(string content, string title, RssKey rssKey = null)
        {
            if (content != null && title != null)
            {
                var html = new HtmlDocument();
                html.LoadHtml(content);
                HandleImages(html, title, rssKey);
            }
        }

        /// <summary>
        /// Performs image handling.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="title">The title.</param>
        /// <param name="rssKey">The RSS key.</param>
        protected void HandleImages(HtmlDocument doc, string title, RssKey rssKey = null)
        {
            if (doc != null && title != null)
            {
                var images = doc.GetAllImages();
                var link = rssKey != null ? rssKey.Link : Source.Uri;
                var published = rssKey != null ? new DateTime(rssKey.Updated) : DateTime.UtcNow;
                foreach (var image in images)
                {
                    Observer.NotifyNewImageContentFoundThreadSafe(title, image.Item2, image.Item1, link, 
                                                                  published, Source);
                }
            }
        }
        #endregion

        ////Public methods

        #region Start
        /// <summary>
        /// Starts the crawler (if not already running). 
        /// </summary>
        public void Start()
        {
            if (this.scheduler == null)
            {
                TimerCallback callBack = PerformObservation;
                this.scheduler = new Timer(callBack, null, 0, Timeout.Infinite);
            }
        }
        #endregion

        #region Stop
        /// <summary>
        /// Stops the crawler.  
        /// </summary>
        public void Stop()
        {
            var toStop = this.scheduler;
            if (toStop != null)
            {
                this.scheduler = null;
                toStop.Dispose();
            }
        }
        #endregion
    }
}