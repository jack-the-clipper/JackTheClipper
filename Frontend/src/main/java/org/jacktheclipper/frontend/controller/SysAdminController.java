package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.enums.ContentType;
import org.jacktheclipper.frontend.model.Source;
import org.jacktheclipper.frontend.service.SourceService;
import org.jacktheclipper.frontend.utils.AuthenticationUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.util.CollectionUtils;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.UUID;

/**
 * This class handles all pages that are exclusive to a user with the role SystemAdministrator.
 * This includes pages that allow to add sources to the Clipping Service
 */
@RequestMapping("/admin")
@Controller
public class SysAdminController {
    private static final Logger log = LoggerFactory.getLogger(SysAdminController.class);

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
    public String showSources(Model model, Authentication auth, @RequestParam(value = "id",
            required = false) UUID sourceId) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        List<Source> sourceList = sourceService.getAvailableSources(userId);
        model.addAttribute("sources", sourceService.getAvailableSources(userId));
        Source focusedSource;
        if (sourceId != null) {
            focusedSource =
                    sourceList.stream().filter(s -> s.getId().equals(sourceId)).findFirst().orElse(new Source());
        } else {
            focusedSource = CollectionUtils.isEmpty(sourceList) ? new Source() : sourceList.get(0);
        }
        model.addAttribute("focusedSource", focusedSource);
        model.addAttribute("source", new Source());
        model.addAttribute("contentTypes", ContentType.values());
        return "sources";
    }

    /**
     * Deletes the source identified by the supplied UUID
     *
     * @param sourceId The id of the source that should be deleted
     * @param auth     The user deleting the source
     * @return The page showing an overview of all sources
     */
    @PostMapping("/sources/delete")
    public String deleteSource(@RequestParam("sourceId") UUID sourceId, Authentication auth) {

        sourceService.deleteSource(AuthenticationUtils.getUserId(auth), sourceId);
        return "redirect:/admin/sources";
    }

    /**
     * Persists the changes to the supplied source with the backend
     *
     * @param source The updated version of the source
     * @param auth   The user updating the source
     * @return The page showing an overview of all sources
     */
    @PostMapping("/sources/update")
    public String updateSource(@ModelAttribute("focusedSource") Source source,
                               Authentication auth) {

        sourceService.updateSource(AuthenticationUtils.getUserId(auth), source);
        return "redirect:/admin/sources";
    }
}
