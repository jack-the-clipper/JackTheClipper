package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.enums.NotificationSetting;
import org.jacktheclipper.frontend.model.Feed;
import org.jacktheclipper.frontend.model.UserSettings;
import org.jacktheclipper.frontend.service.FeedService;
import org.jacktheclipper.frontend.service.SourceService;
import org.jacktheclipper.frontend.service.UserService;
import org.jacktheclipper.frontend.utils.AuthenticationUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.util.CollectionUtils;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

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

    @Autowired
    public UserConfigController(FeedService feedService, SourceService sourceService,
                                UserService userService) {

        this.feedService = feedService;
        this.sourceService = sourceService;
        this.userService = userService;
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
        model.addAttribute("feed", feed);
        model.addAttribute("feeds", feedService.getUserFeedsNoArticles(userId));
        model.addAttribute("sources", sourceService.getAvailableSources(userId));
        return "configure";
    }

    /**
     * Processes a users's edits to his feeds.
     * This includes updating already existing ones or adding entirely new ones
     *
     * @param feed       The updated feed. This feed is only used if addRequest is not present or
     *                   false
     * @param newlyAdded A newly added feed. This parameter is only used if addRequest is present
     *                   and true
     * @param addRequest This parameter determines whether the user wishes to update a feed or
     *                   add a new one. If it is true and present the user wants to add a feed.
     *                   Otherwise he wishes to update one
     * @param auth       The user trying to change his feeds
     * @return The page where a user can edit his feeds
     */
    @PostMapping("/update")
    public String updateFeed(@ModelAttribute("feed") Feed feed,
                             @ModelAttribute("emptyFeed") Feed newlyAdded, @RequestParam(value =
            "addFeed", required = false) Boolean addRequest, Authentication auth, @PathVariable(
                    "organization") String organization) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        Feed toWorkWith;
        if (addRequest != null && addRequest) {
            toWorkWith = newlyAdded;
        } else {
            toWorkWith = feed;
        }

        toWorkWith.setFeedSources(sourceService.recoverSources(toWorkWith.getFeedSources(),
                userId));
        feedService.updateFeeds(toWorkWith, userId);
        String redirectQuery = toWorkWith.getId() != null ?
                "?feedId=" + toWorkWith.getId().toString() : "";
        return "redirect:/" + organization + "/feed/edit" + redirectQuery;
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
        model.addAttribute("notificationSettings", NotificationSetting.values());
        model.addAttribute("settings", feedService.getSettingsForUser(userId));
        return "accountsettings";
    }


    /**
     * Processes a user's request to update his settings
     *
     * @param settings The (possibly changed) settings of the user
     * @param auth     The user to whom the settings belong
     * @return The page about the user's profile
     */
    @PostMapping("/profile")
    public String updateProfile(@ModelAttribute("settings") UserSettings settings,
                                Authentication auth,
                                @PathVariable("organization") String organization) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        feedService.updateNotificationSettings(settings.getNotificationCheckInterval(), userId,
                settings.getNotificationSetting());
        return "redirect:/" + organization + "/feed/profile";
    }

}
