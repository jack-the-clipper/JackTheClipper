package org.jacktheclipper.frontend.utils;

import org.jacktheclipper.frontend.interceptors.LoggingRequestInterceptor;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.http.client.BufferingClientHttpRequestFactory;
import org.springframework.http.client.SimpleClientHttpRequestFactory;
import org.springframework.web.client.RestTemplate;

/**
 * Holds utility for the used RestTemplate class. The RestTemplate is used to make all REST-calls
 * to te C# backend.
 */
public class RestTemplateUtils {

    //Use this boolean to choose whether to log the content of every request/ response sent/
    // received by a RestTemplate in this application.
    private static boolean debug = true;


    /**
     * Returns a new instance of RestTemplate.
     * If debugging mode is enabled, a LoggingRequestInterceptor is added to the RestTemplate to
     * log the content of the http-requests and -responses of the entire application
     *
     * @return A new instance of RestTemplate
     */
    public static RestTemplate getRestTemplate() {

        if (debug) {
            RestTemplate restTemplate =
                    new RestTemplate(new BufferingClientHttpRequestFactory(new SimpleClientHttpRequestFactory()));
            restTemplate.getInterceptors().add(new LoggingRequestInterceptor());
            return restTemplate;
        } else {
            return new RestTemplate();
        }
    }

    /**
     * Prepares a HttpEntity for the given object. It will be send as application/json
     *
     * @param object The object to be sent with the HttpRequest
     * @param <T>    The type of the object being sent with this request
     * @return A HttpEntity with the proper headers set and the (@code object) in the request body
     */
    public static <T> HttpEntity<T> prepareBasicHttpEntity(T object) {

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        return new HttpEntity<T>(object, headers);
    }

    public static boolean isDebug() {

        return debug;
    }

    public static void setDebug(boolean debug) {

        RestTemplateUtils.debug = debug;
    }
}
