package org.jacktheclipper.frontend.service;

import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.MethodResult;
import org.jacktheclipper.frontend.model.Source;
import org.jacktheclipper.frontend.utils.ResponseEntityUtils;
import org.jacktheclipper.frontend.utils.RestTemplateUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.web.client.RestTemplateBuilder;
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

    private String backendUrl;

    private final RestTemplate template;

    public SourceService(RestTemplateBuilder builder, @Value("${backend.url}") String backendUrl) {

        this.backendUrl = backendUrl;
        this.template = builder.build();
    }

    /**
     * Gets the resources available to a user
     *
     * @param userId The id of the user for whom the available sources should be checked
     * @return A list of the available sources for the specified user. It is empty if the user
     * does not exist.
     */
    public List<Source> getAvailableSources(UUID userId) {

        String url = backendUrl + "/availablesources?userId={userId}";
        try {
            ResponseEntity<Source[]> response = template.getForEntity(url, Source[].class,
                    userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                return Arrays.asList(response.getBody());
            }
        } catch (Exception ex) {
            log.warn("Could not get available sources for user [{}]", userId);
            throw new BackendException("Failed to find sources for user [" + userId.toString() +
                    "]");
        }
        log.info("User [{}] does not seem to have any available sources", userId);
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
            String url = backendUrl + "/addsource?userId={userId}";
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(source), MethodResult.class,
                    userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                switch (response.getBody().getState()) {

                    case Successful:
                        log.debug("Successfully added source [{}]", source);
                        return;
                    case UnknownError:
                        log.warn("Request to add source [{}] failed, reason [{}]", source,
                                response.getBody().getMessage());
                        throw new BackendException(response.getBody().getMessage());
                    case Timeout:
                        log.warn("Request to add source [{}] timed out", source);
                        throw new BackendException("Request to add source timed out");
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

        //no need to check for exceptions since trying to recover sources indicates that a user
        // actually has sources available to him
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

        String uri = backendUrl + "/deletesource?userId={userId}&sourceId={sourceId}";
        ResponseEntity<MethodResult> response = template.exchange(uri, HttpMethod.DELETE, null,
                MethodResult.class, userId.toString(), sourceId.toString());
        if (ResponseEntityUtils.successful(response)) {
            switch (response.getBody().getState()) {

                case Successful:
                    log.debug("Successfully deleted source with id [{}]", sourceId);
                    return;
                case UnknownError:
                    log.warn("Could not delete source [{}] due to [{}]", sourceId,
                            response.getBody().getMessage());
                    throw new BackendException(response.getBody().getMessage());
                case Timeout:
                    log.info("Request to delet source [{}] timed out", sourceId);
                    throw new BackendException("Request to delete source with id[" + sourceId +
                            "] timed out");
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
     * @throws BackendException If the REST-call failed or the backend signals failure via a
     *                          {@link MethodResult} with a {@link MethodResult#state} of
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#Timeout} or
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#UnknownError}
     */
    public void updateSource(UUID userId, Source source)
            throws BackendException {

        if (source.getId() == null) {
            throw new NullPointerException("Source#id cannot be null if you want to update it. A "
                    + "null id implies that this source did not exist before.");
        }
        HttpEntity<Source> entity = RestTemplateUtils.prepareBasicHttpEntity(source);
        String uri = backendUrl + "/changesource?userId={userId}";
        ResponseEntity<MethodResult> response = template.exchange(uri, HttpMethod.PUT, entity,
                MethodResult.class, userId.toString());
        if (ResponseEntityUtils.successful(response)) {
            switch (response.getBody().getState()) {

                case Successful:
                    log.debug("Successfully updated source [{}]", source);
                    return;
                case UnknownError:
                    log.warn("Could not update source [{}] due to [{}]", source,
                            response.getBody().getMessage());
                    throw new BackendException(response.getBody().getMessage());
                case Timeout:
                    log.info("Request to add source [{}] timed out", source);
                    throw new BackendException("Request to update source with id[" + source.getId().toString() + "] timed out");
            }
        }
        throw new BackendException("Request to update source with id[" + source.getId().toString() + "] failed");
    }

    /**
     * Enables a {@link Source} on an existing
     * {@link org.jacktheclipper.frontend.model.OrganizationalUnit} so that it can see
     * {@link org.jacktheclipper.frontend.model.Article} indexed from it in Jack the Clipper.
     * This is the counterpart to {@link #disableSourceForUnit(UUID, UUID, UUID)}.
     *
     * @param userId       The user enabling the source
     * @param ouSettingsId The id of the
     *                     {@link org.jacktheclipper.frontend.model.OrganizationalUnitSettings}
     *                     of the {@link org.jacktheclipper.frontend.model.OrganizationalUnit}
     *                     the source should be enabled for
     * @param sourceId     The id of the source that should be enabled for the
     *                     {@link org.jacktheclipper.frontend.model.OrganizationalUnit}
     * @throws BackendException If the REST-call failed or the backend signals failure via a
     *                          {@link MethodResult} with a {@link MethodResult#state} of
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#Timeout} or
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#UnknownError}
     */
    public void enableSourceForUnit(UUID userId, UUID ouSettingsId, UUID sourceId)
            throws BackendException {

        String url = backendUrl + "/enablesourceonorganizationalunit?userId={userId}" +
                "unitSettingsId={settingsId}&sourceId={sourceId}";
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(""), MethodResult.class,
                    userId.toString(), ouSettingsId.toString(), sourceId.toString());
            if (ResponseEntityUtils.successful(response)) {
                switch (response.getBody().getState()) {

                    case Successful:
                        log.debug("Successfully enabled source [{}] on unitsettings [{}]",
                                sourceId, ouSettingsId);
                        return;
                    case UnknownError:
                        log.warn("Request to enable source [{}] on [{}] failed due to [{}]",
                                sourceId, userId, response.getBody().getMessage());
                        throw new BackendException(response.getBody().getMessage());
                    case Timeout:
                        log.info("Request to enable source [{}] on unitsettings [{}] timed out",
                                sourceId, ouSettingsId);
                        throw new BackendException("Request to enable source timed out");
                }
            }
        } catch (Exception ex) {
            //@formatter:off
            log.info("Something went wrong during enabling of [{}] on [{}] as [{}] due to [{}] "
                    + "with reason [{}]", sourceId, ouSettingsId, userId,
                    ex.getClass().getSimpleName(), ex.getMessage());
            throw new BackendException("Failed to enable sourceunit [" + sourceId.toString() +
                    "] on unitSettings [" + ouSettingsId.toString() + "]");
            //@formatter:on
        }
    }

    /**
     * Disables a {@link Source} on an existing
     * {@link org.jacktheclipper.frontend.model.OrganizationalUnit} so that it cannot see
     * {@link org.jacktheclipper.frontend.model.Article} indexed from it in Jack the Clipper
     * anymore. This is the counterpart to {@link #enableSourceForUnit(UUID, UUID, UUID)}
     *
     * @param userId       The user disabling the source
     * @param ouSettingsId The id of the
     *                     {@link org.jacktheclipper.frontend.model.OrganizationalUnitSettings}
     *                     of the {@link org.jacktheclipper.frontend.model.OrganizationalUnit}
     *                     the source should be disabled for
     * @param sourceId     The id of the source that should be disabled for the
     *                     {@link org.jacktheclipper.frontend.model.OrganizationalUnit}
     * @throws BackendException If the REST-call failed or the backend signals failure via a
     *                          {@link MethodResult} with a {@link MethodResult#state} of
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#Timeout} or
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#UnknownError}
     */
    public void disableSourceForUnit(UUID userId, UUID ouSettingsId, UUID sourceId)
            throws BackendException {

        String url = backendUrl + "/disablesourceonorganizationalunit?userId={userId}" +
                "&unitSettingsId={settingsId}&sourceId={sourceId}";
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(""), MethodResult.class,
                    userId.toString(), ouSettingsId.toString(), sourceId.toString());
            if (ResponseEntityUtils.successful(response)) {
                switch (response.getBody().getState()) {

                    case Successful:
                        log.debug("Successfully disabled source [{}] on unitsettings [{}]",
                                sourceId, ouSettingsId);
                        return;
                    case UnknownError:
                        log.warn("Request to disable source [{}] on [{}] failed due to [{}]",
                                sourceId, userId, response.getBody().getMessage());
                        throw new BackendException(response.getBody().getMessage());
                    case Timeout:
                        log.info("Request to disable source [{}] on unitsettings [{}] timed out",
                                sourceId, ouSettingsId);
                        throw new BackendException("Request to disable source timed out");
                }
            }
        } catch (Exception ex) {
            //@formatter:off
            log.info("Something went wrong during disabling of [{}] on [{}] as [{}] due to [{}] "
                    + "with reason [{}]", sourceId, ouSettingsId, userId,
                    ex.getClass().getSimpleName(), ex.getMessage());
            throw new BackendException("Failed to disable source [" + sourceId.toString() + "] "
                    + "on unitSettings [" + ouSettingsId.toString() + "]");
            //@formatter:on
        }
    }
}
