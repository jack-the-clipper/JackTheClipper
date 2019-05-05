package org.jacktheclipper.frontend.service;

import org.apache.commons.lang3.NotImplementedException;
import org.jacktheclipper.frontend.authentication.User;
import org.jacktheclipper.frontend.exception.BackendException;
import org.jacktheclipper.frontend.model.MethodResult;
import org.jacktheclipper.frontend.utils.ResponseEntityUtils;
import org.jacktheclipper.frontend.utils.RestTemplateUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpMethod;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import java.util.UUID;

/**
 * Class for handling all request to the backend concerning an user.
 * This includes registering, resetting passwords , etc.
 */
@Service
public class UserService {
    @Value("${backend.url}")
    public String backendUrl;

    private static final Logger log = LoggerFactory.getLogger(UserService.class);

    /**
     * Registers the given user with Jack the Clipper.
     *
     * @param user           The user to be registered with all his details
     * @param organizationId The id of the organization the user should belong to
     * @return The registered user holding only the necessary information for the application. E
     * .g. passwords are not transmitted by the backend
     */
    public User registerUser(User user, UUID organizationId) {

        log.info("Attempting registration of User [{}]", user.getName());
        //TODO will be refactored by the backend
        String uriParameters =
                "?userMail=" + user.geteMail() + "&role=" + user.getUserRole().toString() +
                        "&unit=" + organizationId.toString() + "&userName=" + user.getName() +
                        "&password=" + user.getPassword();

        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        try {
            ResponseEntity<User> response =
                    restTemplate.exchange(backendUrl + "/register" + uriParameters,
                            HttpMethod.PUT, RestTemplateUtils.prepareBasicHttpEntity(user),
                            User.class);
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully registered user [{}]", user);
                return response.getBody();
            }
        } catch (Exception ex) {
            log.info("Failed registration due to [{}] with reason [{}]", ex.getClass().getName()
                    , ex.getMessage());
        }
        throw new BackendException("Failed registration");
    }

    /**
     * Raises the access rights of the given user to a staffChief's level
     *
     * @param staffChiefId The staffChief promoting the user
     * @param userId       The id of the user being promoted
     */
    public void promoteUser(UUID staffChiefId, UUID userId) {

        throw new NotImplementedException("UserService#promoteUser is not implemented yet");
    }

    /**
     * Gives a selfregistered user access to Jack the Clipper
     *
     * @param staffChiefId The id of the staffChief unlocking the user for Jack the Clipper
     * @param userId       The id of the user that should be unlocked
     */
    public void unlockUser(UUID staffChiefId, UUID userId) {

        throw new NotImplementedException("UserService#unlockUser is not implemented yet");
    }

    /**
     * Deletes the account of the specified user. Thus the user will not be able to use Jack the
     * Clipper anymore
     *
     * @param staffChiefId The staffChief deleting removing the user from his organizationalUnit
     * @param userId       The id of the user that should be removed
     */
    public void deleteUser(UUID staffChiefId, UUID userId) {

        throw new NotImplementedException("UserService#deleteUser is not implemented yet");
    }

    /**
     * Triggers the reset of a user's password.
     * This is invoked when a user has forgotten his password and thus cannot authenticate
     * himself to Jack the Clipper. If a user wants to set a new password himself
     * {@link #updatePassword(UUID, String)} should be used.
     *
     * @param eMail The mail of the user who forgot his password
     */
    public void resetPassword(String eMail) {

        String uri = backendUrl + "/reset?userMail=" + eMail;

        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        try {
            ResponseEntity<MethodResult> response = restTemplate.exchange(uri, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(eMail), MethodResult.class);
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully reset password of user [{}]", eMail);
            }
        } catch (Exception ex) {
            throw new BackendException("Could not reset user's " + eMail + " " + "password");
        }
    }

    /**
     * Sets the password of a user.
     * This implies that the password is intentionally being set by the user. If a user forgets
     * his password, {@link #resetPassword(String)} is the method that should be used.
     *
     * @param userId      The id of the user changing his password
     * @param newPassword The new password in plain text. The backend will do the hashing
     */
    public void updatePassword(UUID userId, String newPassword) {

        String uri =
                backendUrl + "/changepassword?userId=" + userId.toString() + "&newPassword=" + newPassword;

        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        try {
            ResponseEntity<MethodResult> response = restTemplate.exchange(uri, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(newPassword), MethodResult.class);
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully changed password of user [{}]", userId);
            }
        } catch (Exception ex) {
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
     */
    public void updateMail(UUID userId, String newEMail) {

        String uri = backendUrl + "/changemailaddress?userId=" + userId.toString() +
                "&newUserMail=" + newEMail;

        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        try {
            ResponseEntity<MethodResult> response = restTemplate.exchange(uri, HttpMethod.PUT,
                    RestTemplateUtils.prepareBasicHttpEntity(newEMail), MethodResult.class);
            if (ResponseEntityUtils.successful(response)) {
                log.debug("Successfully changed email of user [{}]", userId);
            }
        } catch (Exception ex) {
            throw new BackendException("Could not update user's " + userId.toString() + " mail " + "to" + " " + newEMail);
        }
    }
}
