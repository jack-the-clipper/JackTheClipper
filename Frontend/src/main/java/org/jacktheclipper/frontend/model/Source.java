package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;
import org.jacktheclipper.frontend.enums.ContentType;

import java.util.List;
import java.util.UUID;

/**
 * Represents a source, a place where articles are published. This might be a website or a rssfeed
 */
public class Source {

    @JsonInclude(JsonInclude.Include.NON_NULL)
    @JsonProperty("SourceId")
    private UUID id;
    @JsonProperty("SourceUri")
    private String uri;
    @JsonProperty("SourceName")
    private String name;
    @JsonProperty("SourceContentType")
    private ContentType contentType;
    @JsonProperty("SourceRegex")
    private String regEx;
    @JsonProperty("SourceBlacklist")
    private List<String> blackList;
    @JsonProperty("SourceXPath")
    private String xPath;

    public Source() {

    }

    /**
     * Constructor to deserialize from a not json-formatted string.
     * It is necessary since this class overrides the default #toString method
     *
     * @param representation The string possibly representing a Source object. For that it needs
     *                       to be formatted 'UUID;;;name'
     */
    public Source(String representation) {

        String[] parts = representation.split(";;;", -1);

        if (parts.length != 2) {
            throw new IllegalArgumentException("this is a lossy converter only needing the UUID;;"
                    + ";Name combination");
        }
        id = UUID.fromString(parts[0]);
        name = parts[1];
    }

    public Source(UUID id, String uri, String name, ContentType contentType, String regEx,
                  List<String> blackList, String xPath) {

        this.id = id;
        this.uri = uri;
        this.name = name;
        this.contentType = contentType;
        this.regEx = regEx;
        this.blackList = blackList;
        this.xPath = xPath;
    }

    @Override
    public boolean equals(Object obj) {

        if (obj instanceof Source) {
            return this.id.equals(((Source) obj).getId());
        }
        return false;
    }

    public UUID getId() {

        return id;
    }

    public void setId(UUID id) {

        this.id = id;
    }

    public String getUri() {

        return uri;
    }

    public void setUri(String uri) {

        this.uri = uri;
    }

    public String getName() {

        return name;
    }

    public void setName(String name) {

        this.name = name;
    }

    public ContentType getContentType() {

        return contentType;
    }

    public void setContentType(ContentType contentType) {

        this.contentType = contentType;
    }

    public String getRegEx() {

        return regEx;
    }

    public void setRegEx(String regEx) {

        this.regEx = regEx;
    }

    public List<String> getBlackList() {

        return blackList;
    }

    public void setBlackList(List<String> blackList) {

        this.blackList = blackList;
    }

    public String getxPath() {

        return xPath;
    }

    public void setxPath(String xPath) {

        this.xPath = xPath;
    }

    /**
     * Returns a string representation of this object
     * The default cannot be used since it causes troubles deserializing from a html-form. Thus
     * this obviously incomplete conversion is needed. It requires the caller to recover the full
     * information of a source by calling FeedService#recoverSources
     *
     * @return A string representation of this object, created by concatenating its id, ';;;' and
     * its name
     */
    @Override
    public String toString() {

        String prefix = id != null ? id.toString() : "";
        String postfix = name != null ? name : "";
        return prefix + ";;;" + postfix;
    }
}
