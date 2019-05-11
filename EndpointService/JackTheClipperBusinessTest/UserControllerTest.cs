using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        internal static readonly Guid SystemUnit = Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000");

        [TestMethod]
        public void AuthenticationFailTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            User user = null;
            try
            {
                user = userController.TryAuthenticateUser("wrong", "just wrong", SystemUnit);
                Assert.Fail();
            }
            catch (Exception)
            {
                // ignored
            }


            Assert.IsNull(user);
        }

        [TestMethod]
        public void AuthenticationTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("root@example.com", "root", SystemUnit);

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void AuthenticationTestLoginByName()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("roor", "root", SystemUnit);

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void GetFeedDefinitionsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);

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

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);

            Assert.IsNotNull(user);

            var feeds = userController.GetFeedDefinitions(user);

            Assert.IsNotNull(feeds);

            var feed = feeds.FirstOrDefault(e => e.Name == "Test1");

            Assert.IsNotNull(feed);

            var elasticFeed = userController.GetFeed(user, feed, 0, true);

            Assert.IsNotNull(elasticFeed);

            Assert.IsTrue(elasticFeed.Count > 0);
        }

        [TestMethod]
        public void GetArticleTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);

            Assert.IsNotNull(user);

            var feeds = userController.GetFeedDefinitions(user);

            Assert.IsNotNull(feeds);

            var feed = feeds.FirstOrDefault(e => e.Name == "Test1");

            Assert.IsNotNull(feed);

            var elasticFeed = userController.GetFeed(user, feed, 0, true);

            Assert.IsNotNull(elasticFeed);

            Assert.IsTrue(elasticFeed.Count > 0);

            var article = userController.GetArticle(elasticFeed.First().Id);

            Assert.IsNotNull(article);
        }

        [TestMethod]
        public void GetUserSettingsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);

            Assert.IsNotNull(user);

            var settings = userController.GetUserSettings(user);

            Assert.IsNotNull(settings);
        }

        [TestMethod]
        public void SaveUserSettingsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);

            Assert.IsNotNull(user);

            var settings = userController.GetUserSettings(user);

            Assert.IsNotNull(settings);

            var random = new Random();
            var pages = random.Next(5, 100);
            var time = random.Next();
            userController.SaveUserSettings(settings.Id, time, NotificationSetting.None, pages);

            user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);
            settings = userController.GetUserSettings(user);
            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ArticlesPerPage, pages);
            Assert.AreEqual(settings.NotificationCheckIntervalInMinutes, time);
            Assert.AreEqual(settings.NotificationSettings, NotificationSetting.None);
        }

        [TestMethod]
        public void AddModifyDeleteFeedTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();
            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);
            Assert.IsNotNull(user);
            var settings = userController.GetUserSettings(user);
            Assert.IsNotNull(settings);

            var sources = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().GetAvailableSources(user);
            Assert.IsNotNull(sources);
            Assert.IsTrue(sources.Count > 3);

            var now = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var toAdd = new Feed(Guid.Empty, sources.Take(2).ToList(), new Filter(Guid.Empty, new List<string>(), new List<string>(), new List<string>()), now);
            userController.AddFeed(settings.Id, toAdd);

            //Reload
            user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);
            settings = userController.GetUserSettings(user);
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.Feeds.Any(x => x.Name == now));
            var feed = settings.Feeds.First(x => x.Name == now);
            Assert.IsTrue(feed.Sources.SequenceEqual(sources.Take(2)));
            Assert.IsTrue(feed.Filter.Equals(new Filter(feed.Filter.Id, new List<string>(), new List<string>(), new List<string>())));

            var updatedFeed = new Feed(feed.Id, sources.Take(3).ToList(), new Filter(feed.Filter.Id, new[] {now}, new string[0], new string[0]), now + "test");
            userController.ModifyFeed(settings.Id, updatedFeed);

            user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);
            settings = userController.GetUserSettings(user);
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.Feeds.Any(x => x.Name == now + "test"));
            feed = settings.Feeds.First(x => x.Name == now + "test");
            Assert.IsTrue(feed.Sources.SequenceEqual(sources.Take(3)));
            Assert.IsTrue(feed.Filter.Equals(new Filter(feed.Filter.Id, new List<string> { now }, new List<string>(), new List<string>())));

            updatedFeed = new Feed(feed.Id, sources.Take(1).ToList(), feed.Filter, now + "test");
            userController.ModifyFeed(settings.Id, updatedFeed);

            user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);
            settings = userController.GetUserSettings(user);
            Assert.IsNotNull(settings);
            Assert.IsTrue(settings.Feeds.Any(x => x.Name == now + "test"));
            feed = settings.Feeds.First(x => x.Name == now + "test");
            Assert.IsTrue(feed.Sources.SequenceEqual(sources.Take(1)));
            Assert.IsTrue(feed.Filter.Equals(updatedFeed.Filter));

            userController.DeleteFeed(feed.Id);
            user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);
            settings = userController.GetUserSettings(user);
            Assert.IsNotNull(settings);
            Assert.IsFalse(settings.Feeds.Any(x => x.Name == now + "test"));
        }


        [TestMethod]
        public void GetAvailableSourcesTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", SystemUnit);

            Assert.IsNotNull(user);

            var sources = userController.GetAvailableSources(user);

            Assert.IsNotNull(sources);

            Assert.IsTrue(sources.Count > 0);
        }

        [TestMethod]
        public void AddUserTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var user = userController.AddUser(milliseconds.ToString(), milliseconds.ToString(), "password", Role.User,
                Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000"), false, true);

            Assert.IsNotNull(user);

            var testuser = userController.TryAuthenticateUser(milliseconds.ToString(), "password", SystemUnit);

            Assert.IsNotNull(testuser);

            Assert.AreEqual(user.Id, testuser.Id);
            Assert.AreEqual(user.IsValid, testuser.IsValid);
            Assert.AreEqual(user.MailAddress, testuser.MailAddress);
            Assert.AreEqual(user.Role, testuser.Role);
            Assert.AreEqual(user.UserName, testuser.UserName);
        }

        [TestMethod]
        public void AddUserDuplicateUserNameFailsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            Thread.Sleep(10);
            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var user = userController.AddUser(milliseconds.ToString(), milliseconds.ToString(), "password", Role.User,
                Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000"), false, true);

            Assert.IsNotNull(user);

            var testuser = userController.TryAuthenticateUser(milliseconds.ToString(), "password", SystemUnit);

            Assert.IsNotNull(testuser);

            Assert.AreEqual(user.Id, testuser.Id);
            Assert.AreEqual(user.IsValid, testuser.IsValid);
            Assert.AreEqual(user.MailAddress, testuser.MailAddress);
            Assert.AreEqual(user.Role, testuser.Role);
            Assert.AreEqual(user.UserName, testuser.UserName);

            userController.AddUser("othermail" + milliseconds + "@example.com", milliseconds.ToString(), "password", Role.User,
                Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000"), false, true);
            testuser = userController.TryAuthenticateUser("othermail" + milliseconds + "@example.com", "password", SystemUnit);
            Assert.IsNull(testuser);
        }

        [TestMethod]
        public void ResetPasswordTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var user2 = userController.AddUser(milliseconds.ToString() + @"@example.com", milliseconds.ToString(), "Passwort", Role.User,
                Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000"), false, true);

            var guidOfUser2 = user2.Id;
            var result = userController.ResetPassword(milliseconds.ToString() + @"@example.com");
            Assert.AreEqual(SuccessState.Successful, result.Status);

            var user = Factory.GetObjectInstanceById<User>(guidOfUser2);
            Assert.IsNotNull(user);
            Assert.AreEqual(true, user.MustChangePassword);

            try
            {
                Factory.GetControllerInstance<IClipperUserAPI>()
                    .TryAuthenticateUser(milliseconds.ToString() + @"@example.com", "Passwort", SystemUnit);
                Assert.Fail("Password reset wasn't successfully. Even tho the MethodResult indicated that it was successfully.");

            }
            catch (Exception)
            {
                // ignored
            }
        }

        [TestMethod]
        public void ChangePasswordTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var user2 = userController.AddUser(milliseconds.ToString() + @"@example.com", milliseconds.ToString(), "Passwort", Role.User,
                Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000"), false, true);

            var result = userController.ChangePassword(user2, "hallo123");
            Assert.AreEqual(SuccessState.Successful, result.Status);

            try
            {
                var user = userController.TryAuthenticateUser(milliseconds.ToString(), "hallo123", SystemUnit);
                Assert.IsNotNull(user);
            }
            catch (Exception)
            {
                Assert.Fail("Password wasn't changed successfully.");
            }
        }

        [TestMethod]
        public void ChangeMailAddressTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var user2 = userController.AddUser(milliseconds.ToString() + @"@example.com", milliseconds.ToString(), "Passwort", Role.User,
                Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000"), false, true);


           
            var result = userController.ChangeMailAddress(user2, milliseconds.ToString() + @"@example.de");
            Assert.AreEqual(SuccessState.Successful, result.Status);

            try
            {
                var user = userController.TryAuthenticateUser(milliseconds.ToString() + @"@example.de", "Passwort", SystemUnit);
                Assert.IsNotNull(user);
            }
            catch (Exception)
            {
                
                Assert.Fail("Mail address wasn't changed successfully.");
            }
        }
    }
}
