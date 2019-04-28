DELIMITER //
CREATE OR REPLACE PROCEDURE SP_UPDATE_SOURCE(
   INOUT id VARCHAR(36),
	IN url TEXT,
	IN name TEXT,
	IN type BIT(4),
	IN regex TEXT,
	IN xpath TEXT,
	IN blacklist TEXT
)
BEGIN
    IF id IS NULL THEN 
      SET id = uuid();
    END IF;

    START TRANSACTION;
	    INSERT INTO tbSource (coId, coUrl, coName, coType, coRegex, coXpath, coBlackList) VALUES(UUID_TO_BIN(id), url, name, type, regex, xpath, blacklist) 
	    ON DUPLICATE KEY UPDATE coUrl = url, coName = name, coType = type, coRegex = regex, coXpath = xpath, coBlackList = blacklist;
	 COMMIT;

END //  
CREATE OR REPLACE PROCEDURE SP_UPDATE_FILTER(
   INOUT id VARCHAR(36),
	IN keywords TEXT,
	IN expressions TEXT,
	IN blacklist TEXT
)
BEGIN    
    IF id IS NULL THEN 
      SET id = uuid();
    END IF;

    START TRANSACTION;
	    INSERT INTO tbFeedFilter (coId, coKeywords, coExpressions, coBlackList) VALUES(UUID_TO_BIN(id), keywords, expressions, blacklist) 
	    ON DUPLICATE KEY UPDATE coKeywords = keywords, coExpressions = expressions, coBlackList = blacklist;
	 COMMIT;

END //

CREATE OR REPLACE PROCEDURE SP_UPDATE_FEED(
   INOUT id VARCHAR(36),
	IN name TEXT,
	IN filterId VARCHAR(36)
)
BEGIN
    IF id IS NULL THEN 
      SET id = uuid();
    END IF;

    START TRANSACTION;
	    INSERT INTO tbFeed (coId, coName, coFilterId) VALUES(UUID_TO_BIN(id), name, UUID_TO_BIN(filterId)) 
	    ON DUPLICATE KEY UPDATE coName = name, coFilterId = UUID_TO_BIN(filterId);
	 COMMIT;

END //
CREATE OR REPLACE PROCEDURE SP_UPDATE_USERSETTINGS(
   IN id VARCHAR(36),
	IN _interval INT,
	IN notification BIT(4)
)
BEGIN
    START TRANSACTION;
	    INSERT INTO tbUserSettings (coId, coNotification, coNotificationInterval) VALUES(UUID_TO_BIN(id), notification, _interval) 
	    ON DUPLICATE KEY UPDATE coNotification = notification, coNotificationInterval = _interval;
	 COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_LINK_SOURCE_FEED(
   IN feedId VARCHAR(36),
	IN sourceId VARCHAR(36)
)
BEGIN
    START TRANSACTION;
	    INSERT IGNORE INTO tbfeedsource (coFeedId, coSourceId) VALUES(UUID_TO_BIN(feedId), UUID_TO_BIN(sourceId));
	 COMMIT;
END //

CREATE OR REPLACE PROCEDURE SP_LINK_SETTINGS_FEED(
   IN feedId VARCHAR(36),
	IN settingsId VARCHAR(36)
)
BEGIN
    START TRANSACTION;
	    INSERT IGNORE INTO tbsettingsfeeds (coFeedId, coSettingsId) VALUES(UUID_TO_BIN(feedId), UUID_TO_BIN(settingsId));
	 COMMIT;
END //
DELIMITER ;

INSERT into tbVersion(coVersion, coUpdateTime) VALUES(2, NOW());