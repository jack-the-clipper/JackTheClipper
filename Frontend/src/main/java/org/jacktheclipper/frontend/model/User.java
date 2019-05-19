package org.jacktheclipper.frontend.model;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;
import org.jacktheclipper.frontend.enums.UserRole;

import java.security.Principal;
import java.util.UUID;

/**
 * Adds additional information  to {@link MinimalUser}. E.g. {@link #eMail} which represents a
 * users mail address which is unique in the entire application. It also holds a user's role on
 * which some access control decisions are based.
 * This class represents a user for this application. A user can be uniquely identified by his
 * {@link UUID}, by his eMail or by his username within the organization he belongs to. A name does
 * not need to be unique in the entire application.
 */
public class User extends MinimalUser  {

    @JsonProperty("UserRole")
    private UserRole userRole;

    @JsonProperty("UserMail")
    private String eMail;
    @JsonIgnore
    private String password;
    @JsonProperty("UserPrincipalUnit")
    private String organization;
    @JsonProperty("MustChangePassword")
    private boolean mustChangePassword = false;
    @JsonInclude(JsonInclude.Include.NON_NULL)
    @JsonProperty("UserSettingsId")
    private UUID settingsId;
    @JsonProperty("UserPrincipalUnitId")
    private UUID principalUnitId;


    //Used by Jackson to deserialize from JSON
    public User() {

    }

    public User(UUID userId, UserRole userRole, String name, String eMail, String password,
                String organization, boolean mustChangePassword, boolean unlocked,
                UUID settingsId, UUID principalUnitId) {

        super(name, userId, unlocked);
        this.userRole = userRole;
        this.eMail = eMail;
        this.password = password;
        this.organization = organization;
        this.mustChangePassword = mustChangePassword;
        this.settingsId = settingsId;
        this.principalUnitId = principalUnitId;
    }


    public UserRole getUserRole() {

        return userRole;
    }

    public void setUserRole(UserRole userRole) {

        this.userRole = userRole;
    }

    @Override
    public boolean equals(Object another) {

        if (another instanceof User && getUserId() != null) {
            return getUserId().equals(((User) another).getUserId());
        }
        return false;
    }

    @Override
    public String toString() {

        if (userRole != null)
            return getName() + ", " + userRole.toString();
        else {
            return getName();
        }
    }

    public String geteMail() {

        return eMail;
    }

    public void seteMail(String eMail) {

        this.eMail = eMail;
    }

    public String getPassword() {

        return password;
    }

    public void setPassword(String password) {

        this.password = password;
    }

    public String getOrganization() {

        return organization;
    }

    public void setOrganization(String organization) {

        this.organization = organization;
    }

    public boolean isMustChangePassword() {

        return mustChangePassword;
    }

    public void setMustChangePassword(boolean mustChangePassword) {

        this.mustChangePassword = mustChangePassword;
    }

    public UUID getSettingsId() {

        return settingsId;
    }

    public void setSettingsId(UUID settingsId) {

        this.settingsId = settingsId;
    }

    public UUID getPrincipalUnitId() {

        return principalUnitId;
    }

    public void setPrincipalUnitId(UUID principalUnitId) {

        this.principalUnitId = principalUnitId;
    }
}
