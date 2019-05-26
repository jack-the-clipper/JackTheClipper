import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.security.Principal;
import java.util.UUID;

/**
 * Holds the minimal information of an user. This includes his unique id, a name unique within
 * the user's organization and whether he is allowed to use the application.
 */
public class MinimalUser implements Principal {

    @JsonProperty("UserName")
    private String name;

    @JsonInclude(JsonInclude.Include.NON_NULL)
    @JsonProperty("UserId")
    private UUID userId;

    @JsonProperty("UserValid")
    private boolean unlocked = false;

    public MinimalUser() {

    }

    public MinimalUser(String name, UUID userId, boolean unlocked) {

        this.name = name;
        this.userId = userId;
        this.unlocked = unlocked;
    }

    @Override
    public String getName() {

        return name;
    }

    public void setName(String name) {

        this.name = name;
    }

    public UUID getUserId() {

        return userId;
    }

    public void setUserId(UUID userId) {

        this.userId = userId;
    }

    public boolean isUnlocked() {

        return unlocked;
    }

    public void setUnlocked(boolean unlocked) {

        this.unlocked = unlocked;
    }
}
