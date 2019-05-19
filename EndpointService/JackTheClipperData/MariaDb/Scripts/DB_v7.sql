DELIMITER //
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
	        SIGNAL SQLSTATE 'ERR0R' SET MESSAGE_TEXT = 'ERROR999991';
	    END IF;
		IF EXISTS (SELECT coId FROM tbUser 
			 							WHERE tbUser.coMail = mail)  
		 THEN
		    SIGNAL SQLSTATE 'ERR0R' SET MESSAGE_TEXT = 'ERROR999993';
	    END IF;
	    
	   SET tempuuid = UUID_TO_BIN(UUID());
		SET usersettingsuuid = UUID_TO_BIN(UUID());
		INSERT INTO tbusersettings(coId, coNotification) VALUES(usersettingsuuid, 0);
		INSERT INTO tbUser(coId, coName, coMail, coPasswordHash, coRole, coSettingsId, coPrincipalUnit, coMustChangePassword, coValid) 
		 			VALUES(tempuuid, CONCAT('PB_', name), mail, adminPwHash,  b'0111', usersettingsuuid, UUID_TO_BIN('00000000-BEEF-BEEF-BEEF-000000000000'), TRUE, TRUE);
		SET newUserId = BIN_TO_UUID(tempuuid);

		SET settingsuuid = UUID_TO_BIN(UUID());
		SET unituuid = UUID_TO_BIN(uuid());
		INSERT INTO tbusersettings(coId, coNotification) VALUES(settingsuuid, 0);
		INSERT INTO tborganizationalunitsettings(coUserSettingsId, coBlacklist) VALUES(settingsuuid, NULL);
		INSERT INTO tborganizationalunit(coId,	coName, coParentId, coSettingsId, coMail) VALUES(unituuid, name, UUID_TO_BIN('00000000-BEEF-BEEF-BEEF-000000000000'), settingsuuid, mail);
	 	SET newPrincipalUnitId = BIN_TO_UUID(unituuid);
	 	INSERT INTO tbuserorganizationalunit(coUserId, coOrganizationalUnitId) VALUES(tempuuid, unituuid);
	 	UPDATE tbUser
	 	SET coPrincipalUnit = unituuid
	 	where coId = tempuuid;
	 COMMIT;
END //  

CREATE OR REPLACE PROCEDURE SP_DELETE_UNIT(
	 IN unitId VARCHAR(36)
)
BEGIN
   DECLARE settingsuuid BINARY(16);
   START TRANSACTION;
		SET settingsuuid = (SELECT coSettingsId FROM tborganizationalunit WHERE coId = UUID_TO_BIN(unitId));
		DELETE FROM tborganizationalunitsettings WHERE coUserSettingsId  = settingsuuid;
		DELETE FROM tbusersettings WHERE coId = settingsuuid;
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_CREATE_USER(
    IN name TEXT,
    IN mail VARCHAR(191),
    IN pwHash TEXT,
    IN role BIT(4),
    IN principalUnit VARCHAR(36),
    IN userUnit VARCHAR(36),
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
		    SIGNAL SQLSTATE 'ERR0R' SET MESSAGE_TEXT = 'ERROR999992';
	    END IF;
	    
	    IF EXISTS (SELECT coId FROM tbUser 
			 							WHERE tbUser.coMail = mail)  
		 THEN
		    SIGNAL SQLSTATE 'ERR0R' SET MESSAGE_TEXT = 'ERROR999993';
	    END IF;
	    SET settingsuuid = UUID_TO_BIN(UUID());
	    SET tempuuid = UUID_TO_BIN(UUID());
		INSERT INTO tbusersettings(coId, coNotification) VALUES(settingsuuid, 0);
		INSERT INTO tbUser(coId, coName, coMail, coPasswordHash, coRole, coSettingsId, coPrincipalUnit, coMustChangePassword, coValid) 
		 			VALUES(tempuuid, name, mail, pwhash, role, settingsuuid, UUID_TO_BIN(principalUnit), mustChangePassword, valid);
		INSERT INTO tbuserorganizationalunit(coUserId, coOrganizationalUnitId) VALUES (tempuuid, UUID_TO_BIN(userUnit)); 
		SET newUserId = BIN_TO_UUID(tempuuid);
	 COMMIT;

END //  

CREATE OR REPLACE PROCEDURE SP_UPDATE_USER_AND_CLEAR_USERUNITS(
	 IN userId VARCHAR(36),
     IN name TEXT,
     IN role BIT(4),
	 IN valid BOOL
)
BEGIN
	 DECLARE settingsuuid BINARY(16);
	 DECLARE tempuuid BINARY(16);
     START TRANSACTION;
	    UPDATE tbuser SET
	    tbuser.coName = name,
	    tbuser.coRole = role,
	    tbuser.coValid = valid
	    WHERE tbuser.coId = UUID_TO_BIN(userId);
	 DELETE FROM tbuserorganizationalunit WHERE tbuserorganizationalunit.coUserId = UUID_TO_BIN(userId);
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
		WHERE tborganizationalunitsettings.coUserSettingsId = settingsuuid;
	COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_DEL_USER(
   IN id VARCHAR(36)
)
BEGIN
    DECLARE settingsuuid BINARY(16);
    START TRANSACTION;
    	IF EXISTS (SELECT coId 
		 			  FROM tborganizationalunit 
			 		  WHERE tborganizationalunit.coMail = (SELECT tbuser.coMail FROM tbuser where tbuser.coId = UUID_TO_BIN(id)))  
		 THEN
		    SIGNAL SQLSTATE 'ERR0R' SET MESSAGE_TEXT = 'ERROR999994';
	    END IF;
	    SET settingsuuid = (SELECT coSettingsId FROM tbuser WHERE coId = UUID_TO_BIN(id));
		 DELETE FROM tbusersettings WHERE tbusersettings.coId  = settingsuuid;
	    -- DELETE from tbuser WHERE coId = UUID_TO_BIN(id);
	 COMMIT;
END //

 CREATE OR REPLACE PROCEDURE SP_DELETE_UNIT(
	 IN unitId VARCHAR(36)
)
BEGIN
   DECLARE settingsuuid BINARY(16);
   START TRANSACTION;
		SET settingsuuid = (SELECT coSettingsId FROM tborganizationalunit WHERE coId = UUID_TO_BIN(unitId));
		DELETE FROM tbuser 
			WHERE coId IN (SELECT coUserId
								FROM tbuserorganizationalunit 
								WHERE coOrganizationalUnitId = UUID_TO_BIN(unitId)
								AND coUserId IN (SELECT coUserId
													  FROM tbuserorganizationalunit
													  GROUP BY coUserId HAVING COUNT(coUserId) = 1));
		DELETE FROM tbusersettings WHERE coId = settingsuuid;
	COMMIT;
END //

DELIMITER ;

UPDATE tbuser SET coValid = TRUE where coMail = 'SA';
ALTER TABLE tborganizationalunit MODIFY coMail VARCHAR(191); -- allow null