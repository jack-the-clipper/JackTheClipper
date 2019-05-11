package org.jacktheclipper.frontend.authentication;

import org.jacktheclipper.frontend.model.OrganizationalUnit;
import org.jacktheclipper.frontend.service.OuService;
import org.jacktheclipper.frontend.utils.AuthenticationUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Component;

/**
 * A class concerned with giving/ denying user's access to parts of the Jack the Clipper
 * application.
 * It adds checks concerning the organization names to the application. E. g. nobody can access
 * /hansi/login if there is no {@link org.jacktheclipper.frontend.model.OrganizationalUnit} for
 * which {@link OrganizationalUnit#getName()} returns {@code "hansi"}.
 */
@Component
public class OrganizationGuard {
    private final OuService ouService;
    private final static Logger log = LoggerFactory.getLogger(OrganizationGuard.class);

    public OrganizationGuard(OuService ouService) {

        this.ouService = ouService;
    }

    /**
     * Compares the {@link User#organization} with the organization he is trying to access
     *
     * @param authentication The authentication holding the user. Normally of type
     *                       {@link CustomAuthenticationToken}
     * @param accessedOrga   The organization context the user is trying to access. If he was
     *                       trying to access {@code "/audi/feed/"} this would be {@code "audi"}.
     * @return {@code true} if the organization does exist and matches the user's, {@code false}
     * otherwise
     */
    public boolean isOwnOrganization(Authentication authentication, String accessedOrga) {

        if (authentication == null || authentication.getPrincipal() == null || !(authentication.getPrincipal() instanceof User)) {
            return false;
        }
        log.debug("trying to access [{}] with own orga [{}]", accessedOrga,
                AuthenticationUtils.getOrganization(authentication));

        return isValidOrganization(accessedOrga) && accessedOrga.equals(AuthenticationUtils.getOrganization(authentication));
    }

    /**
     * Checks whether an organization does exist
     *
     * @param organization The name of the organization to check
     * @return {@code true} if the organization exists, {@code false} otherwise
     */
    public boolean isValidOrganization(String organization) {

        return ouService.mapOuNameToOuUUID(organization) != null;
    }

    /**
     * Checks whether a user has to change his password. It basically only delegates to
     * {@link AuthenticationUtils#isMustChangePassword(Authentication)}.
     *
     * @param authentication
     * @return {@code True} if the user does not need to change his password, {@code false}
     * otherwise
     */
    public boolean passwordOkay(Authentication authentication) {

        if (authentication == null || !(authentication.getPrincipal() instanceof User)) {
            return false;
        }
        return !AuthenticationUtils.isMustChangePassword(authentication);
    }
}
