-- Purpose: Remove the default gladmin user, its RBAC role assignments, and its location access.
-- Date: 2026-07-06

SET SERVEROUTPUT ON;

BEGIN
    DELETE FROM MASTER_USER_ROLES_LOC WHERE USER_ID = 'gladmin';
    DBMS_OUTPUT.PUT_LINE('REMOVED MASTER_USER_ROLES_LOC rows for gladmin: ' || SQL%ROWCOUNT);

    DELETE FROM MASTER_USER_ROLES WHERE USER_ID = 'gladmin';
    DBMS_OUTPUT.PUT_LINE('REMOVED MASTER_USER_ROLES rows for gladmin: ' || SQL%ROWCOUNT);

    DELETE FROM MASTER_LOGIN WHERE USERID = 'gladmin';
    DBMS_OUTPUT.PUT_LINE('REMOVED MASTER_LOGIN row for gladmin: ' || SQL%ROWCOUNT);

    COMMIT;
END;
/
