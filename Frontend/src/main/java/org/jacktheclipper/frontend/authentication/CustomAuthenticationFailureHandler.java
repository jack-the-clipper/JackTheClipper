package org.jacktheclipper.frontend.authentication;

import org.jacktheclipper.frontend.exception.UserLockedException;
import org.jacktheclipper.frontend.utils.Constants;
import org.springframework.security.core.AuthenticationException;
import org.springframework.security.web.authentication.AuthenticationFailureHandler;
import org.thymeleaf.util.StringUtils;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;

/**
 * Handles authentication failures as the name implies. This one redirects user's to the login
 * page of their assumed organization. The default implementation does not suffice as we need to
 * simulate a sort of contextpath to allow for organization specific URLs
 */
public class CustomAuthenticationFailureHandler implements AuthenticationFailureHandler {

    /**
     * Redirects the {@link HttpServletResponse} to /{organization}/login.
     * The organization pathvariable is determined by accessing the
     * {@link javax.servlet.http.HttpSession} of the {@link HttpServletRequest}. The value is
     * stored under {@link Constants#SAVED_ORGANIZATION_KEY}.
     *
     * @param request
     * @param response
     * @param e        The authentication exception thrown by the
     *                 {@link org.springframework.security.authentication.AuthenticationProvider}
     *                 implementation
     *                 that tried to authenticate the user. Allows for handling different types of
     *                 exceptions
     * @throws IOException
     * @throws ServletException
     */
    @Override
    public void onAuthenticationFailure(HttpServletRequest request, HttpServletResponse response,
                                        AuthenticationException e)
            throws IOException, ServletException {

        String organization =
                (String) request.getSession().getAttribute(Constants.SAVED_ORGANIZATION_KEY);

        organization = organization == null ? "default" : organization;
        String hostPart = request.getRequestURL().toString().replace(request.getRequestURI(), "");
        String contextPath = StringUtils.isEmpty(request.getContextPath()) ? "" :
                request.getContextPath();
        String query;
        if (e instanceof UserLockedException) {
            query = "locked";
        } else {
            query = "error";
        }
        String redirectUri = hostPart + contextPath + "/" + organization + "/login?" + query;
        response.sendRedirect(redirectUri);
    }
}
