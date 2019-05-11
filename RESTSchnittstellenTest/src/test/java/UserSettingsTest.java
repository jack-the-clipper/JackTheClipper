import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import enums.ContentType;
import enums.NotificationSetting;
import enums.SuccessState;
import io.restassured.response.Response;
import io.restassured.specification.RequestSpecification;
import org.junit.Assert;
import org.junit.FixMethodOrder;
import org.junit.Test;
import org.junit.runners.MethodSorters;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.UUID;

import static io.restassured.RestAssured.given;
import static org.junit.Assert.*;

@FixMethodOrder(MethodSorters.NAME_ASCENDING)
public class UserSettingsTest {

    @Test
    public void aGetUserSettings() {

        Response response = given().relaxedHTTPSValidation().when().get("/clipper/getusersettings"
                + "?userId=" + Constants.registeredUser.getUserId());
        assertEquals(200, response.getStatusCode());
        UserSettings actual = response.as(UserSettings.class);
        UserSettings expected = new UserSettings(null, Collections.EMPTY_LIST,
                NotificationSetting.None, 60, 20);

        assertNotNull(actual);
        assertNotNull(actual.getFeeds());
        //assertEquals(expected.getFeeds(), actual.getFeeds());
        //possible since the notificationSettings are default values the user can only change at
        //with another update
        assertEquals(expected.getNotificationCheckInterval(),
                actual.getNotificationCheckInterval());
        assertEquals(expected.getNotificationSetting(), actual.getNotificationSetting());
        assertEquals(expected.getArticlesPerPage(),actual.getArticlesPerPage());
        Constants.settings = actual; //need to cache usersettings so we can use them later on
    }

    @Test
    public void bGetAvailableSources() {

        Response response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/availablesources?userId=" + Constants.sysAdminId.toString());
        assertEquals(200, response.getStatusCode());
        Source[] actual = response.as(Source[].class);
        Source[] expected = {Constants.existingSource};
        assertNotNull(actual);
        assertTrue(actual.length > 0); //currently we have sources in the system
        /*assertEquals(expected.length, actual.length);

        for (int i = 0; i < actual.length; i++) {
            compareSources(expected[i], actual[i]);
        }*/
    }

    @Test
    public void cPutUserSettings() {

        Constants.settings.getFeeds().add(Constants.defaultFeed);
        Response response =
                // @formatter:off
                given().contentType("application/json").body(Constants.settings).relaxedHTTPSValidation()
                        .when().put("/clipper/saveusersettings?userId=" +
                        Constants.registeredUser.getUserId());
                // @formatter:on
        assertEquals(200, response.getStatusCode());
        MethodResult actual = response.as(MethodResult.class);
        MethodResult expected = new MethodResult(SuccessState.Successful, null);
        assertEquals(expected.getState(), actual.getState());
        assertEquals(expected.getMessage(), actual.getMessage());
    }

    @Test
    public void dCheckUpdatedSettings() {

        Response response = given().relaxedHTTPSValidation().when().get("/clipper/getusersettings"
                + "?userId=" + Constants.registeredUser.getUserId());
        assertEquals(200, response.getStatusCode());
        UserSettings actual = response.as(UserSettings.class);
        //compareUserSettings(Constants.settings, actual);
        assertNotNull(actual);
        assertTrue(actual.getFeeds().stream()
                // @formatter:off
                .anyMatch(feed -> feed.getFeedSources().equals(Constants.defaultFeed.getFeedSources())
                        && feed.getFilter().idIgnoringCompare(Constants.defaultFeed.getFilter())
                        && feed.getName().equals(Constants.defaultFeed.getName())));
                // @formatter:on
    }

    @Test
    public void eGetFeeds() {

        Response response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/getfeeddefinitions?userId=" + Constants.registeredUser.getUserId());
        assertEquals(200, response.getStatusCode());
        Feed[] actual = response.as(Feed[].class);
        Feed[] expected = {Constants.defaultFeed};
        //works since the default user does not have
        //any feeds besides the one added during the tests
        assertEquals(expected.length, actual.length);
        for (int i = 0; i < expected.length; i++) {
            compareFeeds(expected[i], actual[i]);
        }
    }


