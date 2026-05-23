using Accounting._1.Interface;
using Accounting._2.DataAccess;
using Accounting.Models.Login;
using Accounting.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting._3.Services
{
    internal class Permission_Services
    {
        static readonly IPermission repository;
        static Permission_Services()
        {
            repository = new Permission_DA();
        }
        public static DataTable GetRoleList()
        {
            return repository.GetRoleList();
        }

        public static IEnumerable<RoleSummary> GetRoleSummaries(int moduleId)
        {
            return repository.GetRoleSummaries(moduleId);
        }

        public static IEnumerable<Permission> GetRolePermissions(string roleName, string moduleName)
        {
            return repository.GetRolePermissions(roleName, moduleName);
        }

        public static IEnumerable<Permission> GetRolePermissionMatrix(int roleId, string moduleName)
        {
            return repository.GetRolePermissionMatrix(roleId, moduleName);
        }

        public static IEnumerable<Permission> GetPermissionsForUser(string userId, string moduleName)
        {
            return repository.GetPermissionsForUser(userId, moduleName);
        }

        public static IEnumerable<Permission> GetMasterAkses(string moduleName)
        {
            return repository.GetMasterAkses(moduleName);
        }


        public static bool CanUserPerformAction(string userId, string moduleName, string permissionName, Func<Permission, bool> actionCheck)
        {
            return repository.CanUserPerformAction(userId, moduleName, permissionName, actionCheck);
        }

        public static IEnumerable<Permission_Users> GetUserList()
        {
            return repository.GetUserList();
        }
        public static int GetModuleId(string moduleName)
        {
            return repository.GetModuleId(moduleName);
        }
        public static IEnumerable<Permission_Users> GetUserLevelList(int moduleid)
        {
            return repository.GetUserLevelList(moduleid);
        }

        public static IEnumerable<Permission_Users> GetUsersByRole(int roleId, int? moduleId)
        {
            return repository.GetUsersByRole(roleId, moduleId);
        }

        public static void UpdateUserRole(int newRoleId, string userId, int moduleId)
        {
            AuthorizationService.EnsureCanManageRoleAssignments(userId, newRoleId);
            repository.UpdateUserRole(newRoleId, userId, moduleId);
            AuthorizationService.InvalidateCurrentUserPermissions();
        }

        public static int DuplicateRole(string roleName, int sourceRoleId, string moduleName)
        {
            AuthorizationService.EnsureCanManageRolePermissions(sourceRoleId);

            int newRoleId = repository.DuplicateRole(roleName, sourceRoleId, moduleName);
            AuthorizationService.InvalidateAllPermissions();
            return newRoleId;
        }

        public static void RenameRole(int roleId, string roleName)
        {
            AuthorizationService.EnsureCanManageRolePermissions(roleId);
            repository.RenameRole(roleId, roleName);
            AuthorizationService.InvalidateAllPermissions();
        }

        public static void DeleteRole(int roleId)
        {
            AuthorizationService.EnsureCanManageRolePermissions(roleId);
            repository.DeleteRole(roleId);
            AuthorizationService.InvalidateAllPermissions();
        }

        public static void SaveRolePermissions(int roleId, IEnumerable<Permission> permissions, string moduleName)
        {
            AuthorizationService.EnsureCanManageRolePermissions(roleId);
            repository.SaveRolePermissions(roleId, permissions, moduleName);
            AuthorizationService.InvalidateAllPermissions();
        }

        public static void AssignUsersToRole(IEnumerable<string> userIds, int roleId, int moduleId)
        {
            string[] effectiveUserIds = userIds?.Where(userId => !string.IsNullOrWhiteSpace(userId)).ToArray() ?? Array.Empty<string>();
            foreach (string userId in effectiveUserIds)
            {
                AuthorizationService.EnsureCanManageRoleAssignments(userId, roleId);
            }

            repository.AssignUsersToRole(effectiveUserIds, roleId, moduleId);
            AuthorizationService.InvalidateAllPermissions();
        }

        public static void RemoveUsersFromRole(IEnumerable<string> userIds, int moduleId)
        {
            string[] effectiveUserIds = userIds?.Where(userId => !string.IsNullOrWhiteSpace(userId)).ToArray() ?? Array.Empty<string>();
            foreach (string userId in effectiveUserIds)
            {
                AuthorizationService.EnsureCanManageRoleAssignments(userId);
            }

            repository.RemoveUsersFromRole(effectiveUserIds, moduleId);
            AuthorizationService.InvalidateAllPermissions();
        }
    }
}
