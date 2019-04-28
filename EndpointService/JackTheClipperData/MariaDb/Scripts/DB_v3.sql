DELIMITER //
CREATE OR REPLACE PROCEDURE SP_RESET_USERPW(
   IN mail TEXT,
	IN pwHash TEXT,
	OUT success BOOL
)
BEGIN
	 SET success = FALSE;
    START TRANSACTION;
	    IF EXISTS(SELECT coId FROM tbuser WHERE tbuser.coMail = mail) 
	    THEN
	    	BEGIN
			    UPDATE tbuser 
				 SET tbuser.coPasswordHash = pwHash
				 WHERE tbuser.coMail = mail;
				 SET success = TRUE;
			 END;
		 END IF;
	 COMMIT;
END //
DELIMITER ;

INSERT into tbVersion(coVersion, coUpdateTime) VALUES(3, NOW());