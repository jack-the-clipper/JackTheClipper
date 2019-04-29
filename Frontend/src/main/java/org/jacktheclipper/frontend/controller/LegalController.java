package org.jacktheclipper.frontend.controller;

import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;

@Controller
@RequestMapping("/{organization}")
public class LegalController {

    @GetMapping("/impressum")
    public String impressum(Model model, @PathVariable("organization") String organization) {

        model.addAttribute("org", organization);
        return "impressum";
    }
    @GetMapping("/privacypolicy")
    public String privacyPolicy(Model model, @PathVariable("organization") String organization) {

        model.addAttribute("org", organization);
        return "privacypolicy";
    }
}
