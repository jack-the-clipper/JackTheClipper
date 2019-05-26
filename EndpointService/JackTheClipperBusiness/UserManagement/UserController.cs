using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JackTheClipperBusiness.CrawlerManagement;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Extensions;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.Localization;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;
using static JackTheClipperBusiness.MailController;

namespace JackTheClipperBusiness.UserManagement
{
    /// <summary>
    /// Contains Methods to get Articles from the ElasticServer, which match the Users preferences
    /// Also contains Methods which correspond to the class name
    /// </summary>
    internal class UserController : IClipperUserAPI, IClipperSystemAdministratorAPI, IClipperStaffChiefAPI
    {
        /// <summary>
        /// Tries to authenticate the user.
        /// </summary>
        /// <param name="userMailOrName">The user email or name.</param>
        /// <param name="userPassword">The password of the user.</param>
        /// <param name="principalUnit">The principal unit.</param>
        /// <returns>The <see cref="User"/>; if authenticated successfully</returns>
        public User TryAuthenticateUser(string userMailOrName, string userPassword, Guid principalUnit)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().GetUserByCredentials(userMailOrName, userPassword, principalUnit, true);
        }

        /// <summary>
        /// Gets the feed definitions of the given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Collection of feeds for the given user.</returns>
        public IReadOnlyCollection<Feed> GetFeedDefinitions(User user)
        {
            var userSettings = GetUserSettings(user.Id);
            return userSettings != null ? userSettings.Feeds : new List<Feed>();
        }

        /// <summary>
        /// Gets the feed data.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="feedId">The feed id.</param>
        /// <param name="page">The requested page.</param>
        /// <param name="showArchived">A value indicating whether the archived articles should be shown or not.</param>
        /// <returns>List of <see cref="ShortArticle"/>s within the feed.</returns>
        public IReadOnlyCollection<ShortArticle> GetFeed(Guid userId, Guid feedId, int page, bool showArchived)
        {
            var adapter = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>();
            var data = adapter.GetFeedRequestData(userId, feedId);
            var since = showArchived ? DateTime.MinValue : data.Item2;
            var inheritedUnit = adapter.GetUnitInheritedBlackList(userId);
            return DatabaseAdapterFactory.GetControllerInstance<IIndexerService>().GetFeedAsync(data.Item1, since, data.Item3, page, inheritedUnit).Result;
        }

        /// <summary>
        /// Gets a specific article.
        /// </summary>
        /// <param name="articleId">The article id.</param>
        /// <returns>The (full) <see cref="Article"/>.</returns>
        public Article GetArticle(Guid articleId)
        {
            return DatabaseAdapterFactory.GetControllerInstance<IIndexerService>().GetArticleAsync(articleId).Result;
        }

        /// <summary>
        /// Gets the user settings.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>The settings of the given user.</returns>
        public UserSettings GetUserSettings(Guid userId)
        {
            return DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().GetUserSettingsByUserId(userId);
        }

        /// <summary>
        /// Attempts to reset the password.
        /// </summary>
        /// <param name="userMail">The users mail address.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult ResetPassword(string userMail)
        {
            var result = InnerResetPassword(userMail);
            if (result.IsSucceeded())
            {
                QuerySendMailAsync(new Notifiable(userMail, userMail), ClipperTexts.PasswordResetMailSubject,
                                   string.Format(ClipperTexts.PasswordResetMailBody, result.Result));
            }

            return result;
        }

