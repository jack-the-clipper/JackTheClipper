package org.jacktheclipper.frontend.service;

import org.jacktheclipper.frontend.model.OrganizationalUnit;
import org.jacktheclipper.frontend.model.OrganizationalUnitSettings;
import org.jacktheclipper.frontend.utils.ResponseEntityUtils;
import org.jacktheclipper.frontend.utils.RestTemplateUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

/**
 * Handles all request to the backend which concern organizational units. This includes e. g.
 * requesting an organizational unit's settings or getting a list of all existing organizational
 * units.
 */
@Service
public class OuService {
    private static final Logger log = LoggerFactory.getLogger(OuService.class);

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
                "/getorganizationalunits?" + "userId" + "=" + sysAdminUuid /* + user.getUserId()
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
}
