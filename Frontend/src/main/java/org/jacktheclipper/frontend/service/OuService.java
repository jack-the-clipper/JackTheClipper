package org.jacktheclipper.frontend.service;

import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.OrganizationalUnit;
import org.jacktheclipper.frontend.model.OrganizationalUnitSettings;
import org.jacktheclipper.frontend.model.Tuple;
import org.jacktheclipper.frontend.model.UuidStringTuple;
import org.jacktheclipper.frontend.utils.ResponseEntityUtils;
import org.jacktheclipper.frontend.utils.RestTemplateUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.ResponseEntity;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import java.util.*;

/**
 * Handles all request to the backend which concern organizational units. This includes e. g.
 * requesting an organizational unit's settings or getting a list of all existing organizational
 * units.
 */
@Service
public class OuService {
    private static final Logger log = LoggerFactory.getLogger(OuService.class);

    private volatile HashMap<String, UUID> nameToUuidCache = new HashMap<>();
    @Value("${sysadmin.uuid}")
    private String sysAdminUuid;
    @Value("${backend.url}")
    private String backendUrl;

    /**
     * Returns a list of all currently existing organizational units.
     * This does not separte between top level organizational units (like BMW, Audi, etc.) and
     * lower level organizational units (like BMW/Marketing, Audi/HR, etc.).
     *
     * @param userId This parameter is currently unused. Stay tuned for all its possible uses
     * @return A list of all currently existing organizational units
     */
    public List<OrganizationalUnit> getOrganizationalUnits(UUID userId) {
        //TODO static frontenduser is needed
        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        ResponseEntity<OrganizationalUnit[]> response = restTemplate.getForEntity(backendUrl +
                "/getorganizationalunits?userId=" + sysAdminUuid /* + user.getUserId()
                .toString()*/, OrganizationalUnit[].class);
        if (ResponseEntityUtils.successful(response)) {
            return Arrays.asList(response.getBody());
        }
        return Collections.emptyList();
    }

    /**
     * Returns an organizational units settings
     *
     * @param ouId The id of the organizational unit for which the settings are requested
     * @return The settings of the specified organizational unit or null if the unit does not exist
     */
    public OrganizationalUnitSettings getOuSettings(UUID ouId) {

        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        ResponseEntity<OrganizationalUnitSettings> response =
                restTemplate.getForEntity(backendUrl + "/getorganizationalunitsettings" +
                        "?userId=" + sysAdminUuid + "&unitid=" + ouId.
                toString(), OrganizationalUnitSettings.class);
        if (ResponseEntityUtils.successful(response)) {
            return response.getBody();
        }
        return new OrganizationalUnitSettings();
    }

    /**
     * Returns a list of all toplevel organizations.
     * Those are childs of the root {@link OrganizationalUnit} SYSTEM. SYSTEM is also returned
     * and needs to be filtered out
     *
     * @return A list of all toplevel organizations but only their UUID and name
     */
    public List<Tuple<UUID, String>> getTopLevelOrganizations() {

        //TODO filter out SYSTEM. SYSTEM is currently needed to atleast have one organization
        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        String uri = backendUrl + "/principalunits";
        ResponseEntity<UuidStringTuple[]> response = restTemplate.getForEntity(uri,
                UuidStringTuple[].class);
        if (ResponseEntityUtils.successful(response)) {
            return Arrays.asList(response.getBody());
        }
        throw new BackendException("No top level organizations found");
    }

    /**
     * Updates {@link #nameToUuidCache} in a regular interval.
     * The interval is between the begin of method execution. The old call to the method does not
     * have to be completed before the next method call.
     * The interval itself is a magic number and currently sits at 2 minutes.
     */

    @Scheduled(fixedRate = 2 * 1000 * 60)
    private void updateCache() {

        HashMap<String, UUID> temp = new HashMap<>();
        List<Tuple<UUID, String>> topLevelOus = getTopLevelOrganizations();
        for (Tuple<UUID, String> topLevelOu : topLevelOus) {
            temp.put(topLevelOu.second(), topLevelOu.first());
        }
        nameToUuidCache = temp;
        //TODO see how this develops when more Top Level Ous exist
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

}
