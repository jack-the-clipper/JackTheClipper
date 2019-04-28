package org.jacktheclipper.frontend.exception;

/**
 * A wrapper to show that fault lies with the C# backend of this application. This is used to
 * better be able to distinguish between errors and possibly handle them
 */
public class BackendException extends RuntimeException {
    public BackendException(String message) {

        super(message);
    }
}
