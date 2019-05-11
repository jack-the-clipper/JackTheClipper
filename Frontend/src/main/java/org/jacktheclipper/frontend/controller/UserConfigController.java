package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.authentication.CustomAuthenticationToken;
import org.jacktheclipper.frontend.authentication.User;
import org.jacktheclipper.frontend.enums.NotificationSetting;
import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.Feed;
import org.jacktheclipper.frontend.model.UserSettings;
import org.jacktheclipper.frontend.service.FeedService;
import org.jacktheclipper.frontend.service.SourceService;
import org.jacktheclipper.frontend.service.UserService;
import org.jacktheclipper.frontend.utils.AuthenticationUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContext;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.util.CollectionUtils;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpSession;
import java.util.UUID;

import static org.jacktheclipper.frontend.utils.RedirectAttributesUtils.populateDefaultRedirectAttributes;
import static org.springframework.security.web.context.HttpSessionSecurityContextRepository.SPRING_SECURITY_CONTEXT_KEY;

/**
 * This class covers all pages that a normal user needs to edit his settings. This includes
 * adding new feeds and modifying his notification settings.
 */
@Controller
@RequestMapping("/{organization}/feed")
public class UserConfigController {
    private static final Logger log = LoggerFactory.getLogger(UserConfigController.class);

    private final FeedService feedService;

    private final SourceService sourceService;

    private final UserService userService;

    private final AuthenticationManager authManager;

    @Autowired
    public UserConfigController(FeedService feedService, SourceService sourceService,
                                UserService userService, AuthenticationManager authManager) {

        this.feedService = feedService;
        this.sourceService = sourceService;
        this.userService = userService;
        this.authManager = authManager;
    }

