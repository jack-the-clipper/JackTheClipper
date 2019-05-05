import com.fasterxml.jackson.annotation.JsonProperty;
import enums.NotificationSetting;

import java.util.List;
import java.util.UUID;

/**
 * Represents a user's settings. This includes the frequency and kind of notification a user
 * wishes for and a user's feeds
 */
public class UserSettings {
    private UUID id;
    private List<Feed> feeds;
    private NotificationSetting notificationSetting;
    private int notificationCheckInterval;
    private int articlesPerPage;

    public UserSettings() {

    }

    public UserSettings(UUID id, List<Feed> feeds, NotificationSetting notificationSetting, int notificationCheckInterval, int articlesPerPage) {

        this.id = id;
        this.feeds = feeds;
        this.notificationSetting = notificationSetting;
        this.notificationCheckInterval = notificationCheckInterval;
        this.articlesPerPage = articlesPerPage;
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

    public UUID getId() {

        return id;
    }

    @JsonProperty("SettingsId")
    public void setId(UUID id) {

        this.id = id;
    }

    public int getArticlesPerPage() {

        return articlesPerPage;
    }

    @JsonProperty("UserNumberOfArticles")
    public void setArticlesPerPage(int articlesPerPage) {

        this.articlesPerPage = articlesPerPage;
    }
}
