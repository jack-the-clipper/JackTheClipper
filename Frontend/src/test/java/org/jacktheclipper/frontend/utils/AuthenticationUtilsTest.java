package org.jacktheclipper.frontend.utils;

import org.jacktheclipper.frontend.enums.UserRole;
import org.jacktheclipper.frontend.model.User;
import org.junit.Assert;
import org.junit.Test;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;

import java.util.UUID;

public class AuthenticationUtilsTest {

    private User user = new User(UUID.fromString("10000000-0000-0000-0000-000000000000"),
            UserRole.User, "test", "test@example.com", "test", "Example", false, true, null, null);
    private Authentication invalidAuthentication = new UsernamePasswordAuthenticationToken("otto"
            , "georg");

    @Test(expected = NullPointerException.class)
    public void getUuidNull() {

        AuthenticationUtils.getUserId(null);
    }

    @Test(expected = IllegalArgumentException.class)
    public void getUuidInvalidArgument() {


        AuthenticationUtils.getUserId(invalidAuthentication);
    }

    @Test
    public void getUuid() {

        UUID id = AuthenticationUtils.getUserId(new UsernamePasswordAuthenticationToken(user,
                user.getPassword()));
        Assert.assertEquals(UUID.fromString("10000000-0000-0000-0000-000000000000"), id);
    }

    @Test(expected = NullPointerException.class)
    public void getOrganizationNull() {

        AuthenticationUtils.getOrganization(null);
    }

    @Test(expected = IllegalArgumentException.class)
    public void getOrganizationInvalidArgument() {

        AuthenticationUtils.getOrganization(invalidAuthentication);
    }

    @Test()
    public void getOrganization() {

        String orga =
                AuthenticationUtils.getOrganization(new UsernamePasswordAuthenticationToken(user,
                        user.getPassword()));
        Assert.assertEquals("Organization [" + orga + "] differed from expected [Example]",
                "Example", orga);
    }

    @Test(expected = NullPointerException.class)
    public void getEmailNull() {

        AuthenticationUtils.getEmail(null);
    }

    @Test(expected = IllegalArgumentException.class)
    public void getEmailInvalidArgument() {

        AuthenticationUtils.getEmail(invalidAuthentication);
    }

    @Test
    public void getEmail() {

        String mail = AuthenticationUtils.getEmail(new UsernamePasswordAuthenticationToken(user,
                user.getPassword()));
        Assert.assertEquals("Email[" + mail + "] differed from expected [test@example.com]",
                "test@example.com", mail);
    }

    @Test(expected = NullPointerException.class)
    public void isMustChangePasswordNull() {

        AuthenticationUtils.isMustChangePassword(null);
    }

    @Test(expected = IllegalArgumentException.class)
    public void isMustChangePasswordInvalidArgument() {

        AuthenticationUtils.isMustChangePassword(invalidAuthentication);
    }

    @Test
    public void isMustChangePassword() {

        boolean mustChangePassword =
                AuthenticationUtils.isMustChangePassword(new UsernamePasswordAuthenticationToken(user, user.getPassword()));
        Assert.assertFalse("mustChangePassword should have been false but was not",
                mustChangePassword);
    }
}
