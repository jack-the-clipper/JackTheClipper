package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.enums.SuccessState;
import org.jacktheclipper.frontend.enums.UserRole;
import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.*;
import org.jacktheclipper.frontend.service.FeedService;
import org.jacktheclipper.frontend.service.OuService;
import org.jacktheclipper.frontend.service.SourceService;
import org.jacktheclipper.frontend.service.UserService;
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
import org.springframework.web.servlet.mvc.support.RedirectAttributes;

import java.util.Comparator;
import java.util.List;
import java.util.UUID;

import static org.jacktheclipper.frontend.utils.RedirectAttributesUtils.populateDefaultRedirectAttributes;

@PreAuthorize("hasRole('ROLE_STAFFCHIEF')")
@Controller
@RequestMapping("/{organization}")
public class StaffChiefController {

    private final static Logger log = LoggerFactory.getLogger(StaffChiefController.class);

    private final SourceService sourceService;

    private final OuService ouservice;

    private final FeedService feedservice;

    private final UserService userService;

    @Autowired
    public StaffChiefController(SourceService sourceService, OuService ouService,
                                UserService userService, FeedService feedService) {

        this.sourceService = sourceService;
        this.ouservice = ouService;
        this.feedservice = feedService;
        this.userService = userService;
    }

    /**
     * Shows a page where a StaffChief can edit his organizations
     *
     * @param model
     * @param auth   The StaffChief who wants to edit the organizations
     * @param unitId The id of the organization that should be edited
     * @return The page where a StaffChief can edit his organizations
     */
    @GetMapping("/editorganizations")
    public String editOrganisations(Model model, Authentication auth, @RequestParam(value =
            "unitId", required = false) UUID unitId) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        List<OrganizationalUnit> units = ouservice.getOrganizationalUnits(userId);
        OrganizationalUnit unit = null;
        if (unitId != null) {
            unit = getUnitById(unitId, units);
        } else {
            unit = units.get(0);
        }
        OrganizationalUnitSettings unitSettings = ouservice.getOuSettings(unit.getId(), userId);
        OrganizationalUnitSettings parentSettings = null;
        if (!unit.isPrincipalUnit()) {
            parentSettings = ouservice.getOuSettings(unit.getParentId(), userId);
        }
        model.addAttribute("unit", unit);
        model.addAttribute("units", units);
        model.addAttribute("unitId", unitId);
        model.addAttribute("unitSettings", unitSettings);
        model.addAttribute("parentSettings", parentSettings);
        return "editorganizationalunits";
    }

    /**
     * Adds a new organization
     *
     * @param auth               The StaffChief who wants to add the organization
     * @param name               The name of the new {@link OrganizationalUnit}
     * @param parentId           The id of the parent of the new {@link OrganizationalUnit}
     * @param organization       The name of the users organization
     * @param redirectAttributes
     * @return A redirect to the editorganizations page
     */
    @PostMapping("/addorganization")
    public String addOrganization(Authentication auth, @RequestParam("parentId") UUID parentId,
                                  @RequestParam("name") String name,
                                  @PathVariable("organization") String organization,
                                  final RedirectAttributes redirectAttributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        try {
            ouservice.addOrganizationalUnit(userId, parentId, name);
            populateDefaultRedirectAttributes(redirectAttributes, false, "Organisation " +
                    "erfolgreich hinzugefügt");
        } catch (BackendException ex) {
            log.info("Failed to add organization [{}] to parent [{}]", name, parentId);
            populateDefaultRedirectAttributes(redirectAttributes, true, "Organisation konnte " +
                    "nicht hinzugefügt werden");
        }
        return "redirect:/" + organization + "/editorganizations";
    }

    /**
     * Edits an existing organizaation
     *
     * @param auth         The StaffChief who wants to edit the organization
     * @param unitSettings The updated settings of the organizations
     * @param organization The name of the user's organization
     * @param unitId       The id of the organization that should be edited
     * @param attributes
     * @return A redirect to the editorganizations page
     */
    //@formatter:off
    @PostMapping("/editorganization")
    public String editOrganization(Authentication auth,
                                   @ModelAttribute("unitSettings") OrganizationalUnitSettings unitSettings,
                                   @PathVariable("organization") String organization,
                                   @RequestParam("unitId") UUID unitId,
                                   final RedirectAttributes attributes) {
    //@formatter:on
        UUID userId = AuthenticationUtils.getUserId(auth);
        List<Feed> feeds = ouservice.getOuSettings(unitId, userId).getFeeds();
        unitSettings.setFeeds(feeds);
        try {
            unitSettings.setAvailableSources(sourceService.recoverSources(unitSettings.getAvailableSources(), userId));
            ouservice.updateOrganizationalUnitSettings(userId, unitSettings);
            log.info("Successfully updated ouSettings to [{}]", unitSettings);
            populateDefaultRedirectAttributes(attributes, false, "Organisationseinstellungen " +
                    "erfolgreich aktualisiert");
        } catch (BackendException ex) {
            log.info("Failed to update ouSettings to [{}]", unitSettings);
            populateDefaultRedirectAttributes(attributes, true, "Organisationseinstellungen " +
                    "konnten nicht aktualisiert werden");
        }
        return "redirect:/" + organization + "/editorganizations?unitId=" + unitId.toString();
    }

    /**
     * Deletes an existing organization
     *
     * @param auth         The StaffChief who wants to delete the organization
     * @param unitId       The id of the organization that should be deleted
     * @param organization The name of the users organization
     * @param attributes
     * @return A redirect to the editorganizations page
     */
    @PostMapping("/deleteorganization")
    public String deleteOrganization(Authentication auth, @RequestParam("unitId") UUID unitId,
                                     @PathVariable("organization") String organization,
                                     final RedirectAttributes attributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        try {
            ouservice.deleteOrganizationalUnit(userId, unitId);
            populateDefaultRedirectAttributes(attributes, false, "Organisation erfolgreich " +
                    "gelöscht");
        } catch (BackendException ex) {
            populateDefaultRedirectAttributes(attributes, true, "Das Löschen der Organisation ist" +
                    " fehlgeschlagen");
        }
        return "redirect:/" + organization + "/editorganizations";
    }

    /**
     * Shows a page where the StaffChief can edit the default feeds of an organization
     *
     * @param model
     * @param auth   The StaffChief who wants to edit the default feeds
     * @param feedId The id of the feed that is shown
     * @param unitId The id of the unit the default feeds belong to
     * @return The page where a StaffChief can edit the default feeds
     */
    @GetMapping("/editdefaultfeeds")
    public String editDefaultFeeds(Model model, Authentication auth, @RequestParam(value =
            "feedId", required = false) UUID feedId, @RequestParam("unitId") UUID unitId) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        OrganizationalUnitSettings settings = ouservice.getOuSettings(unitId, userId);
        Feed feed;
        if (feedId != null) {
            feed = settings.getFeeds().stream().
                    filter(otherFeed -> feedId.equals(otherFeed.getId())).findAny().orElse(new Feed());
        } else {
            feed = CollectionUtils.isEmpty(settings.getFeeds()) ? new Feed() :
                    settings.getFeeds().get(0);
        }
        model.addAttribute("name", feed.getName());
        model.addAttribute("emptyFeed", new Feed());
        model.addAttribute("settingsId", settings.getId());
        model.addAttribute("unitId", unitId);
        model.addAttribute("feed", feed);
        model.addAttribute("feeds", settings.getFeeds());
        model.addAttribute("sources", settings.getAvailableSources());
        return "configuredefaultfeeds";
    }

    /**
     * Updates a default feed
     *
     * @param feed               The feed to be updated
     * @param auth               The StaffChief who wants to update the feed
     * @param settingsId         The id of the settings the feed belongs to
     * @param organization       The name of the users organization
     * @param unitId             The id of the organization the feed belongs to
     * @param redirectAttributes
     * @return A redirect to the editdefaultfeeds page
     */
    @PostMapping("/updatedefaultfeed")
    public String updateFeed(@ModelAttribute("feed") Feed feed, Authentication auth,
                             @RequestParam("settingsId") UUID settingsId, @PathVariable(
                                     "organization") String organization,
                             @RequestParam("unitId") UUID unitId,
                             final RedirectAttributes redirectAttributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);

        feed.setFeedSources(sourceService.recoverSources(feed.getFeedSources(), userId));

        String redirectQuery = "?feedId=" + feed.getId() + "&unitId=" + unitId;
        try {
            feedservice.updateFeed(feed, settingsId);
            populateDefaultRedirectAttributes(redirectAttributes, false, "Feed erfolgreich " +
                    "aktualisiert");
        } catch (BackendException badRequest) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Etwas ist falsch " +
                    "gelaufen");
        }
        return "redirect:/" + organization + "/editdefaultfeeds" + redirectQuery;
    }

    /**
     * Adds a default feed
     *
     * @param feed               The feed to be added
     * @param auth               The StaffChief who wants to add the feed
     * @param settingsId         The id of the settings the feed should be added to
     * @param organization       The name of the users organization
     * @param unitId             The id of the unit the feed should be added to
     * @param redirectAttributes
     * @return A redirect to the editdefaultfeeds page
     */
    @PostMapping("/adddefaultfeed")
    public String addFeed(@ModelAttribute("emptyFeed") Feed feed, Authentication auth,
                          @RequestParam("settingsId") UUID settingsId, @PathVariable(
                                  "organization") String organization,
                          @RequestParam("unitId") UUID unitId,
                          final RedirectAttributes redirectAttributes) {

        UUID userId = AuthenticationUtils.getUserId(auth);
        try {
            feed.setFeedSources(sourceService.recoverSources(feed.getFeedSources(), userId));
            feedservice.addFeed(settingsId, feed);
            populateDefaultRedirectAttributes(redirectAttributes, false, "Feed erfolgreich " +
                    "hinzugefügt");
        } catch (BackendException badRequest) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Das Hinzufügen des " +
                    "Feeds ist fehlgeschlagen");
        }
        return "redirect:/" + organization + "/editdefaultfeeds?unitId=" + unitId;
    }

    /**
     * Deletes a default feed from an organization
     *
     * @param feedId             The id of the default feed that should be deleted
     * @param redirectAttributes
     * @param organization       The name of the users organization
     * @param unitId             The id of the unit the feed should be deleted from
     * @return A redirect to the editdefaultfeeds page
     */
    @PostMapping("/removedefaultfeed")
    public String removeFeed(@RequestParam("feedId") UUID feedId,
                             final RedirectAttributes redirectAttributes, @PathVariable(
                                     "organization") String organization,
                             @RequestParam("unitId") UUID unitId) {

        try {
            feedservice.deleteFeed(feedId);
            populateDefaultRedirectAttributes(redirectAttributes, false, "Feed erfolgreich " +
                    "entfernt");
        } catch (BackendException badRequest) {
            populateDefaultRedirectAttributes(redirectAttributes, true, "Feed konnte nicht " +
                    "gelöscht werden");
        }
        return "redirect:/" + organization + "/editdefaultfeeds?unitId=" + unitId;
    }

    /**
     * Finds a specific organization in a list of organization by id
     *
     * @param unitId The id of the organization
     * @param units  The list of organizations
     * @return The organization that matches the given id or null of no organization matches it
     */
    private OrganizationalUnit getUnitById(UUID unitId, List<OrganizationalUnit> units) {

        for (OrganizationalUnit unit : units) {
            if (unit.getId().equals(unitId)) {
                return unit;
            } else {
                OrganizationalUnit result = getUnitById(unitId, unit.getChildren());
                if (result != null) {
                    return result;
                }
            }
        }
        return null;
    }

    /**
     * Shows a html page to a {@link UserRole#StaffChief} where he can manage his users
     *
     * @param model
     * @param auth
     * @param toEdit The id of the {@link User} the {@link UserRole#StaffChief} wants to edit.
     *               The parameter is optional. If {@code null} is passed it defaults to the id
     *               of the first found user
     * @return The page where a {@link UserRole#StaffChief} can manage his users
     */
    @GetMapping("/users")
    public String showManageableUsers(Model model, Authentication auth, @RequestParam(value =
            "userId", required = false) UUID toEdit) {

        UUID staffChiefId = AuthenticationUtils.getUserId(auth);
        List<MinimalUser> users = userService.getManageableUsers(staffChiefId);
        users.sort(Comparator.comparing(minimalUser -> minimalUser.getName().toLowerCase()));
        model.addAttribute("users", users);
        ExtendedUser userToEdit = null;

        if (toEdit == null) {
            if (users.size() > 0) {
                toEdit = users.get(0).getUserId();
            }
        }
        if (toEdit != null) {
            userToEdit = userService.getUserInformation(staffChiefId, toEdit);
        }
        model.addAttribute("ous", ouservice.getOrganizationalUnits(staffChiefId, true));
        model.addAttribute("currentUser", userToEdit);
        model.addAttribute("newUser", new User(null, UserRole.User, "", "", "", "", true, true,
                null, null));
        return "editUsers";
    }

    /**
     * Updates a user and redirects to {@code "/{organization}/users"}
     *
     * @param user               The updated version of the user. Not every field of the user is
     *                           populated
     *                           correctly though
     * @param redirectAttributes
     * @param auth
     * @param organization
     * @param promote            Whether the given user should be promoted to
     *                           {@link UserRole#StaffChief}. If null he is instead demoted to
     *                           {@link UserRole#User}. Due to the nature of Html-Checkboxes this
     *                           value can only be {@code null} or {@code True}.
     * @param units              A list of {@link UUID}s that the user should belong to.
     *                           Mirroring the {@link ExtendedUser#units} field but with less data.
     * @return The page where a {@link UserRole#StaffChief} can manage his users
     */
    @PostMapping("/users/update")
    public String updateUser(@ModelAttribute("currentUser") ExtendedUser user,
                             final RedirectAttributes redirectAttributes, Authentication auth,
                             @PathVariable("organization") String organization,
                             @RequestParam(value = "promote", required = false) Boolean promote,
                             @RequestParam("selectedUnits") List<UUID> units) {

        if (promote == null) {
            user.setUserRole(UserRole.User);
        } else {
            user.setUserRole(UserRole.StaffChief);
        }
        user.setUnlocked(true);
        try {
            MethodResult result = userService.modifyUser(AuthenticationUtils.getUserId(auth),
                    new UserUuidsTuple(user, units));
            if (!result.getState().equals(SuccessState.Successful)) {
                populateDefaultRedirectAttributes(redirectAttributes, true, result.getMessage());
                log.info("Failed update of user [{}]", user);
            } else {
                populateDefaultRedirectAttributes(redirectAttributes, false, "Benutzer " +
                        "erfolgreich aktualisiert");
                log.debug("update of user [{}] was successful", user);
            }

        } catch (BackendException bE) {
            log.info("Failed to modify user [{}]", user);
            populateDefaultRedirectAttributes(redirectAttributes, true,
                    "Benutzer konnte nicht aktualisiert werden");
        }
        return "redirect:/" + organization + "/users?userId=" + user.getUserId().toString();
    }

    @PostMapping("/users/delete")
    public String deleteUser(final RedirectAttributes redirectAttributes, Authentication auth,
                             @RequestParam("toDelete") UUID toDelete, @PathVariable("organization"
    ) String organization) {

        try {
            MethodResult result = userService.deleteUser(AuthenticationUtils.getUserId(auth),
                    toDelete);
            if (!result.getState().equals(SuccessState.Successful)) {
                populateDefaultRedirectAttributes(redirectAttributes, true, result.getMessage());
                log.info("Failed deletion of user [{}]", toDelete);
            } else {
                populateDefaultRedirectAttributes(redirectAttributes, false, "Benutzer " +
                        "erfolgreich gelöscht");
                log.debug("Deletion of [{}] was successful", toDelete);
            }
        } catch (BackendException ex) {
            populateDefaultRedirectAttributes(redirectAttributes, true,
                    "Benutzer konnte nicht gelöscht werden");
        }

        return "redirect:/" + organization + "/users";
    }

    @PostMapping("/users/add")
    public String addUser(@ModelAttribute("newUser") User user,
                          @RequestParam("units") List<UUID> units, Authentication auth,
                          @RequestParam(value = "newPromoted", required = false) Boolean promote,
                          final RedirectAttributes attributes,
                          @PathVariable("organization") String organization) {

        user.setPrincipalUnitId(ouservice.mapOuNameToOuUUID(organization));
        if (promote == null) {
            user.setUserRole(UserRole.User);
        } else {
            user.setUserRole(UserRole.StaffChief);
        }
        user.setUnlocked(true);
        user.setMustChangePassword(true);
        try {
            MethodResult result = userService.addUser(AuthenticationUtils.getUserId(auth),
                    new UserUuidsTuple(user, units));
            if (!result.getState().equals(SuccessState.Successful)) {
                populateDefaultRedirectAttributes(attributes, true, result.mapErrorCodeToMessage(
                        "Registrierung des Benutzers fehlgeschlagen. "));
                log.info("Could not add user [{}] with units [{}]", user, units);
            } else {
                populateDefaultRedirectAttributes(attributes, false, "Benutzer erfolgreich " +
                        "hinzugefügt");
                log.debug("Successfully added user [{}] with units [{}]", user, units);
            }
        } catch (BackendException ex) {
            populateDefaultRedirectAttributes(attributes, true, "Benutzer konnte nicht " +
                    "hinzugefügt werden");
            log.info("addition of user [{}] to ous [{}] failed", user, units);
        }
        return "redirect:/" + organization + "/users";
    }

    /**
     * Used to search for a user
     *
     * @param auth
     * @param toSearch   The name or mail of the user to search
     * @param attributes
     * @return The page where a {@link UserRole#StaffChief} can manage his users. If a user was
     * found he is the one selected to edit. Otherwise the default behaviour of
     * {@link #showManageableUsers(Model, Authentication, UUID)} applies
     */
    @PostMapping("/users/search")
    public String searchUser(Authentication auth, @RequestParam("name") String toSearch,
                             final RedirectAttributes attributes) {

        UUID staffChiefId = AuthenticationUtils.getUserId(auth);
        List<MinimalUser> users = userService.getManageableUsers(staffChiefId);
        UUID relevantOne =
                users.stream().filter(t -> t.getName().equalsIgnoreCase(toSearch)).findFirst().orElse(new MinimalUser(null, null, false)).getUserId();
        String query;
        if (relevantOne == null) {
            populateDefaultRedirectAttributes(attributes, true,
                    "Benutzer konnte nicht gefunden werden");
            query = "";
        } else {
            populateDefaultRedirectAttributes(attributes, false, "Benutzer gefunden");
            query = "?userId=" + relevantOne.toString();
        }
        return "redirect:/" + AuthenticationUtils.getOrganization(auth) + "/users" + query;
    }
}
