package org.jacktheclipper.frontend.authentication;

import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.GrantedAuthority;

import java.util.Collection;

/**
 * A custom Token holding information from the login form, which in this case is a username, a
 * password and the name of an organization. The username may be an email
 *
 */
public class CustomAuthenticationToken extends UsernamePasswordAuthenticationToken {

    private String organization;

    public CustomAuthenticationToken(Object principal, Object credentials, String organization) {

        super(principal, credentials);
        this.organization = organization;
        super.setAuthenticated(false);
    }

    public CustomAuthenticationToken(Object principal, Object credentials, String organization,
                                     Collection<? extends GrantedAuthority> authorities) {

        super(principal, credentials, authorities);
        this.organization = organization;
    }

    public String getOrganization() {

        return this.organization;
    }
}