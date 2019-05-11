import enums.NotificationSetting;
import enums.SuccessState;
import enums.UserRole;
import io.restassured.response.Response;
import org.junit.BeforeClass;
import org.junit.FixMethodOrder;
import org.junit.Test;
import org.junit.runners.MethodSorters;

import java.util.ArrayList;

import static io.restassured.RestAssured.given;
import static org.junit.Assert.*;


/**
 * The tested methods are dependant on a specific order. That's why the execution order is fixed
 * via (@code @FixMethodOrder) and every test method has one letter prepended to enforce this
 * correct lexicographic ordering.
 */
@FixMethodOrder(MethodSorters.NAME_ASCENDING)
public class RegistrationAndLoginTest {


    @BeforeClass
    public static void setupRestAssured() {

        Constants.init();
    }

    @Test
    public void aGetStatusOfService() {

        Response response = given().relaxedHTTPSValidation().when().get("/clipper/status");

        assertEquals(200, response.statusCode());

        MethodResult actual = response.as(MethodResult.class);
        MethodResult expected = new MethodResult(SuccessState.Successful, "Version: ");
        //TODO will be invalid if version string format is changed. That should not really happen
        // though
        assertEquals(expected.getState(), actual.getState());
        assertTrue(actual.getMessage().startsWith(expected.getMessage()));
    }

    @Test
    public void bGetOusOfService() {

        Response response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/getorganizationalunits?userId=" + Constants.sysAdminId.toString());
        assertEquals(200, response.statusCode());
        OrganizationalUnit[] ous = response.as(OrganizationalUnit[].class);
        assertNotNull(ous);
        assertTrue(ous.length > 0);
        Constants.unitId = ous[0].getId();
    }

    @Test
    public void cPutRegistration() {

        //TODO remove when users can be deleted
        //generate a unique username
        String mail = System.currentTimeMillis() + "@example.com";
        System.out.println("The mail of the randomly added user is [" + mail + "]");
        User toRegister = new User(null, UserRole.User, "restAssured", mail, "secure", "unused",
                false);
        String url = "/clipper/register?userMail=" + mail + "&password=secure" + "&userName" +
                "=restAssured&role=User&unit=" + Constants.unitId.toString();
        Response response = given().relaxedHTTPSValidation().when().put(url);

        assertEquals(200, response.statusCode());

        User actual = response.as(User.class);

        assertEquals(toRegister.getUserRole(), actual.getUserRole());
        assertEquals(toRegister.getName(), actual.getName());

        toRegister.setUserId(actual.getUserId());
        Constants.registeredUser = toRegister;
        Constants.settings = new UserSettings(Constants.registeredUser.getUserId(), new ArrayList<Feed>(),
                NotificationSetting.None, 60, 20);
    }

    @Test
    public void dGetAuthentication() {

        String uri = "/clipper/login?userMail=" + Constants.registeredUser.geteMail() +
                "&userPassword=" + Constants.registeredUser.getPassword();
        Response response = given().relaxedHTTPSValidation().when().get(uri);
        assertEquals(200, response.getStatusCode());
        User actual = response.as(User.class);
        assertEquals(Constants.registeredUser.getUserId(), actual.getUserId());
        assertEquals(Constants.registeredUser.getUserRole(), actual.getUserRole());
        assertEquals(Constants.registeredUser.getName(), actual.getName());
    }

    @Test
    public void eFailAuthenticationWrongMail() {

        String uri = "/clipper/login?userMail=" + Constants.registeredUser.geteMail() + "haha" +
                "&userPassword=" + Constants.registeredUser.getPassword();
        Response response = given().relaxedHTTPSValidation().when().get(uri);
        assertEquals(400, response.getStatusCode());
    }

    @Test
    public void fFailAuthenticationWrongPassword() {

        String uri = "/clipper/login?userMail=" + Constants.registeredUser.geteMail() +
                "&userPassword=12" + Constants.registeredUser.getPassword();
        Response response = given().relaxedHTTPSValidation().when().get(uri);
        assertEquals(400, response.getStatusCode());
    }

    @Test
    public void gFailRegistration() {

        String url = "/clipper/register?userMail=" + Constants.registeredUser.geteMail() +
                "&password=secure" + "&userName" + "=restAssured&role=User&unit=" + Constants.unitId.toString();
        Response response = given().relaxedHTTPSValidation().when().put(url);

        assertEquals(204, response.statusCode());
    }
}
