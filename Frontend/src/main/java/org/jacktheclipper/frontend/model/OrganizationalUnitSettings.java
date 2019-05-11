package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonProperty;
import org.jacktheclipper.frontend.enums.NotificationSetting;

import java.util.List;
import java.util.UUID;

/**
 * Represents default settings for all users registered to it. It might limit the sources to all
 * users belonging to it
 */
public class OrganizationalUnitSettings extends UserSettings {
    private List<Source> availableSources;

    public OrganizationalUnitSettings() {

        super();
    }

    public OrganizationalUnitSettings(UUID id, List<Feed> feeds,
                                      NotificationSetting notificationSetting,
                                      int notificationCheckInterval,
                                      List<Source> availableSources, int articlesPerPage) {

        super(id, feeds, notificationSetting, notificationCheckInterval, articlesPerPage);
        this.availableSources = availableSources;
    }

    public List<Source> getAvailableSources() {

        return availableSources;
    }

    //TODO Backend currently does not pass those
    @JsonProperty("AvailableSources")
    public void setAvailableSources(List<Source> availableSources) {

        this.availableSources = availableSources;
    }
}
