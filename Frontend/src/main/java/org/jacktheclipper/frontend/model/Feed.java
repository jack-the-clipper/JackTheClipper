package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

/**
 * Represents a feed of an user. Holds information about which sources should be searched and
 * keywords for articles
 */
public class Feed {
    @JsonInclude(JsonInclude.Include.NON_NULL)
    @JsonProperty("FeedId")
    private UUID id;
    @JsonProperty("FeedSources")
    private List<Source> feedSources;
    @JsonProperty("FeedFilter")
    private Filter filter;
    @JsonProperty("FeedName")
    private String name;

    public Feed() {

        this.feedSources = new ArrayList<>();
    }

    public Feed(UUID id, List<Source> feedSources, Filter filter, String name) {

        this.id = id;
        this.feedSources = feedSources;
        this.filter = filter;
        this.name = name;
    }

    public UUID getId() {

        return id;
    }

    public void setId(UUID id) {

        this.id = id;
    }

    public List<Source> getFeedSources() {

        return feedSources;
    }


    public void setFeedSources(List<Source> feedSources) {

        this.feedSources = feedSources;
    }

    public Filter getFilter() {

        return filter;
    }


    public void setFilter(Filter filter) {

        this.filter = filter;
    }


    public String getName() {

        return name;
    }

    public void setName(String name) {

        this.name = name;
    }

    @Override
    public boolean equals(Object obj) {

        if (obj instanceof Feed) {
            return this.id.equals(((Feed) obj).getId());
        }
        return super.equals(obj);
    }

}
