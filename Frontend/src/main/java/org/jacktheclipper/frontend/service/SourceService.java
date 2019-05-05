package org.jacktheclipper.frontend.service;

import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.MethodResult;
import org.jacktheclipper.frontend.model.Source;
import org.jacktheclipper.frontend.utils.ResponseEntityUtils;
import org.jacktheclipper.frontend.utils.RestTemplateUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpMethod;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.client.RestTemplate;

import java.util.*;

/**
 * Handles all requests to the backend concerning sources. This includes e. g. adding them or
 * checking which are available for a given user.
 */
@Service
public class SourceService {

    private static final Logger log = LoggerFactory.getLogger(SourceService.class);

    @Value("${backend.url}")
    private String backendUrl;

    /**
     * Gets the resources available to a user
     *
     * @param userId The id of the user for whom the available sources should be checked
     * @return A list of the available sources for the specified user. It is empty if the user
     * does not exist.
     */
    public List<Source> getAvailableSources(UUID userId) {

        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        ResponseEntity<Source[]> response = restTemplate.getForEntity(backendUrl +
                "/availablesources?userId=" + userId.toString(), Source[].class);
        if (ResponseEntityUtils.successful(response)) {
            return Arrays.asList(response.getBody());
        }
        return Collections.emptyList();
    }

    /**
     * Adds the source
     * This does not make the source available to any existing organizational unit
     *
     * @param source The source to add
     * @param userId The user performing this request. It is used by the backend to check
     *               permissions
     */
    public void addSource(Source source, UUID userId) {

        try {
            String uriParameters = "/addsource?userId=" + userId.toString();
            RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
            ResponseEntity<MethodResult> response =
                    restTemplate.exchange(backendUrl + uriParameters, HttpMethod.PUT,
                            RestTemplateUtils.prepareBasicHttpEntity(source), MethodResult.class);
            if (ResponseEntityUtils.successful(response)) {
                switch (response.getBody().getState()) {

                    case Successful:
                        log.debug("Successfully added source [{}]", source);
                        break;
                    case UnknownError:
                        throw new BackendException(response.getBody().getMessage());
                    case Timeout:
                        throw new BackendException("Request to [" + uriParameters + "] timed out");
                }
            }
        } catch (HttpClientErrorException.BadRequest exception) {
            log.warn("Backend fucked up");
            throw new BackendException("Could not add Source " + source.getName());
        }
    }

    /**
     * Uses the list of incomplete sources to return a list with complete ones
     * The method is necessary since the Source class uses a #toString method that does not use
     * all of its fields.
     *
     * @param receivedSources A list of source, where only the id and name field is set
     * @param userId          The user needed to access the list of sources in the backend
     * @return A list of complete Source objects, where every source from the input list is
     * included and has each of its fields set
     */
    public List<Source> recoverSources(List<Source> receivedSources, UUID userId) {

        List<Source> allSources = getAvailableSources(userId);
        List<Source> returnValues = new ArrayList<>();
        for (Source source : allSources) {
            if (receivedSources.contains(source)) {
                returnValues.add(source);
            }
        }
        return returnValues;
    }

    /**
     * Tells the backend to erase the source with the supplied UUID from the database
     *
     * @param userId   The user attempting the deletion
     * @param sourceId The id of the source that should be deleted
     */
    public void deleteSource(UUID userId, UUID sourceId) {

        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        String uri =
                backendUrl + "/deletesource?userId=" + userId.toString() + "&sourceId=" + sourceId.toString();
        ResponseEntity<MethodResult> response = restTemplate.exchange(uri, HttpMethod.DELETE,
                null, MethodResult.class);
        if (ResponseEntityUtils.successful(response)) {
            switch (response.getBody().getState()) {

                case Successful:
                    log.debug("Successfully deleted source with id [{}]", sourceId);
                    return;
                case UnknownError:
                    throw new BackendException(response.getBody().getMessage());
                case Timeout:
                    throw new BackendException("Request to delete source with id[" + sourceId +
                            "] " + "timed out");
            }
        }
        throw new BackendException("Request to delete source with id[" + sourceId + "] " +
                "failed");
    }

    /**
     * Updates the source. This requires the source to have an id present. Otherwise this method
     * will not work
     *
     * @param userId The id of the user doing the update
     * @param source The updated version of the source
     */
    public void updateSource(UUID userId, Source source) {

        if (source.getId() == null) {
            throw new NullPointerException("Source#id cannot be null if you want to update it. A "
                    + "null id " + "implies that this source did not exist before.");
        }
        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        HttpEntity<Source> entity = RestTemplateUtils.prepareBasicHttpEntity(source);
        String uri =
                backendUrl + "/changesource?userId=" + userId.toString() + "&tochange=" + source.getId().toString();
        ResponseEntity<MethodResult> response = restTemplate.exchange(uri, HttpMethod.PUT, entity
                , MethodResult.class);
        if (ResponseEntityUtils.successful(response)) {
            switch (response.getBody().getState()) {

                case Successful:
                    log.debug("Successfully updated source [{}]", source);
                    return;
                case UnknownError:
                    throw new BackendException(response.getBody().getMessage());
                case Timeout:
                    throw new BackendException("Request to update source with id[" + source.getId().toString() + "] " + "timed out");
            }
        }
        throw new BackendException("Request to update source with id[" + source.getId().toString() + "] " + "failed");

    }
}
