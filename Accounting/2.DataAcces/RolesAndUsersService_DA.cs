using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Accounting._1.Interface;
using Accounting.Models.Login;
using Accounting.Services;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System;

namespace Accounting._2.DataAccess
{
    public class RolesAndUsersService : IRolesAndUsers
    {
        public List<Permission> GetPermissionsFromGridControl(GridControl gridControl)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return gridControl.DataSource as List<Permission>;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public bool TryGetRoleId(object editValue, out int roleId)
        {
            return int.TryParse(editValue?.ToString(), out roleId);
        }

        public void AddPermission(List<Permission> list, Permission permission)
        {
            list.Add(permission);
        }

        public void InsertPermissionToDatabase(int roleId, Permission permission, OracleCommand command)
        {
            AuthorizationService.EnsureCanManageRolePermissions(roleId);

            const string InsertCommandText = @"
            INSERT INTO MASTER_ROLE_PERMISSIONS (Role_Id, Permission_Id, Can_Create, Can_Read, Can_Update, Can_Delete)
            VALUES (:RoleId, :PermissionId, :CanCreate, :CanRead, :CanUpdate, :CanDelete)";

            command.CommandText = InsertCommandText;
            command.Parameters.Clear();
            command.Parameters.Add(new OracleParameter("RoleId", roleId));
            command.Parameters.Add(new OracleParameter("PermissionId", permission.PermissionId));
            command.Parameters.Add(new OracleParameter("CanCreate", 'Y'));
            command.Parameters.Add(new OracleParameter("CanRead", 'Y'));
            command.Parameters.Add(new OracleParameter("CanUpdate", 'Y'));
            command.Parameters.Add(new OracleParameter("CanDelete", 'Y'));
            command.ExecuteNonQuery();
            AuthorizationService.InvalidateCurrentUserPermissions();
        }

        public void DeletePermissionsFromDatabase(int roleId, List<Permission> permissions, OracleCommand command)
        {
            AuthorizationService.EnsureCanManageRolePermissions(roleId);

            const string DeleteCommandText = @"
            DELETE FROM MASTER_ROLE_PERMISSIONS 
            WHERE Role_Id = :RoleId 
            AND Permission_Id = :PermissionId";

            command.CommandText = DeleteCommandText;
            foreach (var permission in permissions)
            {
                command.Parameters.Clear();
                command.Parameters.Add(new OracleParameter("RoleId", roleId));
                command.Parameters.Add(new OracleParameter("PermissionId", permission.PermissionId));
                command.ExecuteNonQuery();
            }

            AuthorizationService.InvalidateCurrentUserPermissions();
        }

        public void RefreshGridData(GridView gridView)
        {
            gridView.RefreshData();
        }

        public void ClearSelection(GridView gridView)
        {
            gridView.ClearSelection();
        }

        public void ShowMessage(string message)
        {
            System.Windows.MessageBox.Show(message);
        }

        public DialogResult ShowConfirmationDialog(string message)
        {
            return (DialogResult)System.Windows.MessageBox.Show(message, "Confirm Deletion", (MessageBoxButton)MessageBoxButtons.YesNo, (MessageBoxImage)MessageBoxIcon.Warning);
        }

        public void UpdatePermissionInDatabase(Permission permission)
        {
            AuthorizationService.EnsureCanManageRolePermissions(permission.RoleId);

            try
            {
                var CanCreateValue = permission.CanCreate ? "Y" : "N";
                var CanReadValue = permission.CanRead ? "Y" : "N";
                var CanUpdateValue = permission.CanUpdate ? "Y" : "N";
                var CanDeleteValue = permission.CanDelete ? "Y" : "N";

                using var connection = new OracleConnection(LoginInfo.OracleConnString);
                connection.Open();
                var query = "UPDATE MASTER_ROLE_PERMISSIONS SET CAN_CREATE = :CAN_CREATE, CAN_READ = :CAN_READ, CAN_UPDATE = :CAN_UPDATE, CAN_DELETE = :CAN_DELETE WHERE ROLE_ID = :ROLE_ID AND PERMISSION_ID=:PERMISSION_ID";
                using var cmd = new OracleCommand(query, connection);
                cmd.Parameters.Add(new OracleParameter("CAN_CREATE", CanCreateValue));
                cmd.Parameters.Add(new OracleParameter("CAN_READ", CanReadValue));
                cmd.Parameters.Add(new OracleParameter("CAN_UPDATE", CanUpdateValue));
                cmd.Parameters.Add(new OracleParameter("CAN_DELETE", CanDeleteValue));
                cmd.Parameters.Add(new OracleParameter("ROLE_ID", permission.RoleId));
                cmd.Parameters.Add(new OracleParameter("PERMISSION_ID", permission.PermissionId));
                cmd.ExecuteNonQuery();
                AuthorizationService.InvalidateCurrentUserPermissions();
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to update Role Permission: {ex.Message}");
            }
        }

        public void DeleteUser(string userid, int moduleid)
        {
            AuthorizationService.EnsureCanManageRoleAssignments(userid);

            using var connection = new OracleConnection(LoginInfo.OracleConnString);
            connection.Open();
            var query = "DELETE MASTER_USER_ROLES WHERE USER_ID=:p_userid and MODULE_ID=:p_moduleid";
            using var cmd = new OracleCommand(query, connection);
            cmd.Parameters.Add(new OracleParameter("p_userid", userid));
            cmd.Parameters.Add(new OracleParameter("p_moduleid", moduleid));
            cmd.ExecuteNonQuery();
            AuthorizationService.InvalidateCurrentUserPermissions();
        }
    }

}
