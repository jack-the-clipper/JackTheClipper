using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace JackTheClipperBusiness.CrawlerManagement
{
    /// <summary>
    /// Extensions for crawling process.
    /// </summary>
    public static class CrawlerExtensions
    {
        #region GetTextFromHtml
        /// <summary>
        /// Extracts the Text of a HTML string into a normal string
        /// </summary>
        /// <param name="source">The string to convert.</param>
        /// <returns>Plain text.</returns>
        public static string GetTextFromHtml(this string source)
        {
            var html = new HtmlDocument();
            html.LoadHtml(source);
            return GetTextFromHtml(html);
        }

        /// <summary>
        /// Extracts the Text of a HTML string into a normal string
        /// </summary>
        /// <param name="source">The source to convert.</param>
        /// <returns>Plain text.s</returns>
        public static string GetTextFromHtml(this HtmlDocument source)
        {
            var plainText = new StringBuilder();
            var nodes = source.DocumentNode.SelectNodes("//text()");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node != null && !node.InnerText.StartsWith("<?xml version=\""))
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

            return plainText.ToString();
        }
        #endregion

        #region GetAllImages        
        /// <summary>
        /// Gets all images.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>List of image data.</returns>
        public static IReadOnlyCollection<Tuple<string, string>> GetAllImages(this string source)
        {
            var html = new HtmlDocument();
            html.LoadHtml(source);
            return html.GetAllImages();
        }

        /// <summary>
        /// Gets all images.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns>List of image data.</returns>
        public static IReadOnlyCollection<Tuple<string, string>> GetAllImages(this HtmlDocument doc)
        {
            var data = new HashSet<Tuple<string, string>>();
            var nodes = doc.DocumentNode.SelectNodes("//img");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var link = node.GetAttributeValue("src", null);
                    var text = node.GetAttributeValue("alt", null);
                    if (!string.IsNullOrEmpty(link) && !string.IsNullOrEmpty(text))
                    {
                        data.Add(new Tuple<string, string>(link, text));
                    }
                }
            }

            return data;
        }
        #endregion
    }
}