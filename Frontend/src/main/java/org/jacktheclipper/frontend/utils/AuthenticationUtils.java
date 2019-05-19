package org.jacktheclipper.frontend.utils;

import org.jacktheclipper.frontend.model.User;
import org.springframework.security.core.Authentication;

import java.util.UUID;

/**
 * A Utility class for everything concerning {@link Authentication}. Only works if the
 * authentication has
 * a principal object of type {@link User}
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
     * @throws NullPointerException     If the authentication object is null
     * @throws IllegalArgumentException If the authentication does not hold a principal of type
     *                                  {@link User}
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
                ".frontend.authentication.User but was of type " +
                auth.getPrincipal().getClass().getCanonicalName());
    }

    /**
     * Extracts the user's organization from the given authentication token.
     * This is used frequently during the authentication process. One example where it's used is
     * to determine to which login page a user should be redirected to after he logs out
     *
     * @param auth the authentication token encapsulating the user
     * @return the organization the user belongs to
     *
     * @throws NullPointerException     If the authentication object is null
     * @throws IllegalArgumentException If the authentication does not hold a principal of type
     *                                  {@link User}
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
                ".frontend.authentication.User but was of type " +
                auth.getPrincipal().getClass().getCanonicalName());
    }

    /**
     * Extracts the user's email address from the supplied authentication token.
     *
     * @param auth The authentication token encapsulating the user
     * @return The user's email address
     *
     * @throws IllegalArgumentException If {@link Authentication#getPrincipal()} does not return
     *                                  an Object of type {@link User}
     * @throws NullPointerException     If the authentication parameter is {@code null}
     */
    public static String getEmail(Authentication auth)
            throws IllegalArgumentException, NullPointerException {

        if (auth == null) {
            throw new NullPointerException("Parameter auth must not be null");
        }
        if (auth.getPrincipal() instanceof User) {
            return ((User) auth.getPrincipal()).geteMail();
        }
        throw new IllegalArgumentException("auth#Principal must be of type org.jacktheclipper" +
                ".frontend.authentication.User but was of type " +
                auth.getPrincipal().getClass().getCanonicalName());
    }

    /**
     * Extracts whether a user should change his password.
     * The method acts like a standard getter and thus does the same as
     * {@link User#isMustChangePassword()}. Due to the {@link Authentication} using
     * {@link Object} to store the principal this also makes accessing easier and prevents
     * casting errors
     *
     * @param auth The authentication token encapsulating the user
     * @return Whether the user should change his password. This is {@code true} if he recently
     * reset his password via
     * {@link org.jacktheclipper.frontend.service.UserService#resetPassword(String)} or the user
     * was created by a {@link org.jacktheclipper.frontend.enums.UserRole#StaffChief}. Otherwise
     * {@code false} is returned
     *
     * @throws NullPointerException     If the authentication object is null
     * @throws IllegalArgumentException If the authentication does not hold a principal of type
     *                                  {@link User}
     */
    public static boolean isMustChangePassword(Authentication auth)
            throws IllegalArgumentException, NullPointerException {

        if (auth == null) {
            throw new NullPointerException("Parameter auth must not be null");
        }
        if (auth.getPrincipal() instanceof User) {
            return ((User) auth.getPrincipal()).isMustChangePassword();
        }
        throw new IllegalArgumentException("auth#Principal must be of type org.jacktheclipper" +
                ".frontend.authentication.User but was of type " +
                auth.getPrincipal().getClass().getCanonicalName());
    }
}
