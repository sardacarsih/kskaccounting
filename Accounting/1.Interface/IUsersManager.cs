using Accounting.Models.Login;
using System.Collections.Generic;

namespace Accounting.Interface
{
    public interface IUsersManager
    {
        void AddUser(USERID_DTO user);
        void UpdateUser(USERID_DTO updatedUser);
        void DeleteUser(string userId);
        List<USERID_DTO> GetAllUsers();
        List<WILAYAH_IDDATA_DTO> GetListIDDATA();
        List<WILAYAH_ESTATE_DTO> GetWilayahEstate();
        Dictionary<int, string> GetUserLevel();
        void AddUserRole(string userid, int role_id, int moduleid);
        void AddUserRole_Estate(string userid,int estateid);
        void AddUserRole_IDDATA(string userid, string iddata);
        void DeleteIDData(string userid, string iddata);
        void DeleteAksesEstate(string userid, int estateid);
        List<appsdetail_dto> GetAppDetailsByUserId(string userId, string modulename);
        List<LOGIN_USERS_DTO> UserLogin(string userId, string appName);
        List<LOGIN_USERS_DTO> UserLoginByEstate(string userId, string appName);
        List<LOGIN_USERS_DTO> UserLoginByIDDATA(string userId, string appName);
        USERID_DTO? GetUserCredential(string userId);
        bool HasModuleAccess(string userId, string modulename);
        string GetHashPassword(string userId);
        void ResetPassword(string userId, string newHashedPassword);

        List<AKSES_ESTATE_DTO> GetAksesEstatebyUserID(string userId, string modulename);
        List<AKSES_IDDATA_DTO> GetAksesIDDATAbyUserID(string userId, string modulename);


    }
}
