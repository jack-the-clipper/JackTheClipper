import com.fasterxml.jackson.annotation.JsonProperty;
import enums.SuccessState;

/**
 * A generic result signaling whether a method succeeded and if not why.
 */
public class MethodResult {

    private SuccessState state;
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

    @JsonProperty("Status")
    public void setState(SuccessState state) {

        this.state = state;
    }

    public String getMessage() {

        return message;
    }

    @JsonProperty("UserMessage")
    public void setMessage(String message) {

        this.message = message;
    }
}
