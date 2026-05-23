
using Accounting.Models.Login;
using System;
using System.Collections.Generic;
using System.Data;

namespace Accounting._1.Interface
{
    public interface IPermission
    {
        DataTable GetRoleList();
        IEnumerable<RoleSummary> GetRoleSummaries(int moduleId);
        IEnumerable<Permission> GetRolePermissions(string roleName, string moduleName);
        IEnumerable<Permission> GetRolePermissionMatrix(int roleId, string moduleName);
        IEnumerable<Permission> GetPermissionsForUser(string userId, string moduleName);
        IEnumerable<Permission> GetMasterAkses(string moduleName);

        IEnumerable<Permission_Users> GetUserList();
        int GetModuleId(string moduleName);
        IEnumerable<Permission_Users> GetUserLevelList(int moduleid);
        void UpdateUserRole(int newRoleId, string userId, int moduleId);
        IEnumerable<Permission_Users> GetUsersByRole(int roleId, int? moduleId);
        int DuplicateRole(string roleName, int sourceRoleId, string moduleName);
        void RenameRole(int roleId, string roleName);
        void DeleteRole(int roleId);
        void SaveRolePermissions(int roleId, IEnumerable<Permission> permissions, string moduleName);
        void AssignUsersToRole(IEnumerable<string> userIds, int roleId, int moduleId);
        void RemoveUsersFromRole(IEnumerable<string> userIds, int moduleId);

        bool CanUserPerformAction(string userId, string moduleName, string permissionName, Func<Permission, bool> actionCheck);
    }
}
