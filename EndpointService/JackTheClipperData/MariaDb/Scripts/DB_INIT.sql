-- MINIMUM VERSION CHECK (thrown error is wired, but the only way to throw an error without a stored procedure)
SELECT CASE WHEN EXISTS (SELECT * FROM tbversion WHERE coVersion = 6) THEN 1 else (select table_name from information_schema.tables) END;

-- CREATE USER
-- TODO: ADJUST PASSWORD
-- ALSO MAYBE RESTRICT TO LOCALHOST BY REPLACING ALL '%' WITH 'localhost'
CREATE USER 'clipperuser'@'%' IDENTIFIED BY 'Clipper1#9fheH)/';
GRANT SELECT ON jacktheclipper_debug.* TO 'clipperuser'@'%';
GRANT EXECUTE ON jacktheclipper_debug.* TO 'clipperuser'@'%';
