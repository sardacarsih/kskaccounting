using Accounting.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace Accounting.ClosingYear;

public sealed class OracleClosingYearRepository : IClosingYearRepository
{
    private const int CommandTimeoutSeconds = 900;

    public ClosingYearResult CloseYear(ClosingYearRequest request)
    {
        using OracleConnection connection = new(LoginInfo.OracleConnString);
        connection.Open();

        using OracleCommand command = new("ACCT_CLOSING_YEAR_V2.CLOSE_YEAR", connection)
        {
            CommandType = CommandType.StoredProcedure,
            BindByName = true,
            CommandTimeout = CommandTimeoutSeconds
        };

        command.Parameters.Add("p_IDDATA", OracleDbType.Varchar2, 20).Value = request.IdData;
        command.Parameters.Add("p_TAHUN", OracleDbType.Int16).Value = request.Year;
        command.Parameters.Add("p_USERID", OracleDbType.Varchar2, 20).Value = request.UserId;
        command.Parameters.Add("p_JENISAKUNTING", OracleDbType.Varchar2, 20).Value = request.JenisAkunting;
        command.Parameters.Add("p_CREATE_CLOSING_JOURNAL", OracleDbType.Char, 1).Value = request.CreateClosingJournal ? "Y" : "N";
        command.Parameters.Add("p_NEXT_YEAR", OracleDbType.Int32).Direction = ParameterDirection.Output;
        command.Parameters.Add("p_COA_ACTION", OracleDbType.Varchar2, 30).Direction = ParameterDirection.Output;
        command.Parameters.Add("p_LABA_RUGI", OracleDbType.Decimal).Direction = ParameterDirection.Output;

        command.ExecuteNonQuery();

        int nextYear = Convert.ToInt32(command.Parameters["p_NEXT_YEAR"].Value.ToString());
        string coaAction = command.Parameters["p_COA_ACTION"].Value?.ToString() ?? string.Empty;
        decimal labaRugi = Convert.ToDecimal(command.Parameters["p_LABA_RUGI"].Value.ToString());

        return ClosingYearResult.Success(request.Year, nextYear, $"12/{request.Year:0000}", coaAction, labaRugi);
    }
}
