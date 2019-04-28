package org.jacktheclipper.frontend.authentication;

import org.jacktheclipper.frontend.utils.Constants;
import org.springframework.security.core.AuthenticationException;
import org.springframework.security.web.authentication.AuthenticationFailureHandler;

import javax.servlet.ServletException;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;

public class CustomAuthenticationFailureHandler implements AuthenticationFailureHandler {

    @Override
    public void onAuthenticationFailure(HttpServletRequest request, HttpServletResponse response,
                                        AuthenticationException e)
            throws IOException, ServletException {
        
        String organization =
                (String) request.getSession().getAttribute(Constants.SAVED_ORGANIZATION_KEY);

        response.sendRedirect("/" + organization + "/login?error");
    }
}