    /**
     * Shows the page where a user can edit his feeds.
     * By default the first feed in the users feed is automatically selected (or an entirely new
     * feed if the user has none).
     *
     * @param feedId The feed the user wants to edit. This overrides the default behaviour and
     *               automatically loads the feed matching this id (or a new feed if the user
     *               does not have a feed matching that id)
     * @param auth   The user wishing to edit his feeds
     * @param model
     * @return The page where a user can edit his feeds
     */
    @GetMapping("/edit")
    public String editFeed(@RequestParam(value = "feedId", required = false) UUID feedId,
                           Authentication auth, Model model) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        UserSettings settings = feedService.getSettingsForUser(userId);
        Feed feed;
        if (feedId != null) {
            feed = settings.getFeeds().stream().
                    filter(otherFeed -> feedId.equals(otherFeed.getId())).findAny().orElse(new Feed());

        } else {
            feed = CollectionUtils.isEmpty(settings.getFeeds()) ? new Feed() :
                    settings.getFeeds().get(0);
        }
        model.addAttribute("name", feed.getName());
        model.addAttribute("emptyFeed", new Feed());
        model.addAttribute("settingsId", settings.getId());
        model.addAttribute("feed", feed);
        model.addAttribute("feeds", feedService.getUserFeedsNoArticles(userId));
        model.addAttribute("sources", sourceService.getAvailableSources(userId));
        return "configure";
    }

    /**
     * Processes a users's edits to his feeds.
     * This includes updating already existing ones or adding entirely new ones
     *
     * @param feed               The updated feed. This feed is only used if addRequest is not
     *                           present or
     *                           false
     * @param settingsId         The settings the feed should be added to
     * @param organization       The organization the user belongs to. This parameter just needs
     *                           to be
     *                           bound somewhere in the controller, so Spring is capable of matching
     *                           paths correctly. It could also be extracted by calling
     *                           {@link User#getOrganization()}
     * @param auth               The user trying to change his feed
     * @param redirectAttributes
     * @return The page where a user can edit his feeds
     */
    @PostMapping("/update")
    public String updateFeed(@ModelAttribute("feed") Feed feed, Authentication auth,
                             @RequestParam("settingsId") UUID settingsId, @PathVariable(
                                     "organization") String organization,
                             final RedirectAttributes redirectAttributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);

        feed.setFeedSources(sourceService.recoverSources(feed.getFeedSources(), userId));

        String redirectQuery = "?feedId=" + feed.getId();
        try {
            feedService.updateFeed(feed, settingsId);
            populateDefaultRedirectAttributes(redirectAttributes, false,
                    "Feed " + "erfolgreich " + "aktualisiert");
        } catch (HttpClientErrorException.BadRequest badRequest) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Etwas ist falsch " +
                    "gelaufen");
        }
        return "redirect:/" + organization + "/feed/edit" + redirectQuery;
    }

    /**
     * Adds a feed to the supplied {@link UserSettings} via their {@link UUID}
     *
     * @param feed               The feed that should be added
     * @param auth               The user adding the feed
     * @param settingsId         The id of the {@link UserSettings} the {@link Feed} should be
     *                           added to
     * @param organization       The organization the user belongs to
     * @param redirectAttributes
     * @return The page where a user can edit his feeds
     */
    @PostMapping("/addFeed")
    public String addFeed(@ModelAttribute("emptyFeed") Feed feed, Authentication auth,
                          @RequestParam("settingsId") UUID settingsId, @PathVariable(
                                  "organization") String organization,
                          final RedirectAttributes redirectAttributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        try {
            feed.setFeedSources(sourceService.recoverSources(feed.getFeedSources(), userId));
            feedService.addFeed(settingsId, feed);
            populateDefaultRedirectAttributes(redirectAttributes, false, "Feed erfolgreich " +
                    "hinzugefügt");
        } catch (HttpClientErrorException.BadRequest badRequest) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Das Hinzufügen des " +
                    "Feeds ist fehlgeschlagen");
        }
        return "redirect:/" + organization + "/feed/edit";
    }

    /**
     * Deletes the supplied feed
     *
     * @param feedId             The {@link UUID} of the {@link Feed} that should be deleted
     * @param redirectAttributes
     * @param organization       The organization the user belongs to
     * @return The page where a user can edit his feeds
     */
    @PostMapping("/remove")
    public String removeFeed(@RequestParam("feedId") UUID feedId,
                             final RedirectAttributes redirectAttributes, @PathVariable(
                                     "organization") String organization) {

        try {
            feedService.deleteFeed(feedId);
            populateDefaultRedirectAttributes(redirectAttributes, false, "Feed erfolgreich " +
                    "entfernt");
        } catch (HttpClientErrorException.BadRequest badRequest) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Feed konnte nicht " +
                    "gelöscht werden");
        }
        return "redirect:/" + organization + "/feed/edit";
    }

    /**
     * Shows a user's profile settings.
     * This includes information about everything except his feeds. Included are e. g. settings
     * about notification intervals.
     *
     * @param model
     * @param auth  The user wishing to see his settings and maybe update them
     * @return A page showing the settings
     */
    @GetMapping("/profile")
    public String profile(Model model, Authentication auth) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        if (AuthenticationUtils.isMustChangePassword(auth)) {
            model.addAttribute("msg", "Bitte ändern Sie Ihr Passwort");
            model.addAttribute("css", "alert-warning");
        }
        model.addAttribute("notificationSettings", NotificationSetting.values());
        model.addAttribute("settings", feedService.getSettingsForUser(userId));
        return "accountsettings";
    }


    /**
     * Processes a user's request to update his settings
     *
     * @param settings           The (possibly changed) settings of the user
     * @param settingsId         The id of the userSettings
     * @param organization       The organization the user belongs to
     * @param redirectAttributes
     * @return The page about the user's profile
     */
    @PostMapping("/profile")
    public String updateProfile(@ModelAttribute("settings") UserSettings settings, @RequestParam(
            "settingsId") UUID settingsId, @PathVariable("organization") String organization,
                                final RedirectAttributes redirectAttributes) {

        try {
            feedService.updateNotificationSettings(settings.getNotificationCheckInterval(),
                    settingsId, settings.getNotificationSetting(), settings.getArticlesPerPage());
            populateDefaultRedirectAttributes(redirectAttributes, false,
                    "Benachrichtigungseinstellungen erfolgreich aktualisiert");
        } catch (BackendException backend) {
            populateDefaultRedirectAttributes(redirectAttributes, true,
                    "Benachrichtigungseinstellungen konnten nicht aktualisiert werden");
        }
        return "redirect:/" + organization + "/feed/profile";
    }


    /**
     * @param auth               The user who wants to change the email address
     * @param email              The new email address
     * @param password           The users password
     * @param organization       The organization the user belongs to
     * @param request
     * @param redirectAttributes
     * @return The page about the user's profile
     */
    @PostMapping("/changemailaddress")
    public String changeMailAddress(Authentication auth, @RequestParam("email") String email,
                                    @RequestParam("password") String password, @PathVariable(
                                            "organization") String organization,
                                    HttpServletRequest request,
                                    final RedirectAttributes redirectAttributes) {


        UUID userId = AuthenticationUtils.getUserId(auth);
        if (!email.equals("")) {
            try {
                userService.authenticate(AuthenticationUtils.getEmail(auth),
                        AuthenticationUtils.getOrganization(auth), password);
                userService.updateMail(userId, email);

                //Updating the actual authentication object swirling around in the httpsession so
                // information is always accurate
                User user = (User) auth.getPrincipal();
                user.seteMail(email);
                user.setPassword(password);
                login(request, user);
                populateDefaultRedirectAttributes(redirectAttributes, false, "E-Mail " +
                        "erfolgreich geändert");
            } catch (BackendException ex) {
                log.info("something went wrong");
                populateDefaultRedirectAttributes(redirectAttributes, true, "E-Mail konnte nicht "
                        + "geändert werden");
            } catch (HttpClientErrorException.BadRequest badRequest) {
                log.info("Supplied password [{}] did not match actual", password);
                populateDefaultRedirectAttributes(redirectAttributes, true, "E-Mail konnte " +
                        "nicht geändert werden");
            }
        }
        return "redirect:/" + organization + "/feed/profile";
    }

    /**
     * @param auth               The user who wants to change the password
     * @param newPassword        The new password
     * @param oldPassword        The users old password
     * @param organization       The organization the user belongs to
     * @param request
     * @param redirectAttributes
     * @return The page about the user's profile
     */
    @PostMapping("/changepassword")
    public String changePassword(Authentication auth,
                                 @RequestParam("newPassword") String newPassword, @RequestParam(
                                         "oldPassword") String oldPassword, @PathVariable(
                                                 "organization") String organization,
                                 HttpServletRequest request,
                                 final RedirectAttributes redirectAttributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);

        if (!newPassword.equals("")) {
            try {
                userService.authenticate(AuthenticationUtils.getEmail(auth),
                        AuthenticationUtils.getOrganization(auth), oldPassword);
                userService.updatePassword(userId, newPassword);

                //Updating the actual authentication object swirling around in the HttpSession so
                // information is always accurate
                User user = (User) auth.getPrincipal();
                user.setPassword(newPassword);
                login(request, user);
                populateDefaultRedirectAttributes(redirectAttributes, false, "Passwort " +
                        "erfolgreich geändert");
            } catch (BackendException ex) {
                log.info("something went wrong");
                populateDefaultRedirectAttributes(redirectAttributes, true, "Passwort konnte " +
                        "nicht geändert werden");
            } catch (HttpClientErrorException.BadRequest badRequest) {
                log.info("Supplied password [{}] did not match actual", oldPassword);
                populateDefaultRedirectAttributes(redirectAttributes, true, "Passwort konnte " +
                        "nicht geändert werden");
            }
        }
        return "redirect:/" + organization + "/feed/profile";
    }

    /**
     * Logs the user into the session. This is duplicated code from
     * {@link RegistrationController#login(HttpServletRequest, User)}
     * but it will be removed from there soon as being logged in after registering will not be
     * supported in the final version anymore
     *
     * @param request the http-request holding the http session
     * @param user    the user to authenticate
     */
    private void login(HttpServletRequest request, User user) {

        CustomAuthenticationToken authReq = new CustomAuthenticationToken(user,
                user.getPassword(), user.getOrganization());
        Authentication auth = authManager.authenticate(authReq);

        SecurityContext sc = SecurityContextHolder.getContext();
        sc.setAuthentication(auth);
        HttpSession session = request.getSession(true);
        session.setAttribute(SPRING_SECURITY_CONTEXT_KEY, sc);
    }

}
