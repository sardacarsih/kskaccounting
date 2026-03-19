
using Accounting.Models.Login;
using System;
using System.Collections.Generic;
using System.Data;

namespace Accounting._1.Interface
{
    public interface IPermission
    {
        DataTable GetRoleList();
        IEnumerable<Permission> GetRolePermissions(string roleName, string moduleName);
        IEnumerable<Permission> GetPermissionsForUser(string userId, string moduleName);
        IEnumerable<Permission> GetMasterAkses(string moduleName);

        IEnumerable<Permission_Users> GetUserList();
        int GetModuleId(string moduleName);
        IEnumerable<Permission_Users> GetUserLevelList(int moduleid);
        void UpdateUserRole(int newRoleId, string userId, int moduleId);

        bool CanUserPerformAction(string userId, string moduleName, string permissionName, Func<Permission, bool> actionCheck);
    }
}
