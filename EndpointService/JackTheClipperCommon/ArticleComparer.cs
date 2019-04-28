using System.Collections.Generic;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperCommon
{
    /// <summary>
    /// Contains Methods, which compare different types of articles e.g Article and ShortArticle
    /// </summary>
    public class ArticleComparer : IEqualityComparer<ShortArticle>, IEqualityComparer<Article>
    {
        /// <summary>
        /// The short article comparer
        /// </summary>
        public static readonly IEqualityComparer<ShortArticle> ShortArticleComparer = new ArticleComparer();
        
        /// <summary>
        /// The full article comparer
        /// </summary>
        public static readonly IEqualityComparer<Article> FullArticleComparer = new ArticleComparer();

        /// <summary>Determines whether the specified objects are equal.</summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(ShortArticle x, ShortArticle y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.Title, y.Title) && string.Equals(x.ShortText, y.ShortText);
        }

        /// <summary>Returns a hash code for the specified object.</summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj">obj</paramref> is a reference type and <paramref name="obj">obj</paramref> is null.</exception>
        public int GetHashCode(ShortArticle obj)
        {
            unchecked
            {
                var hashCode = (obj.Title != null ? obj.Title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (obj.ShortText != null ? obj.ShortText.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>Determines whether the specified objects are equal.</summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(Article x, Article y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.Title, y.Title) && string.Equals(x.ShortText, y.ShortText) && string.Equals(x.Text, y.Text);
        }

        /// <summary>Returns a hash code for the specified object.</summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj">obj</paramref> is a reference type and <paramref name="obj">obj</paramref> is null.</exception>
        public int GetHashCode(Article obj)
        {
            var hashCode = (obj.Title != null ? obj.Title.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (obj.ShortText != null ? obj.ShortText.GetHashCode() : 0);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            hashCode = (hashCode * 397) ^ (obj.Text != null ? obj.Text.GetHashCode() : 0);
            return hashCode;
        }
    }
}