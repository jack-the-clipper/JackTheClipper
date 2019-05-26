import com.fasterxml.jackson.annotation.JsonProperty;
import enums.UserRole;

import java.util.List;
import java.util.UUID;

/**
 * An extension to {@link User}. Normally for this application it is irrelevant to which
 * {@link OrganizationalUnit} a user belongs to. The only {@link OrganizationalUnit} the
 * {@link User} cares about is his principal unit. Since a user can belong to multiple subunits
 * of his principal unit this class is needed to represent this additional information so that an
 * {@link UserRole#StaffChief} can manage it.
 */
public class ExtendedUser extends User {

    @JsonProperty("UserOrganizationalUnits")
    private List<OrganizationalUnit> units;

    public ExtendedUser() {

    }

    public ExtendedUser(UUID userId, UserRole userRole, String name, String eMail,
                        String password, String organization, boolean mustChangePassword,
                        boolean unlocked, UUID settingsId, UUID principalUnitId,
                        List<OrganizationalUnit> units) {

        super(userId, userRole, name, eMail, password, organization, mustChangePassword, unlocked
                , settingsId, principalUnitId);
        this.units = units;
    }

    public List<OrganizationalUnit> getUnits() {

        return units;
    }

    public void setUnits(List<OrganizationalUnit> units) {

        this.units = units;
    }
}
