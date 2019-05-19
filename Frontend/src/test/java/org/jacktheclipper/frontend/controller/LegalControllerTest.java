package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.authentication.BackendAuthenticationProvider;
import org.jacktheclipper.frontend.authentication.OrganizationGuard;
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

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.matches;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@RunWith(SpringRunner.class)
@AutoConfigureMockMvc
@SpringBootTest
public class LegalControllerTest {
    @Autowired
    private MockMvc mockMvc;

    @MockBean(name = "organizationGuard")
    OrganizationGuard organizationGuard;

    @MockBean
    BackendAuthenticationProvider mockProvider;


    @Before
    public void init() {

        when(organizationGuard.isOwnOrganization(any(), matches("MOCK"))).thenReturn(false);
        when(organizationGuard.isOwnOrganization(any(), matches("HansiMaier"))).thenReturn(true);
        when(organizationGuard.isValidOrganization("MOCK")).thenReturn(false);
        when(organizationGuard.isValidOrganization("HansiMaier")).thenReturn(true);

    }

    @Test
    @WithAnonymousUser
    public void testImpressumAvailability()
            throws Exception {

        mockMvc.perform(get("/HansiMaier/impressum")).andExpect(status().isOk());
        mockMvc.perform(get("/MOCK/impressum")).andExpect(status().is4xxClientError());
    }

    @Test
    @WithAnonymousUser
    public void testPrivacyPolicyAvailability()
            throws Exception {

        mockMvc.perform(get("/HansiMaier/privacypolicy")).andExpect(status().isOk());
        mockMvc.perform(get("/MOCK/privacypolicy")).andExpect(status().is4xxClientError());
    }


}
