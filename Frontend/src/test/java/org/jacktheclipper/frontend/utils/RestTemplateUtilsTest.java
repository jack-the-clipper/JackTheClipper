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

import java.util.Collections;
import java.util.UUID;

public class RestTemplateUtilsTest {
    private User user = new User(UUID.fromString("10000000-0000-0000-0000-000000000000"),
            UserRole.User, "test", "test@example.com", "test", "Example", false);

    @Test
    public void testRestTemplateFactoryDebugEnabled() {

        RestTemplateUtils.setDebug(true);

        Assert.assertTrue(RestTemplateUtils.isDebug());
        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();

        Assert.assertEquals(1, restTemplate.getInterceptors().size());
        Assert.assertTrue(restTemplate.getInterceptors().get(0) instanceof LoggingRequestInterceptor);

    }

    @Test
    public void testRestTemplateFactoryDebugDisabled() {

        RestTemplateUtils.setDebug(false);
        Assert.assertFalse(RestTemplateUtils.isDebug());
        RestTemplate restTemplate = RestTemplateUtils.getRestTemplate();
        Assert.assertTrue("there were interceptors",
                CollectionUtils.isEmpty(restTemplate.getInterceptors()));
    }

    @Test
    public void testHttpEntityFactory() {

        HttpEntity<User> entity = RestTemplateUtils.prepareBasicHttpEntity(user);
        HttpHeaders expectedHeaders = new HttpHeaders();
        expectedHeaders.setContentType(MediaType.APPLICATION_JSON);
        expectedHeaders.setAccept(Collections.singletonList(MediaType.APPLICATION_JSON));
        Assert.assertEquals(user, entity.getBody());
        Assert.assertEquals(expectedHeaders, entity.getHeaders());
    }
}
