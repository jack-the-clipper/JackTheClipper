import com.fasterxml.jackson.annotation.JsonProperty;
import enums.NotificationSetting;

import java.util.List;
import java.util.UUID;

/**
 * Represents default settings for all users registered to it. It might limit the sources to all
 * users belonging to it
 */
public class OrganizationalUnitSettings extends UserSettings {

    @JsonProperty("OrganizationalUnitSources")
    private List<Source> availableSources;

    @JsonProperty("OrganizationalUnitBlackList")
    private List<String> blackList;

    public OrganizationalUnitSettings() {

        super();
    }

    public OrganizationalUnitSettings(UUID id, List<Feed> feeds,
                                      NotificationSetting notificationSetting,
                                      int notificationCheckInterval,
                                      List<Source> availableSources, int articlesPerPage,
                                      List<String> blackList) {

        super(id, feeds, notificationSetting, notificationCheckInterval, articlesPerPage);
        this.availableSources = availableSources;
        this.blackList = blackList;
    }

    public List<Source> getAvailableSources() {

        return availableSources;
    }

    public void setAvailableSources(List<Source> availableSources) {

        this.availableSources = availableSources;
    }

    public List<String> getBlackList() {

        return blackList;
    }

    public void setBlackList(List<String> blackList) {

        this.blackList = blackList;
    }
}
