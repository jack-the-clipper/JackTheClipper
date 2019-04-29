package org.jacktheclipper.frontend.configuration;

import org.springframework.context.annotation.Configuration;
import org.springframework.web.servlet.config.annotation.ViewControllerRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;

/**
 * A class to add to the default configuration of Spring-Boot. This one handles the registration
 * of views that do not need any logic behind them
 */
@Configuration
public class WebMvcConfiguration implements WebMvcConfigurer {

    /**
     * Adds simple views that do not need logic to display like the login page
     *
     * @param registry
     */
    @Override
    public void addViewControllers(ViewControllerRegistry registry) {

        registry.addViewController("/403").setViewName("403");
    }

}