package org.jacktheclipper.frontend.controller;


import org.jacktheclipper.frontend.enums.UserRole;
import org.jacktheclipper.frontend.model.MethodResult;
import org.jacktheclipper.frontend.model.User;
import org.jacktheclipper.frontend.service.OuService;
import org.jacktheclipper.frontend.service.UserService;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.security.authentication.BadCredentialsException;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.server.ResponseStatusException;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;

import java.util.UUID;

import static org.jacktheclipper.frontend.utils.RedirectAttributesUtils.populateDefaultRedirectAttributes;

/**
 * Handles the registration process with the backend
 *
 */
@Controller
public class RegistrationController {
    private static final Logger log = LoggerFactory.getLogger(RegistrationController.class);
    private final OuService ouService;
    private final UserService userService;

    @Autowired
    public RegistrationController(OuService ouService, UserService userService) {

        this.userService = userService;
        this.ouService = ouService;
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

        if (organization.equals("SYSTEM")) {
            throw new ResponseStatusException(HttpStatus.NOT_FOUND);
        }
        model.addAttribute("OUs", ouService.getUnitChildren(organization));
        model.addAttribute("org", organization);
        if (!model.containsAttribute("user")) {
            model.addAttribute("user", new User(null, UserRole.User, "", "", "", organization,
                    false, false, null, null));
        }
        return "register";
    }

    /**
     * Processes the registration of the user
     *
     * @param redirectAttributes
     * @param user               the userobject whose fields are filled by the form
     * @param ouId               the organization to which the user belongs
     * @param organization       The organization the user wants to register to
     * @param inputPassword      The password from the second field. Used to determine if the user
     *                           entered two identical passwords
     * @return Sends the user to his feedconfiguration page if the registration was successful
     * otherwise he is redirected to the registration page with all his previous data (besides
     * the passwords) being displayed
     */
    @PostMapping(value = "/{organization}/register")
    public String processRegistration(@ModelAttribute("user") User user, @RequestParam(value =
            "ouId") UUID ouId, @RequestParam("inputPassword") String inputPassword,
                                      @PathVariable("organization") String organization,
                                      final RedirectAttributes redirectAttributes) {

        if (organization.equals("SYSTEM")) {
            throw new ResponseStatusException(HttpStatus.NOT_FOUND);
        }
        try {
            if (!user.getPassword().equals(inputPassword)) {
                throw new BadCredentialsException("Passwörter stimmen nicht überein.");
            }
            user.setPrincipalUnitId(ouService.mapOuNameToOuUUID(organization));
            MethodResult result = userService.registerUser(user, ouId);
            switch (result.getState()) {

                case Successful:
                    populateDefaultRedirectAttributes(redirectAttributes, false, "Registrierung " +
                            "erfolgreich abgeschlossen. Sie können die Applikation nutzen, " +
                            "sobald Ihr Personalbeauftragter Sie freigeschaltet hat.");
                    break;
                case UnknownError:
                    log.warn("Registration of [{}] failed due to [{}]", user, result.getMessage());

                    populateDefaultRedirectAttributes(redirectAttributes, true,
                            result.mapErrorCodeToMessage("Registrierung fehlgeschlagen. "));
                    break;
                case Timeout:
                    log.warn("Registration of [{}] timed out", user);
                    populateDefaultRedirectAttributes(redirectAttributes, true, "Die " +
                            "Registrierung konnte nicht durchgeführt werden. Bitte versuchen" +
                            " Sie es zu einem späteren Zeitpunkt erneut. Sollte das Problem " +
                            "trotzdem noch bestehen, kontaktieren Sie bitte Ihren " +
                            "Personalbeauftragten.");
                    break;
            }
        } catch (Exception ex) {
            log.info("Got [{}], reason: [{}]", ex.getClass().getName(), ex.getMessage());
            populateDefaultRedirectAttributes(redirectAttributes, true, ex.getMessage());
            redirectAttributes.addFlashAttribute(user);
            return "redirect:/" + organization + "/register";
        }
        return "redirect:/" + organization + "/register";
    }


}
