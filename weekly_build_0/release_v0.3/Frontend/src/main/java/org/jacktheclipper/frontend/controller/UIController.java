package org.jacktheclipper.frontend.controller;

import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;


/**
 * This just renders a landing page serving no actual functionality. Said landing page is the
 * default page running at the root of the application and SHOULD be replaced by a more useful one.
 */
@Deprecated
@Controller
public class UIController {

    /**
     * Renders the starting page of the application.
     * The starting page currently does not serve any greater purpose. Thus this class is
     * deprecated. It is not removed yet since leaving the application root unmapped seems like a
     * bad idea
     *
     * @param model
     * @return
     */
    @GetMapping("/*/")
    public String home(Model model) {


        return "index";
    }
}


