package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.enums.ContentType;
import org.jacktheclipper.frontend.enums.UserRole;
import org.jacktheclipper.frontend.model.Source;
import org.jacktheclipper.frontend.service.OuService;
import org.jacktheclipper.frontend.service.SourceService;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.security.test.context.support.WithAnonymousUser;
import org.springframework.test.context.junit4.SpringRunner;
import org.springframework.test.web.servlet.MockMvc;

import java.util.Collections;
import java.util.UUID;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;


@RunWith(SpringRunner.class)
@AutoConfigureMockMvc
@SpringBootTest
public class SysAdminControllerTest {

    private Source source = new Source(UUID.randomUUID(), "http://www.mock.com", "MockSource",
            ContentType.Rss, "", Collections.emptyList(), "");

    @Autowired
    MockMvc mockMvc;

    @MockBean
    OuService ouService;

    @MockBean
    SourceService sourceService;

    @Before
    public void init() {

    }

    @WithAnonymousUser
    @Test
    public void testSourcePageUnaccessibleWhenAnonymous()
            throws Exception {

        mockMvc.perform(get("/admin/sources")).andExpect(status().isUnauthorized());
    }

    @Test
    @WithMockCustomUser(name = "admin", userRole = UserRole.SystemAdministrator)
    public void testSourcePageAccessibleBySysAdmin()
            throws Exception {


        when(sourceService.getAvailableSources(any())).thenReturn(Collections.singletonList(source));
        mockMvc.perform(get("/admin/sources")).andExpect(status().isOk());
    }

    @Test
    @WithMockCustomUser(name = "user")
    public void testSourcePageUnaccessibleWhenUser()
            throws Exception {

        mockMvc.perform(get("/admin/sources")).andExpect(status().isForbidden());
    }

    @Test
    @WithMockCustomUser(name = "staffChief", userRole = UserRole.StaffChief)
    public void testSourcePageUnaccessibleWhenStaffChief()
            throws Exception {

        mockMvc.perform(get("/admin/sources")).andExpect(status().isForbidden());
    }


    @WithAnonymousUser
    @Test
    public void testPrincipalPageUnaccessibleWhenAnonymous()
            throws Exception {

        mockMvc.perform(get("/admin/editclients")).andExpect(status().isUnauthorized());
    }

    @Test
    @WithMockCustomUser(name = "admin", userRole = UserRole.SystemAdministrator)
    public void testPrincipalPageAccessibleBySysAdmin()
            throws Exception {


        when(ouService.getPrincipalUnits(any())).thenReturn(Collections.emptyList());
        mockMvc.perform(get("/admin/editclients")).andExpect(status().isOk());
    }

    @Test
    @WithMockCustomUser(name = "user")
    public void testPrincipalPageUnaccessibleWhenUser()
            throws Exception {

        mockMvc.perform(get("/admin/editclients")).andExpect(status().isForbidden());
    }

    @Test
    @WithMockCustomUser(name = "staffChief", userRole = UserRole.StaffChief)
    public void testPrincipalPageUnaccessibleWhenStaffChief()
            throws Exception {

        mockMvc.perform(get("/admin/editclients")).andExpect(status().isForbidden());
    }


}
