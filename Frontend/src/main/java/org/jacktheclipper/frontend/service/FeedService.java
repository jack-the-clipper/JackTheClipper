package org.jacktheclipper.frontend.service;

import org.jacktheclipper.frontend.enums.NotificationSetting;
import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.*;
import org.jacktheclipper.frontend.utils.ResponseEntityUtils;
import org.jacktheclipper.frontend.utils.RestTemplateUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.web.client.RestTemplateBuilder;
import org.springframework.http.HttpMethod;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;
import org.springframework.util.Assert;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.client.RestTemplate;

import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

/**
 * This class bundles up all requests to the backend which involve {@link Feed}s. This includes @
 * {@link UserSettings}s.
 */
@Service
public class FeedService {

    private static final Logger log = LoggerFactory.getLogger(FeedService.class);

    private String backendUrl;

    private final RestTemplate template;

    @Autowired
    public FeedService(RestTemplateBuilder builder, @Value("${backend.url}") String backendUrl) {

        this.backendUrl = backendUrl;
        this.template = builder.build();
    }

    /**
     * Returns the settings (and within his feeds) of the given user
     *
     * @param userId The user requesting the settings
     * @return The {@link UserSettings} for the given user or null, if the user was not found
     *
     * @throws BackendException If no userSettings could be found for the supplied
     */
    public UserSettings getSettingsForUser(UUID userId)
            throws BackendException {

        String url = backendUrl + "/getusersettings?userId={userId}";
        try {

            ResponseEntity<UserSettings> response = template.
                    getForEntity(url, UserSettings.class, userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Found UserSettings for user [{}]", userId);
                return response.getBody();
            }
        } catch (HttpClientErrorException.BadRequest badRequest) {
            log.warn("Got a bad request for url [{}] with userid [{}]", url, userId);
        }
        log.warn("No UserSettings found for user with id [{}]", userId);
        throw new BackendException("Could not find settings for userId " + userId.toString());
    }

    /**
     * Gets all necessary information on a user that is needed to manage him.
     * In comparison this also gets all the {@link OrganizationalUnit}s a user belongs to.
     *
     * @param requestingUserId The id of the user wanting to know the information
     * @param userId           The id of the user whose information is requested
     * @return The information of the user as an {@link ExtendedUser}.
     *
     * @throws BackendException If the REST-call failed or returned an empty body.
     */
    public ExtendedUser getUserInformation(UUID requestingUserId, UUID userId)
            throws BackendException {

        String url = backendUrl + "/getuserinformation?userId={userId}&requested={requested}";
        try {
            ResponseEntity<ExtendedUser> response = template.getForEntity(url, ExtendedUser.class
                    , requestingUserId.toString(), userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully retrieved information of user [{}]", userId);
                return response.getBody();
            }
        } catch (HttpClientErrorException.BadRequest badRequest) {
            log.warn("Could not retrieve information of user [{}] for user [{}]", userId,
                    requestingUserId);
            throw new BackendException("Got Http-400 while trying to access information of user " + userId.toString());
        }
        log.info("Did not receive information for request [{}]", url);
        throw new BackendException("Could not retrieve information of  user " + userId.toString());
    }

    /**
     * Gets all the feeds a user has
     *
     * @param userId The user which feeds should be looked up
     * @return A list of the user's feeds
     */
    public List<Feed> getUserFeedsNoArticles(UUID userId) {

        String url = backendUrl + "/getfeeddefinitions?userid={userId}";
        try {
            ResponseEntity<Feed[]> response = template.getForEntity(url, Feed[].class,
                    userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                return Arrays.asList(response.getBody());
            }
        } catch (Exception ex) {
            log.warn("Could not retrieve the feed definitions of user [{}]", userId);
        }
        log.debug("Could not find any configured feeds for user [{}]. Returning empty list",
                userId);
        return Collections.emptyList();
    }

    /**
     * Adds the given feed to the supplied settings
     *
     * @param settingsId The id of the settings the feed should be added to
     * @param feed       The feed that should be added
     * @throws BackendException If the addition of the feed failed
     */
    public void addFeed(UUID settingsId, Feed feed)
            throws BackendException {

        String uri = backendUrl + "/addfeed?settingsId={settingsId}";
        try {
            template.exchange(uri, HttpMethod.PUT, RestTemplateUtils.prepareBasicHttpEntity(feed)
                    , String.class, settingsId);
        } catch (HttpClientErrorException.BadRequest badRequest) {
            log.warn("Could not add feed [{}] to settings [{}]", feed, settingsId);
            throw new BackendException("Could not add feed to settings [" + settingsId.toString() + "]");
        }
    }

