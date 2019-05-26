import org.junit.BeforeClass;
import org.junit.runner.RunWith;
import org.junit.runners.Suite;

@RunWith(Suite.class)
@Suite.SuiteClasses({RegistrationAndLoginTest.class, UserSettingsTest.class})
public class TestSuite {
    @BeforeClass
    public static void setUp()
    {
        Constants.init();
    }
}
