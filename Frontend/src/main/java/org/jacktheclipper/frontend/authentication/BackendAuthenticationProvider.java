package org.jacktheclipper.frontend.authentication;

import org.jacktheclipper.frontend.exception.UserLockedException;
import org.jacktheclipper.frontend.model.User;
import org.jacktheclipper.frontend.service.UserService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.security.authentication.AuthenticationProvider;
import org.springframework.security.authentication.BadCredentialsException;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.AuthenticationException;
import org.springframework.stereotype.Component;
import org.springframework.web.client.HttpClientErrorException;

/**
 * Authenticates users with the backend.
 *
 * @author SBG
 */
@Component
public class BackendAuthenticationProvider implements AuthenticationProvider {


    private static final Logger log = LoggerFactory.getLogger(BackendAuthenticationProvider.class);

    private final UserService userService;

    @Autowired
    public BackendAuthenticationProvider(UserService userService) {

        this.userService = userService;
    }

    /**
     * Tries to authenticate the given token at the backend
     *
     * @param authentication The token representing an authentication. It is one of the supported
     *                       classes, see {@link #supports(Class)}
     * @return An authenticated user
     *
     * @throws AuthenticationException if the user could not be authenticated. This happens
     *                                 if e. g. the backend is not reachable or the user mistyped
     *                                 his password or username
     */
    @Override
    public Authentication authenticate(Authentication authentication)
            throws AuthenticationException {

        String username;
        if (authentication.getPrincipal() instanceof User) {
            username = ((User) authentication.getPrincipal()).geteMail();
        } else {
            username = authentication.getName();
        }
        String password = (String) authentication.getCredentials();
        String organization = ((CustomAuthenticationToken) authentication).getOrganization();


        try {
            User user = userService.authenticate(username, organization, password);
            if (!user.isUnlocked()){
                log.info("Access denied for User [{}] since he is not unlocked", username);
                throw  new UserLockedException("User was not unlocked but does exist");
            }
            return new CustomAuthenticationToken(user, password, organization,
                    user.getUserRole().resolveAuthorities());
        } catch (HttpClientErrorException.BadRequest ex) {
            log.info("Access denied for User [{}]", username);
            //Stacktrace is ignored as HttpStatusCode 400 is an intended signal from the backend
            //implying that authentication failed
        }

        throw new BadCredentialsException(organization + ": No user called [" + username + "] "
                + "with supplied PW");
    }

    /**
     * Checks whether the supplied class is supported by this class
     * This tells the Spring framework which classes this AuthenticationProvider can authenticate
     * and thus process
     *
     * @param authentication The class to check
     * @return {@code True} if the class is supported, {@code False} otherwise
     */
    @Override
    public boolean supports(Class<?> authentication) {

        return authentication.equals(CustomAuthenticationToken.class);
    }


}
