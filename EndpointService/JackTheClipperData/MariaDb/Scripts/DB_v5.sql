DELIMITER //
CREATE OR REPLACE PROCEDURE SP_CHANGE_MAILADDRESS(
   IN userId VARCHAR(36) ,
	IN newMail TEXT,
	OUT success BOOL
)
BEGIN
	 SET success = FALSE;
    START TRANSACTION;
	    IF EXISTS(SELECT coId FROM tbuser WHERE tbuser.coId = UUID_TO_BIN(userId)) 
	    THEN
	    	BEGIN
			    UPDATE tbuser 
				 SET tbuser.coMail = newMail
				 WHERE tbuser.coId = UUID_TO_BIN(userId);
				 SET success = TRUE;
			 END;
		 END IF;
	 COMMIT;
END //
DELIMITER ;

INSERT into tbVersion(coVersion, coUpdateTime) VALUES(5, NOW());