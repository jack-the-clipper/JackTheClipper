using System;
using System.IO;
using JackTheClipperCommon;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.SharedClasses;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperCommonTest
{
    [TestClass]
    public class ArticleComparerTest
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            var config = new ConfigurationBuilder()
                         .AddJsonFile(new FileInfo(@"..\..\..\..\JackTheClipperRequestHandler\appsettings.json").FullName, optional: false)
                         .Build();
            AppConfiguration.RegisterConfig(config);
        }

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
