package org.jacktheclipper.frontend.authentication;

import org.jacktheclipper.frontend.enums.UserRole;
import org.jacktheclipper.frontend.model.User;
import org.jacktheclipper.frontend.service.OuService;
import org.junit.Assert;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.security.core.Authentication;
import org.springframework.test.context.junit4.SpringRunner;

import java.util.UUID;

import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.when;

@SpringBootTest
@RunWith(SpringRunner.class)
public class OrganizationGuardTest {
    @MockBean
    OuService ouService;

    @Autowired
    OrganizationGuard guard;

    private User mockUser = new User(UUID.randomUUID(), UserRole.User, "mockUser",
            "mock@example.com", null, "MOCK", false, true, null, null);

    private Authentication auth = new CustomAuthenticationToken(mockUser, "",
            mockUser.getOrganization());

    @Test
    public void validOrganization() {

        when(ouService.mapOuNameToOuUUID(anyString())).thenReturn(UUID.randomUUID());
        Assert.assertTrue(guard.isValidOrganization("MOCK"));
    }

    @Test
    public void invalidOrganization() {

        when(ouService.mapOuNameToOuUUID(anyString())).thenReturn(null);
        Assert.assertFalse(guard.isValidOrganization("MOCK"));
    }

    @Test
    public void accessOwnOrganization() {

        when(ouService.mapOuNameToOuUUID(anyString())).thenReturn(UUID.randomUUID());
        Assert.assertTrue(guard.isOwnOrganization(auth, "MOCK"));
    }

    @Test
    public void accessOtherOrganization() {

        when(ouService.mapOuNameToOuUUID(anyString())).thenReturn(UUID.randomUUID());
        Assert.assertFalse(guard.isOwnOrganization(auth, "WRONG_MOCK"));
    }

    @Test
    public void validPassword() {

        Assert.assertTrue(guard.passwordOkay(auth));
    }

    @Test
    public void invalidPassword() {

        User otherUser = new User(UUID.randomUUID(), UserRole.User, "mockUser", "mock@example" +
                ".com", null, "MOCK", true, true, null, null);
        Authentication authentication = new CustomAuthenticationToken(otherUser, "secure", "MOCK");
        Assert.assertFalse(guard.passwordOkay(authentication));
    }
}
