using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    internal sealed class ReportSettingRepository
    {
        internal const int CommandTimeoutSeconds = 15;

        internal const string SectionQuery = @"
            SELECT SECTION_ID,
                   SECTION_CODE,
                   SECTION_NAME,
                   DISPLAY_ORDER,
                   NORMAL_POSISI,
                   DISPLAY_LVL,
                   SHOW_ZERO,
                   IS_ACTIVE
              FROM ACCT_REPORT_SECTION
             WHERE REPORT_CODE = :reportCode
               AND (
                    :reportCode <> 'LABARUGI'
                    OR (:isPks = 'Y' AND SECTION_CODE LIKE 'PKS\_%' ESCAPE '\')
                    OR (:isPks = 'N' AND SECTION_CODE NOT LIKE 'PKS\_%' ESCAPE '\')
               )
             ORDER BY DISPLAY_ORDER";

        internal const string MappingQuery = @"
            SELECT account.SECTION_ACCOUNT_ID,
                   account.DISPLAY_ORDER URUT,
                   account.KODEACC_ROOT KODEACC,
                   coa.NAMAACC,
                   coa.PARENTACC,
                   coa.ISHEADER,
                   coa.LVL,
                   coa.POSISI,
                   account.JENIS_AKUNTING,
                   NVL(account.IDDATA, '*') IDDATA_SCOPE,
                   NVL(account.MATCH_MODE, 'TREE') MATCH_MODE,
                   account.GRP_CODE,
                   account.INCLUDE_CHILDREN,
                   account.IS_ACTIVE
              FROM ACCT_REPORT_SECTION_ACCOUNT account
              LEFT JOIN ACCT_COA coa
                ON coa.IDDATA = :iddata
               AND coa.TAHUN = :tahun
               AND coa.KODEACC = account.KODEACC_ROOT
             WHERE account.SECTION_ID = :sectionId
               AND account.IS_ACTIVE = 'Y'
               AND (
                    (:jenisAkunting = 'PKS' AND account.JENIS_AKUNTING = 'PKS')
                    OR
                    (:jenisAkunting <> 'PKS' AND account.JENIS_AKUNTING IN ('*', :jenisAkunting))
               )
               AND (account.IDDATA IS NULL OR account.IDDATA = :iddata)
             ORDER BY account.DISPLAY_ORDER, account.KODEACC_ROOT";

        internal const string ValidationQuery = @"
            WITH scoped_sections AS (
                SELECT section.SECTION_ID,
                       section.SECTION_NAME,
                       section.DISPLAY_ORDER SECTION_ORDER,
                       NVL(section.DISPLAY_LVL, 1) DISPLAY_LVL
                  FROM ACCT_REPORT_SECTION section
                 WHERE section.REPORT_CODE = :reportCode
                   AND section.IS_ACTIVE = 'Y'
                   AND (
                        :reportCode <> 'LABARUGI'
                        OR (:isPks = 'Y' AND section.SECTION_CODE LIKE 'PKS\_%' ESCAPE '\')
                        OR (:isPks = 'N' AND section.SECTION_CODE NOT LIKE 'PKS\_%' ESCAPE '\')
                   )
            ),
            scoped_mappings AS (
                SELECT section.SECTION_ID,
                       section.SECTION_NAME,
                       section.SECTION_ORDER,
                       section.DISPLAY_LVL,
                       account.SECTION_ACCOUNT_ID,
                       account.DISPLAY_ORDER ACCOUNT_ORDER,
                       account.KODEACC_ROOT,
                       NVL(account.MATCH_MODE, 'TREE') MATCH_MODE,
                       account.GRP_CODE
                  FROM scoped_sections section
                  JOIN ACCT_REPORT_SECTION_ACCOUNT account
                    ON account.SECTION_ID = section.SECTION_ID
                 WHERE account.IS_ACTIVE = 'Y'
                   AND (
                        (:jenisAkunting = 'PKS' AND account.JENIS_AKUNTING = 'PKS')
                        OR
                        (:jenisAkunting <> 'PKS' AND account.JENIS_AKUNTING IN ('*', :jenisAkunting))
                   )
                   AND (account.IDDATA IS NULL OR account.IDDATA = :iddata)
            ),
            tree_mappings AS (
                SELECT *
                  FROM scoped_mappings
                 WHERE MATCH_MODE = 'TREE'
            ),
            tree_levels AS (
                SELECT tree_mappings.SECTION_ACCOUNT_ID,
                       coa.LVL
                  FROM tree_mappings
                  JOIN ACCT_COA coa
                    ON coa.IDDATA = :iddata
                   AND coa.TAHUN = :tahun
                 START WITH coa.KODEACC = tree_mappings.KODEACC_ROOT
                CONNECT BY NOCYCLE PRIOR coa.KODEACC = coa.PARENTACC
                       AND PRIOR coa.IDDATA = coa.IDDATA
                       AND PRIOR coa.TAHUN = coa.TAHUN
                       AND PRIOR tree_mappings.SECTION_ACCOUNT_ID = tree_mappings.SECTION_ACCOUNT_ID
            ),
            parent_levels AS (
                SELECT mapping.SECTION_ACCOUNT_ID,
                       coa.LVL
                  FROM scoped_mappings mapping
                  JOIN ACCT_COA coa
                    ON coa.IDDATA = :iddata
                   AND coa.TAHUN = :tahun
                   AND coa.PARENTACC = mapping.KODEACC_ROOT
                 WHERE mapping.MATCH_MODE = 'PARENT'
            ),
            group_levels AS (
                SELECT mapping.SECTION_ACCOUNT_ID,
                       coa.LVL
                  FROM scoped_mappings mapping
                  JOIN ACCT_COA coa
                    ON coa.IDDATA = :iddata
                   AND coa.TAHUN = :tahun
                   AND coa.GRP = mapping.GRP_CODE
                 WHERE mapping.MATCH_MODE = 'GRP_LVL'
            ),
            distinct_levels AS (
                SELECT DISTINCT SECTION_ACCOUNT_ID, LVL
                  FROM (
                        SELECT SECTION_ACCOUNT_ID, LVL FROM tree_levels
                        UNION ALL
                        SELECT SECTION_ACCOUNT_ID, LVL FROM parent_levels
                        UNION ALL
                        SELECT SECTION_ACCOUNT_ID, LVL FROM group_levels
                  )
            ),
            level_summary AS (
                SELECT SECTION_ACCOUNT_ID,
                       LISTAGG(TO_CHAR(LVL), ', ') WITHIN GROUP (ORDER BY LVL) AVAILABLE_LVLS
                  FROM distinct_levels
                 GROUP BY SECTION_ACCOUNT_ID
            )
            SELECT section.SECTION_NAME SECTION,
                   CASE
                       WHEN mapping.SECTION_ACCOUNT_ID IS NULL THEN '-'
                       WHEN mapping.MATCH_MODE = 'GRP_LVL' THEN 'GRP:' || NVL(mapping.GRP_CODE, '-')
                       ELSE mapping.KODEACC_ROOT
                   END ROOT,
                   NVL(mapping.MATCH_MODE, '-') MATCH_MODE,
                   section.DISPLAY_LVL TARGET_LVL,
                   CASE
                       WHEN mapping.SECTION_ACCOUNT_ID IS NULL THEN '-'
                       ELSE NVL(summary.AVAILABLE_LVLS, 'tidak ada')
                   END AVAILABLE_LVLS,
                   CASE
                       WHEN mapping.SECTION_ACCOUNT_ID IS NULL THEN 'No Root'
                       WHEN EXISTS (
                           SELECT 1
                             FROM distinct_levels levels
                            WHERE levels.SECTION_ACCOUNT_ID = mapping.SECTION_ACCOUNT_ID
                              AND levels.LVL = section.DISPLAY_LVL
                       ) THEN 'Valid'
                       ELSE 'Gap LVL'
                   END STATUS,
                   CASE
                       WHEN mapping.SECTION_ACCOUNT_ID IS NULL THEN 'Tambahkan root mapping aktif.'
                       WHEN EXISTS (
                           SELECT 1
                             FROM distinct_levels levels
                            WHERE levels.SECTION_ACCOUNT_ID = mapping.SECTION_ACCOUNT_ID
                              AND levels.LVL = section.DISPLAY_LVL
                       ) THEN '-'
                       ELSE 'Pilih LVL yang tersedia atau perbaiki hierarchy COA.'
                   END RECOMMENDATION
              FROM scoped_sections section
              LEFT JOIN scoped_mappings mapping
                ON mapping.SECTION_ID = section.SECTION_ID
              LEFT JOIN level_summary summary
                ON summary.SECTION_ACCOUNT_ID = mapping.SECTION_ACCOUNT_ID
             ORDER BY section.SECTION_ORDER, mapping.ACCOUNT_ORDER, mapping.SECTION_ACCOUNT_ID";

        private readonly string connectionString;

        public ReportSettingRepository(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<int> GetMaximumCoaYearAsync(string idData, CancellationToken cancellationToken)
        {
            const string sql = "SELECT NVL(MAX(TAHUN), 0) FROM ACCT_COA WHERE IDDATA = :iddata";

            using CancellationTokenSource timeoutCancellation = CreateTimeoutCancellation(cancellationToken);
            CancellationToken effectiveToken = timeoutCancellation.Token;
            try
            {
                using OracleConnection connection = new(connectionString);
                await connection.OpenAsync(effectiveToken).ConfigureAwait(false);
                using OracleCommand command = new(sql, connection)
                {
                    BindByName = true,
                    CommandTimeout = CommandTimeoutSeconds,
                    CommandType = CommandType.Text
                };
                command.Parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = idData;

                object result = await command.ExecuteScalarAsync(effectiveToken).ConfigureAwait(false);
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"Oracle operation exceeded {CommandTimeoutSeconds} seconds.", ex);
            }
        }

        public Task<DataTable> LoadSectionsAsync(
            string reportCode,
            bool isPks,
            CancellationToken cancellationToken)
        {
            return ExecuteTableAsync(
                SectionQuery,
                "Setup",
                parameters =>
                {
                    parameters.Add("reportCode", OracleDbType.Varchar2, 30).Value = reportCode;
                    parameters.Add("isPks", OracleDbType.Char, 1).Value = isPks ? "Y" : "N";
                },
                cancellationToken);
        }

        public Task<DataTable> LoadMappingsAsync(
            int sectionId,
            string idData,
            int year,
            string jenisAkunting,
            CancellationToken cancellationToken)
        {
            return ExecuteTableAsync(
                MappingQuery,
                "Mappings",
                parameters =>
                {
                    parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = idData;
                    parameters.Add("tahun", OracleDbType.Int32).Value = year;
                    parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
                    parameters.Add("jenisAkunting", OracleDbType.Varchar2, 20).Value = jenisAkunting;
                },
                cancellationToken);
        }

        public Task<DataTable> ValidateSectionsAsync(
            string reportCode,
            string idData,
            int year,
            string jenisAkunting,
            CancellationToken cancellationToken)
        {
            return ExecuteTableAsync(
                ValidationQuery,
                "SectionLevelValidation",
                parameters =>
                {
                    parameters.Add("reportCode", OracleDbType.Varchar2, 30).Value = reportCode;
                    parameters.Add("isPks", OracleDbType.Char, 1).Value =
                        string.Equals(jenisAkunting, "PKS", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
                    parameters.Add("jenisAkunting", OracleDbType.Varchar2, 20).Value = jenisAkunting;
                    parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = idData;
                    parameters.Add("tahun", OracleDbType.Int32).Value = year;
                },
                cancellationToken);
        }

        private async Task<DataTable> ExecuteTableAsync(
            string sql,
            string tableName,
            Action<OracleParameterCollection> bindParameters,
            CancellationToken cancellationToken)
        {
            using CancellationTokenSource timeoutCancellation = CreateTimeoutCancellation(cancellationToken);
            CancellationToken effectiveToken = timeoutCancellation.Token;
            try
            {
                using OracleConnection connection = new(connectionString);
                await connection.OpenAsync(effectiveToken).ConfigureAwait(false);
                using OracleCommand command = new(sql, connection)
                {
                    BindByName = true,
                    CommandTimeout = CommandTimeoutSeconds,
                    CommandType = CommandType.Text
                };
                bindParameters(command.Parameters);

                using DbDataReader reader = await command.ExecuteReaderAsync(effectiveToken).ConfigureAwait(false);
                return await ReadTableAsync(reader, tableName, effectiveToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"Oracle operation exceeded {CommandTimeoutSeconds} seconds.", ex);
            }
        }

        private static CancellationTokenSource CreateTimeoutCancellation(CancellationToken cancellationToken)
        {
            CancellationTokenSource timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCancellation.CancelAfter(TimeSpan.FromSeconds(CommandTimeoutSeconds));
            return timeoutCancellation;
        }

        private static async Task<DataTable> ReadTableAsync(
            DbDataReader reader,
            string tableName,
            CancellationToken cancellationToken)
        {
            DataTable table = new(tableName);
            for (int columnIndex = 0; columnIndex < reader.FieldCount; columnIndex++)
            {
                table.Columns.Add(reader.GetName(columnIndex), reader.GetFieldType(columnIndex));
            }

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                object[] values = new object[reader.FieldCount];
                reader.GetValues(values);
                table.Rows.Add(values);
            }

            table.AcceptChanges();
            return table;
        }
    }
}
