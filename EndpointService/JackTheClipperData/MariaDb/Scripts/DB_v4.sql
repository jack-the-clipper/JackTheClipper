ALTER TABLE tbuser ADD COLUMN coMustChangePassword BOOL NOT NULL DEFAULT TRUE;
ALTER TABLE tbuser ADD COLUMN coValid BOOL NOT NULL DEFAULT FALSE;
ALTER TABLE tbusersettings ADD COLUMN coArticlesPerPage INT;

CREATE OR REPLACE TABLE tbUserLogins (
	coPk BIGINT NOT NULL AUTO_INCREMENT,
	coUserId BINARY(16) NOT NULL,
	coLoginTime DATETIME NOT NULL,
	PRIMARY KEY (coPk)
)
ENGINE=InnoDB
;
ALTER TABLE tbUserLogins 
ADD CONSTRAINT fk40 FOREIGN KEY (couserId) REFERENCES tbuser(coId) ON DELETE CASCADE;

DROP PROCEDURE SP_CREATE_USER;
DROP PROCEDURE SP_UPDATE_USERSETTINGS;
DELIMITER //
CREATE OR REPLACE PROCEDURE SP_LOG_USERLOGIN(
   IN userId VARCHAR(36)
)
BEGIN
    START TRANSACTION;
	    INSERT IGNORE INTO tbUserLogins (coUserId, coLoginTime) VALUES(UUID_TO_BIN(userId), UTC_TIMESTAMP());
	 COMMIT;
END //
CREATE OR REPLACE PROCEDURE SP_CREATE_USER(
    IN name TEXT,
    IN mail TEXT,
    IN pwHash TEXT,
    IN role BIT(4),
    IN unit VARCHAR(36),
    IN mustChangePassword BOOL,
    IN valid BOOL,
    OUT newUserId VARCHAR(36)
)
BEGIN
DECLARE settingsuuid BINARY(16);
DECLARE tempuuid BINARY(16);

    START TRANSACTION;

    SET settingsuuid = UUID_TO_BIN(UUID());
    SET tempuuid = UUID_TO_BIN(UUID());
	 Insert into tbusersettings(coId, coNotification) VALUES(settingsuuid, 0);
	 Insert into tbUser(coId, coName, coMail, coPasswordHash, coRole, coSettingsId, coMustChangePassword, coValid) VALUES(tempuuid, name, mail, pwhash, role, settingsuuid, mustChangePassword, valid);
    Insert into  tbuserorganizationalunit(coUserId, coOrganizationalUnitId) VALUES(tempuuid, UUID_TO_BIN(unit));
	 SET newUserId = BIN_TO_UUID(tempuuid);
	 COMMIT;

END //  
CREATE PROCEDURE SP_UPDATE_USERSETTINGS(
   IN id VARCHAR(36),
	IN _interval INT,
	IN notification BIT(4),
	IN articlesPerPage INT
)
BEGIN
    START TRANSACTION;
	    INSERT INTO tbUserSettings (coId, coNotification, coNotificationInterval, coArticlesPerPage) VALUES(UUID_TO_BIN(id), notification, _interval, articlesPerPage) 
	    ON DUPLICATE KEY UPDATE coNotification = notification, coNotificationInterval = _interval, coArticlesPerPage = articlesPerPage;
	 COMMIT;
END //
CREATE PROCEDURE SP_DEL_SOURCE(
   IN id VARCHAR(36)
)
BEGIN
    START TRANSACTION;
	    DELETE from tbSource WHERE coId = UUID_TO_BIN(id);
	 COMMIT;
END //
CREATE PROCEDURE SP_DEL_USER(
   IN id VARCHAR(36)
)
BEGIN
    START TRANSACTION;
	    DELETE from tbuser WHERE coId = UUID_TO_BIN(id);
	 COMMIT;
END //
DELIMITER ;

INSERT into tbVersion(coVersion, coUpdateTime) VALUES(4, NOW());