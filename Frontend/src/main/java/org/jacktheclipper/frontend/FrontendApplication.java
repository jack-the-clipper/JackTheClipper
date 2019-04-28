package org.jacktheclipper.frontend;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.web.servlet.support.SpringBootServletInitializer;

/**
 * The class responsible to start up this entire application. It extends
 * SpringBootServletInitializer to enable running as a .war in a tomcat application server.
 */
@SpringBootApplication
public class FrontendApplication extends SpringBootServletInitializer {


    public static void main(String[] args) {

        SpringApplication.run(FrontendApplication.class, args);
    }

}
