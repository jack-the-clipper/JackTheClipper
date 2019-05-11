package org.jacktheclipper.frontend.controller;


import org.jacktheclipper.frontend.authentication.CustomAuthenticationToken;
import org.jacktheclipper.frontend.authentication.User;
import org.jacktheclipper.frontend.enums.UserRole;
import org.jacktheclipper.frontend.service.OuService;
import org.jacktheclipper.frontend.service.UserService;
import org.jacktheclipper.frontend.utils.RedirectAttributesUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.BadCredentialsException;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContext;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;

import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpSession;
import java.util.UUID;

import static org.springframework.security.web.context.HttpSessionSecurityContextRepository.SPRING_SECURITY_CONTEXT_KEY;

/**
 * Handles the registration process with the backend
 *
 * @author SBG
 */
@Controller
public class RegistrationController {
    private static final Logger log = LoggerFactory.getLogger(RegistrationController.class);
    private final OuService ouService;
    private final UserService userService;
    private final AuthenticationManager authManager;


    @Autowired
    public RegistrationController(OuService ouService, AuthenticationManager authManager,
                                  UserService userService) {

        this.userService = userService;
        this.ouService = ouService;
        this.authManager = authManager;
    }

    /**
     * Prepares the register page for the user
     *
     * @param model
     * @param organization The organization the user wants to register to
     * @return The page where users can register
     */
    @GetMapping(value = "/{organization}/register")
    public String registerPage(Model model, @PathVariable("organization") String organization) {
        //TODO static Frontend User
        model.addAttribute("OUs", ouService.getOrganizationalUnits(null));
        model.addAttribute("org", organization);
        if (!model.containsAttribute("user")) {
            model.addAttribute("user", new User(null, UserRole.User, "", "", "", organization,
                    false));
        }
        return "register";
    }

    /**
     * Processes the registration of the user
     *
     * @param redirectAttributes
     * @param user               the userobject whose fields are filled by the form
     * @param ouId               the organization to which the user belongs
     * @param request            holds the session so the user can be logged in immediately after
     *                           registration
     * @param organization       The organization the user wants to register to
     * @param inputPassword      The password from the second field. Used to determine if the user
     *                           entered two identical passwords
     * @return Sends the user to his feedconfiguration page if the registration was successful
     * otherwise he is redirected to the registration page with all his previous data (besides
     * the passwords) being displayed
     */
    @PostMapping(value = "/{organization}/register")
    public String processRegistration(@ModelAttribute("user") User user, @RequestParam(value =
            "ouId") UUID ouId, HttpServletRequest request,
                                      @RequestParam("inputPassword") String inputPassword,
                                      @PathVariable("organization") String organization,
                                      final RedirectAttributes redirectAttributes) {


        try {
            String password = user.getPassword();
            if (!user.getPassword().equals(inputPassword)) {
                throw new BadCredentialsException("Passwörter stimmen nicht überein");
            }
            user = userService.registerUser(user, ouId);
            //necessary to automatically log the user in since backend does not provide password
            user.setPassword(password);
            login(request, user);
        } catch (Exception ex) {
            log.info("Got [{}], reason: [{}]", ex.getClass().getName(), ex.getMessage());
            RedirectAttributesUtils.populateDefaultRedirectAttributes(redirectAttributes, true,
                    ex.getMessage());
            redirectAttributes.addFlashAttribute(user);
            return "redirect:/" + organization + "/register";
        }

        return "redirect:/" + organization + "/feed/edit";
    }

    /**
     * Logs the user into the session
     *
     * @param request the http-request holding the http session
     * @param user    the user to authenticate
     */
    private void login(HttpServletRequest request, User user) {

        CustomAuthenticationToken authReq = new CustomAuthenticationToken(user,
                user.getPassword(), user.getOrganization());
        Authentication auth = authManager.authenticate(authReq);

        SecurityContext sc = SecurityContextHolder.getContext();
        sc.setAuthentication(auth);
        HttpSession session = request.getSession(true);
        session.setAttribute(SPRING_SECURITY_CONTEXT_KEY, sc);
    }
}
