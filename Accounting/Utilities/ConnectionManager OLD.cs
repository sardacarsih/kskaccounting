using System;

namespace Accounting.DBConnection
{
    public static class ConnectionManager
    {
        public static string GetOracleConnection()
        {
            string oracleConnString = global::Accounting.Utilities.ConnectionManager.GetOracleConnection();
            Acct.OracleConnString = oracleConnString;
            return oracleConnString;
        }
    }
}
