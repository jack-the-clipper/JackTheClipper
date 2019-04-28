package org.jacktheclipper.frontend.authentication;

import org.jacktheclipper.frontend.utils.AuthenticationUtils;
import org.jacktheclipper.frontend.utils.Constants;
import org.springframework.security.core.Authentication;
import org.springframework.security.web.authentication.SavedRequestAwareAuthenticationSuccessHandler;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;

public class CustomAuthenticationSuccessHandler
        extends SavedRequestAwareAuthenticationSuccessHandler {
    /**
     * Handles a successful authentication event.
     * If a user attempted to access a protected page before, he will be redirected to that page.
     * Otherwise he will be redirected to the landing page of his organization.
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

        if (request.getSession(false).getAttribute(Constants.SAVED_REQUEST_KEY) != null) {
            super.onAuthenticationSuccess(request, response, authentication);
        } else {
            String organization = AuthenticationUtils.getOrganization(authentication);
            response.sendRedirect("/" + organization + "/");
        }
    }
}
