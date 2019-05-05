package org.jacktheclipper.frontend.utils;

import org.jacktheclipper.frontend.authentication.User;
import org.jacktheclipper.frontend.enums.UserRole;
import org.jacktheclipper.frontend.interceptors.LoggingRequestInterceptor;
import org.junit.Assert;
import org.junit.Test;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.util.CollectionUtils;
import org.springframework.web.client.RestTemplate;

import java.lang.reflect.Field;
import java.util.UUID;

public class RestTemplateUtilsTest {
    private User user = new User(UUID.fromString("10000000-0000-0000-0000-000000000000"),
            UserRole.User, "test", "test@example.com", "test", "Example",false);
    @Test
    public void testRestTemplateFactory() {

        Field debug = null;
        try {
            debug = RestTemplateUtils.class.getDeclaredField("debug");
        } catch (NoSuchFieldException e) {
            e.printStackTrace();
            Assert.fail("RestTemplateUtils#debug does not exist");
        }
        debug.setAccessible(true);
        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        try {
            if (!(Boolean) debug.get(new RestTemplateUtils())) {
                Assert.assertTrue(CollectionUtils.isEmpty(restTemplate.getInterceptors()));
            } else {
                Assert.assertEquals(1, restTemplate.getInterceptors().size());
                Assert.assertTrue(restTemplate.getInterceptors().get(0) instanceof LoggingRequestInterceptor);
            }
        } catch (IllegalAccessException e) {
            e.printStackTrace();
            Assert.fail("RestTemplateUtils#debug could not be accessed");
        }
    }
    @Test
    public void testHttpEntityFactory(){

        HttpEntity<User> entity= RestTemplateUtils.prepareBasicHttpEntity(user);
        HttpHeaders expectedHeaders = new HttpHeaders();
        expectedHeaders.setContentType(MediaType.APPLICATION_JSON);

        Assert.assertEquals(user,entity.getBody());
        Assert.assertEquals(expectedHeaders,entity.getHeaders());
    }
}
