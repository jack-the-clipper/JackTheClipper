package org.jacktheclipper.frontend.configuration;

import org.jacktheclipper.frontend.authentication.*;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.scheduling.annotation.EnableScheduling;
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
 */
@Configuration
@EnableWebSecurity
@EnableScheduling
@EnableGlobalMethodSecurity(prePostEnabled = true)
public class WebSecurityConfig extends WebSecurityConfigurerAdapter {

    private final BackendAuthenticationProvider authProvider;

    @Autowired
    public WebSecurityConfig(BackendAuthenticationProvider authProvider) {

        this.authProvider = authProvider;
    }

    /**
     * Registers the {@link BackendAuthenticationProvider} with the Spring framework
     *
     * @param auth
     */
    @Override
    public void configure(AuthenticationManagerBuilder auth) {

        auth.authenticationProvider(authProvider);
    }

    /**
     * Configures which sites can be accessed with and without authentication and other
     * configuration concerning security. This is the actual meat of the class
     *
     * @param http the filterchain to configure
     * @throws Exception as the super method declares so
     */
    @Override
    public void configure(HttpSecurity http)
            throws Exception {

        http.addFilterBefore(authenticationFilter(), UsernamePasswordAuthenticationFilter.class).
                antMatcher("/**").csrf().disable().
                formLogin().loginPage("/login").successHandler(successHandler()).permitAll().
                and().exceptionHandling().accessDeniedPage("/403").and().
                logout().logoutSuccessHandler(logoutSuccessHandler()).permitAll().and().
                authorizeRequests().
                //users can only access login pages for existing organizations
                        antMatchers("/{organization}/login").access("@organizationGuard" +
                ".isValidOrganization(#organization)").

                //anybody can see the error page
                        antMatchers("/error").permitAll().

                //users can only reset passwords at sites with an existing organization
                        antMatchers("/{organization}/resetpassword").
                        access("@organizationGuard.isValidOrganization(#organization)").

                //users can only register to existing organizations
                        antMatchers("/{organization}/register").
                        access("@organizationGuard.isValidOrganization(#organization)").

                //users can only see the privacy policy under existing organizations. They do not
                // need to be authenticated
                        antMatchers("/{organization}/privacypolicy").
                        access("@organizationGuard.isValidOrganization(#organization)").

                //users can only see the impressum under existing organizations. They do not
                // need to be authenticated
                        antMatchers("/{organization}/impressum").
                        access("@organizationGuard.isValidOrganization(#organization)").

                //anybody can load our css, js, etc. used for displaying sites
                        antMatchers("/css/**", "/webjars/**",
                        "/img/**", "/bootstrap_select/**").permitAll().

                //only systemadministrators can access the admin portion of this application
                        antMatchers("/admin/**").hasRole("SYSADMIN").

                //every user can access the profile page under his organization
                        antMatchers("/{organization}/feed/profile").
                        access("authenticated and " +
                        "@organizationGuard.isOwnOrganization(authentication,#organization)").


                //every authenticated user can update his password when doing that under his
                // organization
                        antMatchers("/{organization}/feed/changepassword").
                        access("authenticated and " +
                        "@organizationGuard.isOwnOrganization(authentication,#organization)").

                //every authenticated user can update his mail address when doing that under his
                // organization
                        antMatchers("/{organization}/feed/changemailaddress").
                        access("authenticated " +
                        "and @organizationGuard.isOwnOrganization(authentication,#organization)").

                //users can only access the rest of the application if they are authenticated,
                // in the same organization and do not need to change their password
                        antMatchers("/{organization}/**").access("authenticated " +
                        "and @organizationGuard.isOwnOrganization(authentication,#organization) " +
                        "and @organizationGuard.passwordOkay(authentication)").

                //nothing else can be accessed with only one path element
                        antMatchers("/*").denyAll().
                and().httpBasic();
    }

    /**
     * Creates our {@link CustomAuthenticationFilter} so that it can be registered in
     * {@link #configure(HttpSecurity)}. This allows us to use more fields than just username and
     * password during authentication
     *
     * @return A fully functional {@link CustomAuthenticationFilter}
     *
     * @throws Exception getting the AuthenticationManager might result in an error
     */
    private CustomAuthenticationFilter authenticationFilter()
            throws Exception {

        CustomAuthenticationFilter filter = new CustomAuthenticationFilter();
        filter.setAuthenticationManager(authenticationManagerBean());
        filter.setAuthenticationSuccessHandler(successHandler());
        filter.setAuthenticationFailureHandler(failureHandler());
        return filter;
    }

    /**
     * Used to register our {@link CustomAuthenticationFailureHandler} with the framework
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
     * Used to register our {@link CustomLogoutHandler} with the framework
     *
     * @return
     */
    private LogoutSuccessHandler logoutSuccessHandler() {

        return new CustomLogoutHandler();
    }

    /**
     * Used to register our {@link CustomAuthenticationSuccessHandler} with the framework
     *
     * @return
     */
    private AuthenticationSuccessHandler successHandler() {

        return new CustomAuthenticationSuccessHandler();
    }
}