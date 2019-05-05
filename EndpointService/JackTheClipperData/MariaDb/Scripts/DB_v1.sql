CREATE OR REPLACE DATABASE jacktheclipper;
USE jacktheclipper;

CREATE OR REPLACE TABLE tbUserSettings (
	coId BINARY(16) NOT NULL,
	coNotification BIT(4) NOT NULL,
	coNotificationInterval INT,
	PRIMARY KEY (coId)
)
ENGINE=InnoDB
;


CREATE OR REPLACE TABLE tbOrganizationalUnitSettings (
	coUserSettingsId BINARY(16) NOT NULL,
	coBlacklist TEXT,
	PRIMARY KEY (coUserSettingsId),
	FOREIGN KEY (coUserSettingsId) REFERENCES tbUserSettings(coId)
)
ENGINE=InnoDB
;

CREATE OR REPLACE TABLE tbUser (
	coId BINARY(16) NOT NULL,
	coName VARCHAR(128) NOT NULL,
	coMail VARCHAR(128) NOT NULL,
	coPasswordHash TEXT NOT NULL,
	coRole BIT(4) NOT NULL,
	coSettingsId BINARY(16) NOT NULL,
	PRIMARY KEY (coId),
	UNIQUE INDEX (coMail),
	INDEX (coName)
)
ENGINE=InnoDB
;

CREATE OR REPLACE TABLE tbOrganizationalUnit (
	coId BINARY(16) NOT NULL,
	coName TEXT NOT NULL,
	coParentId BINARY(16) NOT NULL,
	coSettingsId BINARY(16) NOT NULL,
	PRIMARY KEY (coId),
	INDEX coParentIdIdx (coParentId),
	UNIQUE INDEX coSettingsIdIdx (coSettingsId)
)
ENGINE=InnoDB
;

CREATE OR REPLACE TABLE tbUserOrganizationalUnit (
	coUserId BINARY(16) NOT NULL,
	coOrganizationalUnitId BINARY(16) NOT NULL,
	PRIMARY KEY (coUserId, coOrganizationalUnitId),
	INDEX coUserIdx (coUserId)
)
ENGINE=InnoDB
;


CREATE OR REPLACE TABLE tbSource (
	coId BINARY(16) NOT NULL,
	coUrl TEXT NOT NULL,
	coName TEXT NOT NULL,
	coType BIT(4) NOT NULL,
	coRegex TEXT,
	coXpath TEXT,
	coBlackList TEXT,
	PRIMARY KEY (coId)
)
ENGINE=InnoDB
;

CREATE OR REPLACE TABLE tbFeed (
	coId BINARY(16) NOT NULL,
	coName TEXT NOT NULL,
	coFilterId BINARY(16) NOT NULL,
	PRIMARY KEY (coId)
)
ENGINE=InnoDB
;

CREATE OR REPLACE TABLE tbSettingsFeeds (
	coFeedId BINARY(16) NOT NULL,
	coSettingsId BINARY(16) NOT NULL,
	PRIMARY KEY (coFeedId, coSettingsId)
)
ENGINE=InnoDB
;

CREATE OR REPLACE TABLE tbFeedFilter (
	coId BINARY(16) NOT NULL,
	coBlackList TEXT,
	coKeywords TEXT,
	coExpressions TEXT,
	PRIMARY KEY (coId)
)
ENGINE=InnoDB
;

CREATE OR REPLACE TABLE tbFeedSource (
	coFeedId BINARY(16) NOT NULL,
	coSourceId BINARY(16) NOT NULL,
	PRIMARY KEY (coFeedId, coSourceId)
)
ENGINE=InnoDB
;

CREATE OR REPLACE TABLE tbOrganizationalUnitSettingsSource (
	coOrganizationalUnitSettingsId BINARY(16) NOT NULL,
	coSourceId BINARY(16) NOT NULL,
	PRIMARY KEY (coOrganizationalUnitSettingsId, coSourceId)
)
ENGINE=InnoDB
;

CREATE OR REPLACE TABLE tbVersion(
	coVersion INT NOT NULL,
	coUpdateTime DATETIME NOT NULL, 
	PRIMARY KEY (coVersion)
)
ENGINE=InnoDB
;

ALTER TABLE tbUserOrganizationalUnit
ADD CONSTRAINT fk1 FOREIGN KEY (coOrganizationalUnitId) REFERENCES tborganizationalunit(coId) ON DELETE CASCADE,
ADD CONSTRAINT fk2 FOREIGN KEY (coUserId) REFERENCES tbuser(coId) ON DELETE CASCADE; 

ALTER TABLE tborganizationalunit
ADD CONSTRAINT fk3 FOREIGN KEY (coParentId) REFERENCES tborganizationalunit(coId) ON DELETE CASCADE,
ADD CONSTRAINT fk4 FOREIGN KEY (coSettingsId) REFERENCES tbUserSettings(coId) ON DELETE CASCADE;

ALTER TABLE tbOrganizationalUnitSettingsSource
ADD CONSTRAINT fk5 FOREIGN KEY (coOrganizationalUnitSettingsId) REFERENCES tborganizationalunit(coId) ON DELETE CASCADE,
ADD CONSTRAINT fk6 FOREIGN KEY (coSourceId) REFERENCES tbsource(coId) ON DELETE CASCADE; 