    private Source source = new Source(UUID.randomUUID(), "http://schlechte.de", "RestTest", ContentType.WebSite, "", Collections.EMPTY_LIST, "");
    @Test
    public void fAddSource() throws JsonProcessingException {
        ObjectMapper om = new ObjectMapper();
        String tmp = om.writeValueAsString(source);
        Response response = given().contentType(io.restassured.http.ContentType.JSON).body(tmp).relaxedHTTPSValidation()
                .when().put("/clipper" + "/addsource?userId=" + Constants.sysAdminId.toString());

        assertEquals(200, response.getStatusCode());
        MethodResult actual = response.as(MethodResult.class);
        MethodResult expected = new MethodResult(SuccessState.Successful, null);
        assertEquals(expected.getState(), actual.getState());
        assertEquals(expected.getMessage(), actual.getMessage());
    }


    @Test
    public void gChangeSource() throws JsonProcessingException {
        Response response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/availablesources?userId=" + Constants.sysAdminId.toString());
        assertEquals(200, response.getStatusCode());
        Source[] actual = response.as(Source[].class);

        var t = Arrays.stream(actual).filter(e -> e.getName().equalsIgnoreCase("RestTest")).findFirst();
        var x = t.get();
        Source changed = source;
        changed.setName("ChangeTest");
        changed.setId(x.getId());

       // ObjectMapper om = new ObjectMapper();
        //String tmp = om.writeValueAsString(source);

        response = given().contentType(io.restassured.http.ContentType.JSON).body(source).relaxedHTTPSValidation().when().put("/clipper" +
                "/changesource?userId=" + Constants.sysAdminId.toString() + "&tochange=" + x.getId().toString());

        assertEquals(200, response.getStatusCode());

        MethodResult actual1 = response.as(MethodResult.class);
        MethodResult expected = new MethodResult(SuccessState.Successful, null);
        assertEquals(expected.getState(), actual1.getState());
        assertEquals(expected.getMessage(), actual1.getMessage());
    }

    @Test
    public void hDeleteSource()
    {
        Response response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/availablesources?userId=" + Constants.sysAdminId.toString());
        assertEquals(200, response.getStatusCode());
        Source[] actual1 = response.as(Source[].class);

        var t = Arrays.stream(actual1).filter(e -> e.getName().equalsIgnoreCase("ChangeTest")).findFirst();
        var x = t.get();

        response = given().relaxedHTTPSValidation().when().delete("/clipper" +
                "/deletesource?userId=" + Constants.sysAdminId.toString() + "&sourceId="+ x.getId().toString());

        assertEquals(200, response.getStatusCode());

        MethodResult actual = response.as(MethodResult.class);
        MethodResult expected = new MethodResult(SuccessState.Successful, null);
        assertEquals(expected.getState(), actual.getState());
        assertEquals(expected.getMessage(), actual.getMessage());
    }

    @Test
    public void iResetPassword() {
        Response response = given().relaxedHTTPSValidation().when().put("/clipper" +
                "/reset?userMail=" + Constants.registeredUser.geteMail());

        assertEquals(200, response.getStatusCode());
    }
    @Test
    public void jChangePassword() {
        Response response = given().relaxedHTTPSValidation().when().put("/clipper" +
                "/changepassword?userId=" + Constants.registeredUser.getUserId() + "&newPassword=hallo123");

        assertEquals(200, response.getStatusCode());

        given().relaxedHTTPSValidation().when().put("/clipper" +
                "/changepassword?userId=" + Constants.registeredUser.getUserId() + "&newPassword=" + Constants.registeredUser.getPassword());
    }
    @Test
    public void kChangeMailAddress() {
        Response response = given().relaxedHTTPSValidation().when().put("/clipper" +
                "/changemailaddress?userId=" + Constants.registeredUser.getUserId() + "&newUserMail=jackxiss@example.com");

        assertEquals(200, response.getStatusCode());

        given().relaxedHTTPSValidation().when().put("/clipper" +
                "/changemailaddress?userId=" + Constants.registeredUser.getUserId() + "&newUserMail=" + Constants.registeredUser.geteMail());
    }

