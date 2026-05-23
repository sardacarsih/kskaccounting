-- Rollback: Remove seeded Jurnal RBAC permissions and default role mappings.

SET SERVEROUTPUT ON;

DECLARE
    PROCEDURE delete_role_mappings_for_permission(p_permission_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        DELETE FROM MASTER_ROLE_PERMISSIONS
         WHERE PERMISSION_ID IN (
             SELECT MP.PERMISSION_ID
               FROM MASTER_PERMISSIONS MP
               JOIN MASTER_MODULES MM
                 ON MM.MODULE_ID = MP.MODULE_ID
              WHERE MM.MODULE_NAME = 'GL'
                AND MP.PERMISSION_NAME = p_permission_name
         );

        v_count := SQL%ROWCOUNT;
        DBMS_OUTPUT.PUT_LINE('REMOVED ROLE MAPPINGS FOR ' || p_permission_name || ': ' || v_count);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('SKIPPED ROLE MAPPINGS FOR ' || p_permission_name || ': ' || SQLERRM);
    END;

    PROCEDURE delete_permission(p_permission_name IN VARCHAR2) IS
        v_count NUMBER := 0;
    BEGIN
        DELETE FROM MASTER_PERMISSIONS
         WHERE MODULE_ID IN (
             SELECT MODULE_ID
               FROM MASTER_MODULES
              WHERE MODULE_NAME = 'GL'
         )
           AND PERMISSION_NAME = p_permission_name;

        v_count := SQL%ROWCOUNT;
        DBMS_OUTPUT.PUT_LINE('REMOVED PERMISSION ' || p_permission_name || ': ' || v_count);
    EXCEPTION
        WHEN OTHERS THEN
            DBMS_OUTPUT.PUT_LINE('SKIPPED PERMISSION ' || p_permission_name || ': ' || SQLERRM);
    END;
BEGIN
    delete_role_mappings_for_permission('JURNAL_EXPORT');
    delete_role_mappings_for_permission('JURNAL_IMPORT');
    delete_role_mappings_for_permission('JURNAL_DELETE');
    delete_role_mappings_for_permission('JURNAL_ENTRY');
    delete_role_mappings_for_permission('JURNAL_VIEW');

    delete_permission('JURNAL_EXPORT');
    delete_permission('JURNAL_IMPORT');
    delete_permission('JURNAL_DELETE');
    delete_permission('JURNAL_ENTRY');
    delete_permission('JURNAL_VIEW');
END;
/
