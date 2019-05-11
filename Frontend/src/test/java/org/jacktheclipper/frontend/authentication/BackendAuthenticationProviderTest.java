package org.jacktheclipper.frontend.authentication;


import org.jacktheclipper.frontend.enums.UserRole;
import org.jacktheclipper.frontend.service.UserService;
import org.junit.Assert;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.security.authentication.BadCredentialsException;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.test.context.junit4.SpringRunner;
import org.springframework.util.CollectionUtils;
import org.springframework.web.client.HttpClientErrorException;

import java.util.UUID;

import static org.mockito.Mockito.when;

@RunWith(SpringRunner.class)
@SpringBootTest
public class BackendAuthenticationProviderTest {
    private User mockUser = new User(UUID.randomUUID(), UserRole.User, "mockUser",
            "mock@example.com", null, "MOCK", false);
    private Authentication auth = new CustomAuthenticationToken("mockUser", "daMock", "MOCK");
    @Autowired
    private BackendAuthenticationProvider provider;

    @MockBean
    private UserService userService;


    @Test
    public void supportsCorrectClassTest() {

        Class clazz = CustomAuthenticationToken.class;
        Assert.assertTrue(provider.supports(clazz));
    }

    @Test
    public void doesNotSupportWrongClass() {

        Class wrongClazz = UsernamePasswordAuthenticationToken.class;
        Assert.assertFalse(provider.supports(wrongClazz));
    }

    @Test
    public void successfulAuthentication() {

        when(userService.authenticate("mockUser", "MOCK", "daMock")).thenReturn(mockUser);

        Authentication actualAuth = provider.authenticate(auth);
        Assert.assertNotNull(actualAuth);
        Assert.assertTrue(actualAuth.isAuthenticated());
        Assert.assertFalse(CollectionUtils.isEmpty(actualAuth.getAuthorities()));

        //Actual implementation of User#equals just checks UUID
        Assert.assertEquals(actualAuth.getPrincipal(), mockUser);

        User actual = (User) actualAuth.getPrincipal();
        Assert.assertEquals(mockUser.getName(), actual.getName());
        Assert.assertNull(actual.getPassword());
        Assert.assertEquals(mockUser.geteMail(), actual.geteMail());
        Assert.assertEquals(mockUser.getOrganization(), actual.getOrganization());
    }

    @Test(expected = BadCredentialsException.class)
    public void unsuccessfulAuthentication() {

        when(userService.authenticate("mockUser", "MOCK", "daMock")).thenThrow(HttpClientErrorException.create(HttpStatus.BAD_REQUEST, "User not found", new HttpHeaders(), null, null));
        provider.authenticate(auth);
    }

    @Test
    public void testAuthenticationWithUserAsPrincipal(){
        when(userService.authenticate("mock@example.com", "MOCK", "daMock")).thenReturn(mockUser);
        Authentication mockAuth = new CustomAuthenticationToken(mockUser, "daMock", "MOCK");
        Authentication actualAuth = provider.authenticate(mockAuth);

        Assert.assertNotNull(actualAuth);
        Assert.assertTrue(actualAuth.isAuthenticated());
        Assert.assertFalse(CollectionUtils.isEmpty(actualAuth.getAuthorities()));

        //Actual implementation of User#equals just checks UUID
        Assert.assertEquals(actualAuth.getPrincipal(), mockUser);

        User actual = (User) actualAuth.getPrincipal();
        Assert.assertEquals(mockUser.getName(), actual.getName());
        Assert.assertNull(actual.getPassword());
        Assert.assertEquals(mockUser.geteMail(), actual.geteMail());
        Assert.assertEquals(mockUser.getOrganization(), actual.getOrganization());
    }

}
