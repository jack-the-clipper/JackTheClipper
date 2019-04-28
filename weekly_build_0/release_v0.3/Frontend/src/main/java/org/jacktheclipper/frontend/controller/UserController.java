package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.model.Feed;
import org.jacktheclipper.frontend.model.ShortArticle;
import org.jacktheclipper.frontend.service.FeedService;
import org.jacktheclipper.frontend.utils.AuthenticationUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.util.CollectionUtils;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestParam;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

/**
 * Handles all requests where a user does not want to edit anything. It basically shows a user a
 * selected feed
 */
@Controller
public class UserController {

    private static final Logger log = LoggerFactory.getLogger(UserController.class);

    private final FeedService feedService;

    @Autowired
    public UserController(FeedService feedService) {

        this.feedService = feedService;
    }

    /**
     * Shows a user all articles of a requested feed.
     * Defaults to the first found feed if no feedId is specified
     *
     * @param auth   The user wanting to see a feed
     * @param feedId The id of the requested feed. This parameter is optional. If present, the
     *               articles matching the criteria of the feed with the selected feed are shown
     * @param model
     * @return The page showing the short form of the articles of the selected feed
     */
    @GetMapping("/*/feed")
    public String showFeedOverview(Authentication auth, @RequestParam(value = "feedId", required
            = false) UUID feedId, Model model) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        List<Feed> feeds = feedService.getUserFeedsNoArticles(userId);
        if (!CollectionUtils.isEmpty(feeds)) {
            if (feedId != null) {
                model.addAttribute("articles", feedService.getSpecificFeed(userId, feedId));
                Feed feed = feeds.stream().filter(otherFeed -> feedId.equals(otherFeed.getId())).
                        findFirst().orElse(new Feed(null, null, null, "not Found"));
                model.addAttribute("name", feed.getName());
            } else {
                model.addAttribute("articles", feedService.getSpecificFeed(userId,
                        feeds.get(0).getId()));
                model.addAttribute("name", feeds.get(0).getName());
            }
        } else {
            model.addAttribute("articles", new ArrayList<ShortArticle>());
            model.addAttribute("name", "You have no Feeds");
        }
        model.addAttribute("feeds", feeds);

        return "feedOverview";
    }
}
