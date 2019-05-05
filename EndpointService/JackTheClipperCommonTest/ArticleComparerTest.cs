using System;
using JackTheClipperCommon;
using JackTheClipperCommon.SharedClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperCommonTest
{
    [TestClass]
    public class ArticleComparerTest
    {
        [TestMethod]
        public void CompareArticleSuccess()
        {
            var guid = Guid.NewGuid();
            var indexedguid = Guid.NewGuid();
            var articleOne = new Article(guid, "Titel",
                 "http://link.de", "http://ilink.de", "hbdsjhbhjkndsjkhnnjuknjkhnjknjhkjnkgfnjknjknjknjknjknjknjknjkjnknjknjknjkjnkjnknjknjkjnknjkasfsadsdfgasdfgfdsgdsgfdsgdsfgsdfgdsfgdfsgsdfgsdgfsdfgsdfgdfsgdsfgsdgdsgdsfgdsfgdsgdsfgdfgsdgdsfgdsfjnknjknjkgjnknjknjkjnknjkhzjvbdsabzhdfsbhz",
                DateTime.Today, DateTime.Today, indexedguid);

            var articleTwo = new Article(guid, "Titel",
                "http://link.de", "http://ilink.de", "hbdsjhbhjkndsjkhnnjuknjkhnjknjhkjnkgfnjknjknjknjknjknjknjknjkjnknjknjknjkjnkjnknjknjkjnknjkasfsadsdfgasdfgfdsgdsgfdsgdsfgsdfgdsfgdfsgsdfgsdgfsdfgsdfgdfsgdsfgsdgdsgdsfgdsfgdsgdsfgdfgsdgdsfgdsfjnknjknjkgjnknjknjkjnknjkhzjvbdsabzhdfsbhz",
                DateTime.Today, DateTime.Today, indexedguid);

            Assert.IsTrue(ArticleComparer.FullArticleComparer.Equals(articleOne, articleTwo));
        }

        [TestMethod]
        public void CompareArticleFail()
        {
            var guid = Guid.NewGuid();
            var indexedguid = Guid.NewGuid();
            var articleOne = new Article(guid, "Titel",
                "http://link.de", "http://ilink.de","hbdsjhbhjkndsjkhnnjuknjkhnjknjhkjnkgfnjknjknjknjknjknjknjknjkjnknjknjknjkjnkjnknjknjkjnknjkasfsadsdfgasdfgfdsgdsgfdsgdsfgsdfgdsfgdfsgsdfgsdgfsdfgsdfgdfsgdsfgsdgdsgdsfgdsfgdsgdsfgdfgsdgdsfgdsfjnknjknjkgjnknjknjkjnknjkhzjvbdsabzhdfsbhz",
                DateTime.Today, DateTime.Today, indexedguid);

            var articleTwo = new Article(guid, "Tit",
                "http://link.de", "http://ilink.de", "hbdsjhbhjkndsjkhnnjuknjkhnjknjhkjnkgfnjknjknjknjknjknjknjknjkjnknjknjknjkjnkjnknjknjkjnknjkasfsadsdfgasdfgfdsgdsgfdsgdsfgsdfgdsfgdfsgsdfgsdgfsdfgsdfgdfsgdsfgsdgdsgdsfgdsfgdsgdsfgdfgsdgdsfgdsfjnknjknjkgjnknjknjkjnknjkhzjvbdsabzhdfsbhz",
                DateTime.Today, DateTime.Today, indexedguid);

            Assert.IsFalse(ArticleComparer.FullArticleComparer.Equals(articleOne, articleTwo));
        }
    }
}
