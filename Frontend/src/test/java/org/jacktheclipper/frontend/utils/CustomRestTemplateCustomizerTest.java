package org.jacktheclipper.frontend.utils;

import org.jacktheclipper.frontend.interceptors.LoggingRequestInterceptor;
import org.junit.Assert;
import org.junit.Before;
import org.junit.Test;
import org.springframework.http.client.InterceptingClientHttpRequestFactory;
import org.springframework.http.client.SimpleClientHttpRequestFactory;
import org.springframework.test.util.ReflectionTestUtils;
import org.springframework.web.client.RestTemplate;

public class CustomRestTemplateCustomizerTest {
    private CustomRestTemplateCustomizer customizer = new CustomRestTemplateCustomizer();
    private RestTemplate template;

    @Before
    public void init() {

        template = new RestTemplate();
    }

    @Test
    public void debugRequestAndResponses() {

        ReflectionTestUtils.setField(customizer, "debugRequests", true);
        ReflectionTestUtils.setField(customizer, "debugResponses", true);

        customizer.customize(template);

        Assert.assertEquals(1, template.getInterceptors().size());
        Assert.assertTrue(template.getInterceptors().get(0) instanceof LoggingRequestInterceptor);
        Assert.assertTrue(template.getRequestFactory() instanceof InterceptingClientHttpRequestFactory);
    }

    @Test
    public void debugRequestsOnly() {

        ReflectionTestUtils.setField(customizer, "debugRequests", true);
        ReflectionTestUtils.setField(customizer, "debugResponses", false);

        customizer.customize(template);

        Assert.assertEquals(1, template.getInterceptors().size());
        System.out.println(template.getRequestFactory().getClass());
        Assert.assertTrue(template.getInterceptors().get(0) instanceof LoggingRequestInterceptor);
        Assert.assertTrue(template.getRequestFactory() instanceof InterceptingClientHttpRequestFactory);
    }

    @Test
    public void debugNothing() {

        ReflectionTestUtils.setField(customizer, "debugRequests", false);
        ReflectionTestUtils.setField(customizer, "debugResponses", false);

        customizer.customize(template);

        Assert.assertEquals(0, template.getInterceptors().size());
        Assert.assertTrue(template.getRequestFactory() instanceof SimpleClientHttpRequestFactory);
    }

    @Test
    public void debugResponsesOnly() {

        ReflectionTestUtils.setField(customizer, "debugRequests", false);
        ReflectionTestUtils.setField(customizer, "debugResponses", true);

        customizer.customize(template);

        Assert.assertEquals(1, template.getInterceptors().size());
        Assert.assertTrue(template.getInterceptors().get(0) instanceof LoggingRequestInterceptor);
        Assert.assertTrue(template.getRequestFactory() instanceof InterceptingClientHttpRequestFactory);
    }
}
