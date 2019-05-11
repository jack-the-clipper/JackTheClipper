package org.jacktheclipper.frontend.authentication;

import org.jacktheclipper.frontend.utils.AuthenticationUtils;
import org.springframework.security.core.Authentication;
import org.springframework.security.web.authentication.logout.LogoutSuccessHandler;
import org.springframework.security.web.authentication.logout.SimpleUrlLogoutSuccessHandler;
import org.thymeleaf.util.StringUtils;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;

/**
 * Handles the logout events of the Jack the Clipper application. A normal
 * {@link SimpleUrlLogoutSuccessHandler} does not suffice as we need to simulate a kind of
 * contextPath for organization specific URLs
 */
public class CustomLogoutHandler extends SimpleUrlLogoutSuccessHandler
        implements LogoutSuccessHandler {
    /**
     * Handles a successful logout event.
     * This one just redirects the user to the login page of his organization
     *
     * @param request
     * @param response
     * @param authentication
     * @throws IOException
     * @throws ServletException
     */
    @Override
    public void onLogoutSuccess(HttpServletRequest request, HttpServletResponse response,
                                Authentication authentication)
            throws IOException, ServletException {

        String organization = AuthenticationUtils.getOrganization(authentication);
        String hostPart = request.getRequestURL().toString().replace(request.getRequestURI(), "");
        String contextPath = StringUtils.isEmpty(request.getContextPath()) ? "" :
                request.getContextPath();
        String redirectUri = hostPart + contextPath + "/" + organization + "/login?logout";
        response.sendRedirect(redirectUri);
    }
}
