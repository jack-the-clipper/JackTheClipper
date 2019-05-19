package org.jacktheclipper.frontend.utils;

import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.web.client.RestTemplate;

import java.util.Arrays;

/**
 * Holds utility for the used {@link RestTemplate} class. The RestTemplate is used to make all
 * REST-calls
 * to te C# backend.
 */
public class RestTemplateUtils {


    /**
     * Prepares a {@link HttpEntity} for the given object. It will be send as application/json
     *
     * @param object The object to be sent with the HttpRequest
     * @param <T>    The type of the object being sent with this request
     * @return A {@link HttpEntity} with the proper {@link HttpHeaders}set and the (@code object) in
     * the request body
     */
    public static <T> HttpEntity<T> prepareBasicHttpEntity(T object) {

        HttpHeaders headers = new HttpHeaders();
        headers.setContentType(MediaType.APPLICATION_JSON);
        headers.setAccept(Arrays.asList(MediaType.APPLICATION_JSON));
        return new HttpEntity<T>(object, headers);
    }

}
