package org.jacktheclipper.frontend.utils;

import org.jacktheclipper.frontend.authentication.User;
import org.jacktheclipper.frontend.enums.UserRole;
import org.junit.Assert;
import org.junit.Test;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;

import java.util.UUID;

public class AuthenticationUtilsTest {

    private User user = new User(UUID.fromString("10000000-0000-0000-0000-000000000000"),
            UserRole.User, "test", "test@example.com", "test", "Example",false);
    private Authentication invalidAuthentication = new UsernamePasswordAuthenticationToken("otto"
            , "georg");

    @Test
    public void getUuidNull() {

        try {
            AuthenticationUtils.getUserId(null);
            Assert.fail("AuthenticationUtils#getUserId worked on null parameter when it " + 
                    "should not");
        } catch (NullPointerException ex) {
            Assert.assertTrue(true);
        }
    }

    @Test
    public void getUuidInvalidArgument() {


        try {
            AuthenticationUtils.getUserId(invalidAuthentication);
            Assert.fail("AuthenticationUtils#getUserid worked on invalid parameter type [" + invalidAuthentication.getPrincipal().getClass() + "]");
        } catch (IllegalArgumentException ex) {
            Assert.assertTrue(true);
        }
    }

    @Test
    public void getUuid() {

        UUID id = AuthenticationUtils.getUserId(new UsernamePasswordAuthenticationToken(user,
                user.getPassword()));
        Assert.assertEquals(UUID.fromString("10000000-0000-0000-0000-000000000000"), id);
    }

    @Test
    public void getOrganizationNull() {

        try {
            AuthenticationUtils.getOrganization(null);
            Assert.fail("AuthenticationUtils#getOrganization worked on null parameter when it " + "should not");
        } catch (NullPointerException ex) {
            Assert.assertTrue(true);
        }
    }

    @Test
    public void getOrganizationInvalidArgument() {

        try {
            AuthenticationUtils.getOrganization(invalidAuthentication);
            Assert.fail();
        } catch (IllegalArgumentException ex) {
            Assert.assertTrue(true);
        }
    }

    @Test
    public void getOrganization() {

        String orga =
                AuthenticationUtils.getOrganization(new UsernamePasswordAuthenticationToken(user,
                        user.getPassword()));
        Assert.assertEquals("Organization [" + orga + "] differed from expected [Example]", 
                "Example", orga);
    }
}
