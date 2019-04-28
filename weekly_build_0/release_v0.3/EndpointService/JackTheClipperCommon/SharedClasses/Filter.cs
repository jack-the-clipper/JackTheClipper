using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// Contains Keywords or Expressions, which should be filtered
    /// </summary>
    [DataContract]
    public class Filter
    {
        /// <summary>
        /// Gets the identifier of the filter.
        /// </summary>
        [DataMember(Name = "FilterId")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the filter keywords.
        /// </summary>
        [NotNull]
        [DataMember(Name = "FilterKeywords")]
        public IReadOnlyCollection<string> Keywords { get; private set; }

        /// <summary>
        /// Gets the filter expressions.
        /// </summary>
        [NotNull]
        [DataMember(Name = "FilterExpressions")]
        public IReadOnlyCollection<string> Expressions { get; private set; }

        /// <summary>
        /// Gets the filter blacklist.
        /// </summary>
        [NotNull]
        [DataMember(Name = "FilterBlacklist")]
        public IReadOnlyCollection<string> Blacklist { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="Filter"/> class from being created.
        /// </summary>
        private Filter()
        {
            //For serialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Filter"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="keywords">The keywords.</param>
        /// <param name="expressions">The expressions.</param>
        /// <param name="blacklist">The blacklist.</param>
        public Filter(Guid id, IReadOnlyCollection<string> keywords, IReadOnlyCollection<string> expressions,
                      IReadOnlyCollection<string> blacklist)
        {
            Id = id;
            Keywords = keywords;
            Expressions = expressions;
            Blacklist = blacklist;
        }
    }
}