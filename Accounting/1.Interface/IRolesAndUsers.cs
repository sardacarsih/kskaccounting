using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Accounting.Models.Login;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Accounting._1.Interface
{
    public interface IRolesAndUsers
    {
        List<Permission> GetPermissionsFromGridControl(GridControl gridControl);
        bool TryGetRoleId(object editValue, out int roleId);
        void AddPermission(List<Permission> list, Permission permission);
        void InsertPermissionToDatabase(int roleId, Permission permission, OracleCommand command);
        void DeletePermissionsFromDatabase(int roleId, List<Permission> permissions, OracleCommand command);
        void DeleteUser(string userid, int moduleid);
        void RefreshGridData(GridView gridView);
        void ClearSelection(GridView gridView);
        void ShowMessage(string message);
        void UpdatePermissionInDatabase(Permission permission);
        DialogResult ShowConfirmationDialog(string message);
    }

}
