package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonProperty;
import org.jacktheclipper.frontend.enums.NotificationSetting;

import java.util.List;
import java.util.UUID;

/**
 * Represents a user's settings. This includes the frequency and kind of notification a user
 * wishes for and a user's feeds
 */
public class UserSettings {

    @JsonProperty("SettingsId")
    private UUID id;
    @JsonProperty("UserSettingsFeeds")
    private List<Feed> feeds;
    @JsonProperty("UserNotificationSetting")
    private NotificationSetting notificationSetting;
    @JsonProperty("UserNotificationCheckInterval")
    private int notificationCheckInterval;
    @JsonProperty("UserNumberOfArticles")
    private int articlesPerPage;

    public UserSettings() {

    }

    public UserSettings(UUID id, List<Feed> feeds, NotificationSetting notificationSetting,
                        int notificationCheckInterval, int articlesPerPage) {

        this.id = id;
        this.feeds = feeds;
        this.notificationSetting = notificationSetting;
        this.notificationCheckInterval = notificationCheckInterval;
        this.articlesPerPage = articlesPerPage;
    }

    public List<Feed> getFeeds() {

        return feeds;
    }

    public void setFeeds(List<Feed> feeds) {

        this.feeds = feeds;
    }

    public NotificationSetting getNotificationSetting() {

        return notificationSetting;
    }

    public void setNotificationSetting(NotificationSetting notificationSetting) {

        this.notificationSetting = notificationSetting;
    }

    public int getNotificationCheckInterval() {

        return notificationCheckInterval;
    }

    public void setNotificationCheckInterval(int notificationCheckInterval) {

        this.notificationCheckInterval = notificationCheckInterval;
    }

    public UUID getId() {

        return id;
    }

    public void setId(UUID id) {

        this.id = id;
    }

    public int getArticlesPerPage() {

        return articlesPerPage;
    }

    public void setArticlesPerPage(int articlesPerPage) {

        this.articlesPerPage = articlesPerPage;
    }
}
