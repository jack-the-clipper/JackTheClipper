package org.jacktheclipper.frontend.authentication;

import org.jacktheclipper.frontend.enums.UserRole;
import org.jacktheclipper.frontend.model.User;
import org.jacktheclipper.frontend.utils.AuthenticationUtils;
import org.jacktheclipper.frontend.utils.Constants;
import org.springframework.security.core.Authentication;
import org.springframework.security.web.authentication.AuthenticationSuccessHandler;
import org.springframework.security.web.authentication.SavedRequestAwareAuthenticationSuccessHandler;
import org.thymeleaf.util.StringUtils;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;


/**
 * Handles successful authentication as the name implies. This one redirects user's to their
 * initial requested page (that's why {@link SavedRequestAwareAuthenticationSuccessHandler} is
 * extended. If there is no saved request saved in the {@link javax.servlet.http.HttpSession}
 * under the {@link Constants#SAVED_REQUEST_KEY} it redirects to /{organization}/, where {
 * organization} is the organization the user belongs to.
 */
public class CustomAuthenticationSuccessHandler
        extends SavedRequestAwareAuthenticationSuccessHandler
        implements AuthenticationSuccessHandler {
    /**
     * Handles a successful authentication event.
     * If an {@link UserRole#SystemAdministrator} is authenticated he will always be redirected
     * to the page where he can manage the {@link org.jacktheclipper.frontend.model.Source}s of
     * the application.
     * If a user needs to change his password he will be redirected to the page where he can do
     * exactly that. If that is not the case and the user attempted to access a protected page
     * before, he will be redirected to that page.
     * If none of the above two conditions apply the user is redirected to the feedoverview.
     *
     * @param request
     * @param response
     * @param authentication
     * @throws IOException
     * @throws ServletException
     */
    @Override
    public void onAuthenticationSuccess(HttpServletRequest request, HttpServletResponse response,
                                        Authentication authentication)
            throws IOException, ServletException {

        String organization = AuthenticationUtils.getOrganization(authentication);
        String hostPart = request.getRequestURL().toString().replace(request.getRequestURI(), "");
        String contextPath = StringUtils.isEmpty(request.getContextPath()) ? "" :
                request.getContextPath();
        //systemadministrators are redirected to the edit source page
        if (((User) authentication.getPrincipal()).getUserRole().equals(UserRole.SystemAdministrator)) {
            String redirectUri = hostPart + contextPath + "/admin/sources";
            response.sendRedirect(redirectUri);
            return;
        }
        //user needs to change his password so redirect him to profile page
        if (AuthenticationUtils.isMustChangePassword(authentication)) {
            String redirectUri = hostPart + contextPath + "/" + organization + "/feed/profile";
            response.sendRedirect(redirectUri);

            //user wanted to access a specific page so redirect him by calling supermethod
        } else if (request.getSession(false).getAttribute(Constants.SAVED_REQUEST_KEY) != null) {
            super.onAuthenticationSuccess(request, response, authentication);
            //user logged in directly on the login page so show him his feedoverview
        } else {
            String redirectUri = hostPart + contextPath + "/" + organization + "/feed/";
            response.sendRedirect(redirectUri);
        }
    }
}
