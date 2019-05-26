package org.jacktheclipper.frontend.service;

import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.*;
import org.jacktheclipper.frontend.utils.ResponseEntityUtils;
import org.jacktheclipper.frontend.utils.RestTemplateUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.web.client.RestTemplateBuilder;
import org.springframework.http.HttpMethod;
import org.springframework.http.ResponseEntity;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Service;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.client.RestTemplate;

import java.util.*;
import java.util.stream.Collectors;

/**
 * Handles all request to the backend which concern organizational units. This includes e. g.
 * requesting an organizational unit's settings or getting a list of all existing organizational
 * units.
 */
@Service
public class OuService {
    private static final Logger log = LoggerFactory.getLogger(OuService.class);

    private volatile HashMap<String, UUID> nameToUuidCache = new HashMap<>();

    private String backendUrl;

    private final RestTemplate template;

    @Autowired
    public OuService(RestTemplateBuilder builder, @Value("${backend.url}") String backendUrl) {

        this.backendUrl = backendUrl;
        this.template = builder.build();
    }

    /**
     * Returns a list of all currently existing {@link OrganizationalUnit}s.
     * This does not separate between top level organizational units (like BMW, Audi, etc.) and
     * lower level organizational units (like BMW/Marketing, Audi/HR, etc.).
     * This method calls {@link #getOrganizationalUnits(UUID, boolean)} with the boolean being set
     * to false.
     *
     * @param userId The user requesting the {@link OrganizationalUnit}s
     * @return A list of all currently existing organizational units. The entries might be nested
     * in {@link OrganizationalUnit#children}.
     *
     * @throws BackendException Propagates the exception from
     *                          {@link #getOrganizationalUnits(UUID, boolean)}
     */
    public List<OrganizationalUnit> getOrganizationalUnits(UUID userId)
            throws BackendException {

        return getOrganizationalUnits(userId, false);
    }

    /**
     * Returns a list of all currently existing {@link OrganizationalUnit}s.
     * This does not separate between top level organizational units (like BMW, Audi, etc.) and
     * lower level organizational units (like BMW/Marketing, Audi/HR, etc.).
     * The method can flatten the result to have every {@link OrganizationalUnit} directly
     * available in the list if instructed to do so via the second parameter
     *
     * @param userId  The user requesting the {@link OrganizationalUnit}s
     * @param flatten Decides whether to flatten the hierarchical structure of
     *                {@link OrganizationalUnit} and their {@link OrganizationalUnit#children}
     * @return A list of all currently existing organizational units
     *
     * @throws BackendException If the REST-call failed
     */
    public List<OrganizationalUnit> getOrganizationalUnits(UUID userId, boolean flatten)
            throws BackendException {

        String url = backendUrl + "/getorganizationalunits?userId={userId}";
        try {

            ResponseEntity<OrganizationalUnit[]> response = template.getForEntity(url,
                    OrganizationalUnit[].class, userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                if (flatten) {
                    return flattenOus(Arrays.asList(response.getBody()));
                } else {
                    return Arrays.asList(response.getBody());
                }
            }
        } catch (Exception ex) {
            log.info("Could not retrieve all OUs");
            throw new BackendException("Failed to retrieve all OrganizationalUnits");
        }
        return Collections.emptyList();
    }

    /**
     * Flattens a {@link List} of {@link OrganizationalUnit}s since an {@link OrganizationalUnit}
     * can contain children. The backend does not return a flat version of the
     * {@link OrganizationalUnit}s but rather a nested one where every child needs to be accessed
     * via {@link OrganizationalUnit#getChildren()} of their respective parent.
     *
     * @param units The list of {@link OrganizationalUnit}s to flatten
     * @return A flat version of the given input where every child is accessible in the list
     * directly and does not need to be searched via its parents
     * {@link OrganizationalUnit#children} field.
     */
    private List<OrganizationalUnit> flattenOus(List<OrganizationalUnit> units) {

        List<OrganizationalUnit> allUnits = new ArrayList<>();
        for (OrganizationalUnit unit : units) {
            allUnits.add(unit);
            if (unit.getChildren() != null && unit.getChildren().size() > 0) {
                allUnits.addAll(flattenOus(unit.getChildren()));
            }
        }
        return allUnits;
    }

