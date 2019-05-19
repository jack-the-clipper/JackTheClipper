package org.jacktheclipper.frontend;

import org.jacktheclipper.frontend.authentication.AuthenticationSuite;
import org.jacktheclipper.frontend.controller.ControllerSuite;
import org.jacktheclipper.frontend.enums.EnumsSuite;
import org.jacktheclipper.frontend.utils.UtilsSuite;
import org.junit.runner.RunWith;
import org.junit.runners.Suite;

@RunWith(Suite.class)
@Suite.SuiteClasses({EnumsSuite.class, UtilsSuite.class, FrontendApplicationTests.class,
        AuthenticationSuite.class, ControllerSuite.class})
public class JTCSuite {
}
