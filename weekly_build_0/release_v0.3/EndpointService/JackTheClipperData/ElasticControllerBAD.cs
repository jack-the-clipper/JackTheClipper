using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using HtmlAgilityPack;
using JackTheClipperCommon;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.SharedClasses;
using Nest;
using User = JackTheClipperCommon.SharedClasses.User;

namespace JackTheClipperData
{
    //TODO: TIM - entspricht nicht dem Dokukonzept.

    /// <summary>
    /// Class to control every interaction that needs to be done with the ElasticServer.
    /// </summary>   
    public class ElasticController : IIndexerService
    {
        private readonly ConnectionSettings settings = new ConnectionSettings(new Uri("http://127.0.0.1:9200"));

        /// <summary>Index an RSS Item</summary>
        /// <param name="source">The source.</param>
        /// <param name="feedId">The feed identifier to build the article.</param>
        /// <returns></returns>
        public MethodResult IndexRssFeedItemThreadSafe(SyndicationItem source, Guid feedId)
        {
            try
            {
                using (new PerfTracer(nameof(IndexRssFeedItemThreadSafe) + "TOTAL"))
                {
                    var client = new ElasticClient(settings);

                    var htmlstring = GetTextfromHtml(source.Summary?.Text);

                    var article = new Article(Guid.NewGuid(), source.Title.Text, htmlstring,
                        source.Links[0].Uri.ToString(), "", source.PublishDate.UtcDateTime, DateTime.UtcNow, feedId);
                    //using (new PerfTracer(nameof(IndexRssFeedItemThreadSafe) + "DUPLICATES"))
                    {
                        var isThereTheArticle = client.Search<Article>(s => s.Index("article").Size(9999).Query(q => q.Bool(l => l
                                .Should(k => k.Terms(c => c
                                                 .Field("ArticleTitle")
                                                 .Terms(article.Title)) ||
                                             k.Terms(c => c
                                                 .Field("ArticleShortText")
                                                 .Terms(article.ShortText))
                                )
                            )
                        )).Documents;
                        //Console.WriteLine("All indexed Articles: " + all.Count);

                        if (isThereTheArticle.Any() && isThereTheArticle.First() == null)
                        {
                            return new MethodResult(SuccessState.Successful, "Already indexed");
                        }

                    }

                    //using (new PerfTracer(nameof(IndexRssFeedItemThreadSafe) + "INDEX"))
                    {
                        var response = client.Index(article, i => i.Index("article"));
                        if (!response.IsValid)
                        {
                            return new MethodResult(SuccessState.UnknownError, "Couldn't index properly");
                        }
                    }
                    Console.WriteLine("Indexed: " + article.Title);
                    return new MethodResult(SuccessState.Successful, "Index successfully");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new MethodResult(SuccessState.UnknownError, "Couldn't index properly");
            }
        }

        private static string GetTextfromHtml(string source)
        {
            var html = new HtmlDocument();
            html.LoadHtml(source);
            var plainText = new StringBuilder();
            var nodes = html.DocumentNode.SelectNodes("//text()");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node != null)
                    {
                        if (plainText.Length > 0)
                        {
                            var lastChar = plainText[plainText.Length - 1];
                            if (lastChar != ' ' && lastChar != '\n')
                            {
                                plainText.Append(' ');
                            }
                        }

                        var toAdd = WebUtility.HtmlDecode(node.InnerText).Trim().Trim('\n');
                        if (!string.IsNullOrEmpty(toAdd))
                        {
                            plainText.Append(toAdd);
                        }
                    }
                }
            }

            var htmlstring = plainText.ToString();
            return htmlstring;
        }
        
        /// <summary>
        /// Clears the elastic server.
        /// Warning: This is not reversible.
        /// Proceed with Caution
        /// </summary>
        public void ClearIndex()
        {
            using (new PerfTracer(nameof(ClearIndex)))
            {
                var client = new ElasticClient(settings);
                client.DeleteIndex("shortarticle");
                client.DeleteIndex("ShortArticle");
                client.DeleteIndex("article");
                client.DeleteIndex("Article");
            }
        }

        /// <summary>
        /// Gets the feed from the ElasticServer
        /// </summary>
        /// <param name="user">The User the feed is for</param>
        /// <param name="feed">The feed that should be generated</param>
        /// <returns>list of articles for the feed</returns>
        public IReadOnlyCollection<ShortArticle> GetFeed(User user, Feed feed)
        {
            try
            {
                using (new PerfTracer(nameof(GetFeed) + user.MailAddress + feed.Sources.First().Name))
                {
                    var client = new ElasticClient(settings);
                    var keywordlist = feed.Filter.Keywords.Select(x => x.ToLower()).ToList(); // Get all Keywords
                    var sourceIds = feed.Sources.Select(x => x.Id).ToHashSet();

                    var searchResult2 = client.Search<Article>(s => s
                        .Index("article")
                        .From(0)
                        .Size(999) // Defaults to 10 results -> size Parameter
                        .Query(q => q.Bool(l => l
                                .Should(k => k.Terms(c => c
                                                 .Field("ArticleTitle") // May need change
                                                 .Terms(keywordlist)) ||
                                             k.Terms(c => c
                                                 .Field("ArticleShortText")
                                                 .Terms(keywordlist))
                                )
                            )
                        )
                    );

                    IReadOnlyCollection<ShortArticle> articles = searchResult2.Documents
                                                                .Where(x => sourceIds.Contains(x.IndexingSourceId))
                                                                .OrderByDescending(x => x.Published)
                                                                .ToList();
                    Console.WriteLine(articles.Count + " articles for user " + user.MailAddress);
                    return articles;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<ShortArticle>();
            }
        }
        
        /// <summary>Gets a specific Article if its in one of the feed of the User</summary>
        /// <param name="user">The user.</param>
        /// <param name="articleId">The article.</param>
        /// <returns></returns>
        public Article GetArticle(User user, Guid articleId)
        {
            throw new NotImplementedException();
        }

        public Article ExactlyThis(string toEqual, string field)
        {
            string test = "";
            if (field.Equals("link", StringComparison.OrdinalIgnoreCase) 
                || field.Equals("ArticleLink", StringComparison.OrdinalIgnoreCase))
            {
                test = "ArticleLink";
            }
            else if (field.Equals("Guid", StringComparison.OrdinalIgnoreCase) 
                     || field.Equals("ArticleId", StringComparison.OrdinalIgnoreCase)
                        || field.Equals("Id", StringComparison.OrdinalIgnoreCase))
            {
                test = "ArticleId";
            }

            if (test == "")
            {
                return null;
            }
            var client = new ElasticClient(settings);

            var searchResponse = client.Search<Article>(s => s
                .Index("article")
                .From(0)
                .Size(999)
                .Query(q => q.Wildcard(w => w
                    .Field(test)
                    .Value(toEqual))
                )
            );
            var received = searchResponse.Documents.FirstOrDefault();

            return received;
        }
    }
}