ALTER TABLE tbfeedsource
ADD CONSTRAINT fk7 FOREIGN KEY (coFeedId) REFERENCES tbfeed(coId) ON DELETE CASCADE,
ADD CONSTRAINT fk8 FOREIGN KEY (coSourceId) REFERENCES tbsource(coId) ON DELETE CASCADE; 

ALTER TABLE tbSettingsFeeds
ADD CONSTRAINT fk9 FOREIGN KEY (coFeedId) REFERENCES tbfeed(coId) ON DELETE CASCADE,
ADD CONSTRAINT fk10 FOREIGN KEY (coSettingsId) REFERENCES tbusersettings(coId) ON DELETE CASCADE; 

alter table tbfeed
add constraint fk11 foreign key (coFilterId) References tbfeedfilter(coId) ON DELETE CASCADE;

alter table tbuser
add constraint fk12 foreign key (coSettingsId) References tbusersettings(coId) ON DELETE CASCADE;

DELIMITER //  
CREATE OR REPLACE FUNCTION BIN_TO_UUID ( uuid BINARY(16) )  
RETURNS VARCHAR(36) DETERMINISTIC  
BEGIN  
	DECLARE temp VARCHAR(36);
   SET temp = HEX(uuid);
 RETURN LOWER(CONCAT(
    SUBSTR(temp, 1, 8), '-',
    SUBSTR(temp, 9, 4), '-',
    SUBSTR(temp, 13, 4), '-',
    SUBSTR(temp, 17, 4), '-',
    SUBSTR(temp, 21)
));  
END; //  
 
CREATE OR REPLACE FUNCTION UUID_TO_BIN ( uuid BINARY(36) )  
RETURNS BINARY(16) DETERMINISTIC  
BEGIN  
   RETURN UNHEX(REPLACE(uuid, "-",""));  
END; //  

CREATE OR REPLACE PROCEDURE SP_CREATE_USER(
    IN name TEXT,
    IN mail TEXT,
    IN pwHash TEXT,
    IN role BIT(4),
    IN unit VARCHAR(36),
    OUT newUserId VARCHAR(36)
)
BEGIN
DECLARE settingsuuid BINARY(16);
DECLARE tempuuid BINARY(16);
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
    	ROLLBACK;
    END;

    START TRANSACTION;

    SET settingsuuid = UUID_TO_BIN(UUID());
    SET tempuuid = UUID_TO_BIN(UUID());
	Insert into tbusersettings(coId, coNotification) VALUES(settingsuuid, 0);
	Insert into tbUser(coId, coName, coMail, coPasswordHash, coRole, coSettingsId) VALUES(tempuuid, name, mail, pwhash, role, settingsuuid);
    Insert into  tbuserorganizationalunit(coUserId, coOrganizationalUnitId) VALUES(tempuuid, UUID_TO_BIN(unit));
	 SET newUserId = HEX(tempuuid);
	 COMMIT;

END //  
CREATE OR REPLACE PROCEDURE SP_CREATE_UNIT(
    IN name TEXT,
    IN parent VARCHAR(36),
    OUT newUnitId BINARY(16)
)
BEGIN
	 DECLARE settingsuuid BINARY(16);
    DECLARE unituuid BINARY(16);
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
    	ROLLBACK;
    END;
    START TRANSACTION;

	SET settingsuuid = UUID_TO_BIN(uuid());
	
	SET unituuid = UUID_TO_BIN(uuid());
	Insert into tbusersettings(coId, coNotification) VALUES(settingsuuid, 0);
	Insert into tborganizationalunitsettings(coUserSettingsId, coBlacklist) VALUES(settingsuuid, NULL);
	Insert into tborganizationalunit(coId,	coName, coParentId, coSettingsId) VALUES(unituuid, name, UUID_TO_BIN(parent), settingsuuid);
	SET newUnitId = unituuid;
	COMMIT;
END //
DELIMITER ;

INSERT into tbVersion(coVersion, coUpdateTime) VALUES(1, NOW());
Insert into tbusersettings(coId, coNotification) VALUES(UUID_TO_BIN('00000000-BEEF-0000-0000-000000000001'), 0);
Insert into tborganizationalunitsettings(coUserSettingsId, coBlacklist) VALUES(UUID_TO_BIN('00000000-BEEF-0000-0000-000000000001'), NULL);
Insert into tborganizationalunit(coId,	coName, coParentId, coSettingsId) VALUES(UUID_TO_BIN('00000000-BEEF-BEEF-BEEF-000000000000'), 'SYSTEM', UUID_TO_BIN('00000000-BEEF-BEEF-BEEF-000000000000'), UUID_TO_BIN('00000000-BEEF-0000-0000-000000000001'));
call SP_CREATE_USER('SA', 'SA','A665A45920422F9D417E4867EFDC4FB8A04A1F3FFF1FA07E998E86F7F7A27AE3', 15, '00000000-BEEF-BEEF-BEEF-000000000000', @out);
