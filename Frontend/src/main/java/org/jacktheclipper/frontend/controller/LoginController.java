package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.utils.Constants;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.HttpStatus;
import org.springframework.security.web.savedrequest.DefaultSavedRequest;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.server.ResponseStatusException;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpSession;

/**
 * Serves a login page for every organization. Could be used to customize the login page on a per
 * organization basis, e. g. to point to a different logo for each organization.
 */
@Controller
public class LoginController {
    private static final Logger log = LoggerFactory.getLogger(LoginController.class);


    /**
     * Renders the login page for every organization.
     * The method does not currently check, whether the organization actually is allowed to use
     * the application
     *
     * @param model
     * @param organization The organization that the user attempting the login presumably belongs to
     * @return The login page
     */
    @GetMapping(value = "/{organization}/login")
    public String loginPage(Model model, @PathVariable String organization, HttpServletRequest request) {

        HttpSession session = request.getSession();
        if (session.getAttribute(Constants.SAVED_ORGANIZATION_KEY) == null) {
            session.setAttribute(Constants.SAVED_ORGANIZATION_KEY, organization);
        }
        model.addAttribute("org", organization);
        return "login";
    }

    /**
     * Redirects a user to the login page of his organization.
     * The organization is determined by which part of the application the user tried to access
     * If a user tries to access this part of the application directly, this method will throw a
     * HttpStatus 404
     *
     * @param session The HttpSession of the (possibly anonymous) user
     * @return A redirect to the login page of the organization the user presumably belongs to.
     *
     * @throws ResponseStatusException Throws a http 404 status if it cannot determine a user's
     *                                 organization
     */
    @GetMapping(value = "/login")
    public String redirectToLoginPage(HttpSession session) {

        DefaultSavedRequest savedRequest =
                (DefaultSavedRequest) session.getAttribute(Constants.SAVED_REQUEST_KEY);
        String savedOrganization = (String) session.getAttribute(Constants.SAVED_ORGANIZATION_KEY);
        if (savedRequest != null) {
            /* ServletPath will have a format in the form of /{organization}/more/possible/paths/
            ... Thus taking index 1 of the split array will result in the organization the user
            tried to access and we can redirect him to the login page
            Index 0 is filled with the empty String
            */
            String[] pathParts = savedRequest.getServletPath().split("/");

            if (pathParts.length < 2) {
                throw new ResponseStatusException(HttpStatus.NOT_FOUND);
            }
            String organization = pathParts[1];
            session.setAttribute(Constants.SAVED_ORGANIZATION_KEY, organization);
            log.info("User seems to belong to organization [{}]", organization);
            return "redirect:/" + organization + "/login";

        } else if (savedOrganization != null) {

            return "redirect:/" + savedOrganization + "/login";
        } else {
            log.info("Could not figure out to which organization the user belongs!");
        }
        throw new ResponseStatusException(HttpStatus.NOT_FOUND);
    }
}
