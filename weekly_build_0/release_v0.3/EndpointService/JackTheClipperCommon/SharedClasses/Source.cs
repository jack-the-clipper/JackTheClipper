using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JackTheClipperCommon.Enums;
using JetBrains.Annotations;

namespace JackTheClipperCommon.SharedClasses
{
    /// <summary>
    /// Contains the definition of a Source with its unique Id, its URI, its Name and its ContentType
    /// </summary>
    [DataContract]
    public class Source
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        [DataMember(Name = "SourceId")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the URI.
        /// </summary>
        [DataMember(Name = "SourceUri")]
        public string Uri { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [DataMember(Name = "SourceName")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the content type.
        /// </summary>
        [IgnoreDataMember]
        public ContentType Type { get; private set; }

        /// <summary>
        /// Gets the content type as string.
        /// </summary>
        [DataMember(Name = "SourceContentType")]
        public string ContentTypeAsString
        {
            get { return Type.ToString(); }
            private set { Type = Enum.Parse<ContentType>(value);  }
        }

        /// <summary>
        /// Gets the regex.
        /// </summary>
        [DataMember(Name = "SourceRegex")]
        public string Regex { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [DataMember(Name = "SourceXPath")]
        public string XPath { get; private set; }

        /// <summary>
        /// The black list.
        /// </summary>
        [DataMember(Name = "SourceBlacklist")]
        [NotNull]
        public IReadOnlyList<string> BlackList { get; private set; }

        /// <summary>
        /// Prevents a default instance of the <see cref="Source"/> class from being created.
        /// </summary>
        private Source()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Source"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="regex">The regex.</param>
        /// <param name="xpath">The Xpath.</param>
        public Source(Guid id, string uri, string name, ContentType type, string regex, 
                      string xpath, IReadOnlyList<string> blacklist)
        {
            Id = id;
            Uri = uri;
            Name = name;
            Type = type;
            Regex = regex;
            XPath = xpath;
            BlackList = blacklist;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Source"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="source">The source.</param>
        public Source(Guid id, Source source)
        {
            Id = id;
            Uri = source.Uri;
            Name = source.Name;
            Type = source.Type;
            Regex = source.Regex;
            XPath = source.XPath;
            BlackList = source.BlackList.ToList();
        }
    }
}