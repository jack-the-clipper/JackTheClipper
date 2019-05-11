package org.jacktheclipper.frontend.utils;

import org.springframework.http.ResponseEntity;

/**
 * Holds all utility for {@link ResponseEntity}. The class is returned as an answer to REST calls
 * with {@link org.springframework.web.client.RestTemplate}
 */
public class ResponseEntityUtils {

    /**
     * Checks whether the REST-called returned a successful response.
     * This is the case if its statuscode is 2xx and its body is not empty/null
     *
     * @param entity The ResponseEntity to check
     * @return True, if the http status code is 2xx and the response does have a non null body
     */
    public static boolean successful(ResponseEntity entity) {

        return (entity.getStatusCode().is2xxSuccessful() && entity.getBody() != null);
    }
}
