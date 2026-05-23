using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.BusinessLayer;

public static class FixedAssetQueryServices
{
    public static DataTable GetAssetCategories(string idData)
    {
        const string sql = """
            SELECT
                CATEGORY_ID,
                CATEGORY_CODE,
                CATEGORY_NAME,
                CATEGORY_CODE || ' - ' || CATEGORY_NAME AS DISPLAY_NAME
            FROM ACCT_FA_CATEGORY
            WHERE IDDATA = :p_iddata
              AND NVL(IS_ACTIVE, 'Y') = 'Y'
            ORDER BY CATEGORY_CODE
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        });
    }

    public static DataTable GetAssetGroups(string idData, long? categoryId)
    {
        const string sql = """
            SELECT
                GROUP_ID,
                CATEGORY_ID,
                GROUP_CODE,
                GROUP_NAME,
                GROUP_CODE || ' - ' || GROUP_NAME AS DISPLAY_NAME
            FROM ACCT_FA_GROUP
            WHERE IDDATA = :p_iddata
              AND NVL(IS_ACTIVE, 'Y') = 'Y'
              AND (:p_category_id IS NULL OR CATEGORY_ID = :p_category_id)
            ORDER BY GROUP_CODE
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_category_id", OracleDbType.Int64).Value = categoryId.HasValue ? categoryId.Value : DBNull.Value;
        });
    }

    public static Dictionary<string, long> GetCategoryLookup(string idData)
    {
        const string sql = """
            SELECT CATEGORY_ID, UPPER(TRIM(CATEGORY_CODE)) AS CATEGORY_CODE
            FROM ACCT_FA_CATEGORY
            WHERE IDDATA = :p_iddata
              AND NVL(IS_ACTIVE, 'Y') = 'Y'
            """;

        DataTable dt = ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        });

        var dict = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (DataRow row in dt.Rows)
        {
            string code = row["CATEGORY_CODE"]?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(code))
                dict[code] = Convert.ToInt64(row["CATEGORY_ID"]);
        }
        return dict;
    }

    public static Dictionary<string, (long GroupId, long CategoryId)> GetGroupLookup(string idData)
    {
        const string sql = """
            SELECT GROUP_ID, CATEGORY_ID, UPPER(TRIM(GROUP_CODE)) AS GROUP_CODE
            FROM ACCT_FA_GROUP
            WHERE IDDATA = :p_iddata
              AND NVL(IS_ACTIVE, 'Y') = 'Y'
            """;

        DataTable dt = ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        });

        var dict = new Dictionary<string, (long GroupId, long CategoryId)>(StringComparer.OrdinalIgnoreCase);
        foreach (DataRow row in dt.Rows)
        {
            string code = row["GROUP_CODE"]?.ToString() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(code))
                dict[code] = (Convert.ToInt64(row["GROUP_ID"]), Convert.ToInt64(row["CATEGORY_ID"]));
        }
        return dict;
    }

    public static HashSet<string> CheckAssetCodesExist(string idData, List<string> assetCodes)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (assetCodes == null || assetCodes.Count == 0) return result;

        // Batch per 100 parameters to avoid Oracle limit
        const int batchSize = 100;
        using var conn = new OracleConnection(LoginInfo.OracleConnString);
        conn.Open();

        for (int i = 0; i < assetCodes.Count; i += batchSize)
        {
            var batch = assetCodes.GetRange(i, Math.Min(batchSize, assetCodes.Count - i));
            var paramNames = new string[batch.Count];
            for (int j = 0; j < batch.Count; j++)
                paramNames[j] = $":p_{j}";

            string sql = $"""
                SELECT UPPER(TRIM(ASSET_CODE)) AS ASSET_CODE
                FROM ACCT_FA_ASSET
                WHERE IDDATA = :p_iddata
                  AND IS_DELETED = 'N'
                  AND UPPER(TRIM(ASSET_CODE)) IN ({string.Join(", ", paramNames)})
                """;

            using var cmd = new OracleCommand(sql, conn) { BindByName = true, CommandType = CommandType.Text };
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            for (int j = 0; j < batch.Count; j++)
                cmd.Parameters.Add(paramNames[j], OracleDbType.Varchar2, 50).Value = batch[j].Trim().ToUpperInvariant();

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string code = reader["ASSET_CODE"]?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(code))
                    result.Add(code);
            }
        }

        return result;
    }

    public static DataTable GetAssetMaster(string idData, string search, string status)
    {
        const string sql = """
            SELECT
                A.ASSET_ID,
                A.ASSET_CODE,
                A.ASSET_NAME,
                C.CATEGORY_CODE,
                C.CATEGORY_NAME,
                G.GROUP_CODE,
                G.GROUP_NAME,
                A.ACQUISITION_DATE,
                A.IN_SERVICE_DATE,
                A.ACQUISITION_COST,
                A.RESIDUAL_VALUE,
                A.USEFUL_LIFE_MONTHS,
                A.DEPR_METHOD,
                A.STATUS,
                A.DEPARTMENT_ID,
                A.COST_CENTER_ID,
                A.LOCATION_ID,
                A.VENDOR_ID,
                A.SERIAL_NO,
                A.CURRENCY_CODE,
                A.EXCHANGE_RATE
            FROM ACCT_FA_ASSET A
            LEFT JOIN ACCT_FA_CATEGORY C
                   ON C.CATEGORY_ID = A.CATEGORY_ID
                  AND C.IDDATA = A.IDDATA
            LEFT JOIN ACCT_FA_GROUP G
                   ON G.GROUP_ID = A.GROUP_ID
                  AND G.IDDATA = A.IDDATA
            WHERE A.IDDATA = :p_iddata
              AND A.IS_DELETED = 'N'
              AND (:p_status IS NULL OR A.STATUS = :p_status)
              AND (
                    :p_search IS NULL
                    OR UPPER(A.ASSET_CODE) LIKE :p_search_like
                    OR UPPER(A.ASSET_NAME) LIKE :p_search_like
                    OR UPPER(NVL(A.SERIAL_NO, '')) LIKE :p_search_like
                  )
            ORDER BY A.ASSET_CODE
            """;

        using var conn = new OracleConnection(LoginInfo.OracleConnString);
        using var cmd = new OracleCommand(sql, conn)
        {
            BindByName = true,
            CommandType = CommandType.Text
        };
        cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        cmd.Parameters.Add(":p_status", OracleDbType.Varchar2, 30).Value = string.IsNullOrWhiteSpace(status) ? DBNull.Value : status.Trim().ToUpperInvariant();

        string normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToUpperInvariant();
        cmd.Parameters.Add(":p_search", OracleDbType.Varchar2, 200).Value = normalizedSearch ?? (object)DBNull.Value;
        cmd.Parameters.Add(":p_search_like", OracleDbType.Varchar2, 220).Value = normalizedSearch is null ? DBNull.Value : $"%{normalizedSearch}%";

        using var adapter = new OracleDataAdapter(cmd);
        DataTable table = new();
        adapter.Fill(table);
        return table;
    }

    public static DataTable GetAssetDetail(string idData, long assetId)
    {
        const string sql = """
            SELECT
                ASSET_ID,
                IDDATA,
                ASSET_CODE,
                ASSET_NAME,
                CATEGORY_ID,
                GROUP_ID,
                ACQUISITION_DATE,
                IN_SERVICE_DATE,
                DEPRECIATION_START_DATE,
                ACQUISITION_COST,
                RESIDUAL_VALUE,
                USEFUL_LIFE_MONTHS,
                DEPR_METHOD,
                CURRENCY_CODE,
                EXCHANGE_RATE,
                STATUS,
                DEPARTMENT_ID,
                COST_CENTER_ID,
                LOCATION_ID,
                VENDOR_ID,
                SERIAL_NO,
                NOTES
            FROM ACCT_FA_ASSET
            WHERE IDDATA = :p_iddata
              AND ASSET_ID = :p_asset_id
              AND IS_DELETED = 'N'
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_asset_id", OracleDbType.Int64).Value = assetId;
        });
    }

    public static DataTable GetAssetDepreciationHistory(string idData, long assetId)
    {
        const string sql = """
            SELECT
                HISTORY_ID,
                PERIOD,
                OPENING_NBV,
                DEPR_AMOUNT,
                CLOSING_NBV,
                NOJURNAL,
                JURNALID,
                RUN_ID
            FROM ACCT_FA_DEPR_HISTORY
            WHERE IDDATA = :p_iddata
              AND ASSET_ID = :p_asset_id
            ORDER BY PERIOD_KEY, HISTORY_ID
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_asset_id", OracleDbType.Int64).Value = assetId;
        });
    }

    public static DataTable GetAssetTransactionHistory(string idData, long assetId)
    {
        const string sql = """
            SELECT
                TRX_ID,
                DOC_NO,
                TRX_TYPE,
                DOC_DATE,
                PERIOD,
                AMOUNT_BASE,
                OLD_AMOUNT_BASE,
                NEW_AMOUNT_BASE,
                STATUS,
                NOJURNAL,
                JURNALID,
                SOURCE_REF_NO,
                REMARKS
            FROM ACCT_FA_TRX_HDR
            WHERE IDDATA = :p_iddata
              AND ASSET_ID = :p_asset_id
            ORDER BY PERIOD_KEY, TRX_ID
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_asset_id", OracleDbType.Int64).Value = assetId;
        });
    }

    public static long SaveAsset(FixedAssetMasterSaveRequest request, string userId)
    {
        ValidateAssetRequest(request);
        using var conn = new OracleConnection(LoginInfo.OracleConnString);
        conn.Open();
        using OracleTransaction trx = conn.BeginTransaction();
        try
        {
            if (request.AssetId <= 0)
            {
                string assetCode = string.IsNullOrWhiteSpace(request.AssetCode)
                    ? GenerateAssetCode(conn, trx, request.IdData, request.AcquisitionDate)
                    : request.AssetCode.Trim().ToUpperInvariant();
                long newId = InsertAsset(conn, trx, request, userId, assetCode);
                trx.Commit();
                return newId;
            }

            UpdateAsset(conn, trx, request, userId);
            trx.Commit();
            return request.AssetId;
        }
        catch
        {
            trx.Rollback();
            throw;
        }
    }

    public static void SoftDeleteAsset(string idData, long assetId, string userId)
    {
        const string sql = """
            UPDATE ACCT_FA_ASSET
               SET IS_DELETED = 'Y',
                   MODIFIED_BY = :p_user,
                   MODIFIED_DATE = SYSTIMESTAMP
             WHERE IDDATA = :p_iddata
               AND ASSET_ID = :p_asset_id
               AND IS_DELETED = 'N'
            """;

        using var conn = new OracleConnection(LoginInfo.OracleConnString);
        conn.Open();
        using var cmd = new OracleCommand(sql, conn)
        {
            BindByName = true,
            CommandType = CommandType.Text
        };
        cmd.Parameters.Add(":p_user", OracleDbType.Varchar2, 50).Value = userId;
        cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        cmd.Parameters.Add(":p_asset_id", OracleDbType.Int64).Value = assetId;
        int affected = cmd.ExecuteNonQuery();
        if (affected == 0)
        {
            throw new InvalidOperationException($"Asset {assetId} tidak ditemukan/ sudah dihapus.");
        }
    }

    public static DataTable GetCipSummary(string idData, string search, string status)
    {
        const string sql = """
            SELECT
                H.CIP_ID,
                H.CIP_CODE,
                H.PROJECT_NAME,
                H.START_DATE,
                H.TARGET_COMPLETE_DATE,
                H.STATUS,
                NVL(C.TOTAL_COST, 0) AS TOTAL_COST,
                NVL(K.TOTAL_CAPITALIZED, 0) AS TOTAL_CAPITALIZED,
                NVL(C.TOTAL_COST, 0) - NVL(K.TOTAL_CAPITALIZED, 0) AS OUTSTANDING_AMOUNT
            FROM ACCT_FA_CIP_HDR H
            LEFT JOIN (
                SELECT IDDATA, CIP_ID, SUM(AMOUNT_BASE) AS TOTAL_COST
                FROM ACCT_FA_CIP_COST
                GROUP BY IDDATA, CIP_ID
            ) C
              ON C.IDDATA = H.IDDATA
             AND C.CIP_ID = H.CIP_ID
            LEFT JOIN (
                SELECT IDDATA, CIP_ID, SUM(CAPITALIZED_AMOUNT) AS TOTAL_CAPITALIZED
                FROM ACCT_FA_CIP_CAPITALIZATION
                GROUP BY IDDATA, CIP_ID
            ) K
              ON K.IDDATA = H.IDDATA
             AND K.CIP_ID = H.CIP_ID
            WHERE H.IDDATA = :p_iddata
              AND (:p_status IS NULL OR H.STATUS = :p_status)
              AND (
                    :p_search IS NULL
                    OR UPPER(H.CIP_CODE) LIKE :p_search_like
                    OR UPPER(H.PROJECT_NAME) LIKE :p_search_like
                  )
            ORDER BY H.CIP_CODE
            """;

        using var conn = new OracleConnection(LoginInfo.OracleConnString);
        using var cmd = new OracleCommand(sql, conn)
        {
            BindByName = true,
            CommandType = CommandType.Text
        };
        cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        cmd.Parameters.Add(":p_status", OracleDbType.Varchar2, 30).Value = string.IsNullOrWhiteSpace(status) ? DBNull.Value : status.Trim().ToUpperInvariant();

        string normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToUpperInvariant();
        cmd.Parameters.Add(":p_search", OracleDbType.Varchar2, 200).Value = normalizedSearch ?? (object)DBNull.Value;
        cmd.Parameters.Add(":p_search_like", OracleDbType.Varchar2, 220).Value = normalizedSearch is null ? DBNull.Value : $"%{normalizedSearch}%";

        using var adapter = new OracleDataAdapter(cmd);
        DataTable table = new();
        adapter.Fill(table);
        return table;
    }

    public static DataTable GetCipCostDetail(string idData, long cipId)
    {
        const string sql = """
            SELECT
                CIP_COST_ID,
                DOC_NO,
                COST_DATE,
                PERIOD,
                ACCOUNT_CODE,
                AMOUNT_BASE,
                SOURCE_REF_NO
            FROM ACCT_FA_CIP_COST
            WHERE IDDATA = :p_iddata
              AND CIP_ID = :p_cip_id
            ORDER BY COST_DATE, CIP_COST_ID
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_cip_id", OracleDbType.Int64).Value = cipId;
        });
    }

    public static DataTable GetCipCapitalizationDetail(string idData, long cipId)
    {
        const string sql = """
            SELECT
                C.CAP_ID,
                C.DOC_NO,
                C.CAPITALIZE_DATE,
                C.PERIOD,
                C.ASSET_ID,
                A.ASSET_CODE,
                C.CAPITALIZED_AMOUNT
            FROM ACCT_FA_CIP_CAPITALIZATION C
            LEFT JOIN ACCT_FA_ASSET A
              ON A.IDDATA = C.IDDATA
             AND A.ASSET_ID = C.ASSET_ID
            WHERE C.IDDATA = :p_iddata
              AND C.CIP_ID = :p_cip_id
            ORDER BY C.CAPITALIZE_DATE, C.CAP_ID
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_cip_id", OracleDbType.Int64).Value = cipId;
        });
    }

    public static DataTable GetApprovalInbox(string idData, string periodFrom, string periodTo)
    {
        int fromKey = PeriodToKey(periodFrom);
        int toKey = PeriodToKey(periodTo);

        const string sql = """
            SELECT
                T.TRX_ID,
                T.DOC_NO,
                T.TRX_TYPE,
                T.DOC_DATE,
                T.PERIOD,
                T.ASSET_ID,
                A.ASSET_CODE,
                A.ASSET_NAME,
                T.AMOUNT_BASE,
                T.OLD_AMOUNT_BASE,
                T.NEW_AMOUNT_BASE,
                T.STATUS,
                T.REMARKS,
                NVL((
                    SELECT MAX(STEP_NO)
                    FROM ACCT_FA_APPROVAL_DTL X
                    WHERE X.TRX_ID = T.TRX_ID
                ), 0) AS LAST_STEP
            FROM ACCT_FA_TRX_HDR T
            LEFT JOIN ACCT_FA_ASSET A
              ON A.IDDATA = T.IDDATA
             AND A.ASSET_ID = T.ASSET_ID
            WHERE T.IDDATA = :p_iddata
              AND T.STATUS = 'SUBMITTED'
              AND T.PERIOD_KEY BETWEEN :p_from_key AND :p_to_key
              AND T.TRX_TYPE IN ('REVALUATION', 'FULL_DISPOSAL', 'SALE', 'WRITE_OFF')
            ORDER BY T.PERIOD_KEY, T.TRX_ID
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_from_key", OracleDbType.Int32).Value = fromKey;
            cmd.Parameters.Add(":p_to_key", OracleDbType.Int32).Value = toKey;
        });
    }

    public static DataTable GetApprovalWorklist(string idData, string periodFrom, string periodTo, string statusFilter)
    {
        int fromKey = PeriodToKey(periodFrom);
        int toKey = PeriodToKey(periodTo);

        const string sql = """
            SELECT
                T.TRX_ID,
                T.DOC_NO,
                T.TRX_TYPE,
                T.DOC_DATE,
                T.PERIOD,
                T.ASSET_ID,
                A.ASSET_CODE,
                A.ASSET_NAME,
                T.AMOUNT_BASE,
                T.OLD_AMOUNT_BASE,
                T.NEW_AMOUNT_BASE,
                T.STATUS,
                T.NOJURNAL,
                T.JURNALID,
                T.REMARKS,
                NVL((
                    SELECT MAX(STEP_NO)
                    FROM ACCT_FA_APPROVAL_DTL X
                    WHERE X.TRX_ID = T.TRX_ID
                ), 0) AS LAST_STEP
            FROM ACCT_FA_TRX_HDR T
            LEFT JOIN ACCT_FA_ASSET A
              ON A.IDDATA = T.IDDATA
             AND A.ASSET_ID = T.ASSET_ID
            WHERE T.IDDATA = :p_iddata
              AND T.PERIOD_KEY BETWEEN :p_from_key AND :p_to_key
              AND T.TRX_TYPE IN ('REVALUATION', 'FULL_DISPOSAL', 'SALE', 'WRITE_OFF')
              AND (:p_status IS NULL OR T.STATUS = :p_status)
            ORDER BY T.PERIOD_KEY, T.TRX_ID
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_from_key", OracleDbType.Int32).Value = fromKey;
            cmd.Parameters.Add(":p_to_key", OracleDbType.Int32).Value = toKey;
            cmd.Parameters.Add(":p_status", OracleDbType.Varchar2, 20).Value =
                string.IsNullOrWhiteSpace(statusFilter) || string.Equals(statusFilter, "ALL", StringComparison.OrdinalIgnoreCase)
                    ? DBNull.Value
                    : statusFilter.Trim().ToUpperInvariant();
        });
    }

    public static DataTable GetAssetRegisterReport(string idData)
    {
        const string sql = """
            SELECT
                A.ASSET_CODE,
                A.ASSET_NAME,
                C.CATEGORY_NAME,
                A.ACQUISITION_DATE,
                A.IN_SERVICE_DATE,
                A.ACQUISITION_COST,
                A.RESIDUAL_VALUE,
                NVL(H.ACC_DEPR, 0) AS ACCUMULATED_DEPRECIATION,
                A.ACQUISITION_COST - NVL(H.ACC_DEPR, 0) AS NET_BOOK_VALUE,
                A.STATUS,
                A.LOCATION_ID,
                A.DEPARTMENT_ID
            FROM ACCT_FA_ASSET A
            LEFT JOIN ACCT_FA_CATEGORY C
                   ON C.CATEGORY_ID = A.CATEGORY_ID
                  AND C.IDDATA = A.IDDATA
            LEFT JOIN (
                SELECT IDDATA, ASSET_ID, SUM(DEPR_AMOUNT) AS ACC_DEPR
                FROM ACCT_FA_DEPR_HISTORY
                GROUP BY IDDATA, ASSET_ID
            ) H
              ON H.IDDATA = A.IDDATA
             AND H.ASSET_ID = A.ASSET_ID
            WHERE A.IDDATA = :p_iddata
              AND A.IS_DELETED = 'N'
            ORDER BY A.ASSET_CODE
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        });
    }

    public static DataTable GetDepreciationExpenseReport(string idData, string periodFrom, string periodTo)
    {
        int fromKey = PeriodToKey(periodFrom);
        int toKey = PeriodToKey(periodTo);

        const string sql = """
            SELECT
                H.PERIOD,
                SUM(H.DEPR_AMOUNT) AS TOTAL_DEPRECIATION,
                COUNT(DISTINCT H.ASSET_ID) AS ASSET_COUNT
            FROM ACCT_FA_DEPR_HISTORY H
            WHERE H.IDDATA = :p_iddata
              AND H.PERIOD_KEY BETWEEN :p_from_key AND :p_to_key
            GROUP BY H.PERIOD, H.PERIOD_KEY
            ORDER BY H.PERIOD_KEY
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_from_key", OracleDbType.Int32).Value = fromKey;
            cmd.Parameters.Add(":p_to_key", OracleDbType.Int32).Value = toKey;
        });
    }

    public static DataTable GetMovementReport(string idData, string periodFrom, string periodTo)
    {
        int fromKey = PeriodToKey(periodFrom);
        int toKey = PeriodToKey(periodTo);

        const string sql = """
            SELECT
                T.PERIOD,
                T.TRX_TYPE,
                T.STATUS,
                COUNT(*) AS TRX_COUNT,
                SUM(NVL(T.AMOUNT_BASE, 0)) AS TOTAL_AMOUNT
            FROM ACCT_FA_TRX_HDR T
            WHERE T.IDDATA = :p_iddata
              AND T.PERIOD_KEY BETWEEN :p_from_key AND :p_to_key
            GROUP BY T.PERIOD, T.PERIOD_KEY, T.TRX_TYPE, T.STATUS
            ORDER BY T.PERIOD_KEY, T.TRX_TYPE
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_from_key", OracleDbType.Int32).Value = fromKey;
            cmd.Parameters.Add(":p_to_key", OracleDbType.Int32).Value = toKey;
        });
    }

    public static DataTable GetSummaryByCategoryReport(string idData)
    {
        const string sql = """
            SELECT
                C.CATEGORY_CODE,
                C.CATEGORY_NAME,
                COUNT(A.ASSET_ID) AS ASSET_COUNT,
                SUM(NVL(A.ACQUISITION_COST, 0)) AS TOTAL_ACQUISITION_COST
            FROM ACCT_FA_CATEGORY C
            LEFT JOIN ACCT_FA_ASSET A
              ON A.IDDATA = C.IDDATA
             AND A.CATEGORY_ID = C.CATEGORY_ID
             AND A.IS_DELETED = 'N'
            WHERE C.IDDATA = :p_iddata
            GROUP BY C.CATEGORY_CODE, C.CATEGORY_NAME
            ORDER BY C.CATEGORY_CODE
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        });
    }

    public static DataTable GetFullyDepreciatedAssetsReport(string idData)
    {
        const string sql = """
            SELECT
                A.ASSET_CODE,
                A.ASSET_NAME,
                A.ACQUISITION_COST,
                A.RESIDUAL_VALUE,
                NVL(H.ACC_DEPR, 0) AS ACCUMULATED_DEPRECIATION,
                A.ACQUISITION_COST - NVL(H.ACC_DEPR, 0) AS NET_BOOK_VALUE,
                A.STATUS
            FROM ACCT_FA_ASSET A
            LEFT JOIN (
                SELECT IDDATA, ASSET_ID, SUM(DEPR_AMOUNT) AS ACC_DEPR
                FROM ACCT_FA_DEPR_HISTORY
                GROUP BY IDDATA, ASSET_ID
            ) H
              ON H.IDDATA = A.IDDATA
             AND H.ASSET_ID = A.ASSET_ID
            WHERE A.IDDATA = :p_iddata
              AND A.IS_DELETED = 'N'
              AND (A.ACQUISITION_COST - NVL(H.ACC_DEPR, 0)) <= A.RESIDUAL_VALUE
            ORDER BY A.ASSET_CODE
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        });
    }

    public static DataTable GetIdleAssetsReport(string idData)
    {
        const string sql = """
            SELECT
                ASSET_CODE,
                ASSET_NAME,
                STATUS,
                LOCATION_ID,
                DEPARTMENT_ID,
                COST_CENTER_ID,
                ACQUISITION_DATE,
                IN_SERVICE_DATE
            FROM ACCT_FA_ASSET
            WHERE IDDATA = :p_iddata
              AND IS_DELETED = 'N'
              AND STATUS IN ('DRAFT', 'UNDER_CONSTRUCTION', 'RETIRED', 'TRANSFERRED')
            ORDER BY ASSET_CODE
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        });
    }

    public static DataTable GetDisposalReport(string idData, string periodFrom, string periodTo)
    {
        int fromKey = PeriodToKey(periodFrom);
        int toKey = PeriodToKey(periodTo);

        const string sql = """
            SELECT
                T.TRX_ID,
                T.DOC_NO,
                T.PERIOD,
                T.TRX_TYPE,
                A.ASSET_CODE,
                A.ASSET_NAME,
                T.AMOUNT_BASE,
                T.STATUS,
                T.NOJURNAL,
                T.JURNALID
            FROM ACCT_FA_TRX_HDR T
            LEFT JOIN ACCT_FA_ASSET A
              ON A.IDDATA = T.IDDATA
             AND A.ASSET_ID = T.ASSET_ID
            WHERE T.IDDATA = :p_iddata
              AND T.PERIOD_KEY BETWEEN :p_from_key AND :p_to_key
              AND T.TRX_TYPE IN ('FULL_DISPOSAL', 'SALE', 'WRITE_OFF')
            ORDER BY T.PERIOD_KEY, T.TRX_ID
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_from_key", OracleDbType.Int32).Value = fromKey;
            cmd.Parameters.Add(":p_to_key", OracleDbType.Int32).Value = toKey;
        });
    }

    public static DataTable GetRevaluationReport(string idData, string periodFrom, string periodTo)
    {
        int fromKey = PeriodToKey(periodFrom);
        int toKey = PeriodToKey(periodTo);

        const string sql = """
            SELECT
                T.TRX_ID,
                T.DOC_NO,
                T.PERIOD,
                A.ASSET_CODE,
                A.ASSET_NAME,
                T.OLD_AMOUNT_BASE,
                T.NEW_AMOUNT_BASE,
                (NVL(T.NEW_AMOUNT_BASE, 0) - NVL(T.OLD_AMOUNT_BASE, 0)) AS DELTA_AMOUNT,
                T.STATUS,
                T.NOJURNAL,
                T.JURNALID
            FROM ACCT_FA_TRX_HDR T
            LEFT JOIN ACCT_FA_ASSET A
              ON A.IDDATA = T.IDDATA
             AND A.ASSET_ID = T.ASSET_ID
            WHERE T.IDDATA = :p_iddata
              AND T.PERIOD_KEY BETWEEN :p_from_key AND :p_to_key
              AND T.TRX_TYPE = 'REVALUATION'
            ORDER BY T.PERIOD_KEY, T.TRX_ID
            """;

        return ExecuteDataTable(sql, cmd =>
        {
            cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
            cmd.Parameters.Add(":p_from_key", OracleDbType.Int32).Value = fromKey;
            cmd.Parameters.Add(":p_to_key", OracleDbType.Int32).Value = toKey;
        });
    }

    private static DataTable ExecuteDataTable(string sql, Action<OracleCommand> parameterize)
    {
        using var conn = new OracleConnection(LoginInfo.OracleConnString);
        using var cmd = new OracleCommand(sql, conn)
        {
            BindByName = true,
            CommandType = CommandType.Text
        };
        parameterize(cmd);
        using var adapter = new OracleDataAdapter(cmd);
        DataTable table = new();
        adapter.Fill(table);
        return table;
    }

    private static string GenerateAssetCode(OracleConnection conn, OracleTransaction trx, string idData, DateTime acquisitionDate)
    {
        const string mergeSql = """
            MERGE INTO ACCT_FA_DOC_SEQ T
            USING (SELECT :p_iddata IDDATA, :p_doc_type DOC_TYPE, :p_year YYYY, :p_month MM FROM DUAL) S
            ON (T.IDDATA = S.IDDATA AND T.DOC_TYPE = S.DOC_TYPE AND T.YYYY = S.YYYY AND T.MM = S.MM)
            WHEN MATCHED THEN UPDATE SET T.LAST_NO = T.LAST_NO + 1, T.MODIFIED_DATE = SYSTIMESTAMP
            WHEN NOT MATCHED THEN INSERT (IDDATA, DOC_TYPE, YYYY, MM, LAST_NO, CREATED_DATE)
                 VALUES (S.IDDATA, S.DOC_TYPE, S.YYYY, S.MM, 1, SYSTIMESTAMP)
            """;

        const string getNoSql = """
            SELECT LAST_NO
            FROM ACCT_FA_DOC_SEQ
            WHERE IDDATA = :p_iddata
              AND DOC_TYPE = :p_doc_type
              AND YYYY = :p_year
              AND MM = :p_month
            """;

        using var mergeCmd = new OracleCommand(mergeSql, conn)
        {
            BindByName = true,
            Transaction = trx,
            CommandType = CommandType.Text
        };
        mergeCmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        mergeCmd.Parameters.Add(":p_doc_type", OracleDbType.Varchar2, 30).Value = "FA-AST";
        mergeCmd.Parameters.Add(":p_year", OracleDbType.Int32).Value = acquisitionDate.Year;
        mergeCmd.Parameters.Add(":p_month", OracleDbType.Int32).Value = acquisitionDate.Month;
        mergeCmd.ExecuteNonQuery();

        using var getCmd = new OracleCommand(getNoSql, conn)
        {
            BindByName = true,
            Transaction = trx,
            CommandType = CommandType.Text
        };
        getCmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        getCmd.Parameters.Add(":p_doc_type", OracleDbType.Varchar2, 30).Value = "FA-AST";
        getCmd.Parameters.Add(":p_year", OracleDbType.Int32).Value = acquisitionDate.Year;
        getCmd.Parameters.Add(":p_month", OracleDbType.Int32).Value = acquisitionDate.Month;
        int no = Convert.ToInt32(getCmd.ExecuteScalar());
        return $"FA-{acquisitionDate:yyyyMM}-{no:000000}";
    }

    private static long InsertAsset(OracleConnection conn, OracleTransaction trx, FixedAssetMasterSaveRequest request, string userId, string assetCode)
    {
        const string sql = """
            INSERT INTO ACCT_FA_ASSET
            (
                IDDATA, ASSET_CODE, ASSET_NAME, CATEGORY_ID, GROUP_ID, ACQUISITION_DATE, IN_SERVICE_DATE, DEPRECIATION_START_DATE,
                ACQUISITION_COST, RESIDUAL_VALUE, USEFUL_LIFE_MONTHS, DEPR_METHOD, CURRENCY_CODE, EXCHANGE_RATE, STATUS,
                DEPARTMENT_ID, COST_CENTER_ID, LOCATION_ID, VENDOR_ID, SERIAL_NO, NOTES, IS_DELETED, CREATED_BY, CREATED_DATE
            )
            VALUES
            (
                :p_iddata, :p_asset_code, :p_asset_name, :p_category_id, :p_group_id, :p_acq_date, :p_in_service_date, :p_depr_start_date,
                :p_cost, :p_residual, :p_useful_life, :p_method, :p_currency, :p_rate, :p_status,
                :p_dept, :p_cc, :p_loc, :p_vendor, :p_serial, :p_notes, 'N', :p_user, SYSTIMESTAMP
            )
            RETURNING ASSET_ID INTO :p_asset_id
            """;

        using var cmd = new OracleCommand(sql, conn)
        {
            BindByName = true,
            Transaction = trx,
            CommandType = CommandType.Text
        };
        BindAssetCommand(cmd, request, userId, assetCode);
        OracleParameter outParam = cmd.Parameters.Add(":p_asset_id", OracleDbType.Int64);
        outParam.Direction = ParameterDirection.Output;
        cmd.ExecuteNonQuery();
        return Convert.ToInt64(outParam.Value.ToString());
    }

    private static void UpdateAsset(OracleConnection conn, OracleTransaction trx, FixedAssetMasterSaveRequest request, string userId)
    {
        const string sql = """
            UPDATE ACCT_FA_ASSET
               SET ASSET_NAME = :p_asset_name,
                   CATEGORY_ID = :p_category_id,
                   GROUP_ID = :p_group_id,
                   ACQUISITION_DATE = :p_acq_date,
                   IN_SERVICE_DATE = :p_in_service_date,
                   DEPRECIATION_START_DATE = :p_depr_start_date,
                   ACQUISITION_COST = :p_cost,
                   RESIDUAL_VALUE = :p_residual,
                   USEFUL_LIFE_MONTHS = :p_useful_life,
                   DEPR_METHOD = :p_method,
                   CURRENCY_CODE = :p_currency,
                   EXCHANGE_RATE = :p_rate,
                   STATUS = :p_status,
                   DEPARTMENT_ID = :p_dept,
                   COST_CENTER_ID = :p_cc,
                   LOCATION_ID = :p_loc,
                   VENDOR_ID = :p_vendor,
                   SERIAL_NO = :p_serial,
                   NOTES = :p_notes,
                   MODIFIED_BY = :p_user,
                   MODIFIED_DATE = SYSTIMESTAMP
             WHERE IDDATA = :p_iddata
               AND ASSET_ID = :p_asset_id
               AND IS_DELETED = 'N'
            """;

        using var cmd = new OracleCommand(sql, conn)
        {
            BindByName = true,
            Transaction = trx,
            CommandType = CommandType.Text
        };
        BindAssetCommand(cmd, request, userId, request.AssetCode.Trim().ToUpperInvariant());
        cmd.Parameters.Add(":p_asset_id", OracleDbType.Int64).Value = request.AssetId;
        int affected = cmd.ExecuteNonQuery();
        if (affected == 0)
        {
            throw new InvalidOperationException($"Asset {request.AssetId} tidak ditemukan/ tidak aktif.");
        }
    }

    private static void BindAssetCommand(OracleCommand cmd, FixedAssetMasterSaveRequest request, string userId, string assetCode)
    {
        cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = request.IdData;
        cmd.Parameters.Add(":p_asset_code", OracleDbType.Varchar2, 50).Value = assetCode;
        cmd.Parameters.Add(":p_asset_name", OracleDbType.Varchar2, 200).Value = request.AssetName.Trim();
        cmd.Parameters.Add(":p_category_id", OracleDbType.Int64).Value = request.CategoryId;
        cmd.Parameters.Add(":p_group_id", OracleDbType.Int64).Value = request.GroupId.HasValue ? request.GroupId.Value : DBNull.Value;
        cmd.Parameters.Add(":p_acq_date", OracleDbType.Date).Value = request.AcquisitionDate.Date;
        cmd.Parameters.Add(":p_in_service_date", OracleDbType.Date).Value = request.InServiceDate.HasValue ? request.InServiceDate.Value.Date : DBNull.Value;
        cmd.Parameters.Add(":p_depr_start_date", OracleDbType.Date).Value = request.DepreciationStartDate.HasValue ? request.DepreciationStartDate.Value.Date : DBNull.Value;
        cmd.Parameters.Add(":p_cost", OracleDbType.Decimal).Value = request.AcquisitionCost;
        cmd.Parameters.Add(":p_residual", OracleDbType.Decimal).Value = request.ResidualValue;
        cmd.Parameters.Add(":p_useful_life", OracleDbType.Int32).Value = request.UsefulLifeMonths;
        cmd.Parameters.Add(":p_method", OracleDbType.Varchar2, 10).Value = request.DepreciationMethod.Trim().ToUpperInvariant();
        cmd.Parameters.Add(":p_currency", OracleDbType.Varchar2, 10).Value = request.CurrencyCode.Trim().ToUpperInvariant();
        cmd.Parameters.Add(":p_rate", OracleDbType.Decimal).Value = request.ExchangeRate <= 0 ? 1m : request.ExchangeRate;
        cmd.Parameters.Add(":p_status", OracleDbType.Varchar2, 30).Value = request.Status.Trim().ToUpperInvariant();
        cmd.Parameters.Add(":p_dept", OracleDbType.Varchar2, 30).Value = NullIfEmpty(request.DepartmentId);
        cmd.Parameters.Add(":p_cc", OracleDbType.Varchar2, 30).Value = NullIfEmpty(request.CostCenterId);
        cmd.Parameters.Add(":p_loc", OracleDbType.Varchar2, 30).Value = NullIfEmpty(request.LocationId);
        cmd.Parameters.Add(":p_vendor", OracleDbType.Varchar2, 30).Value = NullIfEmpty(request.VendorId);
        cmd.Parameters.Add(":p_serial", OracleDbType.Varchar2, 100).Value = NullIfEmpty(request.SerialNo);
        cmd.Parameters.Add(":p_notes", OracleDbType.Varchar2, 1000).Value = NullIfEmpty(request.Notes);
        cmd.Parameters.Add(":p_user", OracleDbType.Varchar2, 50).Value = userId;
    }

    private static object NullIfEmpty(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();
    }

    private static void ValidateAssetRequest(FixedAssetMasterSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdData))
        {
            throw new ArgumentException("IDDATA wajib diisi.");
        }

        if (string.IsNullOrWhiteSpace(request.AssetName))
        {
            throw new ArgumentException("Asset name wajib diisi.");
        }

        if (request.CategoryId <= 0)
        {
            throw new ArgumentException("Category wajib dipilih.");
        }

        if (request.AcquisitionCost < 0m)
        {
            throw new ArgumentException("Acquisition cost tidak boleh negatif.");
        }

        if (request.ResidualValue < 0m)
        {
            throw new ArgumentException("Residual value tidak boleh negatif.");
        }

        if (request.UsefulLifeMonths <= 0)
        {
            throw new ArgumentException("Useful life harus > 0.");
        }
    }

    private static int PeriodToKey(string period)
    {
        if (string.IsNullOrWhiteSpace(period))
        {
            throw new ArgumentException("Periode wajib diisi (MM/YYYY).", nameof(period));
        }

        string[] split = period.Trim().Split('/');
        if (split.Length != 2
            || !int.TryParse(split[0], out int month)
            || !int.TryParse(split[1], out int year)
            || month is < 1 or > 12
            || year < 1900)
        {
            throw new ArgumentException($"Format periode tidak valid: {period}. Gunakan MM/YYYY.");
        }

        return year * 100 + month;
    }
}
