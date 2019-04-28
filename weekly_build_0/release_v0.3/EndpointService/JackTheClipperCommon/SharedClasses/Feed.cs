using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// Contains Feed definitions:
    /// e.g the name, which words to search for, from where to search from and what to filter out
    /// </summary>
    [DataContract]
    public class Feed
    {
        /// <summary>
        /// Gets the identifier of the feed.
        /// </summary>
        [DataMember(Name = "FeedId")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the name the feed.
        /// </summary>
        [DataMember(Name = "FeedName")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the sources of the feed.
        /// </summary>
        [DataMember(Name = "FeedSources")]
        public IReadOnlyCollection<Source> Sources { get; private set; }

        /// <summary>
        /// Gets the filter of the feed.
        /// </summary>
        [DataMember(Name = "FeedFilter")]
        public Filter Filter { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="Feed"/> class from being created.
        /// </summary>
        private Feed()
        {
            //For serialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Feed"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="feedSources">The feed sources.</param>
        /// <param name="feedFilter">The feed filter.</param>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">feedSources or feedFilter is null</exception>
        public Feed(Guid id, [NotNull] IReadOnlyCollection<Source> feedSources, [NotNull] Filter feedFilter, string name)
        {
            Id = id;
            Sources = feedSources ?? throw new ArgumentNullException(nameof(feedSources));
            Filter = feedFilter ?? throw new ArgumentNullException(nameof(feedFilter));
            Name = name;
        }
    }
}