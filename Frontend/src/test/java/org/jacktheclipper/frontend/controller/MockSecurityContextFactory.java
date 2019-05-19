package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.authentication.CustomAuthenticationToken;
import org.jacktheclipper.frontend.model.User;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContext;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.test.context.support.WithSecurityContextFactory;

import java.util.UUID;

/**
 * Provide an {@link SecurityContext} for Unit tests by processing a {@link WithMockCustomUser}
 * annotation. This configures an {@link Authentication} implemented by a
 * {@link CustomAuthenticationToken} where {@link Authentication#getPrincipal()} returns the
 * {@link User} configured by {@link WithMockCustomUser}
 */
public final class MockSecurityContextFactory
        implements WithSecurityContextFactory<WithMockCustomUser> {
    @Override
    public SecurityContext createSecurityContext(WithMockCustomUser customUser) {

        SecurityContext context = SecurityContextHolder.createEmptyContext();

        User user = new User(UUID.randomUUID(), customUser.userRole(), customUser.name(),
                "mock" + customUser.userRole().toString() + "@example.com", customUser.password()
                , customUser.organization(), false, true, null, null);

        Authentication auth = new CustomAuthenticationToken(user, customUser.password(),
                customUser.organization(), customUser.userRole().resolveAuthorities());
        context.setAuthentication(auth);
        return context;
    }
}