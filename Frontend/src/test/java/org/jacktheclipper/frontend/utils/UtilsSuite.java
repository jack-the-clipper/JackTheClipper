package org.jacktheclipper.frontend.utils;

import org.junit.runner.RunWith;
import org.junit.runners.Suite;

@RunWith(Suite.class)
@Suite.SuiteClasses({ResponseEntityUtilsTest.class, RestTemplateUtilsTest.class,
        AuthenticationUtilsTest.class, RedirectAttributeUtilsTest.class})
public class UtilsSuite {
}
