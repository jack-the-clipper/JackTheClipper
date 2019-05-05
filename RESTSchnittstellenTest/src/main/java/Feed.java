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
    private UUID id;
    private List<Source> feedSources;
    private Filter filter;
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

    @JsonProperty("FeedId")
    public void setId(UUID id) {

        this.id = id;
    }

    public List<Source> getFeedSources() {

        return feedSources;
    }

    @JsonProperty("FeedSources")
    public void setFeedSources(List<Source> feedSources) {

        this.feedSources = feedSources;
    }

    public Filter getFilter() {

        return filter;
    }

    @JsonProperty("FeedFilter")
    public void setFilter(Filter filter) {

        this.filter = filter;
    }

    @JsonProperty("FeedName")
    public String getName() {

        return name;
    }

    public void setName(String name) {

        this.name = name;
    }

    @Override
    public boolean equals(Object obj) {

        if (this.id == null) {
            return false;
        }
        if (obj instanceof Feed) {
            return this.id.equals(((Feed) obj).getId());
        }
        return super.equals(obj);
    }

}
