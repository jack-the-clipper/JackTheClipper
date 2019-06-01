package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.enums.ContentType;
import org.jacktheclipper.frontend.enums.UserRole;
import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.OrganizationalUnit;
import org.jacktheclipper.frontend.model.OrganizationalUnitSettings;
import org.jacktheclipper.frontend.model.Source;
import org.jacktheclipper.frontend.service.OuService;
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
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.UUID;

import static org.jacktheclipper.frontend.utils.RedirectAttributesUtils.populateDefaultRedirectAttributes;

/**
 * This class handles all pages that are exclusive to a user with the role SystemAdministrator.
 * This includes pages that allow to add sources to the Clipping Service. Methods in this class
 * are only accessible by a user with
 * role {@link org.jacktheclipper.frontend.enums.UserRole#SystemAdministrator} as defined in
 * {@link UserRole#resolveAuthorities()}
 */
@PreAuthorize("hasRole('ROLE_SYSADMIN')")
@RequestMapping("/admin")
@Controller
public class SysAdminController {
    private static final Logger log = LoggerFactory.getLogger(SysAdminController.class);

    private final SourceService sourceService;
    private final OuService ouservice;

    @Autowired
    public SysAdminController(SourceService sourceService, OuService ouservice) {

        this.sourceService = sourceService;
        this.ouservice = ouservice;
    }

