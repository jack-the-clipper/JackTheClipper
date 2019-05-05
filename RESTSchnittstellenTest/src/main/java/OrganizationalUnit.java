import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.annotation.JsonProperty;

import java.util.UUID;

/**
 * Represents an organizational unit. This might be something like BMW or Audi/marketing. It is
 * used to grant freshly created user's default settings
 */
public class OrganizationalUnit {
    @JsonInclude(JsonInclude.Include.NON_NULL)
    private UUID id;
    private String name;
    private UUID parentId;

    /* Used by Jackson for JSON deserialization*/
    public OrganizationalUnit() {

    }

    public OrganizationalUnit(UUID id, String name, UUID parentId) {

        this.id = id;
        this.name = name;
        this.parentId = parentId;
    }

    public UUID getId() {

        return id;
    }

    @JsonProperty("OrganizationalUnitId")
    public void setId(UUID id) {

        this.id = id;
    }

    public String getName() {

        return name;
    }

    @JsonProperty("OrganizationalUnitName")
    public void setName(String name) {

        this.name = name;
    }

    public UUID getParentId() {

        return parentId;
    }

    @JsonProperty("OrganizationalUnitParent")
    public void setParentId(UUID parentId) {

        this.parentId = parentId;
    }
}
