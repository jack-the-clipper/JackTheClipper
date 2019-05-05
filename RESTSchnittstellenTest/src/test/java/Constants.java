import enums.ContentType;
import io.restassured.RestAssured;
import org.junit.Assert;

import java.io.FileInputStream;
import java.io.InputStream;
import java.util.Arrays;
import java.util.Collections;
import java.util.Properties;
import java.util.UUID;

public class Constants {

    public static UUID sysAdminId;
    public static UUID unitId;
    public static User registeredUser;

    public static Source existingSource = new Source(UUID.fromString("72dd0f07-6dc2-11e9-8c47" +
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
            RestAssured.baseURI = prop.getProperty("base.url");
            sysAdminId = UUID.fromString(prop.getProperty("sysadmin.uuid"));
            unitId = UUID.fromString(prop.getProperty("unit.uuid"));
        } catch (Exception ex) {
            ex.printStackTrace();
            Assert.fail("Could not load properties, cannot do tests");
        }
    }
}