package org.jacktheclipper.frontend.model;

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
    private String adminMail;

    /* Used by Jackson for JSON deserialization*/
    public OrganizationalUnit() {

    }

    public OrganizationalUnit(UUID id, String name, UUID parentId, String adminMail) {

        this.id = id;
        this.name = name;
        this.parentId = parentId;
        this.adminMail = adminMail;
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

    public String getAdminMail() {

        return adminMail;
    }

    @JsonProperty("AdminMailAddress")
    public void setAdminMail(String adminMail) {

        this.adminMail = adminMail;
    }
}