    @Test
    public void lGetFeed() {
        Response response = given().relaxedHTTPSValidation().when().get("/clipper/getusersettings"
                + "?userId=" + Constants.registeredUser.getUserId());
        assertEquals(200, response.getStatusCode());
        UserSettings actual = response.as(UserSettings.class);
        UUID feedId = actual.getFeeds().get(0).getId();
        response = given().relaxedHTTPSValidation().when().get("/clipper/getfeed?userId=" + Constants.registeredUser.getUserId().toString()
                                                                                         + "&feedId=" + feedId.toString()
                                                                                         + "&page=0"
                                                                                         + "&showArchived=true");
        assertEquals(200, response.statusCode());
        assertNotNull(response.as(Article[].class));
        Constants.articleId = response.as(Article[].class)[0].getId();
    }

    @Test
    public void mGetArticle() {
        Response response = given().relaxedHTTPSValidation().when().get("/clipper/getArticle?userId=" +
                Constants.registeredUser.getUserId().toString() + "&articleId=" + Constants.articleId);
        assertEquals(200, response.statusCode());
        assertNotNull(response.as(Article.class));
    }

    @Test
    public void mGetOrganizationalUnitSettings() {
        Response response = given().relaxedHTTPSValidation().when().get("/clipper/getorganizationalunitsettings" +
                "?userId=" + Constants.registeredUser.getUserId().toString() +
                "&unitId=" + Constants.unitId.toString());
        assertEquals(200, response.statusCode());
        assertNotNull(response.as(OrganizationalUnitSettings.class));
    }

    private void compareSources(Source expected, Source actual) {

        assertEquals(expected.getId(), actual.getId());
        assertEquals(expected.getBlackList(), actual.getBlackList());
        assertEquals(expected.getContentType(), actual.getContentType());
        assertEquals(expected.getName(), actual.getName());
        assertEquals(expected.getRegEx(), actual.getRegEx());
        assertEquals(expected.getUri(), actual.getUri());
        assertEquals(expected.getxPath(), actual.getxPath());
    }

    private void compareUserSettings(UserSettings expected, UserSettings actual) {

        if (expected == null && actual == null) {
            return;
        }
        if (expected == null || actual == null) {
            Assert.fail("One input parameter in #compareUserSettings was null but not the other");
        }
        assertEquals(expected.getId(), actual.getId());
        assertEquals(expected.getNotificationSetting(), actual.getNotificationSetting());
        assertEquals(expected.getNotificationCheckInterval(),
                actual.getNotificationCheckInterval());
        assertEquals(expected.getFeeds().size(), actual.getFeeds().size());
        for (int i = 0; i < expected.getFeeds().size(); i++) {
            compareFeeds(expected.getFeeds().get(i), actual.getFeeds().get(i));
        }
    }

    private void compareFeeds(Feed expected, Feed actual) {

        if (expected == null && actual == null) {
            return;
        }
        if (expected == null || actual == null) {
            Assert.fail("One input parameter in #compareFeeds was null but not the other");
        }
        assertEquals(expected.getName(), actual.getName());
        assertEquals(expected.getFeedSources().size(), actual.getFeedSources().size());
        for (int i = 0; i < expected.getFeedSources().size(); i++) {
            compareSources(expected.getFeedSources().get(i), actual.getFeedSources().get(i));
        }
        compareFilters(expected.getFilter(), actual.getFilter());
    }

    private void compareFilters(Filter expected, Filter actual) {

        if (expected == null && actual == null) {
            return;
        }
        if (expected == null || actual == null) {
            Assert.fail("One input parameter in #compareFilters was null but not the other");
        }
        assertEquals(expected.getExpressions(), actual.getExpressions());
        assertEquals(expected.getKeywords(), actual.getKeywords());
        assertEquals(expected.getBlackList(), actual.getBlackList());
    }
}
