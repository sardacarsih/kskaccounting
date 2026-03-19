using Dapper;
using Accounting._1.Interface;
using Accounting.Models.Login;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Accounting._2.DataAccess
{

    public class Permission_DA : IPermission
    {
        public IEnumerable<Permission> GetPermissionsForUser(string userId, string moduleName)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            try
            {
                dbConnection.Open();

                string sql = @"
            SELECT 
                MP.PERMISSION_NAME AS PermissionName,
                CASE WHEN MRP.CAN_CREATE = 'Y' THEN 1 ELSE 0 END AS CanCreate,
                CASE WHEN MRP.CAN_READ = 'Y' THEN 1 ELSE 0 END AS CanRead,
                CASE WHEN MRP.CAN_UPDATE = 'Y' THEN 1 ELSE 0 END AS CanUpdate,
                CASE WHEN MRP.CAN_DELETE = 'Y' THEN 1 ELSE 0 END AS CanDelete
            FROM 
                MASTER_LOGIN ML
            JOIN 
                MASTER_USER_ROLES MA ON ML.USERID = MA.USER_ID
            JOIN 
                MASTER_ROLE_PERMISSIONS MRP ON MA.ROLE_ID = MRP.ROLE_ID
            JOIN 
                MASTER_PERMISSIONS MP ON MRP.PERMISSION_ID = MP.PERMISSION_ID AND MP.MODULE_ID = MA.MODULE_ID
            JOIN 
                MASTER_MODULES MM ON MP.MODULE_ID = MM.MODULE_ID
            WHERE 
                ML.USERID = :UserID AND MM.MODULE_NAME = :ModuleName";

                var parameters = new { UserID = userId, ModuleName = moduleName };

                return dbConnection.Query<Permission>(sql, parameters);
            }
            catch (Exception ex)
            {
                // Log the exception (log implementation not shown here)
                throw new ApplicationException("An error occurred while retrieving permissions.", ex);
            }
        }



        // Check if a user can perform a specified action based on their permissions and permission name
        public bool CanUserPerformAction(string userId, string moduleName, string permissionName, Func<Permission, bool> actionCheck)
        {
            var permissions = GetPermissionsForUser(userId, moduleName);
            var specificPermissions = permissions.Where(p => p.PermissionName.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
            return specificPermissions.Any(actionCheck);
        }

        public IEnumerable<Permission> GetRolePermissions(string roleName, string moduleName)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            string sql = @"
                    SELECT 
                        MR.ROLE_ID AS RoleId,
                        MP.PERMISSION_ID AS PermissionId,
                        MP.menu,
                        MP.DESCRIPTION AS Description,
                        CASE WHEN MRP.CAN_CREATE = 'Y' THEN 1 ELSE 0 END AS CanCreate,
                        CASE WHEN MRP.CAN_READ = 'Y' THEN 1 ELSE 0 END AS CanRead,
                        CASE WHEN MRP.CAN_UPDATE = 'Y' THEN 1 ELSE 0 END AS CanUpdate,
                        CASE WHEN MRP.CAN_DELETE = 'Y' THEN 1 ELSE 0 END AS CanDelete
                    FROM 
                        MASTER_ROLE_PERMISSIONS MRP
                    JOIN 
                        MASTER_ROLES MR ON MRP.ROLE_ID = MR.ROLE_ID
                    JOIN 
                        MASTER_PERMISSIONS MP ON MRP.PERMISSION_ID = MP.PERMISSION_ID
                    JOIN 
                        MASTER_MODULES MM ON MP.MODULE_ID = MM.MODULE_ID
                    WHERE 
                        MR.ROLE_NAME = :RoleName AND MM.MODULE_NAME = :ModuleName
                    ORDER BY 
                        MP.URUT1,MP.URUT2";

            var parameters = new { RoleName = roleName, ModuleName = moduleName };

            return dbConnection.Query<Permission>(sql, parameters);
        }

        public DataTable GetRoleList()
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            string sql = "SELECT ROLE_ID, ROLE_NAME FROM MASTER_ROLES";

            var roles = dbConnection.Query(sql);

            DataTable dataTable = new();
            dataTable.Columns.Add("ROLE_ID", typeof(int));
            dataTable.Columns.Add("ROLE_NAME", typeof(string));

            foreach (var role in roles)
            {
                DataRow row = dataTable.NewRow();
                row["ROLE_ID"] = role.ROLE_ID;
                row["ROLE_NAME"] = role.ROLE_NAME;
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public IEnumerable<Permission> GetMasterAkses(string moduleName)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            string sql = @"
        SELECT     
            MP.PERMISSION_ID AS PermissionId,
            MP.menu AS Menu,
            MP.DESCRIPTION AS Description
        FROM    
            MASTER_PERMISSIONS MP 
        JOIN 
            MASTER_MODULES MM ON MP.MODULE_ID = MM.MODULE_ID
        WHERE 
            MM.MODULE_NAME = :ModuleName
        ORDER BY 
            MP.URUT1, MP.URUT2";

            var parameters = new { ModuleName = moduleName };

            // Execute query and retrieve permissions
            IEnumerable<Permission> permissions = dbConnection.Query<Permission>(sql, parameters);

            // Set all bool properties to true for each permission
            foreach (var permission in permissions)
            {
                permission.CanCreate = true;
                permission.CanRead = true;
                permission.CanUpdate = true;
                permission.CanDelete = true;
            }

            return permissions;
        }

        public IEnumerable<Permission_Users> GetUserList()
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            string sql = @"
                    SELECT USERID,NAMA,DEPT,JABATAN,                        
                        CASE WHEN AKTIF = 'Y' THEN 1 ELSE 0 END AS AKTIF
                    FROM 
                        MASTER_LOGIN
                    ORDER BY 
                        NAMA"
            ;
            return dbConnection.Query<Permission_Users>(sql);
        }

        public IEnumerable<Permission_Users> GetUserLevelList(int moduleid)
        {
            using (IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString))
            {
                dbConnection.Open();

                string sql = @"SELECT 
                            l.USERID,
                            l.NAMA,
                            l.DEPT,
                            l.JABATAN,
                            r.ROLE_ID
                       FROM 
                            MASTER_LOGIN l
                       JOIN 
                            MASTER_USER_ROLES ur ON l.USERID = ur.USER_ID
                       JOIN 
                            MASTER_ROLES r ON ur.ROLE_ID = r.ROLE_ID
                       JOIN 
                            MASTER_MODULES m ON ur.MODULE_ID = m.MODULE_ID
                       WHERE 
                            m.MODULE_ID = :P_MODULE_ID
                       ORDER BY 
                            l.NAMA";

                // Parameter binding
                var parameters = new { P_MODULE_ID = moduleid };

                // Query with parameter
                return dbConnection.Query<Permission_Users>(sql, parameters);
            }
        }


        public int GetModuleId(string moduleName)
        {
            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);
            dbConnection.Open();

            string sql = @"SELECT module_id 
                       FROM master_modules 
                       WHERE MODULE_NAME = :p_moduleName";

            // Parameter binding
            var parameters = new { p_moduleName = moduleName };

            // Query for a single value
            return dbConnection.QuerySingle<int>(sql, parameters);
        }

        public void UpdateUserRole(int newRoleId, string userId, int moduleId)
        {

            using IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString);

            // Open the connection
            dbConnection.Open();

            // Define the SQL query
            string sql = "UPDATE MASTER_USER_ROLES SET ROLE_ID = :newRoleId WHERE USER_ID = :userId AND MODULE_ID = :moduleId";

            // Execute the query using Dapper
            dbConnection.Execute(sql, new { newRoleId, userId, moduleId });
        }
    }
}
