package org.jacktheclipper.frontend.enums;

import org.springframework.security.core.GrantedAuthority;
import org.springframework.security.core.authority.SimpleGrantedAuthority;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents the roles a user could possibly have
 */
public enum UserRole {
    User, StaffChief, SystemAdministrator;

    /**
     * A method to grant an user authority based on his role.
     * The user only holds one role only, so this method adds all roles, that are lower in
     * the hierarchy than the user's. The authorities themselves are used for access control
     * decisions.
     *
     * @return The list with all authorities linked to a role
     */
    public List<GrantedAuthority> resolveAuthorities() {

        List<GrantedAuthority> grantedAuthorities = new ArrayList<>();
        switch (this) {

            case StaffChief:
                grantedAuthorities.add(new SimpleGrantedAuthority("ROLE_STAFFCHIEF"));
                /* Intentional fallthrough */
            case User:
                grantedAuthorities.add(new SimpleGrantedAuthority("ROLE_USER"));
                break;
            case SystemAdministrator:
                //SystemAdministrators are not users. They are incapable of owning feeds
                grantedAuthorities.add(new SimpleGrantedAuthority("ROLE_SYSADMIN"));
                break;
        }
        return grantedAuthorities;
    }
}