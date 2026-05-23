using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace Accounting.DataLayer
{
    public class LevelAksesRepository : ILevelAksesRepository
    {
        private readonly OracleConnection conn = new( LoginInfo.OracleConnString);

        public bool BaruImport(int aksiid, string userid)
        {
            using OracleCommand cmd = new("select baru from vaksesgl where  " +
                "aksiid=:aksiid and userid=:userid", conn)
            {
                CommandType = CommandType.Text
            };
            conn.Open();
            cmd.Parameters.Add(":aksiid", OracleDbType.Int16).Value = aksiid;
            cmd.Parameters.Add(":userid", OracleDbType.Varchar2, 20).Value = userid;
            OracleDataReader dr;
            dr = cmd.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            char val = Convert.ToChar(_dt.Rows[0]["BARU"].ToString());
            if (val == 'Y')
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CetakExport(int aksiid, string userid)
        {
            try
            {
                using OracleConnection conn = new( LoginInfo.OracleConnString);
                conn.Open();

                using OracleCommand cmd = new("SELECT CETAK FROM vaksesgl WHERE aksiid = :aksiid AND userid = :userid", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new OracleParameter("aksiid", OracleDbType.Int16)).Value = aksiid;
                cmd.Parameters.Add(new OracleParameter("userid", OracleDbType.Varchar2, 20)).Value = userid;

                using OracleDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    char val = Convert.ToChar(dr["CETAK"].ToString());
                    return (val == 'Y');
                }
            }
            catch (Exception)
            {
                // Handle exceptions as per your application's error-handling strategy.
                // You can log the error or return a default value.
                // For this example, I'm returning null to indicate an error.
                // Consider logging the exception for debugging and monitoring.
                // You might also want to handle specific exceptions more gracefully.
                return false;
            }

            // If no data was found, return false to indicate no result.
            return false;
        }



        public bool Hapus(int aksiid, string userid)
        {
            try
            {

                using OracleConnection conn = new( LoginInfo.OracleConnString);
                conn.Open();

                string sql = "SELECT HAPUS FROM vaksesgl WHERE AKSIID = :aksiid AND USERID = :userid";

                using OracleCommand cmd = new(sql, conn);
                cmd.Parameters.Add(":aksiid", OracleDbType.Int32).Value = aksiid;
                cmd.Parameters.Add(":userid", OracleDbType.Varchar2, 20).Value = userid;

                using OracleDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    char val = Convert.ToChar(dr["HAPUS"].ToString());
                    return (val == 'Y');
                }
            }
            catch (Exception)
            {
                // Handle exceptions as per your application's error-handling strategy.
                // You can log the error or return a default value.
                // For this example, I'm returning null to indicate an error.
                // Consider logging the exception for debugging and monitoring.
                // You might also want to handle specific exceptions more gracefully.
                return false;
            }

            return false; // Default to false in case of errors or no matching rows.
        }



        public bool OpenForm(int aksiid, string userid)
        {
            try
            {
                using OracleConnection conn = new OracleConnection( LoginInfo.OracleConnString);
                conn.Open();
                using (OracleCommand cmd = new OracleCommand("SELECT buka FROM vaksesgl WHERE aksiid = :aksiid AND userid = :userid", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add("aksiid", OracleDbType.Int16).Value = aksiid;
                    cmd.Parameters.Add("userid", OracleDbType.Varchar2, 20).Value = userid;

                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            char val = Convert.ToChar(dr["BUKA"].ToString());
                            return (val == 'Y');
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions as per your application's error-handling strategy.
                // You can log the error or return a default value.
                // For this example, I'm returning false to indicate an error.
                // Consider logging the exception for debugging and monitoring.
                // You might also want to handle specific exceptions more gracefully.
                return false;
            }

            return false; // Return false if no data is found or an error occurs.
        }



        public bool Simpan(int aksiid, string userid)
        {
            try
            {
                using OracleConnection conn = new( LoginInfo.OracleConnString);
                conn.Open();
                using OracleCommand cmd = new("SELECT simpan FROM vaksesgl WHERE aksiid = :aksiid AND userid = :userid", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new OracleParameter("aksiid", OracleDbType.Int16)).Value = aksiid;
                cmd.Parameters.Add(new OracleParameter("userid", OracleDbType.Varchar2, 20)).Value = userid;

                using OracleDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    char val = Convert.ToChar(dr["SIMPAN"].ToString());
                    return (val == 'Y');
                }
               
            }
            catch (Exception)
            {
                // Handle exceptions as per your application's error-handling strategy.
                // You can log the error or return a default value.
                // For this example, I'm returning null to indicate an error.
                // Consider logging the exception for debugging and monitoring.
                // You might also want to handle specific exceptions more gracefully.
                return false;
            }
            return false;
        }

        public bool Ubah(int aksiid, string userid)
        {
            try
            {
                using OracleConnection conn = new( LoginInfo.OracleConnString);
                {
                    conn.Open();
                    using OracleCommand cmd = new("SELECT ubah FROM vaksesgl WHERE aksiid = :aksiid AND userid = :userid", conn);
                    cmd.Parameters.Add(new OracleParameter(":aksiid", aksiid));
                    cmd.Parameters.Add(new OracleParameter(":userid", userid));
                    OracleDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        char val = Convert.ToChar(dr["UBAH"].ToString());
                        return (val == 'Y');
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions as per your application's error-handling strategy.
                // You can log the error or return a default value.
                // For this example, I'm returning null to indicate an error.
                // Consider logging the exception for debugging and monitoring.
                // You might also want to handle specific exceptions more gracefully.
                return false;
            }
            return false;
        }


    }
}
