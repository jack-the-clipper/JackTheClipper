package org.jacktheclipper.frontend.utils;

import org.jacktheclipper.frontend.model.User;
import org.jacktheclipper.frontend.enums.UserRole;
import org.junit.Assert;
import org.junit.Test;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;

import java.util.Collections;
import java.util.UUID;

public class RestTemplateUtilsTest {
    private User user = new User(UUID.fromString("10000000-0000-0000-0000-000000000000"),
            UserRole.User, "test", "test@example.com", "test", "Example", false, true, null, null);

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
