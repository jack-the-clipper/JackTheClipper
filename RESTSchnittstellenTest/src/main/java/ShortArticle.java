import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.time.format.FormatStyle;
import java.util.Locale;
import java.util.UUID;

/**
 * Represents a short version of an article. Thus the text of it is limited by the backend. This
 * is used to show the content of an user feed.
 */
public class ShortArticle {
    @JsonInclude(JsonInclude.Include.NON_NULL)
    @JsonProperty("ArticleId")
    private UUID id;
    @JsonProperty("ArticleTitle")
    private String title;
    @JsonProperty("ArticleShortText")
    private String shortText;
    @JsonProperty("ArticleLink")
    private String link;
    @JsonProperty("ArticlePublished")
    private LocalDate published;
    @JsonProperty("ArticleIndexed")
    private LocalDate indexed;
    @JsonProperty("IndexingSourceId")
    private UUID indexingSourceId;
    @JsonProperty("ImageLink")
    private String imageLink;
    private static DateTimeFormatter formatter =
            DateTimeFormatter.ofLocalizedDate(FormatStyle.LONG).withLocale(new Locale("de"));

    public ShortArticle() {

    }

    public ShortArticle(UUID id, String title, String shortText, String link, LocalDate published
            , LocalDate indexed, UUID indexingSourceId, String imageLink) {

        this.id = id;
        this.title = title;
        this.shortText = shortText;
        this.link = link;
        this.published = published;
        this.indexed = indexed;
        this.indexingSourceId = indexingSourceId;
        this.imageLink = imageLink;
    }

    public UUID getId() {

        return id;
    }

    public void setId(UUID id) {

        this.id = id;
    }

    public String getTitle() {

        return title;
    }

    public void setTitle(String title) {

        this.title = title;
    }

    public String getShortText() {

        return shortText;
    }

    public void setShortText(String shortText) {

        this.shortText = shortText;
    }

    public String getLink() {

        return link;
    }

    public void setLink(String link) {

        this.link = link;
    }

    public LocalDate getPublished() {

        return published;
    }

    public void setPublished(LocalDate published) {

        this.published = published;
    }


    public LocalDate getIndexed() {

        return indexed;
    }

    public void setIndexed(LocalDate indexed) {

        this.indexed = indexed;
    }

    public UUID getIndexingSourceId() {

        return indexingSourceId;
    }

    public void setIndexingSourceId(UUID indexingSourceId) {

        this.indexingSourceId = indexingSourceId;
    }

    public String getImageLink() {

        return imageLink;
    }

    public void setImageLink(String imageLink) {

        this.imageLink = imageLink;
    }

    /**
     * Formats the {@link #published} as a german date.
     * This allows the feedOverview page to be displayed entirely in german.
     *
     * @return The {@link #published} field formatted as a german date. With the month written
     * out. E. g. 'Mai' instead of '05'.
     */
    public String publishedAsGermanDate() {

        return formatter.format(published);
    }
}
