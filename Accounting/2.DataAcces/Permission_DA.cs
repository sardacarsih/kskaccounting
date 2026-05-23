using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Accounting._1.Interface;
using Accounting.Models.Login;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace Accounting._2.DataAccess
{
    public class Permission_DA : IPermission
    {
        public DataTable GetRoleList()
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            const string sql = "SELECT ROLE_ID, ROLE_NAME FROM MASTER_ROLES ORDER BY ROLE_NAME";

            IEnumerable<dynamic> roles = dbConnection.Query(sql);

            DataTable dataTable = new();
            dataTable.Columns.Add("ROLE_ID", typeof(int));
            dataTable.Columns.Add("ROLE_NAME", typeof(string));

            foreach (dynamic role in roles)
            {
                DataRow row = dataTable.NewRow();
                row["ROLE_ID"] = role.ROLE_ID;
                row["ROLE_NAME"] = role.ROLE_NAME;
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public IEnumerable<RoleSummary> GetRoleSummaries(int moduleId)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            const string sql = @"
                SELECT
                    MR.ROLE_ID AS RoleId,
                    MR.ROLE_NAME AS RoleName,
                    (
                        SELECT COUNT(1)
                        FROM MASTER_USER_ROLES MUR
                        WHERE MUR.ROLE_ID = MR.ROLE_ID
                            AND MUR.MODULE_ID = :P_MODULE_ID
                    ) AS UserCount,
                    CASE WHEN MR.ROLE_ID = 1 THEN 1 ELSE 0 END AS IsProtected,
                    CASE
                        WHEN UPPER(MR.ROLE_NAME) IN ('VIEWER','CHECKER','APPROVER','MAKER','USER','OPERATOR','STAFF','SUPERVISOR','ADMIN','ADMINISTRATOR','SUPERADMIN')
                            THEN 1
                        ELSE 0
                    END AS IsSystemRole
                FROM MASTER_ROLES MR
                WHERE EXISTS (
                    SELECT 1
                    FROM MASTER_ROLE_PERMISSIONS MRP
                    JOIN MASTER_PERMISSIONS MP
                        ON MP.PERMISSION_ID = MRP.PERMISSION_ID
                    WHERE MRP.ROLE_ID = MR.ROLE_ID
                        AND MP.MODULE_ID = :P_MODULE_ID
                )
                    OR EXISTS (
                        SELECT 1
                        FROM MASTER_USER_ROLES MUR
                        WHERE MUR.ROLE_ID = MR.ROLE_ID
                            AND MUR.MODULE_ID = :P_MODULE_ID
                )
                ORDER BY
                    CASE
                        WHEN MR.ROLE_ID = 1 THEN 0
                        WHEN UPPER(MR.ROLE_NAME) IN ('VIEWER','CHECKER','APPROVER','MAKER','USER','OPERATOR','STAFF','SUPERVISOR','ADMIN','ADMINISTRATOR','SUPERADMIN')
                            THEN 1
                        ELSE 2
                    END,
                    MR.ROLE_NAME";

            return dbConnection.Query<RoleSummary>(sql, new { P_MODULE_ID = moduleId }).ToList();
        }

        public IEnumerable<Permission> GetRolePermissions(string roleName, string moduleName)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            const string sql = @"
                SELECT
                    MR.ROLE_ID AS RoleId,
                    MP.PERMISSION_ID AS PermissionId,
                    MP.PERMISSION_NAME AS PermissionName,
                    MP.MENU AS Menu,
                    MP.DESCRIPTION AS Description,
                    CASE WHEN MRP.CAN_CREATE = 'Y' THEN 1 ELSE 0 END AS CanCreate,
                    CASE WHEN MRP.CAN_READ = 'Y' THEN 1 ELSE 0 END AS CanRead,
                    CASE WHEN MRP.CAN_UPDATE = 'Y' THEN 1 ELSE 0 END AS CanUpdate,
                    CASE WHEN MRP.CAN_DELETE = 'Y' THEN 1 ELSE 0 END AS CanDelete
                FROM MASTER_ROLE_PERMISSIONS MRP
                JOIN MASTER_ROLES MR
                    ON MRP.ROLE_ID = MR.ROLE_ID
                JOIN MASTER_PERMISSIONS MP
                    ON MRP.PERMISSION_ID = MP.PERMISSION_ID
                JOIN MASTER_MODULES MM
                    ON MP.MODULE_ID = MM.MODULE_ID
                WHERE MR.ROLE_NAME = :RoleName
                    AND MM.MODULE_NAME = :ModuleName
                ORDER BY MP.URUT1, MP.URUT2";

            return dbConnection.Query<Permission>(sql, new { RoleName = roleName, ModuleName = moduleName }).ToList();
        }

        public IEnumerable<Permission> GetRolePermissionMatrix(int roleId, string moduleName)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            const string sql = @"
                SELECT
                    :P_ROLE_ID AS RoleId,
                    MP.PERMISSION_ID AS PermissionId,
                    MP.PERMISSION_NAME AS PermissionName,
                    MP.MENU AS Menu,
                    MP.DESCRIPTION AS Description,
                    CASE
                        WHEN MP.PERMISSION_NAME LIKE 'JURNAL_%' THEN 'Jurnal'
                        WHEN MP.PERMISSION_NAME LIKE 'COA_%' THEN 'Chart of Account'
                        WHEN MP.PERMISSION_NAME LIKE 'REPORT_%' THEN 'Reports'
                        WHEN MP.PERMISSION_NAME LIKE 'SETTING_%' THEN 'Settings'
                        WHEN MP.PERMISSION_NAME LIKE 'AUDIT_%' THEN 'Settings'
                        WHEN MP.PERMISSION_NAME LIKE 'USER_%' THEN 'Security'
                        WHEN MP.PERMISSION_NAME LIKE 'FA_%' THEN 'Fixed Asset'
                        ELSE 'General'
                    END AS Category,
                    CASE WHEN NVL(MRP.CAN_CREATE, 'N') = 'Y' THEN 1 ELSE 0 END AS CanCreate,
                    CASE WHEN NVL(MRP.CAN_READ, 'N') = 'Y' THEN 1 ELSE 0 END AS CanRead,
                    CASE WHEN NVL(MRP.CAN_UPDATE, 'N') = 'Y' THEN 1 ELSE 0 END AS CanUpdate,
                    CASE WHEN NVL(MRP.CAN_DELETE, 'N') = 'Y' THEN 1 ELSE 0 END AS CanDelete
                FROM MASTER_PERMISSIONS MP
                JOIN MASTER_MODULES MM
                    ON MM.MODULE_ID = MP.MODULE_ID
                LEFT JOIN MASTER_ROLE_PERMISSIONS MRP
                    ON MRP.PERMISSION_ID = MP.PERMISSION_ID
                    AND MRP.ROLE_ID = :P_ROLE_ID
                WHERE MM.MODULE_NAME = :P_MODULE_NAME
                ORDER BY MP.URUT1, MP.URUT2";

            return dbConnection.Query<Permission>(sql, new { P_ROLE_ID = roleId, P_MODULE_NAME = moduleName }).ToList();
        }

        public IEnumerable<Permission> GetPermissionsForUser(string userId, string moduleName)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);

            try
            {
                dbConnection.Open();

                const string sql = @"
                    SELECT
                        MP.PERMISSION_NAME AS PermissionName,
                        CASE WHEN MRP.CAN_CREATE = 'Y' THEN 1 ELSE 0 END AS CanCreate,
                        CASE WHEN MRP.CAN_READ = 'Y' THEN 1 ELSE 0 END AS CanRead,
                        CASE WHEN MRP.CAN_UPDATE = 'Y' THEN 1 ELSE 0 END AS CanUpdate,
                        CASE WHEN MRP.CAN_DELETE = 'Y' THEN 1 ELSE 0 END AS CanDelete
                    FROM MASTER_LOGIN ML
                    JOIN MASTER_USER_ROLES MA
                        ON ML.USERID = MA.USER_ID
                    JOIN MASTER_ROLE_PERMISSIONS MRP
                        ON MA.ROLE_ID = MRP.ROLE_ID
                    JOIN MASTER_PERMISSIONS MP
                        ON MRP.PERMISSION_ID = MP.PERMISSION_ID
                        AND MP.MODULE_ID = MA.MODULE_ID
                    JOIN MASTER_MODULES MM
                        ON MP.MODULE_ID = MM.MODULE_ID
                    WHERE ML.USERID = :UserID
                        AND MM.MODULE_NAME = :ModuleName";

                return dbConnection.Query<Permission>(sql, new { UserID = userId, ModuleName = moduleName }).ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while retrieving permissions.", ex);
            }
        }

        public IEnumerable<Permission> GetMasterAkses(string moduleName)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            const string sql = @"
                SELECT
                    MP.PERMISSION_ID AS PermissionId,
                    MP.PERMISSION_NAME AS PermissionName,
                    MP.MENU AS Menu,
                    MP.DESCRIPTION AS Description
                FROM MASTER_PERMISSIONS MP
                JOIN MASTER_MODULES MM
                    ON MP.MODULE_ID = MM.MODULE_ID
                WHERE MM.MODULE_NAME = :ModuleName
                ORDER BY MP.URUT1, MP.URUT2";

            IEnumerable<Permission> permissions = dbConnection.Query<Permission>(sql, new { ModuleName = moduleName }).ToList();

            foreach (Permission permission in permissions)
            {
                permission.CanCreate = true;
                permission.CanRead = true;
                permission.CanUpdate = true;
                permission.CanDelete = true;
            }

            return permissions;
        }

        public bool CanUserPerformAction(string userId, string moduleName, string permissionName, Func<Permission, bool> actionCheck)
        {
            IEnumerable<Permission> permissions = GetPermissionsForUser(userId, moduleName);
            IEnumerable<Permission> specificPermissions = permissions.Where(p =>
                p.PermissionName.Equals(permissionName, StringComparison.OrdinalIgnoreCase));

            return specificPermissions.Any(actionCheck);
        }

        public IEnumerable<Permission_Users> GetUserList()
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            const string sql = @"
                SELECT
                    USERID,
                    NAMA,
                    DEPT,
                    JABATAN,
                    CASE WHEN AKTIF = 'Y' THEN 1 ELSE 0 END AS AKTIF
                FROM MASTER_LOGIN
                ORDER BY NAMA";

            return dbConnection.Query<Permission_Users>(sql).ToList();
        }

        public int GetModuleId(string moduleName)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            const string sql = @"
                SELECT MODULE_ID
                FROM MASTER_MODULES
                WHERE MODULE_NAME = :P_MODULE_NAME";

            return dbConnection.QuerySingle<int>(sql, new { P_MODULE_NAME = moduleName });
        }

        public IEnumerable<Permission_Users> GetUserLevelList(int moduleid)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            const string sql = @"
                SELECT
                    L.USERID,
                    L.NAMA,
                    L.DEPT,
                    L.JABATAN,
                    R.ROLE_ID,
                    R.ROLE_NAME
                FROM MASTER_LOGIN L
                JOIN MASTER_USER_ROLES UR
                    ON L.USERID = UR.USER_ID
                JOIN MASTER_ROLES R
                    ON UR.ROLE_ID = R.ROLE_ID
                JOIN MASTER_MODULES M
                    ON UR.MODULE_ID = M.MODULE_ID
                WHERE M.MODULE_ID = :P_MODULE_ID
                ORDER BY L.NAMA";

            return dbConnection.Query<Permission_Users>(sql, new { P_MODULE_ID = moduleid }).ToList();
        }

        public IEnumerable<Permission_Users> GetUsersByRole(int roleId, int? moduleId)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            string sql = @"
                SELECT
                    L.USERID,
                    L.NAMA,
                    L.DEPT,
                    L.JABATAN,
                    R.ROLE_ID,
                    R.ROLE_NAME,
                    CASE WHEN L.AKTIF = 'Y' THEN 1 ELSE 0 END AS AKTIF
                FROM MASTER_LOGIN L
                JOIN MASTER_USER_ROLES UR
                    ON L.USERID = UR.USER_ID
                JOIN MASTER_ROLES R
                    ON UR.ROLE_ID = R.ROLE_ID
                WHERE UR.ROLE_ID = :P_ROLE_ID";

            if (moduleId.HasValue)
            {
                sql += " AND UR.MODULE_ID = :P_MODULE_ID";
                return dbConnection.Query<Permission_Users>(sql + " ORDER BY L.NAMA", new
                {
                    P_ROLE_ID = roleId,
                    P_MODULE_ID = moduleId.Value
                }).ToList();
            }

            return dbConnection.Query<Permission_Users>(sql + " ORDER BY L.NAMA", new { P_ROLE_ID = roleId }).ToList();
        }

        public void UpdateUserRole(int newRoleId, string userId, int moduleId)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            const string sql = @"
                UPDATE MASTER_USER_ROLES
                SET ROLE_ID = :NEW_ROLE_ID
                WHERE USER_ID = :USER_ID
                    AND MODULE_ID = :MODULE_ID";

            dbConnection.Execute(sql, new
            {
                NEW_ROLE_ID = newRoleId,
                USER_ID = userId,
                MODULE_ID = moduleId
            });
        }

        public int DuplicateRole(string roleName, int sourceRoleId, string moduleName)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            using (OracleCommand command = new("ACCT_TOOLS.DuplikatRole", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(":rolename", OracleDbType.Varchar2, 30).Value = roleName;
                command.Parameters.Add(":darirole", OracleDbType.Int32).Value = sourceRoleId;
                command.Parameters.Add(":apps", OracleDbType.Varchar2, 10).Value = moduleName;
                command.ExecuteNonQuery();
            }

            using OracleCommand lookupCommand = new(@"
                SELECT ROLE_ID
                FROM MASTER_ROLES
                WHERE UPPER(ROLE_NAME) = UPPER(:P_ROLE_NAME)", connection);

            lookupCommand.Parameters.Add(":P_ROLE_NAME", OracleDbType.Varchar2, 30).Value = roleName;

            object? result = lookupCommand.ExecuteScalar();
            if (result == null || result == DBNull.Value)
            {
                throw new InvalidOperationException("Role baru tidak ditemukan setelah proses duplikasi.");
            }

            return Convert.ToInt32(result);
        }

        public void RenameRole(int roleId, string roleName)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            const string sql = @"
                UPDATE MASTER_ROLES
                SET ROLE_NAME = :P_ROLE_NAME
                WHERE ROLE_ID = :P_ROLE_ID";

            dbConnection.Execute(sql, new
            {
                P_ROLE_NAME = roleName,
                P_ROLE_ID = roleId
            });
        }

        public void DeleteRole(int roleId)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            using OracleTransaction transaction = connection.BeginTransaction();

            try
            {
                using (OracleCommand permissionCommand = new("DELETE FROM MASTER_ROLE_PERMISSIONS WHERE ROLE_ID = :P_ROLE_ID", connection))
                {
                    permissionCommand.Transaction = transaction;
                    permissionCommand.Parameters.Add(":P_ROLE_ID", OracleDbType.Int32).Value = roleId;
                    permissionCommand.ExecuteNonQuery();
                }

                using (OracleCommand roleCommand = new("DELETE FROM MASTER_ROLES WHERE ROLE_ID = :P_ROLE_ID", connection))
                {
                    roleCommand.Transaction = transaction;
                    roleCommand.Parameters.Add(":P_ROLE_ID", OracleDbType.Int32).Value = roleId;
                    roleCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void SaveRolePermissions(int roleId, IEnumerable<Permission> permissions, string moduleName)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            using OracleTransaction transaction = connection.BeginTransaction();

            try
            {
                Dictionary<int, bool> existingPermissionMap = connection.Query<int>(@"
                    SELECT MRP.PERMISSION_ID
                    FROM MASTER_ROLE_PERMISSIONS MRP
                    JOIN MASTER_PERMISSIONS MP
                        ON MP.PERMISSION_ID = MRP.PERMISSION_ID
                    JOIN MASTER_MODULES MM
                        ON MM.MODULE_ID = MP.MODULE_ID
                    WHERE MRP.ROLE_ID = :P_ROLE_ID
                        AND MM.MODULE_NAME = :P_MODULE_NAME",
                    new
                    {
                        P_ROLE_ID = roleId,
                        P_MODULE_NAME = moduleName
                    },
                    transaction).ToDictionary(permissionId => permissionId, _ => true);

                foreach (Permission permission in permissions)
                {
                    bool hasAnyAccess = permission.CanCreate
                        || permission.CanRead
                        || permission.CanUpdate
                        || permission.CanDelete;

                    bool exists = existingPermissionMap.ContainsKey(permission.PermissionId);

                    if (!hasAnyAccess)
                    {
                        if (exists)
                        {
                            using OracleCommand deleteCommand = new(
                                "DELETE FROM MASTER_ROLE_PERMISSIONS WHERE ROLE_ID = :P_ROLE_ID AND PERMISSION_ID = :P_PERMISSION_ID",
                                connection);

                            deleteCommand.Transaction = transaction;
                            deleteCommand.Parameters.Add(":P_ROLE_ID", OracleDbType.Int32).Value = roleId;
                            deleteCommand.Parameters.Add(":P_PERMISSION_ID", OracleDbType.Int32).Value = permission.PermissionId;
                            deleteCommand.ExecuteNonQuery();
                        }

                        continue;
                    }

                    if (exists)
                    {
                        using OracleCommand updateCommand = new(@"
                            UPDATE MASTER_ROLE_PERMISSIONS
                            SET CAN_CREATE = :P_CAN_CREATE,
                                CAN_READ = :P_CAN_READ,
                                CAN_UPDATE = :P_CAN_UPDATE,
                                CAN_DELETE = :P_CAN_DELETE
                            WHERE ROLE_ID = :P_ROLE_ID
                                AND PERMISSION_ID = :P_PERMISSION_ID", connection);

                        updateCommand.Transaction = transaction;
                        updateCommand.Parameters.Add(":P_CAN_CREATE", OracleDbType.Char).Value = permission.CanCreate ? "Y" : "N";
                        updateCommand.Parameters.Add(":P_CAN_READ", OracleDbType.Char).Value = permission.CanRead ? "Y" : "N";
                        updateCommand.Parameters.Add(":P_CAN_UPDATE", OracleDbType.Char).Value = permission.CanUpdate ? "Y" : "N";
                        updateCommand.Parameters.Add(":P_CAN_DELETE", OracleDbType.Char).Value = permission.CanDelete ? "Y" : "N";
                        updateCommand.Parameters.Add(":P_ROLE_ID", OracleDbType.Int32).Value = roleId;
                        updateCommand.Parameters.Add(":P_PERMISSION_ID", OracleDbType.Int32).Value = permission.PermissionId;
                        updateCommand.ExecuteNonQuery();
                        continue;
                    }

                    using OracleCommand insertCommand = new(@"
                        INSERT INTO MASTER_ROLE_PERMISSIONS
                            (ROLE_ID, PERMISSION_ID, CAN_CREATE, CAN_READ, CAN_UPDATE, CAN_DELETE)
                        VALUES
                            (:P_ROLE_ID, :P_PERMISSION_ID, :P_CAN_CREATE, :P_CAN_READ, :P_CAN_UPDATE, :P_CAN_DELETE)", connection);

                    insertCommand.Transaction = transaction;
                    insertCommand.Parameters.Add(":P_ROLE_ID", OracleDbType.Int32).Value = roleId;
                    insertCommand.Parameters.Add(":P_PERMISSION_ID", OracleDbType.Int32).Value = permission.PermissionId;
                    insertCommand.Parameters.Add(":P_CAN_CREATE", OracleDbType.Char).Value = permission.CanCreate ? "Y" : "N";
                    insertCommand.Parameters.Add(":P_CAN_READ", OracleDbType.Char).Value = permission.CanRead ? "Y" : "N";
                    insertCommand.Parameters.Add(":P_CAN_UPDATE", OracleDbType.Char).Value = permission.CanUpdate ? "Y" : "N";
                    insertCommand.Parameters.Add(":P_CAN_DELETE", OracleDbType.Char).Value = permission.CanDelete ? "Y" : "N";
                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void AssignUsersToRole(IEnumerable<string> userIds, int roleId, int moduleId)
        {
            string[] effectiveUserIds = userIds
                .Where(userId => !string.IsNullOrWhiteSpace(userId))
                .Select(userId => userId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (effectiveUserIds.Length == 0)
            {
                return;
            }

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            using OracleTransaction transaction = connection.BeginTransaction();

            try
            {
                foreach (string userId in effectiveUserIds)
                {
                    using OracleCommand existsCommand = new(@"
                        SELECT COUNT(1)
                        FROM MASTER_USER_ROLES
                        WHERE USER_ID = :P_USER_ID
                            AND MODULE_ID = :P_MODULE_ID", connection);

                    existsCommand.Transaction = transaction;
                    existsCommand.Parameters.Add(":P_USER_ID", OracleDbType.Varchar2).Value = userId;
                    existsCommand.Parameters.Add(":P_MODULE_ID", OracleDbType.Int32).Value = moduleId;

                    int existingCount = Convert.ToInt32(existsCommand.ExecuteScalar());

                    if (existingCount > 0)
                    {
                        using OracleCommand updateCommand = new(@"
                            UPDATE MASTER_USER_ROLES
                            SET ROLE_ID = :P_ROLE_ID
                            WHERE USER_ID = :P_USER_ID
                                AND MODULE_ID = :P_MODULE_ID", connection);

                        updateCommand.Transaction = transaction;
                        updateCommand.Parameters.Add(":P_ROLE_ID", OracleDbType.Int32).Value = roleId;
                        updateCommand.Parameters.Add(":P_USER_ID", OracleDbType.Varchar2).Value = userId;
                        updateCommand.Parameters.Add(":P_MODULE_ID", OracleDbType.Int32).Value = moduleId;
                        updateCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        using OracleCommand insertCommand = new(@"
                            INSERT INTO MASTER_USER_ROLES (USER_ID, ROLE_ID, MODULE_ID)
                            VALUES (:P_USER_ID, :P_ROLE_ID, :P_MODULE_ID)", connection);

                        insertCommand.Transaction = transaction;
                        insertCommand.Parameters.Add(":P_USER_ID", OracleDbType.Varchar2).Value = userId;
                        insertCommand.Parameters.Add(":P_ROLE_ID", OracleDbType.Int32).Value = roleId;
                        insertCommand.Parameters.Add(":P_MODULE_ID", OracleDbType.Int32).Value = moduleId;
                        insertCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void RemoveUsersFromRole(IEnumerable<string> userIds, int moduleId)
        {
            string[] effectiveUserIds = userIds
                .Where(userId => !string.IsNullOrWhiteSpace(userId))
                .Select(userId => userId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (effectiveUserIds.Length == 0)
            {
                return;
            }

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();

            using OracleTransaction transaction = connection.BeginTransaction();

            try
            {
                foreach (string userId in effectiveUserIds)
                {
                    using OracleCommand deleteCommand = new(@"
                        DELETE FROM MASTER_USER_ROLES
                        WHERE USER_ID = :P_USER_ID
                            AND MODULE_ID = :P_MODULE_ID", connection);

                    deleteCommand.Transaction = transaction;
                    deleteCommand.Parameters.Add(":P_USER_ID", OracleDbType.Varchar2).Value = userId;
                    deleteCommand.Parameters.Add(":P_MODULE_ID", OracleDbType.Int32).Value = moduleId;
                    deleteCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
