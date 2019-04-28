package org.jacktheclipper.frontend.configuration;

import org.jacktheclipper.frontend.authentication.*;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.config.annotation.authentication.builders.AuthenticationManagerBuilder;
import org.springframework.security.config.annotation.method.configuration.EnableGlobalMethodSecurity;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.config.annotation.web.configuration.EnableWebSecurity;
import org.springframework.security.config.annotation.web.configuration.WebSecurityConfigurerAdapter;
import org.springframework.security.web.authentication.AuthenticationFailureHandler;
import org.springframework.security.web.authentication.AuthenticationSuccessHandler;
import org.springframework.security.web.authentication.UsernamePasswordAuthenticationFilter;
import org.springframework.security.web.authentication.logout.LogoutSuccessHandler;

/**
 * This class defines the generic security settings for the frontend application.
 * It is thus important for any and every thing concerning authentication and authorization. It
 * also enables method level security.
 *
 * @author SBG
 */
@Configuration
@EnableWebSecurity
@EnableGlobalMethodSecurity(prePostEnabled = true)
public class WebSecurityConfig extends WebSecurityConfigurerAdapter {

    private final BackendAuthenticationProvider authProvider;

    @Autowired
    public WebSecurityConfig(BackendAuthenticationProvider authProvider) {

        this.authProvider = authProvider;
    }

    /**
     * Registers the BackendauthenticationProvider with the Spring framework
     *
     * @param auth
     */
    @Override
    public void configure(AuthenticationManagerBuilder auth) {

        auth.authenticationProvider(authProvider);
    }

    /**
     * Configures which sides can be accessed with and without authentication and other
     * configuration concerning security. This is the actual meat of the class
     *
     * @param http the filterchain to configure
     * @throws Exception as the super method declares so
     */
    @Override
    public void configure(HttpSecurity http)
            throws Exception {

        http.addFilterBefore(authenticationFilter(), UsernamePasswordAuthenticationFilter.class).antMatcher("/**").csrf().disable().
                formLogin().loginPage("/login").successHandler(successHandler()).permitAll().
                and().exceptionHandling().accessDeniedPage("/403").and().
                logout().logoutSuccessHandler(logoutSuccessHandler()).permitAll().and().
                authorizeRequests().
                antMatchers("/*/login").permitAll().
                antMatchers("/*/register").permitAll().
                antMatchers("/privacypolicy").permitAll().
                antMatchers("/impressum").permitAll().
                antMatchers("/css/**", "/webjars/**", "/img/**", "/bootstrap_select/**").permitAll().
                antMatchers("/**").authenticated().and().httpBasic();
    }

    /**
     * Creates our CustomAuthenticationFilter so that it can be registered in the #configure
     * (HttpSecurity) method. This allows us to use more fields than just username and password
     * during authentication
     *
     * @return A fully functional CustomAuthenticationFilter
     *
     * @throws Exception getting the authenticationmanager might result in an error
     */
    private CustomAuthenticationFilter authenticationFilter()
            throws Exception {

        CustomAuthenticationFilter filter = new CustomAuthenticationFilter();
        filter.setAuthenticationManager(authenticationManagerBean());
        filter.setAuthenticationFailureHandler(failureHandler());
        return filter;
    }

    /**
     * Used to register our AuthenticationFailureHandler with the framework
     *
     * @return A handler that returns users to /{organization}/login?error if authentication fails
     */
    private AuthenticationFailureHandler failureHandler() {

        return new CustomAuthenticationFailureHandler();
    }

    /**
     * @return The authenticationManagerBean of the application
     *
     * @throws Exception as the super method declares
     */
    @Bean
    @Override
    public AuthenticationManager authenticationManagerBean()
            throws Exception {

        return super.authenticationManagerBean();
    }

    /**
     * Used to register our logoutSuccessHandler with the framework
     *
     * @return
     */
    private LogoutSuccessHandler logoutSuccessHandler() {

        return new CustomLogoutHandler();
    }

    /**
     * Used to register our authenticationSuccessHandler with the framework
     * @return
     */
    private AuthenticationSuccessHandler successHandler() {

        return new CustomAuthenticationSuccessHandler();
    }
}