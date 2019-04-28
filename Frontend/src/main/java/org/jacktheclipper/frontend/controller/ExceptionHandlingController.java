package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.exception.BackendException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.ControllerAdvice;
import org.springframework.web.bind.annotation.ExceptionHandler;

import javax.servlet.http.HttpServletRequest;


/**
 * This class offers a central place to manage all exceptions. This is useful since the code for
 * exceptionhandling does not need to be duplicated into every controller.
 */
@ControllerAdvice
public class ExceptionHandlingController {

    private final Logger log = LoggerFactory.getLogger(this.getClass());

    /**
     * Handles all exception arising from communication with the backend.
     *
     * @param model   The model to customize the error view, if necessary
     * @param ex      The exception thrown
     * @param request The http request that could not be served due to the exception
     * @return The error page
     */
    @ExceptionHandler(BackendException.class)
    public String handleBackendDown(Model model, BackendException ex, HttpServletRequest request) {

        model.addAttribute("error", "Backend Communication");
        model.addAttribute("message", ex.getMessage());
        log.warn("Request to [{}] failed, reason [{}]", request.getRequestURL().toString(),
                ex.getMessage());
        return "redirect:/error";
    }

}
