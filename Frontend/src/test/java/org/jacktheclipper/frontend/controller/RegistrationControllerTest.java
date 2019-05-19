package org.jacktheclipper.frontend.controller;

import org.jacktheclipper.frontend.authentication.OrganizationGuard;
import org.jacktheclipper.frontend.service.OuService;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.boot.test.mock.mockito.MockBean;
import org.springframework.test.context.junit4.SpringRunner;
import org.springframework.test.web.servlet.MockMvc;

import java.util.Collections;

import static org.mockito.ArgumentMatchers.*;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@RunWith(SpringRunner.class)
@AutoConfigureMockMvc
@SpringBootTest
public class RegistrationControllerTest {
    @Autowired
    MockMvc mockMvc;

    @MockBean
    OrganizationGuard organizationGuard;

    @MockBean
    OuService ouService;

    @Before
    public void init() {

        when(organizationGuard.isOwnOrganization(any(), matches("MOCK"))).thenReturn(false);
        when(organizationGuard.isOwnOrganization(any(), matches("HansiMaier"))).thenReturn(true);
        when(organizationGuard.isValidOrganization("MOCK")).thenReturn(false);
        when(organizationGuard.isValidOrganization("SYSTEM")).thenReturn(true);
        when(organizationGuard.isValidOrganization("HansiMaier")).thenReturn(true);
        when(ouService.getUnitChildren(anyString())).thenReturn(Collections.emptyList());
    }

    @Test
    public void checkRegistrationAvailability()
            throws Exception {

        mockMvc.perform(get("/HansiMaier/register")).andExpect(status().isOk());
        mockMvc.perform(get("/MOCK/register")).andExpect(status().is4xxClientError());
    }

    @Test
    public void checkSystemRegistrationUnavailable()
            throws Exception {

        mockMvc.perform(get("/SYSTEM/register")).andExpect(status().is4xxClientError());
    }
}
