ALTER TABLE tborganizationalunitsettingssource
DROP FOREIGN KEY fk5;
ALTER TABLE tbOrganizationalUnitSettingsSource
ADD CONSTRAINT fk5 FOREIGN KEY (coOrganizationalUnitSettingsId) REFERENCES tborganizationalunitsettings(coUserSettingsId) ON DELETE CASCADE;

ALTER TABLE tbUser MODIFY coMail VARCHAR(191) NOT NULL;
ALTER TABLE tbuser ADD COLUMN coPrincipalUnit BINARY(16) NOT NULL DEFAULT 0x00000000BEEFBEEFBEEF000000000000;
ALTER TABLE tbuser ALTER coPrincipalUnit DROP DEFAULT;
ALTER TABLE tbuser
ADD CONSTRAINT fk60 FOREIGN KEY (coPrincipalUnit) REFERENCES tborganizationalunit(coId) ON DELETE CASCADE;


ALTER TABLE tborganizationalunit ADD COLUMN coMail VARCHAR(191) NOT NULL;
UPDATE tborganizationalunit SET coMail = 'SA' where coID =  0x00000000BEEFBEEFBEEF000000000000;
ALTER TABLE tborganizationalunit
ADD CONSTRAINT fk61 FOREIGN KEY (coMail) REFERENCES tbUser(coMail)
ON DELETE CASCADE
ON UPDATE CASCADE;


DELIMITER //
CREATE OR REPLACE PROCEDURE SP_CREATE_USER(
    IN name TEXT,
    IN mail VARCHAR(191),
    IN pwHash TEXT,
    IN role BIT(4),
    IN principalUnit VARCHAR(36),
	IN mustChangePassword BOOL,
	IN valid BOOL,
    OUT newUserId VARCHAR(36)
)
BEGIN
	 DECLARE settingsuuid BINARY(16);
	 DECLARE tempuuid BINARY(16);
     START TRANSACTION;
	    IF EXISTS (SELECT coId FROM tbUser 
			 							WHERE tbUser.coName = name 
										AND tbUser.coPrincipalUnit = UUID_TO_BIN(principalUnit))  
		 THEN
		    SIGNAL SQLSTATE 'ERR0R' SET MESSAGE_TEXT = 'Duplicate pricipal unit name';
	    END IF;
	    SET settingsuuid = UUID_TO_BIN(UUID());
	    SET tempuuid = UUID_TO_BIN(UUID());
		 INSERT INTO tbusersettings(coId, coNotification) VALUES(settingsuuid, 0);
		 INSERT INTO tbUser(coId, coName, coMail, coPasswordHash, coRole, coSettingsId, coPrincipalUnit, coMustChangePassword, coValid) 
		 			VALUES(tempuuid, name, mail, pwhash, role, settingsuuid, UUID_TO_BIN(principalUnit), mustChangePassword, valid);
		 SET newUserId = BIN_TO_UUID(tempuuid);
	 COMMIT;

END //  

CREATE OR REPLACE PROCEDURE SP_ADD_USER_UNIT(
	 IN userId VARCHAR(36),
    IN unitId VARCHAR(36)
)
BEGIN
   START TRANSACTION;
		INSERT INTO tbuserorganizationalunit(coUserId, coOrganizationalUnitId) 
			    VALUES (UUID_TO_BIN(userId), UUID_TO_BIN(unitId)); 
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_REMOVE_USER_UNIT(
	 IN userId VARCHAR(36),
    IN unitId VARCHAR(36)
)
BEGIN
   START TRANSACTION;
		DELETE FROM tbuserorganizationalunit 
			    WHERE coUserId = UUID_TO_BIN(userId)
				 AND coOrganizationalUnitId = UUID_TO_BIN(unitId); 
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_CREATE_PRINZIPALUNIT(
    IN name TEXT,
    IN mail VARCHAR(191),
    IN adminPwHash TEXT,
    OUT newPrincipalUnitId VARCHAR(36),
    OUT newUserId VARCHAR(36)
)
BEGIN
	 DECLARE settingsuuid BINARY(16);
	 DECLARE usersettingsuuid BINARY(16);
	 DECLARE tempuuid BINARY(16);
	 DECLARE unituuid BINARY(16);
    START TRANSACTION;
	 	IF EXISTS (SELECT coId FROM tborganizationalunit 
		 								WHERE tborganizationalunit.coName = name 
										AND tborganizationalunit.coParentId = 0x00000000BEEFBEEFBEEF000000000000)  
		THEN
	        SIGNAL SQLSTATE 'ERR0R' SET MESSAGE_TEXT = 'Duplicate pricipal unit name';
	    END IF;

		SET settingsuuid = UUID_TO_BIN(UUID());
		SET unituuid = UUID_TO_BIN(uuid());
		INSERT INTO tbusersettings(coId, coNotification) VALUES(settingsuuid, 0);
		INSERT INTO tborganizationalunitsettings(coUserSettingsId, coBlacklist) VALUES(settingsuuid, NULL);
		INSERT INTO tborganizationalunit(coId,	coName, coParentId, coSettingsId, coMail) VALUES(unituuid, name, UUID_TO_BIN('00000000-BEEF-BEEF-BEEF-000000000000'), settingsuuid, mail);
		SET tempuuid = UUID_TO_BIN(UUID());
		SET usersettingsuuid = UUID_TO_BIN(UUID());
		INSERT INTO tbusersettings(coId, coNotification) VALUES(usersettingsuuid, 0);
		INSERT INTO tbUser(coId, coName, coMail, coPasswordHash, coRole, coSettingsId, coPrincipalUnit) 
		 			VALUES(tempuuid, name, mail, adminPwHash, 7, usersettingsuuid, unituuid);
		SET newUserId = BIN_TO_UUID(tempuuid);
		SET newPrincipalUnitId = BIN_TO_UUID(unituuid);
	 COMMIT;