    /**
     * Adds the source to the Clipping Service
     *
     * @param source             The source that should be added
     * @param auth               The user adding the source
     * @param redirectAttributes
     * @return The page showing an overview of all sources
     */
    @PostMapping("/addSource")
    public String addSource(@ModelAttribute("source") Source source, Authentication auth,
                            final RedirectAttributes redirectAttributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        try {
            sourceService.addSource(source, userId);
            populateDefaultRedirectAttributes(redirectAttributes, false, "Quelle erfolgreich " +
                    "hinzugefügt");
        } catch (BackendException exception) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Quelle konnte nicht " +
                    "hinzugefügt werden");
        }
        return "redirect:/admin/sources";
    }

    /**
     * Shows all sources that are currently registered in the Clipping Service
     *
     * @param model
     * @param auth
     * @param sourceId The id the user wants to show. Might be null
     * @return The page showing an overview of all sources
     */
    @GetMapping("/sources")
    public String showSources(Model model, Authentication auth, @RequestParam(value = "sourceId",
            required = false) UUID sourceId) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        List<Source> sourceList = sourceService.getAvailableSources(userId);
        Source focusedSource;
        if (sourceId != null) {
            focusedSource = sourceList.stream().filter(s -> s.getId().equals(sourceId)).findFirst().
                    orElse(null);
        } else {
            focusedSource = CollectionUtils.isEmpty(sourceList) ? null : sourceList.get(0);
        }
        model.addAttribute("sources", sourceService.getAvailableSources(userId));
        model.addAttribute("focusedSource", focusedSource);
        model.addAttribute("source", new Source());
        model.addAttribute("contentTypes", ContentType.values());
        return "sources";
    }

    /**
     * Deletes the source identified by the supplied UUID
     *
     * @param sourceId           The id of the source that should be deleted
     * @param auth               The user deleting the source
     * @param redirectAttributes
     * @return The page showing an overview of all sources
     */
    @PostMapping("/sources/delete")
    public String deleteSource(@RequestParam("sourceId") UUID sourceId, Authentication auth,
                               final RedirectAttributes redirectAttributes) {

        try {
            sourceService.deleteSource(AuthenticationUtils.getUserId(auth), sourceId);
            populateDefaultRedirectAttributes(redirectAttributes, false, "Quelle erfolgreich " +
                    "gelöscht");
        } catch (BackendException backend) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Quelle konnte nicht " +
                    "gelöscht werden");
        } catch (HttpClientErrorException.BadRequest badRequest) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Quelle konnte nicht " +
                    "gelöscht werden");
        }
        return "redirect:/admin/sources";
    }

    /**
     * Persists the changes to the supplied source with the backend
     *
     * @param source             The updated version of the source
     * @param auth               The user updating the source
     * @param redirectAttributes
     * @return The page showing an overview of all sources
     */
    @PostMapping("/sources/update")
    public String updateSource(@ModelAttribute("focusedSource") Source source,
                               Authentication auth, final RedirectAttributes redirectAttributes) {

        try {
            sourceService.updateSource(AuthenticationUtils.getUserId(auth), source);
            populateDefaultRedirectAttributes(redirectAttributes, false, "Quelle erfolgreich " +
                    "aktualisiert");
        } catch (BackendException backend) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Quelle konnte nicht " +
                    "aktualisiert werden");
        } catch (HttpClientErrorException.BadRequest badRequest) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Quelle konnte nicht " +
                    "aktualisiert werden");
        }
        String redirectQuery = source.getId() != null ? "?sourceId=" + source.getId().toString()
                : "";
        return "redirect:/admin/sources" + redirectQuery;
    }

    /**
     * Shows a page where a {@link UserRole#SystemAdministrator} can edit all principal units.
     *
     * @param model
     * @param auth
     * @param clientId The id of the {@link OrganizationalUnit} that the user wants to edit. If
     *                 the id is {@code null} this defaults to the first client available
     * @return The edit page for principal units
     */
    @GetMapping("/editclients")
    public String editClients(Model model, Authentication auth, @RequestParam(value = "clientId",
            required = false) UUID clientId) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        List<OrganizationalUnit> clients = ouservice.getPrincipalUnits(userId);
        OrganizationalUnit system = null;
        OrganizationalUnit currentClient = null;
        for (OrganizationalUnit client : clients) {
            if (client.getName().equals("SYSTEM")) {
                system = client;
            } else if (client.getId().equals(clientId)) {
                currentClient = client;
            }
        }
        clients.remove(system);
        if (clientId == null && clients.size() > 0) {
            currentClient = clients.get(0);
        }
        if (currentClient != null) {
            OrganizationalUnitSettings settings = ouservice.getOuSettings(currentClient.getId(),
                    userId);
            model.addAttribute("settings", settings);
        }
        model.addAttribute("sources", sourceService.getAvailableSources(userId));
        model.addAttribute("clients", clients);
        model.addAttribute("currentClient", currentClient);
        return "editclients";
    }

    /**
     * Adds a new principal unit to the Jack the Clipper application
     *
     * @param auth
     * @param name               The name of the new {@link OrganizationalUnit}. Since it is a
     *                           principal unit
     *                           {@link OrganizationalUnit#parentId} defaults to the one of
     *                           {@code "SYSTEM"}.
     * @param mail               The mail of the automatically generated
     *                           {@link UserRole#StaffChief} for the
     *                           new principal unit
     * @param redirectAttributes
     * @return The page where all principal units can be edited
     */
    @PostMapping("/addclient")
    public String addClient(Authentication auth, @RequestParam("name") String name,
                            @RequestParam("mail") String mail,
                            final RedirectAttributes redirectAttributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        try {

            ouservice.addPrincipalUnit(userId, name, mail);
            populateDefaultRedirectAttributes(redirectAttributes, false,
                    "Mandant erfolgreich hinzugefügt");
        } catch (BackendException ex) {
            //@formatter:off
            log.info("Something went wrong while attempting to add a principal unit with mail "
                    + "[{}] and name [{}]", mail, name);
            populateDefaultRedirectAttributes(redirectAttributes, true,
                    "Mandant konnte nicht hinzugefügt werden");
            //@formatter:on
        }
        return "redirect:/admin/editclients";
    }

    /**
     * Updates an already existing {@link OrganizationalUnit} by manipulating the object itself
     * or its corresponding {@link OrganizationalUnitSettings}.
     *
     * @param auth
     * @param unit               The updated version of the {@link OrganizationalUnit}
     * @param blacklist          A list of words this {@link OrganizationalUnit} that determines
     *                           whether an {@link org.jacktheclipper.frontend.model.Article} should
     *                           be shown or not for this principal unit
     * @param sourcesAsString    A a list of {@link Source}'S {@link Source#name}. So that they can
     *                           be reconstructed to the actual objects in the database.
     * @param redirectAttributes
     * @return The page where all principal units can be edited
     */
    @PostMapping("/editclient")
    public String editClient(Authentication auth,
                             @ModelAttribute("currentClient") OrganizationalUnit unit,
                             @RequestParam("blacklist") List<String> blacklist, @RequestParam(
                                     "sources") String sourcesAsString,
                             final RedirectAttributes redirectAttributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        unit.setPrincipalUnit(true);
        OrganizationalUnitSettings settings = ouservice.getOuSettings(unit.getId(), userId);
        List<Source> availableSources = sourceService.getAvailableSources(userId);
        List<String> sources = Arrays.asList(sourcesAsString.split(","));
        List<Source> actualSources = new ArrayList<>();
        for (Source source : availableSources) {
            if (sources.contains(source.getId().toString())) {
                actualSources.add(source);
            }
        }
        settings.setAvailableSources(actualSources);
        settings.setBlackList(blacklist);
        try {
            ouservice.updateOrganizationalUnit(userId, unit);
            ouservice.updateOrganizationalUnitSettings(userId, settings);
            populateDefaultRedirectAttributes(redirectAttributes, false, "Mandant erfogreich " +
                    "aktualisiert");
        } catch (BackendException ex) {
            log.info("Failed to update principal [{}]", unit.getId());
            populateDefaultRedirectAttributes(redirectAttributes, true,
                    "Mandant konnte nicht aktualisiert werden");
        }
        return "redirect:/admin/editclients?clientId=" + unit.getId();
    }

    /**
     * Removes the specified principal unit from the Jack the Clipper application
     *
     * @param auth
     * @param clientId           The id  of the {@link OrganizationalUnit} that should be deleted
     * @param redirectAttributes
     * @return The page where all principal units can be edited
     */
    @PostMapping("/deleteclient")
    public String deleteClient(Authentication auth, @RequestParam("clientId") UUID clientId,
                               final RedirectAttributes redirectAttributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        try {
            ouservice.deleteOrganizationalUnit(userId, clientId);
            populateDefaultRedirectAttributes(redirectAttributes, false,
                    "Mandant erfolgreich entfernt");
        } catch (BackendException ex) {
            log.info("Something went wrong when trying to delete principal unit [{}]", clientId);
            populateDefaultRedirectAttributes(redirectAttributes, true,
                    "Mandant konnte nicht entfernt werden");
        }
        return "redirect:/admin/editclients";
    }
}
