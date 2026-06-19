using Accounting.DataAccess;
using Accounting.Interface;
using Accounting.Models.Login;
using System;
using System.Collections.Generic;

namespace Accounting.Services
{
    public class UserManager_Services
    {
        static readonly IUsersManager repository;

        static UserManager_Services()
        {
            repository = new UserManager_DA();
        }

        public static void AddUser(USERID_DTO user)
        {
            AuthorizationService.EnsureCanManageUsers();
            repository.AddUser(user);
        }

        public static void AddUserRole(string userid, int role_id, int moduleid)
        {
            AuthorizationService.EnsureCanManageUsers(userid);
            repository.AddUserRole(userid, role_id, moduleid);
        }

        public static void AddUserRole_Estate(string userid, int estateid)
        {
            AuthorizationService.EnsureCanManageUserLocationAccess(userid);
            repository.AddUserRole_Estate(userid, estateid);
        }

        public static void AddUserRole_IDDATA(string userid, string iddata)
        {
            AuthorizationService.EnsureCanManageUserLocationAccess(userid);
            repository.AddUserRole_IDDATA(userid, iddata);
        }

        public static void DeleteAksesEstate(string userid, int estateid)
        {
            AuthorizationService.EnsureCanManageUserLocationAccess(userid);
            repository.DeleteAksesEstate(userid, estateid);
        }

        public static void DeleteIDData(string userid, string iddata)
        {
            AuthorizationService.EnsureCanManageUserLocationAccess(userid);
            repository.DeleteIDData(userid, iddata);
        }

        public static void UpdateUser(USERID_DTO user)
        {
            AuthorizationService.EnsureCanManageUsers(user.USERID);
            repository.UpdateUser(user);
        }

        public static void DeleteUser(string user)
        {
            AuthorizationService.EnsureCanManageUsers(user);
            repository.DeleteUser(user);
        }

        public static List<USERID_DTO> GetAllUsers()
        {
            return repository.GetAllUsers();
        }

        public static List<WILAYAH_ESTATE_DTO> GetWilayahEstate()
        {
            return repository.GetWilayahEstate();
        }

        public static List<WILAYAH_IDDATA_DTO> GetListIDDATA()
        {
            return repository.GetListIDDATA();
        }

        public static Dictionary<int, string> GetUserLevel()
        {
            return repository.GetUserLevel();
        }

        public static List<appsdetail_dto> GetAppDetailsByUserId(string userId, string modulename)
        {
            return repository.GetAppDetailsByUserId(userId, modulename);
        }

        public static List<LOGIN_USERS_DTO> UserLogin(string userId, string appName)
        {
            return repository.UserLogin(userId, appName);
        }

        public static List<LOGIN_USERS_DTO> UserLoginByEstate(string userId, string appName)
        {
            return repository.UserLoginByEstate(userId, appName);
        }

        public static List<LOGIN_USERS_DTO> UserLoginByIDDATA(string userId, string appName)
        {
            return repository.UserLoginByIDDATA(userId, appName);
        }

        public static List<AKSES_ESTATE_DTO> GetAksesEstatebyUserID(string userId, string appName)
        {
            return repository.GetAksesEstatebyUserID(userId, appName);
        }

        public static List<AKSES_IDDATA_DTO> GetAksesIDDATAbyUserID(string userId, string appName)
        {
            return repository.GetAksesIDDATAbyUserID(userId, appName);
        }

        public static USERID_DTO? GetUserCredential(string userId)
        {
            return repository.GetUserCredential(userId);
        }

        public static bool HasModuleAccess(string userId, string appName)
        {
            return repository.HasModuleAccess(userId, appName);
        }

        public static LoginAuthResult AuthenticateForModule(string userId, string password, string moduleName)
        {
            return AuthenticationService.AuthenticateForModule(repository, userId, password, moduleName);
        }

        public static void ResetPassword(string userId, string newPlainPassword)
        {
            var crypto = new PasswordCryptographyPbkdf2();
            string hashedPassword = crypto.GetHashPassword(newPlainPassword);
            repository.ResetPassword(userId, hashedPassword);
            repository.SetPasswordResetRequired(userId, false);
        }

        public static string AdminResetPassword(string userId, string moduleName, bool bypassAuthorization = false)
        {
            if (!bypassAuthorization)
            {
                AuthorizationService.EnsureCanResetPassword(userId);
            }

            return AuthenticationService.AdminResetPassword(repository, userId, moduleName);
        }

        public static bool TryValidatePasswordPolicy(string password, out string validationMessage)
        {
            return AuthenticationService.TryValidatePasswordPolicy(password, out validationMessage);
        }

        public static bool TryChangePassword(string userId, string currentPassword, string newPassword, out string validationMessage)
        {
            bool changed = AuthenticationService.TryChangePassword(repository, userId, currentPassword, newPassword, out validationMessage);
            if (changed)
            {
                AppSession.MarkPasswordChangeCompleted();
            }

            return changed;
        }

        public static bool TryChangePasswordAfterVerifiedLogin(string userId, string newPassword, out string validationMessage)
        {
            bool changed = AuthenticationService.TryChangePasswordAfterVerifiedLogin(repository, userId, newPassword, out validationMessage);
            if (changed)
            {
                AppSession.MarkPasswordChangeCompleted();
            }

            return changed;
        }

        public static void RecordLocationSelection(string userId, string moduleName, string idData)
        {
            AuthenticationService.RecordLocationSelection(repository, userId, moduleName, idData);
        }

        public static string GetHashPassword(string userId)
        {
            return repository.GetHashPassword(userId);
        }
    }
}
