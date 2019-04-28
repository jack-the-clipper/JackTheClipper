package org.jacktheclipper.frontend.service;

import org.jacktheclipper.frontend.enums.NotificationSetting;
import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.*;
import org.jacktheclipper.frontend.utils.ResponseEntityUtils;
import org.jacktheclipper.frontend.utils.RestTemplateUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpMethod;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;
import org.springframework.util.Assert;
import org.springframework.web.client.RestTemplate;

import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

/**
 * This class bundles up all requests to the backend which involve feeds
 */
@Service
public class FeedService {

    private static final Logger log = LoggerFactory.getLogger(FeedService.class);
    @Value("${backend.url}")
    private String backendUrl;

    /**
     * Returns the settings (and within his feeds) of the given user
     *
     * @param userId The user requesting the settings
     * @return The settings for the given user or null, if the user was not founnd
     */
    public UserSettings getSettingsForUser(UUID userId) {

        ResponseEntity<UserSettings> response = RestTemplateUtils.getRestTemplate().
                getForEntity(backendUrl + "/getusersettings" + "?userId=" + userId.toString(),
                        UserSettings.class);
        if (ResponseEntityUtils.successful(response)) {
            return response.getBody();
        }
        log.warn("No usersettings found for user with id [{}]", userId);
        throw new BackendException("Could not find settings for userId " + userId.toString());
    }

    /**
     * Gets all the feeds a user has
     *
     * @param userId The user which feeds should be looked up
     * @return A list of the user's feeds
     */
    public List<Feed> getUserFeedsNoArticles(UUID userId) {

        ResponseEntity<Feed[]> response =
                RestTemplateUtils.getRestTemplate().getForEntity(backendUrl +
                        "/getfeeddefinitions?userid=" + userId.toString(), Feed[].class);
        if (ResponseEntityUtils.successful(response)) {
            return Arrays.asList(response.getBody());
        }
        return Collections.emptyList();
    }

    /**
     * Updates the feeds of the given user with the given feed
     * This method both handles adding and editing a feed
     *
     * @param feed   The feed which was updated
     * @param userId The user which the updated feed belongs to
     */
    public void updateFeeds(Feed feed, UUID userId) {

        UserSettings settings = getSettingsForUser(userId);
        settings.getFeeds().remove(feed);
        settings.getFeeds().add(feed);

        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        ResponseEntity<MethodResult> response = restTemplate.exchange(backendUrl +
                "/saveusersettings?userId=" + userId.toString(), HttpMethod.PUT,
                RestTemplateUtils.prepareBasicHttpEntity(settings), MethodResult.class);
        if (ResponseEntityUtils.successful(response)) {

            switch (response.getBody().getState()) {

                case Successful:
                    log.debug("Successfully updated settings for userId [{}]", userId);
                    break;
                case Timeout:
                    throw new BackendException("Request timed out");
                case UnknownError:
                    throw new BackendException(response.getBody().getMessage());
            }

        } else {
            throw new BackendException("Update of UserSettings for userId [" + userId.toString() + "]" + " failed");
        }
    }


    /**
     * Gets the specified article
     *
     * @param articleId The id of the requested article
     * @param userId    The id of the user requesting the article so that the backend can
     *                  verify if
     *                  he has access to it
     * @return The article matching the id or null if any error is encountered
     */
    public Article getArticle(UUID articleId, UUID userId) {

        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        ResponseEntity<Article> response =
                restTemplate.getForEntity(backendUrl + "/getarticle" + "?articleId=" + articleId.toString() + "&userId=" + userId.toString(), Article.class);
        if (ResponseEntityUtils.successful(response)) {
            return response.getBody();
        }
        return null;
    }

    /**
     * Updates a user's notification settings
     *
     * @param checkInterval       The time between notifications. This cannot be negative
     * @param userId              The user which wants to update his settings
     * @param notificationSetting How the notification should be delivered
     */
    public void updateNotificationSettings(int checkInterval, UUID userId,
                                           NotificationSetting notificationSetting) {

        Assert.isTrue(checkInterval >= 0, "Time cannot be negative");
        UserSettings settings = getSettingsForUser(userId);
        settings.setNotificationCheckInterval(checkInterval);
        settings.setNotificationSetting(notificationSetting);
        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        restTemplate.put(backendUrl + "/saveusersettings?userId=" + userId.toString(), settings);
    }

    /**
     * Gets the shortArticles belonging to the requested feed
     *
     * @param userId The user wishing to see his feed
     * @param feedId The feed that the user requested
     * @return A list of articles matching the criteria of the feeds filters
     */
    public List<ShortArticle> getSpecificFeed(UUID userId, UUID feedId) {

        String uriParams = "?userId=" + userId.toString() + "&feedId=" + feedId.toString();
        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        ResponseEntity<ShortArticle[]> response = restTemplate.getForEntity(backendUrl +
                "/getfeed" + uriParams, ShortArticle[].class);
        if (ResponseEntityUtils.successful(response)) {
            return Arrays.asList(response.getBody());
        }
        return Collections.emptyList();
    }
}
