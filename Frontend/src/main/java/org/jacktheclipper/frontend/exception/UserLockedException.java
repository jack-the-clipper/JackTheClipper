package org.jacktheclipper.frontend.exception;

import org.springframework.security.core.AuthenticationException;

/**
 * An exception signalling that the user was correctly identified but cannot use the application
 * since his {@link org.jacktheclipper.frontend.enums.UserRole#StaffChief} has not unlocked him
 * yet or disabled him on purpose.
 */
public class UserLockedException extends AuthenticationException {
    public  UserLockedException(String msg){
        super(msg);
    }

}
