package org.jacktheclipper.frontend.authentication;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonProperty;
import org.jacktheclipper.frontend.enums.UserRole;

import java.security.Principal;
import java.util.UUID;

/**
 * This class represents a user for this application. A user can be uniquely identified by his
 * UUID, by his eMail or by his name in his organization. A name does not need to be unique in
 * the entire application.
 */
public class User implements Principal {
    private UUID userId;
    private UserRole userRole;
    private String name;
    private String eMail;
    private String password;
    private String organization = "PLEASE_CHANGE"; //TODO remove default value when backend
    // passes this parameter


    //Used by Jackson to deserialize from JSON
    public User() {

    }

    public User(UUID userId, UserRole userRole, String name, String eMail, String password,
                String organization) {

        this.userId = userId;
        this.userRole = userRole;
        this.name = name;
        this.eMail = eMail;
        this.password = password;
        this.organization = organization;
    }

    public UUID getUserId() {

        return userId;
    }

    @JsonProperty("UserName")
    public void setName(String name) {

        this.name = name;
    }

    @JsonProperty("UserId")
    public void setUserId(UUID userId) {

        this.userId = userId;
    }

    public UserRole getUserRole() {

        return userRole;
    }

    @JsonProperty("UserRole")
    public void setUserRole(UserRole userRole) {

        this.userRole = userRole;
    }

    @Override
    public boolean equals(Object another) {

        if (another instanceof User) {
            return this.userId.equals(((User) another).getUserId());
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


    @Override
    public String getName() {

        if (name != null)
            return name;
        else {
            return "Anonymous";
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

    @JsonProperty("PassWord")
    public void setPassword(String password) {

        this.password = password;
    }

    //TODO change when passed
    @JsonIgnore
    public String getOrganization() {

        return organization;
    }

    @JsonIgnore
    public void setOrganization(String organization) {

        this.organization = organization;
    }
}
