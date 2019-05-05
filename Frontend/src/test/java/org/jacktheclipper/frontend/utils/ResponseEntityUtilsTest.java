package org.jacktheclipper.frontend.utils;

import org.jacktheclipper.frontend.authentication.User;
import org.jacktheclipper.frontend.enums.UserRole;
import org.junit.Assert;
import org.junit.Test;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;

import java.util.UUID;

public class ResponseEntityUtilsTest {
    private User user = new User(UUID.fromString("10000000-0000-0000-0000-000000000000"),
            UserRole.User, "test", "test@example.com", "test", "Example",false);

    @Test
    public void testSuccessfulResponse() {

        ResponseEntity<User> response = new ResponseEntity<User>(user, HttpStatus.OK);
        Assert.assertTrue(ResponseEntityUtils.successful(response));
    }

    @Test
    public void testNullBody(){
        ResponseEntity<User> response = new ResponseEntity<User>((User)null,HttpStatus.OK);
        Assert.assertFalse(ResponseEntityUtils.successful(response));
    }

    @Test
    public void testUnsuccessfulHttpStatus(){
        ResponseEntity<User> response = new ResponseEntity<User>(user,HttpStatus.BAD_REQUEST);
        Assert.assertFalse(ResponseEntityUtils.successful(response));
    }
}