END //  

CREATE OR REPLACE PROCEDURE SP_CREATE_UNIT(
    IN name TEXT,
    IN parent VARCHAR(36),
    OUT newUnitId VARCHAR(36)
)
BEGIN
	DECLARE settingsuuid BINARY(16);
   DECLARE unituuid BINARY(16);
   START TRANSACTION;
		SET settingsuuid = UUID_TO_BIN(uuid());
		SET unituuid = UUID_TO_BIN(uuid());
		INSERT INTO tbusersettings(coId, coNotification) VALUES(settingsuuid, 0);
		INSERT INTO tborganizationalunitsettings(coUserSettingsId, coBlacklist) VALUES(settingsuuid, NULL);
		INSERT INTO tborganizationalunit(coId,	coName, coParentId, coSettingsId, coMail) VALUES(unituuid, name, UUID_TO_BIN(parent), settingsuuid, NULL);
		SET newUnitId = BIN_TO_UUID(unituuid);
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_UPDATE_UNIT(
	 IN unitId VARCHAR(36),
    IN name TEXT
)
BEGIN
   DECLARE unituuid BINARY(16);
   START TRANSACTION;
		SET unituuid = UUID_TO_BIN(unitId);
		UPDATE tborganizationalunit 
		SET tborganizationalunit.coName = name
		WHERE tborganizationalunit.coId = unituuid;
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_DELETE_UNIT(
	 IN unitId VARCHAR(36)
)
BEGIN
   DECLARE settingsuuid BINARY(16);
   START TRANSACTION;
		SET settingsuuid = (SELECT coSettingsId FROM tborganizationalunit WHERE coId = UUID_TO_BIN(unitId));
		DELETE FROM tborganizationalunitsettings WHERE tborganizationalunitsettings.coUserSettingsId = settingsuuid;
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_SET_UNIT_BLACKLIST(
	 IN settingsId VARCHAR(36),
    IN blackList TEXT
)
BEGIN
   DECLARE settingsuuid BINARY(16);
   START TRANSACTION;
		SET settingsuuid = UUID_TO_BIN(settingsId);
		UPDATE tborganizationalunitsettings 
		SET tborganizationalunitsettings.coBlacklist = blackList
		WHERE tborganizationalunitsettings.coId = settingsuuid;
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_ADD_UNIT_SOURCE(
	 IN settingsId VARCHAR(36),
    IN sourceId VARCHAR(36)
)
BEGIN
   START TRANSACTION;
		INSERT INTO tborganizationalunitsettingssource(coOrganizationalUnitSettingsId, coSourceId) 
			    VALUES (UUID_TO_BIN(settingsId), UUID_TO_BIN(sourceId)); 
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_REMOVE_UNIT_SOURCE(
	 IN settingsId VARCHAR(36),
    IN sourceId VARCHAR(36)
)
BEGIN
   START TRANSACTION;
   	DELETE FROM tborganizationalunitsettingssource
   	WHERE tborganizationalunitsettingssource.coOrganizationalUnitSettingsId = UUID_TO_BIN(settingsId)
   	AND tborganizationalunitsettingssource.coSourceId = UUID_TO_BIN(sourceId);
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_DELETE_FEED(
	 IN feedId VARCHAR(36)
)
BEGIN
   DECLARE filterId BINARY(16);
   START TRANSACTION;
   	SET filterId = (SELECT coFilterId FROM tbfeed WHERE tbfeed.coId = UUID_TO_BIN(feedId));
		DELETE FROM tbfeedfilter WHERE coId = filterId;
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_REMOVE_SOURCE_FEED(
	 IN feedId VARCHAR(36),
	 IN sourceId VARCHAR(36)
)
BEGIN
   START TRANSACTION;
   	DELETE FROM tbfeedsource 
		WHERE coFeedId = UUID_TO_BIN(feedId)
		AND coSourceId = UUID_TO_BIN(sourceId);
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_CHANGE_USERPW(
   IN mail VARCHAR(191),
	IN pwHash TEXT,
	IN mustChangePassword BOOL,
	OUT success BOOL
)
BEGIN
	 SET success = FALSE;
    START TRANSACTION;
	    IF EXISTS(SELECT coId FROM tbuser WHERE tbuser.coMail = mail) 
	    THEN
	    	BEGIN
			    UPDATE tbuser 
				 SET tbuser.coPasswordHash = pwHash, tbuser.coMustChangePassword = mustChangePassword
				 WHERE tbuser.coMail = mail;
				 SET success = TRUE;
			 END;
		 END IF;
	 COMMIT;
END //
DELIMITER ;
DROP PROCEDURE SP_RESET_USERPW;
INSERT into tbVersion(coVersion, coUpdateTime) VALUES(6, NOW());