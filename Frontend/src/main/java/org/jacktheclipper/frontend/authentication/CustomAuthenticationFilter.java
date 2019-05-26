package org.jacktheclipper.frontend.authentication;

import org.jacktheclipper.frontend.utils.Constants;
import org.springframework.security.authentication.AuthenticationServiceException;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.AuthenticationException;
import org.springframework.security.web.authentication.UsernamePasswordAuthenticationFilter;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

/**
 * A filter to produce a custom authentication token, so users can be identified with additional
 * fields from the login form and not just their email and password
 *
 */
public class CustomAuthenticationFilter extends UsernamePasswordAuthenticationFilter {


    /**
     * Processes a login request of a user
     *
     * @param request  A POST-request holding the information of the user
     * @param response
     * @return The authenticated user
     *
     * @throws AuthenticationException if the user could not be authenticated
     */
    @Override
    public Authentication attemptAuthentication(HttpServletRequest request,
                                                HttpServletResponse response)
            throws AuthenticationException {

        if (!request.getMethod().equals("POST")) {
            throw new AuthenticationServiceException("Authentication method not supported: "
                    + request.getMethod());
        }

        CustomAuthenticationToken authRequest = getAuthRequest(request);
        setDetails(request, authRequest);
        return this.getAuthenticationManager().authenticate(authRequest);
    }


    /**
     * Builds a CustomAuthenticationToken from the given HttpServletRequest
     *
     * @param request The authentication request holding all the required information
     * @return A CustomAuthenticationToken holding the information from the request
     */
    private CustomAuthenticationToken getAuthRequest(HttpServletRequest request) {

        String username = obtainUsername(request);
        String password = obtainPassword(request);
        String organization = obtainOrganization(request);

        if (username == null) {
            username = "";
        }
        if (password == null) {
            password = "";
        }
        if (organization == null) {
            organization = "";
        }

        return new CustomAuthenticationToken(username, password, organization);
    }

    /**
     * Extracts the organization parameter from the given HttpServletRequest
     *
     * @param request The authentication request
     * @return The value of the organization parameter or null if it is absent
     */
    private String obtainOrganization(HttpServletRequest request) {

        return request.getParameter(Constants.SPRING_SECURITY_FORM_ORGANIZATION_KEY);
    }
}