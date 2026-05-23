using Dapper;
using Accounting.Interface;
using Accounting.Models.Login;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using Accounting.Utilities;
using DevExpress.Mvvm.Native;

namespace Accounting.DataAccess
{
    public class UserManager_DA : IUsersManager
    {
        public void AddUser(USERID_DTO user)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());

            dbConnection.Open();
            string insertMasterQuery = "INSERT INTO MASTER_LOGIN (USERID, NAMA, DEPT, PASSWORD, JABATAN, AKTIF) VALUES (:USERID, :NAMA, :DEPT, :PASSWORD, :JABATAN, :AKTIF)";
            dbConnection.Execute(insertMasterQuery, user);
        }

        


        public void DeleteIDData(int APPSID)
        {
            using OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            using OracleCommand _command = new("DELETE FROM MASTER_APPS_DETAIL WHERE APPSDETAILID = :p_APPSID", dbConnection);
            _command.CommandType = CommandType.Text;

            // Create parameter and add it to the Parameters collection
            OracleParameter parameter = new(":p_APPSID", OracleDbType.Int16)
            {
                Value = APPSID
            };
            _command.Parameters.Add(parameter);

            // Use ExecuteNonQuery for DELETE operation
            _command.ExecuteNonQuery();
        }

        public void DeleteUser(string userId)
        {
            using OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            using OracleCommand _command = new("DELETE FROM master_login WHERE userid = :p_userid", dbConnection);
            _command.CommandType = CommandType.Text;

            // Create parameter and add it to the Parameters collection
            OracleParameter parameter = new OracleParameter(":p_userid", OracleDbType.Varchar2, 30);
            parameter.Value = userId;
            _command.Parameters.Add(parameter);

            // Use ExecuteNonQuery for DELETE operation
            _command.ExecuteNonQuery();
        }

        public List<AKSES_ESTATE_DTO> GetAksesEstatebyUserID(string userId, string modulename)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            string sqlQuery = @"SELECT URE.ESTATE_ID,MR.ROLE_ID,MR.ROLE_NAME,E.ESTATEID,E.NAMA NAMA_ESTATE,E.IDDATA,PM.NAMAPT,PD.WILAYAH
                            FROM MASTER_USER_ROLES_EST URE
                            JOIN MASTER_USER_ROLES UR ON UR.USER_ID = URE.USER_ID
                            JOIN MASTER_MODULES M ON M.MODULE_ID=UR.MODULE_ID
                            JOIN MASTER_ROLES MR ON MR.ROLE_ID=UR.ROLE_ID
                            JOIN MASTER_ESTATE E ON E.ID=URE.ESTATE_ID
                            JOIN MASTER_PT_DTL PD ON PD.IDDATA=E.IDDATA
                            JOIN MASTER_PT_HDR PM ON PM.IDPT=PD.IDPT
                            WHERE UR.USER_ID = :p_userid AND M.MODULE_NAME=:p_MODULE";

            var akses_estate = dbConnection.Query<AKSES_ESTATE_DTO>(sqlQuery, new { p_userid = userId, p_MODULE = modulename }).ToList();

            return akses_estate;
        }

        public List<USERID_DTO> GetAllUsers()
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            // Example query
            string query = "SELECT USERID, NAMA, DEPT, JABATAN,AKTIF FROM MASTER_LOGIN";

            // Execute query and retrieve results
            var users = dbConnection.Query<USERID_DTO>(query).ToList();

            return users;
        }

        public List<appsdetail_dto> GetAppDetailsByUserId(string userId, string modulename)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            string sqlQuery = @"SELECT APPSDETAILID, D.LEVELID, L.NAMA AS LEVEL_USER, D.ESTATE,D.IDDATA,PM.NAMAPT,PD.WILAYAH
                            FROM MASTER_APPS_DETAIL D
                            JOIN MASTER_LOGIN_LEVEL L ON L.APPSID = D.APPID AND L.LEVELID = D.LEVELID
                            JOIN MASTER_PT_DTL PD ON PD.IDDATA=D.IDDATA
                            JOIN MASTER_PT_HDR PM ON PM.IDPT=PD.IDPT
                            WHERE D.USERID = :p_userid AND D.APPID=:p_appid";

            var appDetailsList = dbConnection.Query<appsdetail_dto>(sqlQuery, new { p_userid = userId, p_appid = modulename }).ToList();

            return appDetailsList;
        }

        public string GetHashPassword(string userId)
        { 
            string password = null; 
            using (OracleConnection connection = new(ConnectionManager.GetOracleConnection()))
            {
                connection.Open();

                string sql = "SELECT password FROM master_login WHERE LOWER(userid)=LOWER(:u)";

                using OracleCommand command = new OracleCommand(sql, connection);
                command.Parameters.Add("u", OracleDbType.Varchar2).Value = userId;

                using OracleDataReader reader = command.ExecuteReader();
                if (reader.Read())
                { 
                    password = reader["password"].ToString(); 
                }
            } 
            return password; 
        }

        public USERID_DTO? GetUserCredential(string userId)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            const string query = @"
                SELECT USERID,
                       PASSWORD,
                       AKTIF,
                       NVL(FAILED_LOGIN_COUNT, 0) AS FAILED_LOGIN_COUNT,
                       LOCKOUT_UNTIL_UTC,
                       LAST_LOGIN_AT_UTC,
                       LAST_FAILED_LOGIN_AT_UTC,
                       PASSWORD_CHANGED_AT_UTC,
                       NVL(PASSWORD_RESET_REQUIRED, 'N') AS PASSWORD_RESET_REQUIRED
                FROM MASTER_LOGIN
                WHERE LOWER(USERID) = LOWER(:p_userid)
                  AND ROWNUM = 1";

            return dbConnection.QueryFirstOrDefault<USERID_DTO>(query, new { p_userid = userId });
        }

        public bool HasModuleAccess(string userId, string modulename)
        {
            return HasRbacModuleAccess(userId, modulename) || HasLegacyModuleAccess(userId, modulename);
        }

        public bool HasRbacModuleAccess(string userId, string modulename)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            const string query = @"
                SELECT 1
                FROM MASTER_USER_ROLES UR
                JOIN MASTER_MODULES M ON M.MODULE_ID = UR.MODULE_ID
                WHERE UR.USER_ID = :p_userid
                  AND M.MODULE_NAME = :p_module
                  AND ROWNUM = 1";

            int? found = dbConnection.QueryFirstOrDefault<int?>(query, new { p_userid = userId, p_module = modulename });
            return found.HasValue;
        }

        public bool HasLegacyModuleAccess(string userId, string modulename)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            const string query = @"
                SELECT 1
                FROM MASTER_LOGIN U
                JOIN MASTER_APPS_DETAIL D ON D.USERID = U.USERID
                WHERE LOWER(D.USERID) = LOWER(:p_userid)
                  AND D.APPID = :p_module
                  AND U.AKTIF = 'Y'
                  AND ROWNUM = 1";

            int? found = dbConnection.QueryFirstOrDefault<int?>(query, new { p_userid = userId, p_module = modulename });
            return found.HasValue;
        }


        public Dictionary<int, string> GetUserLevel()
        {
            Dictionary<int, string> resultDictionary = new();

            using (OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection()))
            {
                dbConnection.Open();

                string selectQuery = "SELECT ROLE_ID,ROLE_NAME FROM MASTER_ROLES ORDER BY ROLE_ID";

                using OracleCommand cmd = new(selectQuery, dbConnection);

                using OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int ROLE_ID;
                    if (reader["ROLE_ID"] != DBNull.Value)
                    {
                        ROLE_ID = Convert.ToInt32(reader["ROLE_ID"]);
                    }
                    else
                    {
                        // Handle NULL value for ID, e.g., assign a default value or skip the entry
                        continue;
                    }

                    string ROLE_NAME;
                    if (reader["ROLE_NAME"] != DBNull.Value)
                    { 
                        ROLE_NAME = Convert.ToString(reader["ROLE_NAME"]); 
                    }
                    else
                    {
                        // Handle NULL value for Nama, e.g., assign a default value or skip the entry
                        continue;
                    } 
                    resultDictionary.Add(ROLE_ID, ROLE_NAME); 
                }
            }

            return resultDictionary;
        }

        public List<WILAYAH_ESTATE_DTO> GetWilayahEstate()
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            string query = @"SELECT  E.ID,E.ESTATEID, E.NAMA ESTATE,p.namapt,d.wilayah,d.iddata
                           FROM master_pt_hdr p 
                           JOIN master_pt_dtl d ON d.idpt = p.idpt
                           JOIN MASTER_ESTATE E ON E.IDDATA=D.IDDATA";

            // Dapper will handle opening and closing the connection
            return dbConnection.Query<WILAYAH_ESTATE_DTO>(query).AsList();
        }

        public void UpdateUser(USERID_DTO updatedUser)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            string updateMasterQuery;
            if (string.IsNullOrEmpty(updatedUser.PASSWORD))
            {
                // Update tanpa PASSWORD — jangan timpa hash lama
                updateMasterQuery = @"
                    UPDATE MASTER_LOGIN
                    SET NAMA = :NAMA, DEPT = :DEPT, JABATAN = :JABATAN, AKTIF=:AKTIF
                    WHERE LOWER(USERID) = LOWER(:USERID)";
            }
            else
            {
                updateMasterQuery = @"
                    UPDATE MASTER_LOGIN
                    SET NAMA = :NAMA, DEPT = :DEPT, PASSWORD = :PASSWORD, JABATAN = :JABATAN, AKTIF=:AKTIF
                    WHERE LOWER(USERID) = LOWER(:USERID)";
            }

            int affectedRows = dbConnection.Execute(updateMasterQuery, updatedUser);
            if (affectedRows == 0)
            {
                throw new InvalidOperationException($"User '{updatedUser.USERID}' tidak ditemukan di database.");
            }
        }

        public List<LOGIN_USERS_DTO> UserLogin(string userId, string appName)
        {
            LoginInfo.OracleConnString = ConnectionManager.GetOracleConnection();
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            // Replace ":p_userid" and ":APPNAME" with actual parameter names
            string query = @"
                SELECT U.USERID, U.NAMA, L.NAMA AS LEVEL_USER,
                       D.IDDATA, PM.NAMAPT, PD.WILAYAH
                FROM MASTER_LOGIN U
                JOIN MASTER_APPS_DETAIL D ON D.USERID = U.USERID
                JOIN MASTER_LOGIN_LEVEL L ON L.APPSID = D.APPID AND L.LEVELID = D.LEVELID
                JOIN MASTER_PT_DTL PD ON PD.IDDATA = D.IDDATA
                JOIN MASTER_PT_HDR PM ON PM.IDPT = PD.IDPT
                WHERE D.USERID = :p_userid AND D.APPID = :APPNAME AND U.AKTIF='Y' ";

            // Use Dapper to execute the query and map the results to the LOGIN_USERS_DTO class
            List<LOGIN_USERS_DTO> userDataList = dbConnection.Query<LOGIN_USERS_DTO>(query, new { p_userid = userId, APPNAME = appName }).ToList();

            return userDataList;
        }

        public List<LOGIN_USERS_DTO> UserLoginByEstate(string userId, string appName)
        {
            LoginInfo.OracleConnString = ConnectionManager.GetOracleConnection();
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            // Replace ":p_userid" and ":APPNAME" with actual parameter names
            string query = @"SELECT U.USERID,U.NAMA,MR.ROLE_NAME LEVEL_USER,PD.IDDATA,E.ESTATEID ESTATE,E.NAMA ESTATENAMA,PM.NAMAPT,PD.WILAYAH
                            FROM MASTER_LOGIN U
                            JOIN MASTER_USER_ROLES_EST URE ON URE.USER_ID=U.USERID
                            JOIN MASTER_USER_ROLES UR ON UR.USER_ID = URE.USER_ID
                            JOIN MASTER_MODULES M ON M.MODULE_ID=UR.MODULE_ID
                            JOIN MASTER_ROLES MR ON MR.ROLE_ID=UR.ROLE_ID
                            JOIN MASTER_ESTATE E ON E.ID=URE.ESTATE_ID
                            JOIN MASTER_PT_DTL PD ON PD.IDDATA=E.IDDATA
                            JOIN MASTER_PT_HDR PM ON PM.IDPT=PD.IDPT
                            WHERE UR.USER_ID = :p_userid AND M.MODULE_NAME=:p_MODULE AND U.AKTIF='Y'";

            // Use Dapper to execute the query and map the results to the LOGIN_USERS_DTO class
            List<LOGIN_USERS_DTO> userDataList = dbConnection.Query<LOGIN_USERS_DTO>(query, new { p_userid = userId, p_MODULE = appName }).ToList();

            return userDataList;
        }

        public void AddUserRole(string userid, int role_id, int moduleid)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            // Define the insert query
            string insertDetailQuery = @"
                INSERT INTO MASTER_USER_ROLES (                    
                    USER_ID,ROLE_ID,MODULE_ID
                ) VALUES (
                    :USER_ID,
                    :ROLE_ID,
                    :MODULE_ID
                )";

            // Define the parameters
            var detailParameters = new
            {
                USER_ID = userid,
                ROLE_ID = role_id,
                MODULE_ID = moduleid
            };

            // Execute the insert query with parameters
            dbConnection.Execute(insertDetailQuery, detailParameters);
        }

        public List<LOGIN_USERS_DTO> UserLoginByIDDATA(string userId, string appName)
        {
            LoginInfo.OracleConnString = ConnectionManager.GetOracleConnection();
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            // Replace ":p_userid" and ":APPNAME" with actual parameter names
            string query = @"SELECT U.USERID,U.NAMA,MR.ROLE_NAME LEVEL_USER,PD.IDDATA,PM.NAMAPT,PD.WILAYAH,PD.JENIS_AKUNTANSI
                            FROM MASTER_LOGIN U
                            JOIN MASTER_USER_ROLES_LOC LOC ON LOC.USER_ID=U.USERID
                            JOIN MASTER_USER_ROLES UR ON UR.USER_ID = LOC.USER_ID
                            JOIN MASTER_MODULES M ON M.MODULE_ID=UR.MODULE_ID
                            JOIN MASTER_ROLES MR ON MR.ROLE_ID=UR.ROLE_ID
                            JOIN MASTER_PT_DTL PD ON PD.IDDATA=LOC.IDDATA
                            JOIN MASTER_PT_HDR PM ON PM.IDPT=PD.IDPT
                            WHERE UR.USER_ID = :p_userid AND M.MODULE_NAME=:p_MODULE AND U.AKTIF='Y'";

            // Use Dapper to execute the query and map the results to the LOGIN_USERS_DTO class
            List<LOGIN_USERS_DTO> userDataList = dbConnection.Query<LOGIN_USERS_DTO>(query, new { p_userid = userId, p_MODULE = appName }).ToList();

            if (userDataList.Count == 0)
            {
                userDataList = GetLegacyUserLoginByIdData(dbConnection, userId, appName);
            }

            return userDataList;
        }

        private static List<LOGIN_USERS_DTO> GetLegacyUserLoginByIdData(IDbConnection dbConnection, string userId, string appName)
        {
            const string query = @"
                SELECT U.USERID,
                       U.NAMA,
                       L.NAMA AS LEVEL_USER,
                       D.IDDATA,
                       PM.NAMAPT,
                       PD.WILAYAH,
                       PD.JENIS_AKUNTANSI,
                       1 AS IsLegacyAccessPath
                FROM MASTER_LOGIN U
                JOIN MASTER_APPS_DETAIL D ON D.USERID = U.USERID
                JOIN MASTER_LOGIN_LEVEL L ON L.APPSID = D.APPID AND L.LEVELID = D.LEVELID
                JOIN MASTER_PT_DTL PD ON PD.IDDATA = D.IDDATA
                JOIN MASTER_PT_HDR PM ON PM.IDPT = PD.IDPT
                WHERE LOWER(D.USERID) = LOWER(:p_userid)
                  AND D.APPID = :p_module
                  AND U.AKTIF = 'Y'";

            return dbConnection.Query<LOGIN_USERS_DTO>(query, new { p_userid = userId, p_module = appName }).ToList();
        }

        public List<WILAYAH_IDDATA_DTO> GetListIDDATA()
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            string query = @"SELECT d.iddata, d.wilayah, p.namapt 
                           FROM master_pt_hdr p 
                           JOIN master_pt_dtl d ON d.idpt = p.idpt";

            // Dapper will handle opening and closing the connection
            return dbConnection.Query<WILAYAH_IDDATA_DTO>(query).AsList();
        }

        public List<AKSES_IDDATA_DTO> GetAksesIDDATAbyUserID(string userId, string modulename)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            string sqlQuery = @"SELECT MR.ROLE_ID,MR.ROLE_NAME,LOC.IDDATA,PM.NAMAPT,PD.WILAYAH
                            FROM MASTER_USER_ROLES_LOC LOC
                            JOIN MASTER_USER_ROLES UR ON UR.USER_ID = LOC.USER_ID
                            JOIN MASTER_MODULES M ON M.MODULE_ID=UR.MODULE_ID
                            JOIN MASTER_ROLES MR ON MR.ROLE_ID=UR.ROLE_ID
                            JOIN MASTER_PT_DTL PD ON PD.IDDATA=LOC.IDDATA
                            JOIN MASTER_PT_HDR PM ON PM.IDPT=PD.IDPT
                            WHERE UR.USER_ID = :p_userid AND M.MODULE_NAME=:p_MODULE";

            var akses_iddata = dbConnection.Query<AKSES_IDDATA_DTO>(sqlQuery, new { p_userid = userId, p_MODULE = modulename }).ToList();

            return akses_iddata;
        }

        public void AddUserRole_Estate(string userid, int estateid)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            // Define the insert query
            string insertDetailQuery = @"
                INSERT INTO MASTER_USER_ROLES_EST (                    
                    USER_ID,
                    ESTATE_ID
                ) VALUES (
                    :userid,
                    :estateid
                )";

            // Define the parameters
            var detailParameters = new
            {
                userid,estateid
            };

            // Execute the insert query with parameters
            dbConnection.Execute(insertDetailQuery, detailParameters);
        }

        public void AddUserRole_IDDATA(string userid, string iddata)
        {
            using IDbConnection dbConnection = new OracleConnection(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            // Define the insert query
            string insertDetailQuery = @"
                INSERT INTO MASTER_USER_ROLES_LOC (                    
                    USER_ID,
                    IDDATA
                ) VALUES (
                    :userid,
                    :iddata
                )";

            // Define the parameters
            var detailParameters = new
            {
                userid,
                iddata
            };

            // Execute the insert query with parameters
            dbConnection.Execute(insertDetailQuery, detailParameters);
        }
        public void ResetPassword(string userId, string newHashedPassword)
        {
            using OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            string updateQuery = @"
                UPDATE MASTER_LOGIN
                SET PASSWORD = :p_password,
                    PASSWORD_CHANGED_AT_UTC = CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS TIMESTAMP)
                WHERE LOWER(USERID) = LOWER(:p_userid)";
            dbConnection.Execute(updateQuery, new { p_password = newHashedPassword, p_userid = userId });
        }

        public void SetPasswordResetRequired(string userId, bool required)
        {
            using OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            const string updateQuery = @"
                UPDATE MASTER_LOGIN
                SET PASSWORD_RESET_REQUIRED = :p_required
                WHERE LOWER(USERID) = LOWER(:p_userid)";

            dbConnection.Execute(updateQuery, new { p_required = required ? "Y" : "N", p_userid = userId });
        }

        public void RegisterFailedLoginAttempt(string userId, int maxFailedAttempts, int lockoutMinutes)
        {
            using OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            const string updateQuery = @"
                UPDATE MASTER_LOGIN
                SET FAILED_LOGIN_COUNT = NVL(FAILED_LOGIN_COUNT, 0) + 1,
                    LAST_FAILED_LOGIN_AT_UTC = CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS TIMESTAMP),
                    LOCKOUT_UNTIL_UTC = CASE
                        WHEN NVL(FAILED_LOGIN_COUNT, 0) + 1 >= :p_max_failed_attempts
                            THEN CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS TIMESTAMP) + NUMTODSINTERVAL(:p_lockout_minutes, 'MINUTE')
                        ELSE NULL
                    END
                WHERE LOWER(USERID) = LOWER(:p_userid)";

            dbConnection.Execute(updateQuery, new
            {
                p_max_failed_attempts = maxFailedAttempts,
                p_lockout_minutes = lockoutMinutes,
                p_userid = userId
            });
        }

        public void ResetLoginFailures(string userId)
        {
            using OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            const string updateQuery = @"
                UPDATE MASTER_LOGIN
                SET FAILED_LOGIN_COUNT = 0,
                    LOCKOUT_UNTIL_UTC = NULL
                WHERE LOWER(USERID) = LOWER(:p_userid)";

            dbConnection.Execute(updateQuery, new { p_userid = userId });
        }

        public void UpdateSuccessfulLogin(string userId)
        {
            using OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            const string updateQuery = @"
                UPDATE MASTER_LOGIN
                SET FAILED_LOGIN_COUNT = 0,
                    LOCKOUT_UNTIL_UTC = NULL,
                    LAST_LOGIN_AT_UTC = CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS TIMESTAMP)
                WHERE LOWER(USERID) = LOWER(:p_userid)";

            dbConnection.Execute(updateQuery, new { p_userid = userId });
        }

        public void RecordLoginAudit(LoginAuditRecord record)
        {
            using OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            const string insertQuery = @"
                INSERT INTO ACCT_LOGIN_AUDIT
                    (EVENT_AT_UTC, USERID, MODULE_NAME, EVENT_TYPE, SUCCESS_FLAG, CLIENT_MACHINE, DETAIL_MESSAGE, LOCKOUT_UNTIL_UTC, IDDATA)
                VALUES
                    (CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) AS TIMESTAMP), :USERID, :MODULE_NAME, :EVENT_TYPE, :SUCCESS_FLAG, :CLIENT_MACHINE, :DETAIL_MESSAGE, :LOCKOUT_UNTIL_UTC, :IDDATA)";

            dbConnection.Execute(insertQuery, record);
        }

        public void DeleteAksesEstate(string userid, int estateid)
        {
            using OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            string deleteQuery = "DELETE FROM MASTER_USER_ROLES_EST WHERE USER_ID = :P_USERID AND ESTATE_ID = :P_ESTATEID";

            var parameters = new { P_USERID = userid, P_ESTATEID = estateid };

            dbConnection.Execute(deleteQuery, parameters);
        }
        public void DeleteIDData(string userid, string iddata)
        {
            using OracleConnection dbConnection = new(ConnectionManager.GetOracleConnection());
            dbConnection.Open();

            string deleteQuery = "DELETE FROM MASTER_USER_ROLES_LOC WHERE USER_ID = :userid AND IDDATA = :iddata";

            var parameters = new {  userid,iddata };

            dbConnection.Execute(deleteQuery, parameters);
        }
    }
}
