package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonProperty;
import org.jacktheclipper.frontend.enums.SuccessState;

/**
 * A generic result signaling whether a method succeeded and if not why.
 */
public class MethodResult {

    @JsonProperty("Status")
    private SuccessState state;
    @JsonProperty("UserMessage")
    private String message;

    public MethodResult() {

    }

    public MethodResult(SuccessState state, String message) {

        this.state = state;
        this.message = message;
    }

    public SuccessState getState() {

        return state;
    }

    public void setState(SuccessState state) {

        this.state = state;
    }

    public String getMessage() {

        return message;
    }

    public void setMessage(String message) {

        this.message = message;
    }

    /**
     * Turns the error codes returned by the backend into human readable messages
     *
     * @param prefix The prefix to be appended to every mapped error message
     * @return A human read- and understandable version of the returned error code.
     */
    public String mapErrorCodeToMessage(String prefix) {

        String msgToShow;
        if (message.startsWith("ERROR999992")) {
            msgToShow = "Der Benutzername wird bereits verwendet.";
        } else if (message.startsWith("ERROR999993")) {
            msgToShow = "Die E-Mail-Adresse ist bereits vergeben.";
        } else if (message.startsWith("ERROR999991")) {
            msgToShow = "Der Mandantenname wird bereits verwendet.";
        } else {
            msgToShow = "Etwas ist schief gelaufen";
        } return prefix + msgToShow;
    }

}
