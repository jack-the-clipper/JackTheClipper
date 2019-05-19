package org.jacktheclipper.frontend.service;

import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.*;
import org.jacktheclipper.frontend.utils.ResponseEntityUtils;
import org.jacktheclipper.frontend.utils.RestTemplateUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.web.client.RestTemplateBuilder;
import org.springframework.http.HttpMethod;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;
import org.springframework.web.client.HttpClientErrorException;
import org.springframework.web.client.RestTemplate;

import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.UUID;

/**
 * Class for handling all request to the backend concerning an user.
 * This includes registering, resetting passwords , etc.
 */
@Service
public class UserService {
    private String backendUrl;

    private static final Logger log = LoggerFactory.getLogger(UserService.class);

    private final OuService ouService;

    private final RestTemplate template;

    @Autowired
    public UserService(OuService ouService, RestTemplateBuilder builder,
                       @Value("${backend.url}") String backendUrl) {

        this.ouService = ouService;
        template = builder.build();
        this.backendUrl = backendUrl;
    }

    /**
     * Registers the given user with Jack the Clipper.
     *
     * @param user           The user to be registered with all his details
     * @param organizationId The id of the organization the user should belong to
     * @return The registered user holding only the necessary information for the application. E
     * .g. passwords are not transmitted by the backend
     *
     * @throws BackendException If the registration of the user failed. Currenlty there is no way
     *                          to determine why it failed though. There is no distinction
     *                          between an already taken
     *                          username or email
     */
    public MethodResult registerUser(User user, UUID organizationId)
            throws BackendException {

        log.debug("Attempting registration of User [{}]", user.getName());
        String url = backendUrl + "/register?password={password}" + "&selectedUnit={unitId}";
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(user), MethodResult.class,
                    user.getPassword(), organizationId.toString());
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully sent request to try to register user [{}]", user);
                return response.getBody();
            }
        } catch (Exception ex) {
            log.info("Failed registration due to [{}] with reason [{}]", ex.getClass().getName(),
                    ex.getMessage());
        }
        throw new BackendException("Failed registration request");
    }


    /**
     * Deletes the account of the specified user. Thus the user will not be able to use Jack the
     * Clipper anymore
     *
     * @param staffChiefId The staffChief removing the user from his organizationalUnit
     * @param userId       The id of the user that should be removed
     * @return A {@link MethodResult} indicating the success or failure of the call. Callers need
     * to check {@link MethodResult#state} to see whether the request truly was successful.
     *
     * @throws BackendException If the user could not be registered
     */
    public MethodResult deleteUser(UUID staffChiefId, UUID userId)
            throws BackendException {

        String url = backendUrl + "/deleteuser?staffChiefId=" + staffChiefId.toString() +
                "&userId=" + userId.toString();
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.DELETE,
                    RestTemplateUtils.prepareBasicHttpEntity(""), MethodResult.class);
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully requested deletion of user [{}] as user [{}]", userId,
                        staffChiefId);
                return response.getBody();
            }
        } catch (HttpClientErrorException.BadRequest badRequest) {
            log.warn("Could not execute delete [{}] due to http 400", url);
            throw new BackendException("Failed delet due to http 400, requesturl: " + url);
        }
        log.warn("Failed deletion request [{}]. Response empty or not 2xx", url);
        throw new BackendException("Could not delete user since response body was null or " +
                "statuscode not 2xx");
    }

    /**
     * Triggers the reset of a user's password.
     * This is invoked when a user has forgotten his password and thus cannot authenticate
     * himself to Jack the Clipper. If a user wants to set a new password himself
     * {@link #updatePassword(UUID, String)} should be used.
     *
     * @param eMail The mail of the user who forgot his password
     * @throws BackendException If the password for the specified mail could not be reset
     */
    public void resetPassword(String eMail)
            throws BackendException {

        String uri = backendUrl + "/reset?userMail={mail}";

        try {
            ResponseEntity<MethodResult> response = template.exchange(uri, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(eMail), MethodResult.class, eMail);
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully reset password of user [{}]", eMail);
            }
        } catch (Exception ex) {
            log.warn("Could not reset password of user with mail [{}]", eMail);
            throw new BackendException("Could not reset user's [" + eMail + "] password");
        }
    }

    /**
     * Sets the password of a user.
     * This implies that the password is intentionally being set by the user. If a user forgets
     * his password, {@link #resetPassword(String)} is the method that should be used.
     *
     * @param userId      The id of the user changing his password
     * @param newPassword The new password in plain text. The backend will do the hashing
     * @throws BackendException If the update of the password failed
     */
    public void updatePassword(UUID userId, String newPassword)
            throws BackendException {

        String uri = backendUrl + "/changepassword?userId={userId}&newPassword={newPassword}";

        try {
            ResponseEntity<MethodResult> response = template.exchange(uri, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(newPassword), MethodResult.class,
                    userId.toString(), newPassword);
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully changed password of user [{}]", userId);
            }
        } catch (Exception ex) {
            log.warn("Could not update password of user [{}] to [{}]", userId, newPassword);
            throw new BackendException("Could not update user's " + userId.toString() + " " +
                    "password to " + newPassword);
        }
    }

    /**
     * Updates the email of a user.
     * The email must be unique in the entirety of the Jack the Clipper application as it is used
     * to uniquely identify a user.
     *
     * @param userId   The id of the user that wants to change his email
     * @param newEMail The new email address
     * @throws BackendException If the update of the mail address failed
     */
    public void updateMail(UUID userId, String newEMail)
            throws BackendException {

        String uri = backendUrl + "/changemailaddress?userId={userId}" + "&newUserMail={mail}";

        try {
            ResponseEntity<MethodResult> response = template.exchange(uri, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(newEMail), MethodResult.class,
                    userId.toString(), newEMail);
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully changed email of user [{}]", userId);
            }
        } catch (Exception ex) {
            log.warn("Could not update the mail of user [{}] to [{}]", userId, newEMail);
            //@formatter:off
            throw new BackendException("Could not update user's " + userId.toString() + " mail " +
                    "to " + newEMail);
            //@formatter:on
        }
    }

    /**
     * Tries to authenticate the given credentials by throwing them against the backend
     *
     * @param username     The name of the user. This might be an emailadress which is unique for
     *                     every user or a username which is just unique within one organization
     * @param organization The organization the user presumably belongs to
     * @param password     The entered password of the user
     * @return The authenticated user
     *
     * @throws HttpClientErrorException.BadRequest If the backend could not authenticate the user
     */
    public User authenticate(String username, String organization, String password)
            throws HttpClientErrorException.BadRequest {

        UUID principalUnit = ouService.mapOuNameToOuUUID(organization);
        String loginUri =
                backendUrl + "/login?userMailOrName={username}&userPassword={password}" +
                        "&principalUnit={unit}";
        ResponseEntity<User> response = template.getForEntity(loginUri, User.class, username,
                password, principalUnit.toString());
        if (ResponseEntityUtils.successful(response)) {
            return response.getBody();
        }
        //should not land here as before that the request should return HttpStatus 400 and thus
        // throw an exception
        throw new BackendException("Could not authenticate User");
    }

    /**
     * Returns which users a given user can manage.
     * No full {@link User} objects are returned but only a {@link Tuple}. {@link Tuple#first()}
     * will return {@link User#userId}, while {@link Tuple#second()} will return {@link User#name}.
     *
     * @param userId The id of the user requesting the information
     * @return A list of {@link MinimalUser} representing a user's id , username and whether he
     * is unlocked respectively.
     *
     * @throws BackendException If the REST-call to the backend failed due to
     *                          {@link org.springframework.http.HttpStatus#BAD_REQUEST}.
     */
    public List<MinimalUser> getManageableUsers(UUID userId)
            throws BackendException {

        String url = backendUrl + "/getmanageableusers?userId={userId}";
        try {
            ResponseEntity<MinimalUser[]> response = template.getForEntity(url,
                    MinimalUser[].class, userId.toString());
            if (ResponseEntityUtils.successful(response)) {
                return Arrays.asList(response.getBody());
            }
        } catch (HttpClientErrorException.BadRequest badRequest) {
            log.warn("Could not get manageable users of user [{}]", userId);
            throw new BackendException("Failed to get manageable user");
        }
        return Collections.emptyList();
    }

    /**
     * Retrieves additional information of a user.
     * This is intended for users with
     * {@link org.jacktheclipper.frontend.enums.UserRole#StaffChief} roles. With this they can
     * see to which {@link OrganizationalUnit}s a {@link User} belongs to.
     *
     * @param requesterId The id of the user requesting the information
     * @param requestedId The id of the user the information is requested of
     * @return An {@link ExtendedUser} holding all information one can manage about a user
     *
     * @throws BackendException If the REST-call failed or returned nothing
     */
    public ExtendedUser getUserInformation(UUID requesterId, UUID requestedId)
            throws BackendException {

        String url = backendUrl + "/getuserinfo?userId={staffChief}&requested={requested}";
        try {
            ResponseEntity<ExtendedUser> response = template.getForEntity(url, ExtendedUser.class
                    , requesterId.toString(), requestedId.toString());
            if (ResponseEntityUtils.successful(response)) {
                return response.getBody();
            }
        } catch (HttpClientErrorException.BadRequest badRequest) {
            log.warn("Failed to retrieve information of [{}] for user [{}]", requestedId,
                    requesterId);
            throw new BackendException("Could not find userinformation of " + requestedId.toString());
        }
        log.warn("Response was empty or not 2xx for request [{}]", url);
        throw new BackendException("Failed to retrieve userinformation of user " + requestedId.toString());
    }

    /**
     * Lets a user with {@link org.jacktheclipper.frontend.enums.UserRole#StaffChief} add an user.
     * The difference to {@link #registerUser(User, UUID)} is that this method indicates that the
     * user himself did not make an effort to gain access to the application. Also users created
     * by this method are valid per default and do not need to wait until they are unlocked
     *
     * @param staffChiefId  The id of the user adding the new user
     * @param basicUserInfo A {@link Tuple} holding the minimal information that is needed to
     *                      register a user
     * @return A {@link MethodResult} indicating the success or failure of the call. Callers need
     * to check {@link MethodResult#state} to see whether the request truly was successful.
     *
     * @throws BackendException If the REST-call failed or did return with a null body or an
     *                          {@link org.springframework.http.HttpStatus} that is not 2xx.
     */
    public MethodResult addUser(UUID staffChiefId, UserUuidsTuple basicUserInfo)
            throws BackendException {

        String url = backendUrl + "/adminadduser?staffChiefId={staffChiefId}";
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(basicUserInfo), MethodResult.class,
                    staffChiefId.toString());
            if (ResponseEntityUtils.successful(response)) {
                //@formatter:off
                log.debug("Successfully sent request to add user [{}] belonging to ous [{}] to "
                        + "the backend", basicUserInfo.first(), basicUserInfo.second());
                //@formatter:on
                return response.getBody();
            }
        } catch (HttpClientErrorException.BadRequest badRequest) {
            //@formatter:off
            log.warn("Could not add user with name [{}] belonging to ous [{}] as user with id " +
                    "[{}]", basicUserInfo.first(), basicUserInfo.second(), staffChiefId);
            //@formatter:on
            throw new BackendException("Could not add user");
        }
        throw new BackendException("Response did not include a body for request or was not 2xx");
    }

    /**
     * Modifies an already existing {@link User}.
     * This can be used by a user with
     * {@link org.jacktheclipper.frontend.enums.UserRole#StaffChief} privileges to promote other
     * users to {@link org.jacktheclipper.frontend.enums.UserRole#StaffChief} or unlock them so
     * that they can use the application.
     *
     * @param staffChiefId       The id of the {@link User} with
     *                           {@link org.jacktheclipper.frontend.enums.UserRole#StaffChief}
     *                           privileges wanting to modify a user, to e.g unlock them
     * @param updatedInformation The user that should be updated
     * @return A {@link MethodResult} indicating the success or failure of the call. Callers need
     * to check {@link MethodResult#state} to see whether the request truly was successful.
     *
     * @throws BackendException If the REST-call failed or did return with a null body or an
     *                          {@link org.springframework.http.HttpStatus} that is not 2xx.
     */
    public MethodResult modifyUser(UUID staffChiefId, UserUuidsTuple updatedInformation)
            throws BackendException {

        String url = backendUrl + "/modifyuser?staffChiefId={staffChiefId}";
        try {
            ResponseEntity<MethodResult> response = template.exchange(url, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(updatedInformation),
                    MethodResult.class, staffChiefId.toString());
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully sent request to update userinformation [{}] as " +
                        "staffchief [{}]", updatedInformation, staffChiefId);
                return response.getBody();
            }
        } catch (HttpClientErrorException.BadRequest badRequest) {
            log.warn("Could not modify user to [{}] as staffchief [{}]", updatedInformation,
                    staffChiefId);
            throw new BackendException("Failed to modify user");
        }
        log.warn("Response to request [{}] was null or not 2xx. Payload [{}]", url,
                updatedInformation);
        throw new BackendException("Failed to modify user");
    }
}
