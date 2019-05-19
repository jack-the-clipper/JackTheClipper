package org.jacktheclipper.frontend.utils;

import org.jacktheclipper.frontend.interceptors.LoggingRequestInterceptor;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.web.client.RestTemplateCustomizer;
import org.springframework.http.client.BufferingClientHttpRequestFactory;
import org.springframework.http.client.SimpleClientHttpRequestFactory;
import org.springframework.web.client.RestTemplate;

/**
 * A class that customizes every {@link RestTemplate} used in the entire application. Thus the
 * customization codes does not need to be repeated it is registered with the Spring context via
 * {@link org.jacktheclipper.frontend.configuration.ThymeleafConfig#customRestTemplateCustomizer()}
 */
public class CustomRestTemplateCustomizer implements RestTemplateCustomizer {

    @Value("${requests.debug.requests}")
    private boolean debugRequests;

    @Value("${requests.debug.responses}")
    private boolean debugResponses;

    /**
     * Customizes the supplied {@link RestTemplate} as needed.
     * This adds a {@link LoggingRequestInterceptor} if {@link #debugRequests} or
     * {@link #debugResponses} is true.
     * {@link #debugRequests} can be set via the property {@code "request.debug"} in any of the
     * .properties files. Remember that profile specific ones like application-pu.properties
     * override properties set in application.properties.
     *
     * @param restTemplate The {@link RestTemplate} to customize
     */
    @Override
    public void customize(RestTemplate restTemplate) {

        if (debugRequests || debugResponses) {
            restTemplate.getInterceptors().
                    add(new LoggingRequestInterceptor(debugResponses,debugRequests));
        }

        if (debugResponses) {
            // Changing the factory is necessary when tracing HttpResponses. Otherwise the
            // interceptor consumes the body of the response, thus making
            // ResponseEntityUtils#successful return false since the body is null. In that
            // case Jackson would not be able to parse the response then anyways
            restTemplate.setRequestFactory(
                    new BufferingClientHttpRequestFactory(new SimpleClientHttpRequestFactory()));
        }
    }
}
