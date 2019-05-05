package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;
import java.util.UUID;

/**
 * Represents something by which a feed should be filterer. This might be simple keywords or more
 * complex regular expressions
 */
public class Filter {
    @JsonInclude(JsonInclude.Include.NON_NULL)
    private UUID id;
    private List<String> keywords;
    private List<String> expressions;
    private List<String> blackList;

    public Filter() {

    }

    public Filter(UUID id, List<String> keywords, List<String> expressions,List<String> blackList) {

        this.id = id;
        this.keywords = keywords;
        this.expressions = expressions;
        this.blackList = blackList;
    }

    public UUID getId() {

        return id;
    }

    @JsonProperty("FilterId")
    public void setId(UUID id) {

        this.id = id;
    }

    public List<String> getKeywords() {

        return keywords;
    }

    @JsonProperty("FilterKeywords")
    public void setKeywords(List<String> keywords) {

        this.keywords = keywords;
    }

    public List<String> getExpressions() {

        return expressions;
    }

    @JsonProperty("FilterExpressions")
    public void setExpressions(List<String> expressions) {

        this.expressions = expressions;
    }

    public List<String> getBlackList() {

        return blackList;
    }

    @JsonProperty("FilterBlacklist")
    public void setBlackList(List<String> blackList) {

        this.blackList = blackList;
    }
}
