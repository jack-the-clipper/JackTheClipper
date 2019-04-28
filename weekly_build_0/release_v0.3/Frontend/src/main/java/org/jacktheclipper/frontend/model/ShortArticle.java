package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.Date;
import java.util.UUID;

/**
 * Represents a short version of an article. Thus the text of it is limited by the backend. This
 * is used to show the content of an user feed.
 *
 * @author SBG
 */
public class ShortArticle {
    @JsonInclude(JsonInclude.Include.NON_NULL)
    private UUID id;
    private String title;
    private String shortText;
    private String link;
    private Date published;
    private Date indexed;
    private UUID indexingSourceId;

    public ShortArticle() {

    }

    public ShortArticle(UUID id, String title, String shortText, String link, Date published,
                        Date indexed, UUID indexingSourceId) {

        this.id = id;
        this.title = title;
        this.shortText = shortText;
        this.link = link;
        this.published = published;
        this.indexed = indexed;
        this.indexingSourceId = indexingSourceId;
    }

    public UUID getId() {

        return id;
    }

    @JsonProperty("ArticleId")
    public void setId(UUID id) {

        this.id = id;
    }

    public String getTitle() {

        return title;
    }

    @JsonProperty("ArticleTitle")
    public void setTitle(String title) {

        this.title = title;
    }

    public String getShortText() {

        return shortText;
    }

    @JsonProperty("ArticleShortText")
    public void setShortText(String shortText) {

        this.shortText = shortText;
    }

    public String getLink() {

        return link;
    }

    @JsonProperty("ArticleLink")
    public void setLink(String link) {

        this.link = link;
    }

    public Date getPublished() {

        return published;
    }

    @JsonProperty("ArticlePublished")
    public void setPublished(Date published) {

        this.published = published;
    }


    public Date getIndexed() {

        return indexed;
    }

    @JsonProperty("ArticleIndexed")
    public void setIndexed(Date indexed) {

        this.indexed = indexed;
    }

    public UUID getIndexingSourceId() {

        return indexingSourceId;
    }

    @JsonProperty("IndexingSourceId")
    public void setIndexingSourceId(UUID indexingSourceId) {

        this.indexingSourceId = indexingSourceId;
    }
}
