using System;
using System.Collections.Generic;
using JackTheClipperBusiness.CrawlerManagement;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;
using User = JackTheClipperCommon.SharedClasses.User;

namespace JackTheClipperBusiness.UserManagement
{
    /// <summary>
    /// Contains Methods to get Articles from the ElasticServer, which match the Users preferences
    /// Also contains Methods which correspond to the class name
    /// </summary>
    internal class UserController : IClipperUserAPI, IClipperSystemAdministratorAPI
    {
        public User TryAuthenticateUser(string userMailOrName, string userPassword)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().GetUserByCredentials(userMailOrName, userPassword);
        }

        public IReadOnlyCollection<Feed> GetFeedDefinitions(User user)
        {
            var userSettings = GetUserSettings(user);
            return userSettings != null ? userSettings.Feeds : new List<Feed>();
        }

        public IReadOnlyCollection<ShortArticle> GetFeed(User user, Feed feed)
        {
            return DatabaseAdapterFactory.GetControllerInstance<IIndexerService>().GetFeed(user, feed);
        }

        public Article GetArticle(User user, ShortArticle article)
        {
            throw new NotImplementedException();
        }

        public UserSettings GetUserSettings(User user)
        {
            return user.Settings;
        }

        public MethodResult SaveUserSettings(User user, UserSettings toSave)
        {
            Factory.GetControllerInstance<IClipperDatabase>().SaveUserSettings(user, toSave);
            return new MethodResult();
        }

        public MethodResult ResetPassword(string userMail)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Source> GetAvailableSources(User user)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().GetAvailableSources(user);
        }

        public User AddUser(string userMail, string userName, string password, JackTheClipperCommon.Enums.Role role, Guid unit)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().AddUser(userMail, userName, password, role, unit);
        }

        public MethodResult AddSource(User user, Source toAdd)
        {
            Factory.GetControllerInstance<IClipperDatabase>().AddSource(toAdd);
            CrawlerControllerBAD.GetCrawlerController().Restart();
            return new MethodResult();
        }

        public MethodResult DeleteSource(User user, Source toAdd)
        {
            //Factory.GetControllerInstance<IClipperDatabase>()(toAdd);
            return new MethodResult();
        }
    }
}