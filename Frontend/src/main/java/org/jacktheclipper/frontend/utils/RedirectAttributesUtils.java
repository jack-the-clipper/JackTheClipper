package org.jacktheclipper.frontend.utils;

import org.springframework.web.servlet.mvc.support.RedirectAttributes;

/**
 * A utility class for the {@link RedirectAttributes} of the Spring framework. This ensures that
 * all parameters are named consistently and their values are consistent as well. This allows
 * fragments like alertInfo in layout.html to be displayed correctly all the time.
 */
public class RedirectAttributesUtils {

    /**
     * Populates the {@link RedirectAttributes} with its default values.
     * Those values are used for displaying an alarm informing a user about the success or
     * failure of his action
     *
     * @param redirectAttributes
     * @param error              {@code True} if the user's action failed, e. g. when he mistypes
     *                           his password
     *                           when he is trying to change his email address, {@code False} if
     *                           the action was completed without failure
     * @param message            The message the user is intended to see. This might be a failure
     *                           message or a message indicating success
     */
    public static void populateDefaultRedirectAttributes(final RedirectAttributes redirectAttributes, boolean error, String message) {

        if (error) {
            redirectAttributes.addFlashAttribute("css", "alert-warning");
        } else {
            redirectAttributes.addFlashAttribute("css", "alert-info");
        }
        redirectAttributes.addFlashAttribute("msg", message);
    }
}
