using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Xml;
using JackTheClipperCommon;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.SharedClasses;
using JetBrains.Annotations;

namespace JackTheClipperBusiness.CrawlerManagement
{
    /// <summary>
    /// The class which represents a crawler.
    /// This class observes a given source, performs basic caching and notifies the observer in case a new article was found.
    /// </summary>
    internal class Crawler
    {
        #region member
        /// <summary>
        /// Default observation interval
        /// </summary>
        private const int Interval = 30 * 1000; //30 secs

        /// <summary>
        /// The observer of the current instance.
        /// </summary>
        [NotNull]
        private readonly ICrawlerObserver observer;

        /// <summary>
        /// The source which should ne observed.
        /// </summary>
        [NotNull]
        private readonly Source toObserve;

        /// <summary>
        /// Cache, containing the last indexed headings
        /// </summary>
        [NotNull]
        private HashSet<string> lastIndexedHeadings;

        /// <summary>
        /// The scheduler.
        /// </summary>
        private Timer scheduler;
        #endregion

        #region ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Crawler"/> class.
        /// </summary>
        /// <param name="observer">The observer of the instance to create.</param>
        /// <param name="toObserve">The source to observe.</param>
        public Crawler([NotNull] ICrawlerObserver observer, [NotNull]Source toObserve)
        {
            this.observer = observer;
            this.toObserve = toObserve;
            this.lastIndexedHeadings = new HashSet<string>();
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
                this.scheduler = new Timer(callBack, null, 0, Interval);
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

        //// Private methods

        #region PerformObservation
        /// <summary>
        /// Performs the observation. Callback event of <see cref="scheduler"/>
        /// </summary>
        /// <param name="timerState">timer state object (never used).</param>
        private void PerformObservation(object timerState)
        {
            using (new PerfTracer(nameof(PerformObservation) + this.toObserve.Name))
            {
                switch (this.toObserve.Type)
                {
                    case ContentType.WebSite:
                        //TODO
                        break;
                    case ContentType.Rss:
                        ObserveSyndicationFeed();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        #endregion

        #region ObserveSyndicationFeed
        /// <summary>
        /// Performs the observation of a syndication feed (therefore <see cref="toObserve"/>.ContentType == <see cref="ContentType.Rss"/>
        /// </summary>
        private void ObserveSyndicationFeed()
        {
            try
            {
                using (var reader = XmlReader.Create(this.toObserve.Uri))
                {
                    var feed = SyndicationFeed.Load(reader);
                    reader.Close();

                    var oldItems = this.lastIndexedHeadings;
                    var currentItems = new HashSet<string>();
                    foreach (var item in feed.Items)
                    {
                        if (item != null)
                        {
                            currentItems.Add(item.Title.Text);
                            if (!oldItems.Contains(item.Title.Text))
                            {
                                this.observer.NotifyNewRssFeedFoundThreadSave(item, toObserve.Id);
                            }
                        }
                    }

                    if (currentItems.Count < 1000)
                    {
                        currentItems.UnionWith(oldItems);
                    }

                    this.lastIndexedHeadings = currentItems;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion
    }
}