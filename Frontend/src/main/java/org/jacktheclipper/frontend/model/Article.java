package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonProperty;

import java.time.LocalDate;
import java.util.UUID;

/**
 * Represents a complete article. This means it has more additional information and no limit on
 * the length of the article's text.
 */
public class Article extends ShortArticle {

    @JsonProperty("ArticleLongText")
    private String longText;

    public Article() {

        super();
    }

    public Article(UUID id, String title, String shortText, String link, LocalDate published,
                   LocalDate indexed, UUID indexingSourceId, String longText, String imageLink) {

        super(id, title, shortText, link, published, indexed, indexingSourceId, imageLink);
        this.longText = longText;
    }

    public String getLongText() {

        return longText;
    }

    public void setLongText(String longText) {

        this.longText = longText;
    }

}
