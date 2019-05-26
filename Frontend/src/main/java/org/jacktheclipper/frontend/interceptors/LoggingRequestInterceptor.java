package org.jacktheclipper.frontend.interceptors;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.HttpRequest;
import org.springframework.http.client.ClientHttpRequestExecution;
import org.springframework.http.client.ClientHttpRequestInterceptor;
import org.springframework.http.client.ClientHttpResponse;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;

/**
 * A class that logs the entirety of all http-requests and -responses from the application for
 * debugging purposes if it gets configured like that. It logs {@link HttpRequest}s if and only if
 * {@link #debugRequests} is {@code true} and logs {@link ClientHttpResponse} if and only if
 * {@link #debugResponses} is {@code true}. See
 * {@link org.jacktheclipper.frontend.utils.CustomRestTemplateCustomizer} for more information on
 * how to set what should be traced.
 *
 */
public class LoggingRequestInterceptor implements ClientHttpRequestInterceptor {

    private final boolean debugResponses;

    private final boolean debugRequests;

    public LoggingRequestInterceptor(boolean debugResponses, boolean debugRequests) {

        this.debugResponses = debugResponses;
        this.debugRequests = debugRequests;
    }

    private final static Logger log = LoggerFactory.getLogger(LoggingRequestInterceptor.class);

    /**
     * Intercepts all requests and logs them as well as their responses
     *
     * @param request
     * @param body
     * @param execution
     * @return The response of the executed request
     *
     * @throws IOException
     */
    @Override
    public ClientHttpResponse intercept(HttpRequest request, byte[] body,
                                        ClientHttpRequestExecution execution)
            throws IOException {

        long start = System.currentTimeMillis();
        if (debugRequests) {
            traceRequest(request, body);
        }
        ClientHttpResponse response = execution.execute(request, body);
        if (debugResponses) {
                traceResponse(response);
        }
        log.info("Tracing of exchange took [{}] ms", System.currentTimeMillis() - start);
        return response;
    }

    /**
     * Logs the given request and its body.
     * This method logs the uri, the http-method, the headers as well as the request body
     *
     * @param request
     * @param body
     * @throws IOException
     */
    private void traceRequest(HttpRequest request, byte[] body)
            throws IOException {

        log.info("===========================request begin" +
                "================================================");
        log.info("URI         : {}", request.getURI());
        log.info("Method      : {}", request.getMethod());
        log.info("Headers     : {}", request.getHeaders());
        log.info("Request body: {}", new String(body, StandardCharsets.UTF_8));
        log.info("==========================request end" +
                "================================================");
    }

    /**
     * Logs the given http-response
     * This includes http-status code and text as well as the response body
     *
     * @param response
     * @throws IOException
     */
    private void traceResponse(ClientHttpResponse response)
            throws IOException {

        StringBuilder inputStringBuilder = new StringBuilder();
        try {
            BufferedReader bufferedReader = new BufferedReader(
                    new InputStreamReader(response.getBody(), StandardCharsets.UTF_8));
            String line = bufferedReader.readLine();
            while (line != null) {
                inputStringBuilder.append(line);
                inputStringBuilder.append('\n');
                line = bufferedReader.readLine();
            }
        }catch (IOException io){
            inputStringBuilder.append("No reason supplied");
        }
        log.info("============================response begin" +
                "==========================================");
        log.info("Status code  : {}", response.getStatusCode());
        log.info("Status text  : {}", response.getStatusText());
        log.info("Headers      : {}", response.getHeaders());
        log.info("Response body: {}", inputStringBuilder.toString());
        log.info("=======================response end" +
                "=================================================");
    }

}