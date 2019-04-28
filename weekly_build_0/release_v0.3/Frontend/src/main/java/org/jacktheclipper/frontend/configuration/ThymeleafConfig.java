package org.jacktheclipper.frontend.configuration;

import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.thymeleaf.extras.springsecurity4.dialect.SpringSecurityDialect;

/**
 * A class to add to the default configuration of Spring-Boot. This one handles everything
 * concerning the template rendering engine which in this case is Thymeleaf
 */
@Configuration
public class ThymeleafConfig {

    /**
     * Adds the security dialect to the template parsers so that expressions in the
     * sec:-namespace can be parsed. This is useful to show/ hide elements depending on a users'
     * authorities
     *
     * @return A SpringSecurityDialectBean so that it may be managed by the framework
     */
    @Bean
    public SpringSecurityDialect springSecurityDialect() {

        return new SpringSecurityDialect();
    }
}