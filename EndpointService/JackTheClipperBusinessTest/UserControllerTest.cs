using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Xsl;
using JackTheClipperBusiness;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperBusinessTest
{
    [TestClass]
    public class UserControllerTest
    {
        internal static readonly Guid TestUnit = Guid.Parse("6d64d93a-7cad-11e9-910b-9615dc5f263c");
        internal static readonly Guid SystemUnit = Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000");


        [TestMethod]
        public void AuthenticationFailTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("wrong", "just wrong", TestUnit);
            
            Assert.IsNull(user);
        }

        [TestMethod]
        public void AuthenticationTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();
            var user = userController.TryAuthenticateUser("timroethel@web.de", "Passwort", TestUnit);

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void GetFeedDefinitionsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);

            Assert.IsNotNull(user);

            var feeds = userController.GetFeedDefinitions(user);

            Assert.IsNotNull(feeds);

            var feed = feeds.FirstOrDefault(e => e.Name == "Test1");

            Assert.IsNotNull(feed);

            Assert.IsTrue(feed.Filter.Keywords.Any(t => t.Equals("Politik", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void GetFeedTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);

            Assert.IsNotNull(user);

            var feeds = userController.GetFeedDefinitions(user);

            Assert.IsNotNull(feeds);

            var feed = feeds.FirstOrDefault(e => e.Name == "Test1");

            Assert.IsNotNull(feed);

            var elasticFeed = userController.GetFeed(user.Id, feed.Id, 0, true);

            Assert.IsNotNull(elasticFeed);

            Assert.IsTrue(elasticFeed.Count > 0);
        }

        [TestMethod]
        public void GetArticleTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);

            Assert.IsNotNull(user);

            var feeds = userController.GetFeedDefinitions(user);

            Assert.IsNotNull(feeds);

            var feed = feeds.FirstOrDefault(e => e.Name == "Test1");

            Assert.IsNotNull(feed);

            var elasticFeed = userController.GetFeed(user.Id, feed.Id, 0, true);

            Assert.IsNotNull(elasticFeed);

            Assert.IsTrue(elasticFeed.Count > 0);

            var article = userController.GetArticle(elasticFeed.First().Id);

            Assert.IsNotNull(article);
        }

        [TestMethod]
        public void GetUserSettingsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);

            Assert.IsNotNull(user);

            var settings = userController.GetUserSettings(user.Id);

            Assert.IsNotNull(settings);
        }

        [TestMethod]
        public void SaveUserSettingsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);

            Assert.IsNotNull(user);

            var settings = userController.GetUserSettings(user.Id);

            Assert.IsNotNull(settings);

            var random = new Random();
            var pages = random.Next(5, 100);
            var time = random.Next();
            userController.SaveUserSettings(settings.Id, time, NotificationSetting.None, pages);

            user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);
            settings = userController.GetUserSettings(user.Id);
            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ArticlesPerPage, pages);
            Assert.AreEqual(settings.NotificationCheckIntervalInMinutes, time);
            Assert.AreEqual(settings.NotificationSettings, NotificationSetting.None);
        }

        /*[TestMethod]
        public void AddModifyDeleteFeedTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();
            var user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);
            Assert.IsNotNull(user);
            var settings = userController.GetUserSettings(user.);
            Assert.IsNotNull(settings);

            var sources = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().GetAvailableSources(user);
            Assert.IsNotNull(sources);
            Assert.IsTrue(sources.Count > 3);

            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var toAdd = new Feed(Guid.Empty, sources.Take(2).ToList(), new Filter(Guid.Empty, new List<string>(), new List<string>(), new List<string>()), now);
            userController.AddFeed(settings.Id, toAdd);

            //Reload
            user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);
            settings = userController.GetUserSettings(user);
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.Feeds.Any(x => x.Name == now));
            var feed = settings.Feeds.First(x => x.Name == now);
            Assert.IsTrue(feed.Sources.SequenceEqual(sources.Take(2)));
            Assert.IsTrue(feed.Filter.Equals(new Filter(feed.Filter.Id, new List<string>(), new List<string>(), new List<string>())));

            var updatedFeed = new Feed(feed.Id, sources.Take(3).ToList(), new Filter(feed.Filter.Id, new[] {now}, new string[0], new string[0]), now + "test");
            userController.ModifyFeed(settings.Id, updatedFeed);

            user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);
            settings = userController.GetUserSettings(user);
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.Feeds.Any(x => x.Name == now + "test"));
            feed = settings.Feeds.First(x => x.Name == now + "test");
            Assert.IsTrue(feed.Sources.SequenceEqual(sources.Take(3)));
            Assert.IsTrue(feed.Filter.Equals(new Filter(feed.Filter.Id, new List<string> { now }, new List<string>(), new List<string>())));

            updatedFeed = new Feed(feed.Id, sources.Take(1).ToList(), feed.Filter, now + "test");
            userController.ModifyFeed(settings.Id, updatedFeed);

            user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);
            settings = userController.GetUserSettings(user);
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.Feeds.Any(x => x.Name == now + "test"));
            feed = settings.Feeds.First(x => x.Name == now + "test");
            Assert.IsTrue(feed.Sources.SequenceEqual(sources.Take(1)));
            Assert.IsTrue(feed.Filter.Equals(updatedFeed.Filter));

            userController.DeleteFeed(feed.Id);
            user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);
            settings = userController.GetUserSettings(user);
            Assert.IsNotNull(settings);
            Assert.IsFalse(settings.Feeds.Any(x => x.Name == now + "test"));
        }*/


        [TestMethod]
        public void GetAvailableSourcesTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);

            Assert.IsNotNull(user);

            var sources = userController.GetAvailableSources(user.Id);

            Assert.IsNotNull(sources);

            Assert.IsTrue(sources.Count > 0);
        }

        [TestMethod]
        public void AddUserDuplicateUserNameFailsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var result = userController.AddUser(new User(Guid.NewGuid(), "blub@blub.de", Role.User, "sticki", Guid.Empty, false, DateTime.Now, true, "TimTest", TestUnit), "lol", TestUnit);

            Assert.AreEqual(SuccessState.UnknownError, result.Status);
        }

        [TestMethod]
        public void ResetPasswordTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var guidOfUser2 = Guid.Parse("38dfe95b-7cae-11e9-910b-9615dc5f263c");
            var result = userController.ResetPassword("reset@reset.com");
            Assert.AreEqual(SuccessState.Successful, result.Status);

            var user = Factory.GetObjectInstanceById<User>(guidOfUser2);
            Assert.IsNotNull(user);
            Assert.AreEqual(true, user.MustChangePassword);

            var nouser = userController.TryAuthenticateUser("reseter", "reset", TestUnit);
            Assert.IsNull(nouser);

            userController.ChangePassword(user, "reset");
            Assert.IsNotNull(userController.TryAuthenticateUser("reseter", "reset", TestUnit));
        }

        [TestMethod]
        public void ChangePasswordTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user2 = userController.TryAuthenticateUser("tester", "Passwort", TestUnit);

            var result = userController.ChangePassword(user2, "hallo123");
            Assert.AreEqual(SuccessState.Successful, result.Status);

            var user = userController.TryAuthenticateUser("tester", "hallo123", TestUnit);
            Assert.IsNotNull(user);

            result = userController.ChangePassword(user2, "Passwort");
            Assert.AreEqual(SuccessState.Successful, result.Status);

            user = userController.TryAuthenticateUser("tester", "Passwort", TestUnit);
            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void ChangeMailAddressTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var user2 = userController.TryAuthenticateUser("tester", "Passwort", TestUnit);

            var result = userController.ChangeMailAddress(user2,"timtest@exam.de");
            Assert.AreEqual(SuccessState.Successful, result.Status);

            var user = userController.TryAuthenticateUser("timtest@exam.de", "Passwort", TestUnit);

            Assert.IsNotNull(user);

            result = userController.ChangeMailAddress(user2, "timtest@example.de");
            Assert.AreEqual(SuccessState.Successful, result.Status);

            user = userController.TryAuthenticateUser("timtest@example.de", "Passwort", TestUnit);

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void AddPrincipalUnitTest()
        {
            var userController = Factory.GetControllerInstance<IClipperSystemAdministratorAPI>();
            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var result = userController.AddPrincipalUnit(null, milliseconds.ToString(), milliseconds+"mail@example.com");

            Assert.AreEqual(SuccessState.Successful, result.Status);

            var principals = userController.GetPrincipalUnits();

            Assert.IsTrue(principals.Any(e=> e.Name == milliseconds.ToString()));
        }

        [TestMethod]
        public void AddFeedTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();
            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var feed = new Feed(Guid.NewGuid(), new List<Source>(), new Filter(Guid.NewGuid(), new List<string>()
                {
                    "Test"
                }, new List<string>(), new List<string>())
                , milliseconds.ToString());

            var user = Factory.GetControllerInstance<IClipperUserAPI>()
                .TryAuthenticateUser("tester", "Passwort", TestUnit);

            userController.AddFeed(user.SettingsId, feed);

            var user2 = Factory.GetControllerInstance<IClipperUserAPI>()
                .TryAuthenticateUser("tester", "Passwort", TestUnit);
            Assert.IsTrue(userController.GetUserSettings(user2.Id).Feeds.Any(e=> e.Name == milliseconds.ToString()));
        }

        [TestMethod]
        public void ModifyFeedTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();
            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var user = Factory.GetControllerInstance<IClipperUserAPI>()
                .TryAuthenticateUser("tester", "Passwort", TestUnit);
            if (userController.GetUserSettings(user.Id).Feeds.Count == 0)
            {
                Assert.IsTrue(true);
                return;
            }
            var feed = new Feed(userController.GetUserSettings(user.Id).Feeds.First().Id, new List<Source>(), new Filter(Guid.NewGuid(), new List<string>()
                {
                    "Test"
                }, new List<string>(), new List<string>())
                , milliseconds+"VERÄNDERT");

            userController.ModifyFeed(userController.GetUserSettings(user.Id).Id, feed);

            var user2 = Factory.GetControllerInstance<IClipperUserAPI>()
                .TryAuthenticateUser("tester", "Passwort", TestUnit);
            Assert.IsTrue(userController.GetUserSettings(user2.Id).Feeds.Any(e => e.Name == milliseconds+"VERÄNDERT"));
        }
        [TestMethod]
        public void DeleteFeedTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = Factory.GetControllerInstance<IClipperUserAPI>()
                .TryAuthenticateUser("tester", "Passwort", TestUnit);

            if (userController.GetUserSettings(user.Id).Feeds.Count == 0)
            {
                Assert.IsTrue(true);
                return;
            }

            var feedCount = userController.GetUserSettings(user.Id).Feeds.Count;

            userController.DeleteFeed(userController.GetUserSettings(user.Id).Feeds.First().Id);

            var user2 = Factory.GetControllerInstance<IClipperUserAPI>()
                .TryAuthenticateUser("tester", "Passwort", TestUnit);

            Assert.AreEqual(feedCount - 1, userController.GetUserSettings(user2.Id).Feeds.Count);
        }

        [TestMethod]
        public void SourceManagementTest()
        {
            var controller = Factory.GetControllerInstance<IClipperSystemAdministratorAPI>();

            var user = Factory.GetControllerInstance<IClipperUserAPI>().TryAuthenticateUser("SA", "123", SystemUnit);

            var count = Factory.GetControllerInstance<IClipperUserAPI>().GetAvailableSources(user.Id).Count;

            var source = new Source(Guid.NewGuid(), "test.de", "Test", ContentType.Rss, "", "", new List<string>());

            controller.AddSource(null, source);

            user = Factory.GetControllerInstance<IClipperUserAPI>().TryAuthenticateUser("SA", "123", SystemUnit);

            var available = Factory.GetControllerInstance<IClipperUserAPI>().GetAvailableSources(user.Id);

            Assert.IsTrue(available.Any(e => e.Name == "Test"));

            var sourceChanged = new Source(source.Id, source.Uri, "Changed", ContentType.Rss, "", "", new List<string>());

            controller.ChangeSource(null, sourceChanged);

            user = Factory.GetControllerInstance<IClipperUserAPI>().TryAuthenticateUser("SA", "123", SystemUnit);

            available = Factory.GetControllerInstance<IClipperUserAPI>().GetAvailableSources(user.Id);

            Assert.IsTrue(available.Any(e => e.Name == "Changed"));

            controller.DeleteSource(null, available.FirstOrDefault(e => e.Name == "Changed").Id);

            user = Factory.GetControllerInstance<IClipperUserAPI>().TryAuthenticateUser("SA", "123", SystemUnit);

            var count2 = Factory.GetControllerInstance<IClipperUserAPI>().GetAvailableSources(user.Id).Count;

            Assert.AreEqual(count, count2);
        }
    }
}
