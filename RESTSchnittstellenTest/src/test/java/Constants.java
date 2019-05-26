import enums.ContentType;
import enums.NotificationSetting;
import enums.UserRole;
import io.restassured.RestAssured;
import org.junit.Assert;

import java.io.FileInputStream;
import java.io.InputStream;
import java.util.*;

public class Constants {

    public static UUID sysAdminId;
    public static UUID unitId;
    public static User registeredUser;
    public static UUID articleId;
    public static UUID resetId;

    public static Source existingSource = new Source(UUID.fromString("e6b4a44f-682a-11e9-8c47" +
            "-9615dc5f263c"), "http://feeds.feedburner.com/blogspot/rkEL", "Postillon",
            ContentType.Rss, null, null, null);
    public static UserSettings settings;
    public static Feed defaultFeed = new Feed(null, Collections.singletonList(existingSource),
            new Filter(null, Arrays.asList("der", "die", "das", "einer", "eine"),
                    Collections.EMPTY_LIST, Collections.emptyList()), "RESTASSUREDFEED");

    public static void init() {

        try (InputStream is = new FileInputStream("application.properties")) {
            Properties prop = new Properties();
            prop.load(is);
            RestAssured.baseURI = prop.getProperty("baseDebug.url");
            sysAdminId = UUID.fromString(prop.getProperty("sysadminDebug.uuid"));
            unitId = UUID.fromString(prop.getProperty("testunit.uuid"));
            registeredUser = new User(UUID.fromString("58626322-7cae-11e9-910b-9615dc5f263c"), UserRole.User,
                    "sticki", "timmaster121@web.de", "Passwort","TimTest",
                    false, true, UUID.fromString("58626256-7cae-11e9-910b-9615dc5f263c"), unitId);
            resetId = UUID.fromString("38dfe95b-7cae-11e9-910b-9615dc5f263c");
        } catch (Exception ex) {
            ex.printStackTrace();
            Assert.fail("Could not load properties, cannot do tests");
        }
    }
}
