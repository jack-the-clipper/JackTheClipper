package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.Date;
import java.util.UUID;

/**
 * Represents a complete article. This means it has more additional information and no limit on
 * the length of the article's text.
 */
public class Article extends ShortArticle {

    private String longText;

    public Article(){
        super();
    }

    public Article(UUID id, String title, String shortText, String link, Date published,
                   Date indexed, UUID indexingSourceId, String longText) {

        super(id, title, shortText, link, published, indexed, indexingSourceId);
        this.longText = longText;
    }

    public String getLongText() {

        return longText;
    }

    @JsonProperty("ArticleLongText")
    public void setLongText(String longText) {

        this.longText = longText;
    }

}
