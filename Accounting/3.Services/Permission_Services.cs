using Accounting._1.Interface;
using Accounting._2.DataAccess;
using Accounting.Models.Login;
using System;
using System.Collections.Generic;
using System.Data;

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
        public static IEnumerable<Permission> GetRolePermissions(string roleName, string moduleName)
        {
            return repository.GetRolePermissions(roleName, moduleName);
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
        public static void UpdateUserRole(int newRoleId, string userId, int moduleId)
        {
            repository.UpdateUserRole(newRoleId, userId, moduleId);
        }
    }
}
