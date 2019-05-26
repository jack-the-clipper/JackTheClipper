import enums.NotificationSetting;
import enums.SuccessState;
import enums.UserRole;
import io.restassured.http.ContentType;
import io.restassured.response.Response;
import org.junit.BeforeClass;
import org.junit.FixMethodOrder;
import org.junit.Test;
import org.junit.runners.MethodSorters;

import java.util.*;

import static io.restassured.RestAssured.given;
import static org.junit.Assert.*;


/**
 * The tested methods are dependant on a specific order. That's why the execution order is fixed
 * via (@code @FixMethodOrder) and every test method has one letter prepended to enforce this
 * correct lexicographic ordering.
 */
@FixMethodOrder(MethodSorters.NAME_ASCENDING)
public class RegistrationAndLoginTest {
    @Test
    public void aGetStatusOfService() {

        Response response = given().relaxedHTTPSValidation().when().get("/clipper/status");

        assertEquals(200, response.statusCode());

        MethodResult actual = response.as(MethodResult.class);
        MethodResult expected = new MethodResult(SuccessState.Successful, "Version: ");
        assertEquals(expected.getState(), actual.getState());
        assertTrue(actual.getMessage().startsWith(expected.getMessage()));
    }

    @Test
    public void bGetOusOfService() {

        Response response = given().relaxedHTTPSValidation().when().get("/clipper" +
                "/getorganizationalunits?userId=" + Constants.sysAdminId.toString());
        assertEquals(200, response.statusCode());
        var json = response.jsonPath();
        assertTrue(json.prettify().length() > 1000);
    }

    @Test
    public void cPutRegistration() {
        var t = given().relaxedHTTPSValidation().when().get("/clipper/" +
                "login?userMailOrName=timroethel@web.de&userPassword=Passwort&principalUnit="
                +Constants.unitId).jsonPath().get("UserId");
        var millis = System.currentTimeMillis();
        String mail = millis + "@example.com";
        System.out.println("The mail of the randomly added user is [" + mail + "]");
        User toRegister = new User(UUID.randomUUID(), UserRole.User, millis+"", mail, "secure", "TimTest", false, false, null, Constants.unitId);
        String url = "/clipper/register?password=secure&selectedUnit=" + Constants.unitId.toString();
        Response response = given().relaxedHTTPSValidation().body(toRegister).contentType(ContentType.JSON).when().put(url);

        assertEquals(200, response.statusCode());

        var g = given().relaxedHTTPSValidation().get("/clipper/getmanageableusers?userId="+t.toString());

        var users = g.as(User[].class);

        var generated = Arrays.stream(users).filter(e -> e.getName().equalsIgnoreCase(String.valueOf(millis))).findFirst().get();

        generated.setUnlocked(true);
        generated.setPrincipalUnitId(Constants.unitId);
        generated.setUserRole(UserRole.User);
        generated.seteMail(mail);
        generated.setOrganization("TimTes");

        var modify = given().relaxedHTTPSValidation().body(
                new UserUuidsTuple(generated, Collections.singletonList(Constants.unitId))).contentType(ContentType.JSON)
                .when().put("/clipper/modifyuser?staffChiefId="+t);

        assertEquals(200, modify.getStatusCode());

        var delete = given().relaxedHTTPSValidation().when().delete("/clipper/deleteuser?staffChiefId="+ t + "&userId="+ generated.getUserId());

        var result = delete.as(MethodResult.class);

        assertEquals(SuccessState.Successful, result.getState());
    }

    @Test
    public void dGetAuthentication() {

        String uri = "/clipper/login?userMailOrName=" + Constants.registeredUser.geteMail() +
                "&userPassword=" + Constants.registeredUser.getPassword() +
                "&principalUnit="+ Constants.unitId;
        Response response = given().relaxedHTTPSValidation().when().get(uri);
        assertEquals(200, response.getStatusCode());
        User actual = response.as(User.class);
        assertEquals(Constants.registeredUser.getUserId(), actual.getUserId());
        assertEquals(Constants.registeredUser.getUserRole(), actual.getUserRole());
        assertEquals(Constants.registeredUser.getName(), actual.getName());
    }

    @Test
    public void eFailAuthenticationWrongMail() {

        String uri = "/clipper/login?userMailOrName=" + Constants.registeredUser.geteMail() + "haha" +
                "&userPassword=" + Constants.registeredUser.getPassword() +
                "&principalUnit="+ Constants.unitId;
        Response response = given().relaxedHTTPSValidation().when().get(uri);
        assertEquals(400, response.getStatusCode());
    }

    @Test
    public void fFailAuthenticationWrongPassword() {

        String uri = "/clipper/login?userMailOrName=" + Constants.registeredUser.geteMail() +
                "&userPassword=12" + Constants.registeredUser.getPassword() +
                "&principalUnit="+ Constants.unitId;
        Response response = given().relaxedHTTPSValidation().when().get(uri);
        assertEquals(400, response.getStatusCode());
    }
}
