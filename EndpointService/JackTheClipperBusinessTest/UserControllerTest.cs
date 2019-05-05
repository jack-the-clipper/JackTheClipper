using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JackTheClipperBusiness;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Extensions;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperBusinessTest
{
    [TestClass]
    public class UserControllerTest
    {
        [TestMethod]
        public void AuthenticationFailTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("wrong", "just wrong");

            Assert.IsNull(user);
        }

        [TestMethod]
        public void AuthenticationTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("root@example.com", "root");

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void GetFeedDefinitionsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort");

            Assert.IsNotNull(user);

            var feeds = userController.GetFeedDefinitions(user);

            Assert.IsNotNull(feeds);

            Assert.AreEqual(2, feeds.Count);

            var feed = feeds.FirstOrDefault(e => e.Name == "TODO");

            Assert.IsNotNull(feed);

            Assert.IsTrue(feed.Filter.Keywords.Any(t => t == "TODO"));
        }

        [TestMethod]
        public void GetFeedTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort");

            Assert.IsNotNull(user);

            var feeds = userController.GetFeedDefinitions(user);

            Assert.IsNotNull(feeds);

            Assert.AreEqual(2, feeds.Count);

            var feed = feeds.FirstOrDefault(e => e.Name == "TODO");

            Assert.IsNotNull(feed);

            var elasticFeed = userController.GetFeed(user, feed, 0, false);

            Assert.IsNotNull(elasticFeed);

            Assert.IsTrue(elasticFeed.Count > 0);
        }

        [TestMethod]
        public void GetArticleTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort");

            Assert.IsNotNull(user);

            var feeds = userController.GetFeedDefinitions(user);

            Assert.IsNotNull(feeds);

            Assert.AreEqual(2, feeds.Count);

            var feed = feeds.FirstOrDefault(e => e.Name == "TODO");

            Assert.IsNotNull(feed);

            var elasticFeed = userController.GetFeed(user, feed, 0, false);

            Assert.IsNotNull(elasticFeed);

            Assert.IsTrue(elasticFeed.Count > 0);

            var article = userController.GetArticle(elasticFeed.First().Id);

            Assert.IsNotNull(article);
        }

        [TestMethod]
        public void GetUserSettingsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort");

            Assert.IsNotNull(user);

            var settings = userController.GetUserSettings(user);

            Assert.IsNotNull(settings);
        }

        [TestMethod]
        public void SaveUserSettingsTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort");

            Assert.IsNotNull(user);

            var settings = userController.GetUserSettings(user);

            Assert.IsNotNull(settings);

            var newUserSettings = new UserSettings(settings.Id, settings.Feeds,
                settings.NotificationSettings, settings.NotificationCheckIntervalInMinutes + 1,
                settings.ArticlesPerPage);

            var result = userController.SaveUserSettings(user, newUserSettings);

            Assert.IsTrue(result.IsSucceeded());
        }

        [TestMethod]
        public void GetAvailableSourcesTest()
        {
            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort");

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

            var user = userController.AddUser(milliseconds.ToString(), milliseconds.ToString(), "password", Role.User, Guid.NewGuid(),
                false, true);

            Assert.IsNotNull(user);

            var testuser = userController.TryAuthenticateUser(milliseconds.ToString(), "password");

            Assert.IsNotNull(testuser);

            Assert.AreEqual(user.Id, testuser.Id);
            Assert.AreEqual(user.IsValid, testuser.IsValid);
            Assert.AreEqual(user.MailAddress, testuser.MailAddress);
            Assert.AreEqual(user.Role, testuser.Role);
            Assert.AreEqual(user.UserName, testuser.UserName);
        }
    }
}