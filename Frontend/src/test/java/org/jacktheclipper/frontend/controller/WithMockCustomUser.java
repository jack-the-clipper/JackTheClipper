package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.enums.UserRole;
import org.springframework.security.test.context.support.WithSecurityContext;

import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;

/**
 * Annotation that allows for an easy configuration of the
 * {@link org.springframework.security.core.Authentication} for Unit tests. You can easily
 * configure the important properties of an {@link org.jacktheclipper.frontend.model.User} with
 * this annotation.
 */
@Retention(RetentionPolicy.RUNTIME)
@WithSecurityContext(factory = MockSecurityContextFactory.class)
public @interface WithMockCustomUser {

    UserRole userRole() default UserRole.User;

    String name();

    String password() default "secure";

    String organization() default "MOCK";
}