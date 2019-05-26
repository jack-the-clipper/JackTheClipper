import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import enums.ContentType;
import enums.NotificationSetting;
import enums.SuccessState;
import io.restassured.response.Response;
import io.restassured.specification.RequestSpecification;
import org.junit.Assert;
import org.junit.BeforeClass;
import org.junit.FixMethodOrder;
import org.junit.Test;
import org.junit.runners.MethodSorters;

import java.util.*;

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
                NotificationSetting.None, 637699949, 99);

        assertNotNull(actual);
        assertNotNull(actual.getFeeds());
        //assertEquals(expected.getFeeds(), actual.getFeeds());
        //possible since the notificationSettings are default values the user can only change at
        //with another update
        assertEquals(expected.getNotificationCheckInterval(),
                actual.getNotificationCheckInterval());
        assertEquals(expected.getNotificationSetting(), actual.getNotificationSetting());
        assertEquals(expected.getArticlesPerPage(),actual.getArticlesPerPage());
    }

    @Test
    public void bGetAvailableSources() {

        Response response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/availablesources?userId=" + Constants.sysAdminId.toString());
        assertEquals(200, response.getStatusCode());
        Source[] actual = response.as(Source[].class);
        Source[] expected = {Constants.existingSource};
        assertNotNull(actual);
        assertTrue(actual.length > 0);
    }

    @Test
    public void cPutUserSettings() {
        Response response =
                given().relaxedHTTPSValidation()
                        .when().put("/clipper/saveusersettings?settingsId=" +
                        Constants.registeredUser.getSettingsId()
                        + "&notificationCheckInterval=637699949&notificationSetting="+ NotificationSetting.None+ "&articlesPerPage=99"
                );
        assertEquals(200, response.getStatusCode());
    }

    @Test
    public void eFeedManagement() {

        Response response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/getfeeddefinitions?userId=" + Constants.registeredUser.getUserId());
        assertEquals(200, response.getStatusCode());
        Feed[] actual = response.as(Feed[].class);
        var feed = Arrays.stream(actual).filter(e -> e.getName().equalsIgnoreCase("Test1")).findFirst().get();
        assertNotNull(feed);

        var uuidOfFeed = UUID.randomUUID();
        var toAdd = new Feed(uuidOfFeed, feed.getFeedSources(),
                new Filter(UUID.randomUUID(), Collections.singletonList("Blub"), new ArrayList <>(), new ArrayList <>()),
                "FeedManagementTest");

        var add = given().relaxedHTTPSValidation().body(toAdd).contentType(io.restassured.http.ContentType.JSON)
                .when().put("/clipper/addfeed?settingsId="+Constants.registeredUser.getSettingsId());

        assertEquals(200, add.statusCode());

        response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/getfeeddefinitions?userId=" + Constants.registeredUser.getUserId());
        assertEquals(200, response.getStatusCode());
        actual = response.as(Feed[].class);
        feed = Arrays.stream(actual).filter(e -> e.getName().equalsIgnoreCase("FeedManagementTest")).findFirst().get();
        assertNotNull(feed);

        compareFeeds(toAdd, feed);

        var newFeed = new Feed(feed.getId(), feed.getFeedSources(),
                new Filter(UUID.randomUUID(), Collections.singletonList("TOK"), new ArrayList <>(), new ArrayList <>()), "Feed");

        var modify = given().relaxedHTTPSValidation().body(newFeed).contentType(io.restassured.http.ContentType.JSON)
                .when().put("/clipper/modifyfeed?settingsId="+Constants.registeredUser.getSettingsId());

        assertEquals(200, modify.statusCode());

        response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/getfeeddefinitions?userId=" + Constants.registeredUser.getUserId());
        assertEquals(200, response.getStatusCode());
        actual = response.as(Feed[].class);
        feed = Arrays.stream(actual).filter(e -> e.getName().equalsIgnoreCase("Feed")).findFirst().get();
        assertNotNull(feed);

        var delete = given().relaxedHTTPSValidation().when().delete("/clipper/deletefeed?feedId="+feed.getId());

        assertEquals(200, delete.statusCode());

        response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/getfeeddefinitions?userId=" + Constants.registeredUser.getUserId());
        assertEquals(200, response.getStatusCode());
        actual = response.as(Feed[].class);

        assertEquals(1 , actual.length);
    }


    private Source source = new Source(UUID.randomUUID(), "http://schlechte.de", "RestTest", ContentType.WebSite, "", Collections.EMPTY_LIST, "");
    @Test
    public void fAddSource() throws JsonProcessingException {
        ObjectMapper om = new ObjectMapper();
        String tmp = om.writeValueAsString(source);
        Response response = given().contentType(io.restassured.http.ContentType.JSON).body(tmp).relaxedHTTPSValidation()
                .when().put("/clipper/addsource?userId=" + Constants.sysAdminId.toString());

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
                "/reset?userMail=reset@reset.com");

        assertEquals(200, response.getStatusCode());
    }
    @Test
    public void jChangePassword() {
        Response response = given().relaxedHTTPSValidation().when().put("/clipper" +
                "/changepassword?userId=" + Constants.resetId + "&newPassword=hallo123");

        assertEquals(200, response.getStatusCode());
    }
    @Test
    public void kChangeMailAddress() {
        Response response = given().relaxedHTTPSValidation().when().put("/clipper" +
                "/changemailaddress?userId=" + Constants.resetId + "&newUserMail=jackxiss@example.com");

        assertEquals(200, response.getStatusCode());

        given().relaxedHTTPSValidation().when().put("/clipper" +
                "/changemailaddress?userId=" + Constants.resetId + "&newUserMail=reset@reset.com");
    }

    @Test
    public void lGetFeed() {
        Response response = given().relaxedHTTPSValidation().when().get("/clipper/getusersettings"
                + "?userId=" + Constants.registeredUser.getUserId());
        assertEquals(200, response.getStatusCode());
        UserSettings actual = response.as(UserSettings.class);
        UUID feedId = actual.getFeeds().get(0).getId();
        response = given().relaxedHTTPSValidation().when().get("/clipper/getfeed?userId=" + Constants.registeredUser.getUserId()
                                                                                         + "&feedId=" + feedId
                                                                                         + "&page=0"
                                                                                         + "&showArchived=true");
        assertEquals(200, response.statusCode());
        Constants.articleId = (UUID) ((HashMap)((((ArrayList)(response.jsonPath().get())).get(0)))).get(6);
        ArrayList x = response.jsonPath().get();
        HashMap t = (HashMap) x.get(0);
        UUID f = UUID.fromString((String) t.get("ArticleId"));

        Constants.articleId = f;
    }

    @Test
    public void mGetArticle() {
        Response response = given().relaxedHTTPSValidation().when().get("/clipper/getArticle?userId=" +
                Constants.registeredUser.getUserId() + "&articleId="+Constants.articleId);
        assertEquals(200, response.statusCode());
        assertNotNull(response.jsonPath().get("ArticleLongText"));
    }

    @Test
    public void mGetOrganizationalUnitSettings() {
        Response response = given().relaxedHTTPSValidation().when().get("/clipper/getorganizationalunitsettings" +
                "?userId=6d64acf7-7cad-11e9-910b-9615dc5f263c"+
                "&unitId=" + Constants.unitId.toString());
        assertEquals(200, response.statusCode());
        assertNotNull(response.as(OrganizationalUnitSettings.class));
    }

    @Test
    public void nUnitMangementTest()
    {
        var response = given().relaxedHTTPSValidation().when().put("/clipper/getprincipalunits" +
                "?userId="+Constants.sysAdminId);

        assertEquals(200, response.statusCode());

        var principals = response.as(OrganizationalUnit[].class);

        var millis = System.currentTimeMillis();
        var addunit = given().relaxedHTTPSValidation().when().put("/clipper/addprincipalunit?userId="+Constants.sysAdminId+
                "&name="+millis+ "&principalUnitMail="+millis+"@example.com");

        assertEquals(200, addunit.statusCode());

        response = given().relaxedHTTPSValidation().when().put("/clipper/getprincipalunits?userId="+Constants.sysAdminId).then().statusCode(200).extract().response();

        var principalsnew = response.as(OrganizationalUnit[].class);

        assertEquals(principals.length +1, principalsnew.length);

        var idtoDelete = Arrays.stream(principalsnew)
                .filter(e->e.getName().equalsIgnoreCase(String.valueOf(millis))).findFirst().get().getId();
        var delete = given().relaxedHTTPSValidation().when().delete("/clipper/deleteorganizationalunit?userId="
                +Constants.sysAdminId+ "&unitId="+idtoDelete);

        assertEquals(200, delete.statusCode());
    }

    @Test
    public void oGetChildrenTest()
    {
        var response = given().relaxedHTTPSValidation().when().get("/clipper/getprincipalunitchildren?principalUnitId="+Constants.unitId);

        assertEquals(200, response.statusCode());

        var children = response.as(UuidStringTuple[].class);

        assertEquals(2, children.length);
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