        /// <summary>
        /// Attempts to change the password.
        /// </summary>
        /// <param name="user">The users mail address</param>
        /// <param name="newPassword">The new user password.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult ChangePassword(User user, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new InvalidDataException("Invalid password");
            }
            var result = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().ChangePassword(user, newPassword);
            if (result.IsSucceeded())
            {
                QuerySendMailAsync(user, ClipperTexts.PasswordChangedMailSubject, ClipperTexts.PasswordChangedMailBody);
            }

            return result;
        }

        /// <summary>
        /// Changes the users mail address.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="newUserMail">The new mail address.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult ChangeMailAddress(User user, string newUserMail)
        {
            if (string.IsNullOrWhiteSpace(newUserMail) || (newUserMail.Contains("@") == false))
            {
                throw new InvalidDataException("Invalid mail address.");
            }

            var oldMail = user.MailAddress;
            var result = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().ChangeMailAddress(user, newUserMail);
            if (result.IsSucceeded())
            {
                string newPasswordAppender = null;
                if (user.MustChangePassword)
                {
                    var innerResult = InnerResetPassword(newUserMail);
                    if (innerResult.IsSucceeded())
                    {
                        newPasswordAppender =
                            string.Format(ClipperTexts.NewlyGeneratedPasswordOnMailReset, innerResult.Result);
                    }
                }

                QuerySendMailAsync(user, ClipperTexts.MailChangedMailSubject, ClipperTexts.MailChangedMailBody + newPasswordAppender);
                QuerySendMailAsync(new Notifiable(oldMail, user.UserName), ClipperTexts.MailChangedMailSubject, ClipperTexts.MailChangedMailBody);
            }

            return result;
        }

        /// <summary>
        /// Gets the available sources for a given user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>List of available sources.</returns>
        public IReadOnlyList<Source> GetAvailableSources(Guid user)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().GetAvailableSources(user);
        }

        public MethodResult AddUser(User toAdd, string password, Guid selectedUnit)
        {
            var adapter = Factory.GetControllerInstance<IClipperDatabase>();
            var toNotify = adapter.GetEligibleStaffChiefs(selectedUnit);
            if (!toNotify.Any())
            {
                throw new InvalidOperationException("There is no possible staff chief which could approve the registration.");
            }

            var userMail = toAdd.MailAddress;
            if (string.IsNullOrWhiteSpace(userMail) || (userMail.Contains("@") == false) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidDataException("Invalid mail or password.");
            }

            var innerResult = adapter.AddUser(toAdd.MailAddress, toAdd.UserName, password, toAdd.Role,
                                              toAdd.PrincipalUnitId, selectedUnit, false, false);
            if (innerResult.IsSucceeded())
            {
                foreach (var user in toNotify)
                {
                    QuerySendMailAsync(user, ClipperTexts.NewPendingUserMailSubject,
                                       string.Format(ClipperTexts.NewPendingUserMailBody, toAdd.UserName, toAdd.UserMailAddress));
                }
            }

            return innerResult;
        }

        /// <summary>
        /// Saves the user settings.
        /// </summary>
        /// <param name="settingsId">The settings identifier.</param>
        /// <param name="notificationCheckInterval">The notification check interval.</param>
        /// <param name="notificationSetting">The notification setting.</param>
        /// <param name="articlesPerPage">The articles per page.</param>
        public void SaveUserSettings(Guid settingsId, int notificationCheckInterval, NotificationSetting notificationSetting,
                                     int articlesPerPage)
        {
            Factory.GetControllerInstance<IClipperDatabase>().SaveUserSettings(settingsId, notificationCheckInterval, notificationSetting, articlesPerPage);
        }


        /// <summary>
        /// Adds the given feed.
        /// </summary>
        /// <param name="settingsId">The settings id.</param>
        /// <param name="feed">The feed to add.</param>
        public void AddFeed(Guid settingsId, Feed feed)
        {
            Factory.GetControllerInstance<IClipperDatabase>().AddFeed(settingsId, feed);
        }

        /// <summary>
        /// Modifies the feed.
        /// </summary>
        /// <param name="settingsId">The settings id.</param>
        /// <param name="feed">The feed to add.</param>
        public void ModifyFeed(Guid settingsId, Feed feed)
        {
            Factory.GetControllerInstance<IClipperDatabase>().ModifyFeed(settingsId, feed);
        }

        /// <summary>
        /// Deletes the feed.
        /// </summary>
        /// <param name="feedId">The feed id.</param>
        public void DeleteFeed(Guid feedId)
        {
            Factory.GetControllerInstance<IClipperDatabase>().DeleteFeed(feedId);
        }

        /// <summary>
        /// Adds the given source.
        /// </summary>
        /// <param name="user">The user who requests the addition.</param>
        /// <param name="toAdd">The source to add.</param>
        /// <returns>MethodResult indicating success</returns>
        public MethodResult AddSource(User user, Source toAdd)
        {
            Factory.GetControllerInstance<IClipperDatabase>().AddSource(toAdd);
            CrawlerController.GetCrawlerController().Restart();
            return new MethodResult();
        }

        /// <summary>
        /// Deletes the given source.
        /// </summary>
        /// <param name="user">The user who requests the deletion.</param>
        /// <param name="toDelete">The source to delete.</param>
        /// <returns>MethodResult indicating success</returns>
        public MethodResult DeleteSource(User user, Guid toDelete)
        {
            Factory.GetControllerInstance<IClipperDatabase>().DeleteSource(toDelete);
            CrawlerController.GetCrawlerController().Restart();
            return new MethodResult();
        }

        /// <summary>
        /// Changes the source.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="updatedSource">The updated source.</param>
        /// <returns>MethodResult indicating success</returns>
        public MethodResult ChangeSource(User user, Source updatedSource)
        {
            Factory.GetControllerInstance<IClipperDatabase>().UpdateSource(updatedSource);
            CrawlerController.GetCrawlerController().Restart();
            return new MethodResult();
        }

        /// <summary>
        /// Adds the client.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="name">The name.</param>
        /// <param name="pbMail">The pb mail.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult AddPrincipalUnit(User user, string name, string pbMail)
        {
            var password = PasswordGenerator.GeneratePw();
            var controller = Factory.GetControllerInstance<IClipperDatabase>();
            var result = controller.AddPrincipalUnit(name, pbMail, password);
            if (result.IsSucceeded() && result.Result.Item1 != Guid.Empty && result.Result.Item2 != Guid.Empty)
            {
                var newUser = controller.GetUserById(result.Result.Item2);
                QuerySendMailAsync(newUser, "Ihr Account für die Organisation " + name + " wurde erstellt.",
                    $"Willkommen bei Jack the Clipper!\nIhr Account wurde erfolgreich erstellt. \n \nSie können sich unter folgendem Link anmelden: {AppConfiguration.MailConfigurationFELoginLink}{name.Replace(" ", "%20")} \n Ihre Anmeldedaten lauten wie folgt: \n\n Mail Adresse: {pbMail} (Alternativ können Sie auch Ihren Benutzernamen verwenden: {newUser.UserName} ) \nPasswort: {password} \n \n Viel Spaß wünscht Ihnen \n \n Jack the Clipper");
                return new MethodResult();
            }

            return result;
        }

        /// <summary>
        /// Gets all principal units.
        /// </summary>
        /// <returns>The principal units</returns>
        public IReadOnlyList<OrganizationalUnit> GetPrincipalUnits()
        {
            return Factory.GetControllerInstance<IClipperDatabase>().GetPrincipalUnits();
        }

        /// <summary>
        /// Deletes the User
        /// </summary>
        /// <param name="userId">The userId of the User to delete</param>
        /// <returns>MethodResult if successful</returns>
        public MethodResult DeleteUser(Guid userId)
        {
            return Factory.GetControllerInstance<IClipperDatabase>().DeleteUser(userId);
        }

        /// <summary>
        /// Administratively adds a user.
        /// </summary>
        /// <param name="toAdd">The user to add.</param>
        /// <param name="userUnits">The users units.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult AdministrativelyAddUser(User toAdd, IReadOnlyList<Guid> userUnits)
        {
            var password = PasswordGenerator.GeneratePw();
            var result = Factory.GetControllerInstance<IClipperDatabase>().AddUser(toAdd.MailAddress, toAdd.UserName,
                                                                                   password, toAdd.Role, toAdd.PrincipalUnitId,
                                                                                   userUnits[0], true, true);
            MethodResult innerResult = result;
            if (result.IsSucceeded())
            {

                if (userUnits.Count > 1)
                {
                    innerResult = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>()
                                                        .SetUserOrganizationalUnits(result.Result, userUnits.Skip(1));
                }

                QuerySendMailAsync(toAdd, ClipperTexts.AccountCreatedMailSubject,
                                   string.Format(ClipperTexts.AccountCreatedMailBody, toAdd.UserName, password));
            }

            return innerResult;
        }

        /// <summary>
        /// Modifies the user.
        /// </summary>
        /// <param name="toModify">The user to modify.</param>
        /// <param name="userUnits">The users units.</param>
        /// <returns>MethodResult indicating success.</returns>
        public MethodResult ModifyUser(User toModify, IReadOnlyList<Guid> userUnits)
        {
            var adapter = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>();
            var currentUser = GetUserInfo(toModify.Id);
            if (toModify.UserMailAddress != currentUser.MailAddress)
            {
               var result = ChangeMailAddress(toModify, toModify.UserMailAddress);
               if (!result.IsSucceeded())
               {
                   return result;
               }
            }

            if (toModify.UserName != currentUser.UserName || toModify.Role != currentUser.Role ||
                toModify.IsValid != currentUser.IsValid ||
                !userUnits.ToHashSet().SetEquals(currentUser.OrganizationalUnits.Select(x => x.Id)))
            {
                return adapter.ModifyUser(toModify.Id, toModify.UserName, toModify.Role, toModify.IsValid, userUnits);
            }

            return new MethodResult();
        }

        /// <summary>
        /// Gets the minimal information of the users a staffchief can manage
        /// </summary>
        /// <param name="userId">The id of the staffchief</param>
        /// <returns>A list of <see cref="BasicUserInformation"/> if the given id actually belonged to a staffchief</returns>
        public IReadOnlyList<BasicUserInformation> GetManageableUsers(Guid userId)
        {
            return DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().GetManageableUsers(userId);
        }

        /// <summary>
        /// Gets all information on a user 
        /// </summary>
        /// <param name="requested">The id of the user whose information is requested</param>
        /// <returns>All the information on a user like the <see cref="OrganizationalUnit"/>s he belongs to</returns>
        public ExtendedUser GetUserInfo(Guid requested)
        {
            return DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().GetUserInfo(requested);
        }

        /// <summary>
        /// Resets a given users password
        /// </summary>
        /// <param name="userMail">The mail of the of which the pw should be reset.</param>
        /// <returns>MethodResult containing generated password.</returns>
        private static MethodResult<string> InnerResetPassword(string userMail)
        {
            var newPassword = PasswordGenerator.GeneratePw();
            var result = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>().ResetPassword(userMail, newPassword);
            return result.IsSucceeded()
                ? new MethodResult<string>(newPassword)
                : new MethodResult<string>(result == null ? SuccessState.UnknownError : result.Status, result?.UserMessage, null);
        }

        private class Notifiable : IMailNotifiable
        {
            /// <summary>
            /// The email address of the notifiable object
            /// </summary>
            public string UserMailAddress { get; private set; }

            /// <summary>
            /// The name of the notifiable object
            /// </summary>
            public string UserName { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Notifiable"/> class.
            /// </summary>
            /// <param name="userMailAddress">The mail address</param>
            /// <param name="userName">The username</param>
            public Notifiable(string userMailAddress, string userName)
            {
                UserMailAddress = userMailAddress;
                UserName = userName;
            }
        }
    }
}
