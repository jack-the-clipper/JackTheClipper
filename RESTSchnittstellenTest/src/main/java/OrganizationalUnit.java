import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.List;
import java.util.UUID;

/**
 * Represents an organizational unit. This might be something like BMW or Audi/marketing. It is
 * used to grant freshly created user's default settings
 */
public class OrganizationalUnit {

    @JsonInclude(JsonInclude.Include.NON_NULL)
    @JsonProperty("OrganizationalUnitId")
    private UUID id;
    @JsonProperty("OrganizationalUnitName")
    private String name;
    @JsonProperty("ParentId")
    private UUID parentId;
    @JsonProperty("AdminMailAddress")
    private String adminMail;
    @JsonProperty("IsPrincipalUnit")
    private boolean principalUnit;
    @JsonInclude(JsonInclude.Include.NON_NULL)
    @JsonProperty("SettingsId")
    private UUID settingsId;
    @JsonProperty("Children")
    private List<OrganizationalUnit> children;

    /* Used by Jackson for JSON deserialization*/
    public OrganizationalUnit() {

    }

    public OrganizationalUnit(UUID id, String name, UUID parentId, String adminMail,
                              boolean principalUnit, UUID settingsId) {

        this.id = id;
        this.name = name;
        this.parentId = parentId;
        this.adminMail = adminMail;
        this.principalUnit = principalUnit;
        this.settingsId = settingsId;
    }

    @Override
    public boolean equals(Object other) {

        if (other instanceof OrganizationalUnit && this.id != null) {
            return this.id.equals(((OrganizationalUnit) other).getId());
        }
        return false;
    }

    public UUID getId() {

        return id;
    }

    public void setId(UUID id) {

        this.id = id;
    }

    public String getName() {

        return name;
    }

    public void setName(String name) {

        this.name = name;
    }

    public UUID getParentId() {

        return parentId;
    }

    public void setParentId(UUID parentId) {

        this.parentId = parentId;
    }

    public String getAdminMail() {

        return adminMail;
    }

    public void setAdminMail(String adminMail) {

        this.adminMail = adminMail;
    }

    public boolean isPrincipalUnit() {

        return principalUnit;
    }

    public void setPrincipalUnit(boolean principalUnit) {

        this.principalUnit = principalUnit;
    }

    public UUID getSettingsId() {

        return settingsId;
    }

    public void setSettingsId(UUID settingsId) {

        this.settingsId = settingsId;
    }

    public List<OrganizationalUnit> getChildren() {

        return children;
    }

    public void setChildren(List<OrganizationalUnit> children) {

        this.children = children;
    }
}