    /**
     * Modifies a feed.
     *
     * @param feed       The feed which should be updated
     * @param settingsId The id of the settings to which the feed should be added
     * @throws BackendException If the feed could not be updated
     */
    public void updateFeed(Feed feed, UUID settingsId)
            throws BackendException {

        String uri = backendUrl + "/modifyfeed?settingsId={settingsId}";
        try {
            template.put(uri, feed, settingsId);
        } catch (HttpClientErrorException.BadRequest badRequest) {
            throw new BackendException("Could not update feed for settings [" + settingsId.toString() + "]");
        }
    }

    /**
     * Deletes the feed with the given id
     *
     * @param feedId The id of the feed that should be deleted
     * @throws BackendException If the feed could not be deleted
     */
    public void deleteFeed(UUID feedId)
            throws BackendException {

        String uri = backendUrl + "/deletefeed?feedId={feedId}";
        try {
            template.delete(uri, feedId.toString());
            log.debug("Deleted feed [{}]", feedId);
        } catch (HttpClientErrorException.BadRequest badRequest) {
            log.warn("Failed to delete feed [{}]", feedId);
            throw new BackendException("Could not delete feed [" + feedId + "]");
        }
    }


    /**
     * Gets the specified article
     *
     * @param articleId The id of the requested article
     * @param userId    The id of the user requesting the article so that the backend can
     *                  verify if he has access to it
     * @return The article matching the id or null if any error is encountered
     *
     * @throws BackendException If the article cannot be found
     */
    public Article getArticle(UUID articleId, UUID userId)
            throws BackendException {

        String url = backendUrl + "/getarticle?articleId={articleId}&userId={userId}";
        try {
            ResponseEntity<Article> response = template.getForEntity(url, Article.class,
                    articleId.toString(), userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Found article [{}]", articleId);
                return response.getBody();
            }
        } catch (Exception ex) {
            log.info("Could not find article [{}]", articleId);

        }
        throw new BackendException("Did not find article with id [" + articleId.toString() + "]");
    }

    /**
     * Updates a user's notification settings
     *
     * @param checkInterval       The time between notifications. This cannot be negative
     * @param settingsId          The id of the settings to update
     * @param notificationSetting How the notification should be delivered
     * @param articlesPerPage     How many articles the user wants to see on a feed page
     * @throws BackendException If the userSettings could not be updated
     */
    public void updateNotificationSettings(int checkInterval, UUID settingsId,
                                           NotificationSetting notificationSetting,
                                           int articlesPerPage)
            throws BackendException {

        Assert.isTrue(checkInterval >= 0, "Time cannot be negative");

        //@formatter:off
        String uri =
                backendUrl + "/saveusersettings?settingsId={settingsId}" +
                "&notificationCheckInterval={interval}"+
                "&notificationSetting={setting}" +
                "&articlesPerPage={articlesAmount}";
        //@formatter:on

        try {
            template.exchange(uri, HttpMethod.PUT, RestTemplateUtils.prepareBasicHttpEntity(""),
                    void.class, settingsId.toString(), checkInterval, notificationSetting.name(),
                    articlesPerPage);
        } catch (HttpClientErrorException.BadRequest badRequest) {
            throw new BackendException("Update of UserSettings for SettingsId [" + settingsId.toString() + "] failed");
        }
    }

    /**
     * Gets the shortArticles belonging to the requested feed
     *
     * @param userId       The user wishing to see his feed
     * @param feedId       The feed that the user requested
     * @param page         The index of the page to get. If null is passed this defaults to 0
     * @param showArchived Whether to show all indexed articles of this feed or only the recent
     *                     ones. Defaults to false which means only the recent articles are shown
     * @return A list of articles matching the criteria of the feeds filters
     */
    public List<ShortArticle> getSpecificFeed(UUID userId, UUID feedId, Integer page,
                                              Boolean showArchived) {

        showArchived = showArchived == null ? false : showArchived;
        page = page == null ? 0 : page;
        Assert.isTrue(page >= 0, "Cannot get a page with negative index");
        String uriParams = "?userId={userId}&feedId={feedId}&page={index}&showArchived={show}";
        String url = backendUrl + "/getfeed" + uriParams;
        try {
            ResponseEntity<ShortArticle[]> response = template.getForEntity(url,
                    ShortArticle[].class, userId.toString(), feedId.toString(), page, showArchived);
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Found articles for feed [{}]", feedId);
                return Arrays.asList(response.getBody());
            }
        } catch (Exception ex) {
            log.warn("Call to get page [{}] of feed [{}] showingArchived [{}] failed due to [{}] "
                    + "with reason [{}]", page, feedId, showArchived,
                    ex.getClass().getSimpleName(), ex.getMessage());
            throw new BackendException("Request to fetch the requested page failed");
        }
        log.warn("Could not find anything for call with following parameters: feedId [{}], " +
                "page [{}], showArchived [{}]. Returning empty list", feedId, page, showArchived);
        return Collections.emptyList();
    }
}
