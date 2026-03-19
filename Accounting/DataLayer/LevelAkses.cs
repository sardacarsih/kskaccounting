using Accounting.Interface;
using DevExpress.XtraMap;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    public class LevelAkses : ILevelAkses
    {
        public bool BaruImport(int aksiid, string userid)
        {
            throw new NotImplementedException();
        }

        public bool CetakExport(int aksiid, string userid)
        {
            throw new NotImplementedException();
        }

        public bool Hapus(int aksiid, string userid)
        {
            throw new NotImplementedException();
        }

        public bool OpenForm(string userId, int aksi, int lvl, string idData, string appId)
        {
            using (OracleConnection connection = new(LoginInfo.OracleConnString))
            {
                connection.Open();

                using (OracleCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                    SELECT ma.BUKA
                    FROM master_login l
                    JOIN MASTER_APPS_DETAIL MAD ON MAD.USERID = l.USERID
                    JOIN master_login_level ll ON LL.APPSID = MAD.APPID
                    JOIN master_akses ma ON ll.levelid = ma.levelid
                    WHERE ma.AKSIID = :AKSI
                      AND l.userid = :USERID
                      AND ll.nama = :LVL
                      AND MAD.IDDATA = :IDDATA
                      AND ma.appsid = :APPSID";

                    command.Parameters.Add("AKSI", OracleDbType.Varchar2).Value = aksi;
                    command.Parameters.Add("USERID", OracleDbType.Varchar2).Value = userId;
                    command.Parameters.Add("LVL", OracleDbType.Varchar2).Value = lvl;
                    command.Parameters.Add("IDDATA", OracleDbType.Varchar2).Value = idData;
                    command.Parameters.Add("APPSID", OracleDbType.Varchar2).Value = appId;

                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Check if the value of "BUKA" is 'Y' for true, otherwise false
                            return string.Equals(reader["BUKA"].ToString(), "Y", StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }
            }

            // If no records are found, return false or handle it based on your requirement
            return false;
        }

        public bool Simpan(int aksiid, string userid)
        {
            throw new NotImplementedException();
        }

        public bool Ubah(int aksiid, string userid)
        {
            throw new NotImplementedException();
        }
    }
}
