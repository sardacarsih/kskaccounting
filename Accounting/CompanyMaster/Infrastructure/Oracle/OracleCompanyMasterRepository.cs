using System.Collections.Generic;
using System.Linq;
using Accounting.CompanyMaster.Application;
using Accounting.CompanyMaster.Domain;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.CompanyMaster.Infrastructure.Oracle;

public sealed class OracleCompanyMasterRepository : ICompanyMasterRepository
{
    private readonly string _connectionString;

    public OracleCompanyMasterRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IReadOnlyList<CompanyMasterRecord> GetCompanies()
    {
        using OracleConnection connection = OpenConnection();
        const string sql = "SELECT IDPT, NAMAPT, IDGROUP FROM MASTER_PT_HDR ORDER BY NAMAPT ASC";
        return connection.Query<CompanyMasterRecord>(sql).AsList();
    }

    public IReadOnlyList<IdDataRecord> GetIdDataRows()
    {
        using OracleConnection connection = OpenConnection();
        const string sql = @"
            SELECT D.IDDATA,
                   D.IDPT,
                   H.NAMAPT,
                   D.WILAYAH,
                   D.JENIS_AKUNTANSI
            FROM MASTER_PT_DTL D
            JOIN MASTER_PT_HDR H ON H.IDPT = D.IDPT
            ORDER BY H.NAMAPT ASC";
        return connection.Query<IdDataRecord>(sql).AsList();
    }

    public IReadOnlyList<CompanyGroupOption> GetGroups()
    {
        using OracleConnection connection = OpenConnection();
        const string sql = "SELECT NAMAGROUP FROM MASTER_PTGROUP ORDER BY NAMAGROUP ASC";
        return connection.Query<CompanyGroupOption>(sql).AsList();
    }

    public bool CompanyExists(string idPt)
    {
        using OracleConnection connection = OpenConnection();
        const string sql = "SELECT COUNT(1) FROM MASTER_PT_HDR WHERE IDPT = :idPt";
        return connection.ExecuteScalar<int>(sql, new { idPt }) > 0;
    }

    public bool IdDataExists(string idData)
    {
        using OracleConnection connection = OpenConnection();
        const string sql = "SELECT COUNT(1) FROM MASTER_PT_DTL WHERE IDDATA = :idData";
        return connection.ExecuteScalar<int>(sql, new { idData }) > 0;
    }

    public void InsertCompany(CompanyMasterRecord company)
    {
        using OracleConnection connection = OpenConnection();
        const string sql = @"
            INSERT INTO MASTER_PT_HDR (IDPT, NAMAPT, IDGROUP)
            VALUES (:IDPT, :NAMAPT, :IDGROUP)";
        connection.Execute(sql, company);
    }

    public void UpdateCompany(CompanyMasterRecord company)
    {
        using OracleConnection connection = OpenConnection();
        const string sql = @"
            UPDATE MASTER_PT_HDR
            SET NAMAPT = :NAMAPT,
                IDGROUP = :IDGROUP
            WHERE IDPT = :IDPT";
        connection.Execute(sql, company);
    }

    public void DeleteCompany(string idPt)
    {
        using OracleConnection connection = OpenConnection();
        const string sql = "DELETE FROM MASTER_PT_HDR WHERE IDPT = :idPt";
        connection.Execute(sql, new { idPt });
    }

    public void InsertIdData(IdDataRecord idData)
    {
        using OracleConnection connection = OpenConnection();
        const string sql = @"
            INSERT INTO MASTER_PT_DTL (IDDATA, IDPT, WILAYAH, JENIS_AKUNTANSI)
            VALUES (:IDDATA, :IDPT, :WILAYAH, :JENIS_AKUNTANSI)";
        connection.Execute(sql, idData);
    }

    public void UpdateIdData(IdDataRecord idData)
    {
        using OracleConnection connection = OpenConnection();
        const string sql = @"
            UPDATE MASTER_PT_DTL
            SET IDPT = :IDPT,
                WILAYAH = :WILAYAH,
                JENIS_AKUNTANSI = :JENIS_AKUNTANSI
            WHERE IDDATA = :IDDATA";
        connection.Execute(sql, idData);
    }

    public void DeleteIdData(string idData)
    {
        using OracleConnection connection = OpenConnection();
        const string sql = "DELETE FROM MASTER_PT_DTL WHERE IDDATA = :idData";
        connection.Execute(sql, new { idData });
    }

    public int CountCompanyDependencies(string idPt)
    {
        using OracleConnection connection = OpenConnection();
        string[] countSql =
        [
            "SELECT COUNT(1) FROM MASTER_PT_DTL WHERE IDPT = :idPt",
            "SELECT COUNT(1) FROM HR_COMPANY WHERE IDPT = :idPt"
        ];

        return countSql.Sum(sql => connection.ExecuteScalar<int>(sql, new { idPt }));
    }

    public int CountIdDataDependencies(string idData)
    {
        using OracleConnection connection = OpenConnection();
        string[] countSql =
        [
            "SELECT COUNT(1) FROM MASTER_APPS_DETAIL WHERE IDDATA = :idData",
            "SELECT COUNT(1) FROM MASTER_USER_ROLES_LOC WHERE IDDATA = :idData",
            "SELECT COUNT(1) FROM ACCT_PERIODE WHERE IDDATA = :idData",
            "SELECT COUNT(1) FROM ACCT_COA WHERE IDDATA = :idData",
            "SELECT COUNT(1) FROM ACCT_JURNAL_HDR WHERE IDDATA = :idData",
            "SELECT COUNT(1) FROM ACCT_DEFAULT WHERE IDDATA = :idData"
        ];

        return countSql.Sum(sql => connection.ExecuteScalar<int>(sql, new { idData }));
    }

    private OracleConnection OpenConnection()
    {
        OracleConnection connection = new(_connectionString);
        connection.Open();
        return connection;
    }
}
