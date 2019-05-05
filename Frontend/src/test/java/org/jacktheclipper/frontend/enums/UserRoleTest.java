package org.jacktheclipper.frontend.enums;

import org.junit.Test;
import org.springframework.security.core.GrantedAuthority;

import java.util.List;

import static org.junit.Assert.assertEquals;

public class UserRoleTest {

    @Test
    public void userRights() {

        List<GrantedAuthority> grantedAuthorities = UserRole.User.resolveAuthorities();

        assertEquals(1, grantedAuthorities.size());
        assertEquals("ROLE_USER", grantedAuthorities.get(0).getAuthority());
    }

    @Test
    public void staffChiefRights() {

        List<GrantedAuthority> grantedAuthorities = UserRole.StaffChief.resolveAuthorities();

        assertEquals(2, grantedAuthorities.size());
        assertEquals("ROLE_USER", grantedAuthorities.get(1).getAuthority());
        assertEquals("ROLE_STAFFCHIEF", grantedAuthorities.get(0).getAuthority());
    }

    @Test
    public void sysAdminRights() {

        List<GrantedAuthority> grantedAuthorities =
                UserRole.SystemAdministrator.resolveAuthorities();

        assertEquals(1, grantedAuthorities.size());
        assertEquals("ROLE_SYSADMIN", grantedAuthorities.get(0).getAuthority());
    }
}
