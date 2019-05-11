package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.Article;
import org.jacktheclipper.frontend.model.Feed;
import org.jacktheclipper.frontend.model.ShortArticle;
import org.jacktheclipper.frontend.service.FeedService;
import org.jacktheclipper.frontend.utils.AuthenticationUtils;
import org.jacktheclipper.frontend.utils.Constants;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.util.CollectionUtils;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.server.ResponseStatusException;

import javax.servlet.http.HttpSession;
import java.util.ArrayList;
import java.util.Collections;
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
     * @param auth         The user wanting to see a feed
     * @param feedId       The id of the requested feed. This parameter is optional. If present, the
     *                     articles matching the criteria of the feed with the selected feed are
     *                     shown
     * @param model
     * @param page         Articles belonging to a feed are paginated. This optional parameter
     *                     determines the index of the requested page. If not present the default
     *                     value is {@code 0}
     * @param showArchived This optional parameter determines whether only the articles indexed
     *                     after the user's last login should be shown or all articles matching
     *                     the feeds filter. Defaults to {@code false}.
     * @param session
     * @return The page showing the short form of the articles of the selected feed
     */
    @GetMapping("/*/feed")
    public String showFeedOverview(Authentication auth, @RequestParam(value = "feedId", required
            = false) UUID feedId, Model model,
                                   @RequestParam(value = "page", required = false) Integer page,
                                   @RequestParam(value = "showArchived", required = false) Boolean showArchived, HttpSession session) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        List<Feed> feeds = feedService.getUserFeedsNoArticles(userId);
        Feed feedToShow;
        if (!CollectionUtils.isEmpty(feeds)) {
            //user has feeds and id is set
            if (feedId != null) {
                feedToShow = feeds.stream().filter(otherFeed -> feedId.equals(otherFeed.getId())).
                        findFirst().orElse(new Feed(null, Collections.EMPTY_LIST, null,
                        "nicht " + "gefunden"));
                //user has feeds but id is missing
            } else {
                feedToShow = feeds.get(0);
            }
            //user does not have feeds
        } else {
            feedToShow = new Feed(null, Collections.EMPTY_LIST, null, "Sie haben keine Feeds");
        }
        page = page == null ? 0 : page;
        showArchived = showArchived == null ? false : showArchived;
        saveQuery(session, feedToShow.getId(), page, showArchived);
        model.addAttribute("articles", feedToShow.getId() != null ?
                feedService.getSpecificFeed(userId, feedToShow.getId(), page, showArchived) :
                new ArrayList<ShortArticle>());
        if (feedToShow.getId() != null) {
            model.addAttribute("hasNextPage",
                    !CollectionUtils.isEmpty(feedService.getSpecificFeed(userId,
                            feedToShow.getId(), page + 1, showArchived)));
        } else {
            //default value if feed does not exist
            model.addAttribute("hasNextPage", false);
        }
        model.addAttribute("name", feedToShow.getName());
        model.addAttribute("feedId", feedToShow.getId());
        model.addAttribute("currentPage", page);
        model.addAttribute("showArchived", showArchived);
        model.addAttribute("feeds", feeds);

        return "feedOverview";
    }

    /**
     * Shows the long version of the requested article
     *
     * @param auth
     * @param articleId The article to show
     * @param model
     * @return A page showing the details of the requested article
     */
    @GetMapping("/*/article/{id}")
    public String showArticle(Authentication auth, @PathVariable("id") UUID articleId,
                              Model model) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        try {
            Article article = feedService.getArticle(articleId, userId);
            model.addAttribute("article", article);
            return "article";
        } catch (BackendException bE) {
            throw new ResponseStatusException(HttpStatus.NOT_FOUND);
        }
    }

    /**
     * Saves the given fields into the user's {@link HttpSession}.
     * This allows for easy link backs to the feedOverview when accessing an individual
     * {@link Article} in the Jack the Clipper application. The keys for each field can be found
     * in {@link Constants}. Which parameter is saved under which key should be clear from naming.
     *
     * @param session      The session to save the parameters in
     * @param feedId       The id of the last visited feed
     * @param page         The last page visited
     * @param showArchived Whether all articles relevant to this feed should be shown or only the
     *                     most recent ones
     */
    private void saveQuery(HttpSession session, UUID feedId, Integer page, Boolean showArchived) {

        session.setAttribute(Constants.LAST_VIEWED_FEED_ID, feedId);
        session.setAttribute(Constants.LAST_VIEWED_FEED_PAGE, page);
        session.setAttribute(Constants.LAST_VIEWED_FEED_SHOWARCHIVED, showArchived);
    }
}
