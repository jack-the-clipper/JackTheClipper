package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.enums.ContentType;
import org.jacktheclipper.frontend.model.Source;
import org.jacktheclipper.frontend.service.SourceService;
import org.jacktheclipper.frontend.utils.AuthenticationUtils;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.ModelAttribute;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestMapping;

import java.util.UUID;

/**
 * This class handles all pages that are exclusive to a user with the role SystemAdministrator.
 * This includes pages that allow to add sources to the Clipping Service
 */
@RequestMapping("/admin")
@Controller
public class SysAdminController {

    private final SourceService sourceService;

    @Autowired
    public SysAdminController(SourceService sourceService) {

        this.sourceService = sourceService;
    }

    /**
     * Adds the source to the Clipping Service
     *
     * @param source The source that should be added
     * @param auth   The user adding the source
     * @return The page showing an overview of all sources
     */
    @PostMapping("/addSource")
    @PreAuthorize("hasRole('ROLE_SYSADMIN')")
    public String addSource(@ModelAttribute("source") Source source, Authentication auth) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        sourceService.addSource(source, userId);
        return "redirect:/admin/sources";
    }

    /**
     * Shows all sources that are currently registered in the Clipping Service
     *
     * @param model
     * @param auth
     * @return The page showing an overview of all sources
     */
    @GetMapping("/sources")
    @PreAuthorize("hasRole('ROLE_SYSADMIN')")
    public String showSources(Model model, Authentication auth) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        model.addAttribute("sources", sourceService.getAvailableSources(userId));
        model.addAttribute("source", new Source());
        model.addAttribute("contentTypes", ContentType.values());
        return "sources";
    }
}