    /**
     * Returns an organizational units settings
     *
     * @param ouId   The id of the {@link OrganizationalUnit} for which the
     *               {@link OrganizationalUnitSettings}are requested
     * @param userId The id of the user requesting the settings
     * @return The settings of the specified organizational unit or null if the unit does not exist
     *
     * @throws BackendException If the REST-call failed or no {@link OrganizationalUnitSettings}
     *                          were found for the supplied id
     */
    public OrganizationalUnitSettings getOuSettings(UUID ouId, UUID userId)
            throws BackendException {

        String url = backendUrl + "/getorganizationalunitsettings?userId={userId}&unitId={unitId}";
        try {
            ResponseEntity<OrganizationalUnitSettings> response = template.getForEntity(url,
                    OrganizationalUnitSettings.class, userId.toString(), ouId.toString());
            if (ResponseEntityUtils.successful(response)) {
                return response.getBody();
            }
        } catch (Exception ex) {
            log.info("Could not get settings for [{}]", ouId);
            throw new BackendException("Settings not found for unit with id [" + ouId + "]");
        }
        log.warn("Failed to retrieve settings for ou [{}] as user [{}]", ouId, userId);
        throw new BackendException("Response for request to get settings of " + ouId.toString() + "was empty");
    }

    /**
     * Updates the settings of  an {@link OrganizationalUnit}.
     * The unit and its settings may not be linked in the classes in the frontend but are so in
     * the backend
     *
     * @param userId   The user performing the update
     * @param settings The updated version of already existing {@link OrganizationalUnitSettings}
     *                 . Existing {@link OrganizationalUnitSettings} have a
     *                 {@link OrganizationalUnitSettings#id} that is not {@code null}
     * @throws BackendException If the REST-call failed or the backend signals failure via a
     *                          {@link MethodResult} with a {@link MethodResult#state} of
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#Timeout} or
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#UnknownError}
     */
    public void updateOrganizationalUnitSettings(UUID userId, OrganizationalUnitSettings settings)
            throws BackendException {

        String url = backendUrl + "/saveorganizationalunitsettings?userId={userId}";
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(settings), MethodResult.class,
                    userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                switch (response.getBody().getState()) {

                    case Successful:
                        log.debug("Successfully unitsettings [{}]", settings.getId());
                        return;
                    case UnknownError:
                        log.warn("Request to update settings [{}] failed due to [{}]",
                                settings.getId(), response.getBody().getMessage());
                        throw new BackendException(response.getBody().getMessage());
                    case Timeout:
                        log.info("Request to update settings [{}] as user [{}] timed out",
                                settings.getId(), userId);
                        throw new BackendException("Request to update unitsettings timed out");
                }
            }
        } catch (Exception ex) {
            log.info("Something went wrong during update of [{}] as [{}] due to [{}] with reason "
                    + "[{}]", settings, userId, ex.getClass().getSimpleName(), ex.getMessage());
            throw new BackendException("Failed to update unitsettings [" + settings.getId() + "]");
        }
    }

    /**
     * Returns a list of all principalunits.
     * Those are childs of the root {@link OrganizationalUnit} SYSTEM. The information is minimal as
     * it only returns the id and the name of those {@link OrganizationalUnit}. This is mostly
     * used for
     * security reasons. E.g. beans like
     * {@link org.jacktheclipper.frontend.authentication.OrganizationGuard} use this to
     * determine whether an organization actually exists. Though they only access this method
     * indirectly via {@link #nameToUuidCache}, or more specifically
     * {@link #mapOuNameToOuUUID(String)}.
     *
     * @return A list of all toplevel organizations but only their UUID and name
     *
     * @throws BackendException If the REST-call failed
     */
    public List<Tuple<UUID, String>> getPrincipalUnitsMinimalInformation()
            throws BackendException {

        return getPrincipalUnitsMinimalInformation(false);

    }

    /**
     * Returns a list of all principalunits.
     * Those are childs of the root {@link OrganizationalUnit} SYSTEM. SYSTEM is also returned
     * and needs to be filtered out. The information is minimal as it only returns the id and the
     * name of those principalunits. This is mostly used for security reasons. E.g. beans
     * like {@link org.jacktheclipper.frontend.authentication.OrganizationGuard} use this to
     * determine whether an organization actually exists. Though they only access this method
     * indirectly via {@link #nameToUuidCache}, or more specifically
     * {@link #mapOuNameToOuUUID(String)}.
     *
     * @param withSystem Whether to return the {@link OrganizationalUnit} {@code "SYSTEM} in the
     *                   result list
     * @return A list of all toplevel organizations but only their UUID and name
     *
     * @throws BackendException If the REST-call failed
     */
    private List<Tuple<UUID, String>> getPrincipalUnitsMinimalInformation(boolean withSystem)
            throws BackendException {

        String uri = backendUrl + "/getprincipalunitsbasic";
        try {
            ResponseEntity<UuidStringTuple[]> response = template.getForEntity(uri,
                    UuidStringTuple[].class);
            if (ResponseEntityUtils.successful(response)) {
                if (withSystem) {
                    return Arrays.asList(response.getBody());
                } else {
                    return Arrays.stream(response.getBody()).filter(tuple -> !tuple.second().equals("SYSTEM")).collect(Collectors.toList());
                }
            }
        } catch (Exception ex) {
            log.warn("Something went wrong due to [{}]. Could not get top level orgas",
                    ex.getMessage());
            throw new BackendException("Could not get top level orgas");
        }
        log.warn("Failed to fetch required basic information on principalunits");
        throw new BackendException("No top level organizations found");
    }

    /**
     * Returns a list of {@link OrganizationalUnit} that are childs of {@code "SYSTEM"}.
     * This means they are the root of a customer like Example Ltd. Compared to
     * {@link #getPrincipalUnitsMinimalInformation()} the returned Objects are actual instances
     * of {@link OrganizationalUnit} and not just a {@link Tuple} of their id and name.
     *
     * @param userId The user requesting
     * @return A list of all principal units
     *
     * @throws BackendException If the REST-call failed. This might be because the
     *                          {@link User} belonging
     *                          to the supplied id does not have sufficient rights
     */
    public List<OrganizationalUnit> getPrincipalUnits(UUID userId)
            throws BackendException {

        String url = backendUrl + "/getprincipalunits?userId={userId}";
        try {
            ResponseEntity<OrganizationalUnit[]> response = template.getForEntity(url, OrganizationalUnit[].class,
                    userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully retrieved principalunits with all information");
                return new ArrayList<>(Arrays.asList(response.getBody()));
            }
        } catch (Exception ex) {
            log.info("Request to get all principalunits with full information as user [{}] " +
                    "failed due to [{}] with message [{}]", userId, ex.getClass().getSimpleName()
                    , ex.getMessage());
            throw new BackendException("Could not get principal units due to [" + ex.getMessage() + "]");
        }
        return Collections.emptyList();
    }

    /**
     * Updates {@link #nameToUuidCache} in a regular interval.
     * The interval is between the begin of method execution. The old call to the method does not
     * have to be completed before the next method call.
     * The interval itself is a magic number and currently sits at 2 minutes.
     */
    @Scheduled(fixedRate = 2 * 1000 * 60)
    private void updateCache() {

        try {
            HashMap<String, UUID> temp = new HashMap<>();
            List<Tuple<UUID, String>> topLevelOus = getPrincipalUnitsMinimalInformation(true);
            for (Tuple<UUID, String> topLevelOu : topLevelOus) {
                temp.put(topLevelOu.second(), topLevelOu.first());
            }
            nameToUuidCache = temp;
        } catch (BackendException backendEx) {
            log.warn("Could not update cache, working on old one [{}]", nameToUuidCache);
        }
    }

    /**
     * Determines the {@link UUID} corresponding to the supplied name.
     * This allows for using a {@link HashMap} as a cache since it might be useful.
     *
     * @param ouName The name for which the {@link UUID} should be found
     * @return The {@link UUID} corresponding to the supplied name. Might be {@code null} if the
     * name does not exist or the cache was not updated fast enough.
     */
    public UUID mapOuNameToOuUUID(String ouName) {

        return nameToUuidCache.get(ouName);
    }

    /**
     * Gets the children of the supplied unit
     *
     * @param unitId The id of the {@link OrganizationalUnit} for which the children should be found
     * @return A list of {@link Tuple} with the children's id and name
     *
     * @throws BackendException If the REST-call failed
     */
    public List<Tuple<UUID, String>> getUnitChildren(UUID unitId)
            throws BackendException {

        String url = backendUrl + "/getprincipalunitchildren?principalUnitId={unitId}";
        try {
            ResponseEntity<UuidStringTuple[]> response = template.getForEntity(url,
                    UuidStringTuple[].class, unitId.toString());
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully got children of [{}]", unitId);
                return Arrays.asList(response.getBody());
            }
        } catch (HttpClientErrorException.BadRequest ex) {

            log.info("Could not get children of [{}]", unitId);
            throw new BackendException("Failed to get children of [" + unitId.toString() + "]");
        }
        log.info("[{}] did not have children", unitId);
        return Collections.emptyList();
    }

    /**
     * Overloaded version of {@link #getUnitChildren(UUID)}.
     * It maps the supplied organization name to its id
     *
     * @param organization The name of the {@link OrganizationalUnit} for which the children
     *                     should be found
     * @return A list of {@link Tuple} with the children's id and name
     *
     * @throws IllegalArgumentException If the organization's name could not be mapped to an id
     * @throws BackendException         If the call to {@link #getUnitChildren(UUID)} throws it.
     *                                  This signals a REST-Call gone wrong
     */
    public List<Tuple<UUID, String>> getUnitChildren(String organization)
            throws IllegalArgumentException, BackendException {

        UUID id = mapOuNameToOuUUID(organization);
        if (id == null) {
            log.warn("Could not map [{}] to an id", organization);
            throw new IllegalArgumentException("Organization [" + organization + " does not " +
                    "exist in cache");
        }
        return getUnitChildren(id);
    }

    /**
     * Adds a normal unit with the supplied name to the supplied parent.
     * {@link #addPrincipalUnit(UUID, String, String)}
     * adds an {@link OrganizationalUnit} as a customer root to Jack the Clipper
     *
     * @param userId     The user adding the unit
     * @param parentOuId The parent of the unit that should be created. E.g. the new unit should
     *                   inherit this {@link OrganizationalUnit}'s
     *                   {@link OrganizationalUnitSettings}.
     * @param name       The name of the created unit
     * @throws BackendException If the REST-call failed or the backend signals failure via a
     *                          {@link MethodResult} with a {@link MethodResult#state} of
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#Timeout} or
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#UnknownError}
     */
    public void addOrganizationalUnit(UUID userId, UUID parentOuId, String name)
            throws BackendException {

        String url = backendUrl + "/addunit?userId={userId}&name={name}&parentUnitId={parentId}";
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(""), MethodResult.class,
                    userId.toString(), name, parentOuId.toString());
            if (ResponseEntityUtils.successful(response)) {
                switch (response.getBody().getState()) {

                    case Successful:
                        log.debug("Successfully added unit [{}] to [{}]", name, parentOuId);
                        return;
                    case UnknownError:
                        log.warn("Request to add [{}] to [{}] by [{}] failed due to [{}]", name,
                                parentOuId, userId, response.getBody().getMessage());
                        throw new BackendException(response.getBody().getMessage());
                    case Timeout:
                        log.info("Request to add [{}] with parent [{}] by [{}] timed out", name,
                                parentOuId, userId);
                        throw new BackendException("Request to add unit timed out");
                }
            }

        } catch (Exception ex) {
            throw new BackendException("Could not add " + name + " to " + parentOuId.toString());
        }
    }

    /**
     * Deletes the specified {@link OrganizationalUnit}
     *
     * @param userId The user requesting the deletion
     * @param unitId The id of the unit that should be deleted
     * @throws BackendException If the deletion failed
     */
    public void deleteOrganizationalUnit(UUID userId, UUID unitId)
            throws BackendException {

        String url = backendUrl + "/deleteorganizationalunit?userId={userId}&unitId={unitId}";
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.DELETE,
                    RestTemplateUtils.prepareBasicHttpEntity(""), MethodResult.class,
                    userId.toString(), unitId.toString());
            log.debug("Successfully deleted [{}]", unitId);
        } catch (Exception ex) {
            log.info("Could not delete unit [{}] due to exception [{}] with message [{}]", unitId
                    , ex.getClass().getSimpleName(), ex.getMessage());
            throw new BackendException("Could not delete unit [" + unitId.toString() + "]");
        }
    }

    /**
     * Updates an already existing {@link OrganizationalUnit} at the backend
     *
     * @param userId The id of user updating the unit
     * @param unit   The updated version of an already existing {@link OrganizationalUnit}. You can
     *               see that a unit does exist by checking if {@link OrganizationalUnit#getId()}
     *               returns {@code null}
     * @throws BackendException If the REST-call failed or the backend signals failure via a
     *                          {@link MethodResult} with a {@link MethodResult#state} of
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#Timeout} or
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#UnknownError}
     */
    public void updateOrganizationalUnit(UUID userId, OrganizationalUnit unit)
            throws BackendException {

        String url = backendUrl + "/updateorganizationalunit?userId={userId}";
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(unit), MethodResult.class,
                    userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                switch (response.getBody().getState()) {

                    case Successful:
                        log.debug("Successfully updated unit [{}]", unit);
                        return;
                    case UnknownError:
                        log.warn("Request to update [{}] as [{}] failed due to [{}]", unit,
                                userId, response.getBody().getMessage());
                        throw new BackendException(response.getBody().getMessage());
                    case Timeout:
                        log.info("Request to update [{}] as user [{}] timed out", unit, userId);
                        throw new BackendException("Request to add unit timed out");
                }
            }
        } catch (Exception ex) {
            log.info("Something went wrong during update of [{}] as [{}] due to [{}] with reason "
                    + "[{}]", unit, userId, ex.getClass().getSimpleName(), ex.getMessage());
            throw new BackendException("Failed to update unit [" + unit.getId() + "]");
        }
    }

    /**
     * Adds a new {@link OrganizationalUnit} but compared to
     * {@link #addOrganizationalUnit(UUID, UUID, String)} this one has the {@code "SYSTEM} unit
     * as its parent ou. The new OrganizationalUnits is thus a customer of Jack the Clipper.
     * Furthermore a default user with
     * {@link org.jacktheclipper.frontend.enums.UserRole#StaffChief} is created for the unit with
     * the supplied principalMail as mail address
     *
     * @param userId        The id of the user adding this new principal unit
     * @param name          The name of the new principalunit
     * @param principalMail The email of the default user that is created for this new principalunit
     * @throws BackendException If the REST-call failed or the backend signals failure via a
     *                          {@link MethodResult} with a {@link MethodResult#state} of
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#Timeout} or
     *                          {@link org.jacktheclipper.frontend.enums.SuccessState#UnknownError}
     */
    public void addPrincipalUnit(UUID userId, String name, String principalMail)
            throws BackendException {

        String url = backendUrl + "/addprincipalunit?userId={userId}&name={name}" +
                "&principalUnitMail={mail}";
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(""), MethodResult.class,
                    userId.toString(), name, principalMail);
            if (ResponseEntityUtils.successful(response)) {
                MethodResult result = response.getBody();
                switch (result.getState()) {

                    case Successful:
                        //@formatter:off
                        log.debug("Successfully added principal unit [{}] with defaultuser mail " +
                                "[{}]", name, principalMail);
                        //@formatter:on
                        updateCache();
                        return;
                    case UnknownError:
                        log.warn("Request to add principalUnit [{}] with mail [{}] failed due to "
                                + "[{}]", name, principalMail, result.getMessage());

                        throw new BackendException(response.getBody().getMessage());
                    case Timeout:
                        log.info("Request to add principalunit [{}] with mail [{}] timed out",
                                name, principalMail);
                        throw new BackendException("Request to add principal unit timed out");
                }
            }
        } catch (Exception ex) {
            //@formatter:off
            log.info("Failed to add principalunit [{}] with mail [{}] as [{}] due to [{}] with " +
                     "reason [{}]", name, principalMail, userId, ex.getClass().getSimpleName(),
                     ex.getMessage());
            //@formatter:on
            throw new BackendException("Failed to update principalunit [" + name + "]");
        }
    }
}
