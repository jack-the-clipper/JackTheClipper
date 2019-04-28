package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonProperty;
import org.jacktheclipper.frontend.enums.NotificationSetting;

import java.util.List;

/**
 * Represents a user's settings. This includes the frequency and kind of notification a user
 * wishes for and a user's feeds
 */
public class UserSettings {

    private List<Feed> feeds;
    private NotificationSetting notificationSetting;
    private int notificationCheckInterval;

    public UserSettings() {

    }

    public UserSettings(List<Feed> feeds, NotificationSetting notificationSetting,
                        int notificationCheckInterval) {

        this.feeds = feeds;
        this.notificationSetting = notificationSetting;
        this.notificationCheckInterval = notificationCheckInterval;
    }

    public List<Feed> getFeeds() {

        return feeds;
    }

    @JsonProperty("UserSettingsFeeds")
    public void setFeeds(List<Feed> feeds) {

        this.feeds = feeds;
    }

    public NotificationSetting getNotificationSetting() {

        return notificationSetting;
    }

    @JsonProperty("UserNotificationSetting")
    public void setNotificationSetting(NotificationSetting notificationSetting) {

        this.notificationSetting = notificationSetting;
    }

    public int getNotificationCheckInterval() {

        return notificationCheckInterval;
    }

    @JsonProperty("UserNotificationCheckInterval")
    public void setNotificationCheckInterval(int notificationCheckInterval) {

        this.notificationCheckInterval = notificationCheckInterval;
    }

}
