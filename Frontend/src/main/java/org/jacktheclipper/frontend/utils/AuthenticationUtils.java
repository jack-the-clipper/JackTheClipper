package org.jacktheclipper.frontend.utils;

import org.jacktheclipper.frontend.authentication.User;
import org.springframework.security.core.Authentication;

import java.util.UUID;

/**
 * A Utility class for everything concerning authentication. Only works if the authentication has
 * a principal object of type org.jacktheclipper.frontend.authentication.User
 */
public class AuthenticationUtils {

    /**
     * Extracts the user's UUID from the given authentication token.
     * This is such a common occurrence (since the backend uses the id to check permissions) that
     * this utility method saves a lot of typing and casting around
     *
     * @param auth The authentication token holding the user object
     * @return The user's UUID
     *
     * @throws NullPointerException     if the authentication object is null
     * @throws IllegalArgumentException if the authentication does not hold a principal of type
     *                                  org.jacktheclipper.frontend.authentication.User
     */
    public static UUID getUserId(Authentication auth)
            throws IllegalArgumentException, NullPointerException {

        if (auth == null) {
            throw new NullPointerException("Parameter auth must not be null");
        }
        if (auth.getPrincipal() instanceof User) {
            return ((User) auth.getPrincipal()).getUserId();
        }
        throw new IllegalArgumentException("auth#Principal must be of type org.jacktheclipper" +
                ".frontend" + ".authentication.User");
    }

    /**
     * Extracts the user's organization from the given authentication token.
     * This is used frequently during the authentication process. One example where it's used is
     * to determine to which login page a user should be redirected to after he logs out
     *
     * @param auth the authentication token encapsulating the user
     * @return the organization the user belongs to
     *
     * @throws NullPointerException     if the authentication object is null
     * @throws IllegalArgumentException if the authentication does not hold a principal of type
     *                                  org.jacktheclipper.frontend.authentication.User
     */
    public static String getOrganization(Authentication auth)
            throws IllegalArgumentException, NullPointerException {

        if (auth == null) {
            throw new NullPointerException("Parameter auth must not be null");
        }
        if (auth.getPrincipal() instanceof User) {
            return ((User) auth.getPrincipal()).getOrganization();
        }
        throw new IllegalArgumentException("auth#Principal must be of type org.jacktheclipper" +
                ".frontend" + ".authentication.User");
    }
}
