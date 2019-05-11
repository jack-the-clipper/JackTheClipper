using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JackTheClipperBusiness;
using JackTheClipperCommon.BusinessObjects;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;
using JackTheClipperData.MariaDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperDataTest
{
    [TestClass]
    public class ElasticControllerTest
    {
        [TestMethod]
        public void IndexRssItemTest()
        {
            var elasticController = DatabaseAdapterFactory.GetControllerInstance<IIndexerService>();
            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var guid = Guid.NewGuid();
            var indexedguid = Guid.NewGuid();
            var articleOne = new Article(guid, "abgef:"+milliseconds,
                "http://" + milliseconds +".de", "http://"+milliseconds+".de", "abgefssmt",
                DateTime.Today, DateTime.Today, indexedguid);

            var rsskey = new RssKey(milliseconds, articleOne.Link);
            try
            {
                elasticController.IndexRssFeedItemThreadSafeAsync(articleOne, rsskey);
                Assert.IsTrue(true);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void IndexArticleTest()
        {
            var elasticController = DatabaseAdapterFactory.GetControllerInstance<IIndexerService>();
            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var guid = Guid.NewGuid();
            var indexedguid = Guid.NewGuid();
            var articleOne = new Article(guid, "abgef"+milliseconds,
                "http://" + milliseconds + ".de", "http://" + milliseconds + ".de", "abgefssmt",
                DateTime.Today, DateTime.Today, indexedguid);

            try
            {
                elasticController.IndexArticleThreadSafeAsync(articleOne);
                Assert.IsTrue(true);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
        }
    }
}
