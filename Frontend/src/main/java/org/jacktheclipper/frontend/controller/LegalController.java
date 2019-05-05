package org.jacktheclipper.frontend.controller;

import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;

/**
 * Responsible for the impressum and privacypolicy. It required logic to keep the organization
 * specific links in the application working, this could not be registered in the
 * WebMvcConfiguration anymore
 */
@Controller
@RequestMapping("/{organization}")
public class LegalController {

    /**
     * Shows the impressum to the user
     * @param model
     * @param organization The name of the organization the user presumably belongs to
     * @return The page containing the impressum
     */
    @GetMapping("/impressum")
    public String impressum(Model model, @PathVariable("organization") String organization) {

        model.addAttribute("org", organization);
        return "impressum";
    }

    /**
     * Shows the privacy policy of Jack The Clipper to the user
     * @param model
     * @param organization The name of the organization the user presumably belongs to
     * @return The page containing the privacy policy
     */
    @GetMapping("/privacypolicy")
    public String privacyPolicy(Model model, @PathVariable("organization") String organization) {

        model.addAttribute("org", organization);
        return "privacypolicy";
    }
}
