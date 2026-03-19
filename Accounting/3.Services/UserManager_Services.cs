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
            repository.AddUser(user);
        }

        public static void AddUserRole(string userid, int role_id, int moduleid)
        {
            repository.AddUserRole(userid, role_id, moduleid);
        }

        public static void AddUserRole_Estate(string userid, int estateid)
        {
            repository.AddUserRole_Estate(userid, estateid);
        }

        public static void AddUserRole_IDDATA(string userid, string iddata)
        {
            repository.AddUserRole_IDDATA(userid, iddata);
        }

        public static void DeleteAksesEstate(string userid, int estateid)
        {
            repository.DeleteAksesEstate(userid, estateid);
        }

        public static void DeleteIDData(string userid, string iddata)
        {
            repository.DeleteIDData(userid, iddata);
        }

        public static void UpdateUser(USERID_DTO user)
        {
            repository.UpdateUser(user);
        }

        public static void DeleteUser(string user)
        {
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
            USERID_DTO? credential = repository.GetUserCredential(userId);
            if (credential == null || string.IsNullOrWhiteSpace(credential.USERID))
            {
                return new LoginAuthResult { Status = LoginAuthStatus.UserNotFound };
            }

            if (!string.Equals(credential.AKTIF, "Y", StringComparison.OrdinalIgnoreCase))
            {
                return new LoginAuthResult { Status = LoginAuthStatus.InactiveUser };
            }

            // --- BEGIN MIGRATION BLOCK (transparent PBKDF2 migration) ---
            var passwordCryptography = new PasswordCryptographyPbkdf2();
            bool isValid;

            if (PasswordHashMigrator.NeedsMigration(credential.PASSWORD))
            {
                var format = PasswordHashMigrator.DetectFormat(credential.PASSWORD);
                isValid = PasswordHashMigrator.VerifyLegacyPassword(password, credential.PASSWORD, format);

                if (isValid)
                {
                    string newHash = passwordCryptography.GetHashPassword(password);
                    repository.ResetPassword(credential.USERID, newHash);
                }
            }
            else
            {
                isValid = passwordCryptography.IsValidPassword(password, credential.PASSWORD);
            }
            // --- END MIGRATION BLOCK ---

            if (!isValid)
            {
                return new LoginAuthResult { Status = LoginAuthStatus.InvalidPassword };
            }

            if (!repository.HasModuleAccess(credential.USERID, moduleName))
            {
                return new LoginAuthResult { Status = LoginAuthStatus.NoModuleAccess };
            }

            List<LOGIN_USERS_DTO> users = repository.UserLoginByIDDATA(credential.USERID, moduleName);
            if (users.Count == 0)
            {
                return new LoginAuthResult { Status = LoginAuthStatus.NoLocationAccess };
            }

            return new LoginAuthResult
            {
                Status = LoginAuthStatus.Success,
                Users = users
            };
        }

        public static void ResetPassword(string userId, string newPlainPassword)
        {
            var crypto = new PasswordCryptographyPbkdf2();
            string hashedPassword = crypto.GetHashPassword(newPlainPassword);
            repository.ResetPassword(userId, hashedPassword);
        }

        public static string GetHashPassword(string userId)
        {
            return repository.GetHashPassword(userId);
        }
    }
}
