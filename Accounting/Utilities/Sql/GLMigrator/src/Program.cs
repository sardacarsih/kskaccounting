using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GLMigrator.Cli;

internal static class Program
{
    private static readonly EmbeddedAssetStore AssetStore = EmbeddedAssetStore.Create();

    private static int Main(string[] args)
    {
        try
        {
            AppOptions options = AppOptions.Parse(args);
            if (options.ShowHelp)
            {
                Console.WriteLine(AppOptions.HelpText);
                return 0;
            }

            options.Connection = ConnectionResolver.Resolve(options);

            EnsureSqlPlusAvailable();

            Directory.CreateDirectory(options.LogDirectory);

            EnsureConnection(options);
            if (options.Mode == MigrationMode.CheckConn)
            {
                return 0;
            }

            Manifest manifest = LoadManifest(options);
            EnsureHistoryTable(options);

            switch (options.Mode)
            {
                case MigrationMode.Up:
                    ApplyUp(options, manifest);
                    break;
                case MigrationMode.Down:
                    ApplyDown(options, manifest);
                    break;
                case MigrationMode.Status:
                    PrintStatus(options, manifest);
                    break;
                case MigrationMode.Verify:
                    RunVerify(options, manifest);
                    break;
                case MigrationMode.ReconcileHistory:
                    ReconcileHistory(options, manifest);
                    break;
                case MigrationMode.RebaselineChecksum:
                    RebaselineChecksum(options, manifest);
                    break;
                case MigrationMode.ShowCompileErrors:
                    ShowCompileErrors(options);
                    break;
                case MigrationMode.ReconcileCoa:
                    ReconcileCoa(options);
                    break;
                case MigrationMode.ShowSource:
                    ShowSource(options);
                    break;
                case MigrationMode.RepairMissingCoa:
                    RepairMissingCoa(options);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported mode: {options.Mode}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[ERROR] " + ex.Message);
            return 1;
        }
    }

    private static void ApplyUp(AppOptions options, Manifest manifest)
    {
        Dictionary<string, AppliedMigration> applied = GetAppliedMigrations(options);
        foreach (MigrationItem migration in manifest.Migrations.OrderBy(m => m.Order))
        {
            AssetContent script = ResolveAsset(options, migration.Script);
            string checksum = ComputeSha256(script.Content);

            if (applied.TryGetValue(migration.Id, out AppliedMigration? existing))
            {
                if (!string.Equals(existing.Checksum, checksum, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Checksum mismatch for '{migration.Id}'. Applied={existing.Checksum}, Current={checksum}");
                }

                Console.WriteLine($"[SKIP] {migration.Id} already applied at {existing.AppliedAt}");
                continue;
            }

            Console.WriteLine($"[APPLY] {migration.Id} -> {migration.Script}");
            Stopwatch sw = Stopwatch.StartNew();
            ExecuteSqlAsset(options, script, $"up_{migration.Id}");
            sw.Stop();

            RegisterMigration(options, migration, checksum, sw.ElapsedMilliseconds);
            Console.WriteLine($"[DONE]  {migration.Id} ({sw.ElapsedMilliseconds}ms)");
        }

        Console.WriteLine("[OK] Migration Up completed.");
    }

    private static void ApplyDown(AppOptions options, Manifest manifest)
    {
        Dictionary<string, AppliedMigration> applied = GetAppliedMigrations(options);
        List<MigrationItem> candidates = manifest.Migrations
            .Where(m => !string.IsNullOrWhiteSpace(m.RollbackScript) && applied.ContainsKey(m.Id))
            .OrderByDescending(m => m.Order)
            .Take(options.Steps)
            .ToList();

        if (candidates.Count == 0)
        {
            Console.WriteLine("[INFO] No applied rollbackable migrations.");
            return;
        }

        foreach (MigrationItem migration in candidates)
        {
            AssetContent rollback = ResolveAsset(options, migration.RollbackScript!);
            Console.WriteLine($"[ROLLBACK] {migration.Id} -> {migration.RollbackScript}");
            ExecuteSqlAsset(options, rollback, $"down_{migration.Id}");
            RemoveMigrationHistory(options, migration.Id);
            Console.WriteLine($"[DONE]     {migration.Id}");
        }

        Console.WriteLine("[OK] Migration Down completed.");
    }

    private static void PrintStatus(AppOptions options, Manifest manifest)
    {
        Dictionary<string, AppliedMigration> applied = GetAppliedMigrations(options);
        foreach (MigrationItem migration in manifest.Migrations.OrderBy(m => m.Order))
        {
            if (applied.TryGetValue(migration.Id, out AppliedMigration? existing))
            {
                Console.WriteLine($"[APPLIED] {migration.Id} at {existing.AppliedAt}");
            }
            else
            {
                Console.WriteLine($"[PENDING] {migration.Id}");
            }
        }
    }

    private static void RunVerify(AppOptions options, Manifest manifest)
    {
        foreach (MigrationItem migration in manifest.Migrations.OrderBy(m => m.Order))
        {
            ExecuteMigrationCheckIfPresent(options, migration, $"verify_{migration.Id}");
        }

        Console.WriteLine("[OK] Verification scripts completed.");
    }

    private static void ExecuteMigrationCheckIfPresent(AppOptions options, MigrationItem migration, string logPrefix)
    {
        if (string.IsNullOrWhiteSpace(migration.CheckScript))
        {
            return;
        }

        AssetContent check = ResolveAsset(options, migration.CheckScript!);
        Console.WriteLine($"[VERIFY] {migration.Id} -> {migration.CheckScript}");
        ExecuteSqlAsset(options, check, logPrefix);
    }

    private static void ReconcileHistory(AppOptions options, Manifest manifest)
    {
        Dictionary<string, AppliedMigration> applied = GetAppliedMigrations(options);
        int baselinedCount = 0;

        foreach (MigrationItem migration in manifest.Migrations.OrderBy(m => m.Order))
        {
            AssetContent script = ResolveAsset(options, migration.Script);
            string checksum = ComputeSha256(script.Content);

            if (applied.TryGetValue(migration.Id, out AppliedMigration? existing))
            {
                if (!string.Equals(existing.Checksum, checksum, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Checksum mismatch for '{migration.Id}'. Applied={existing.Checksum}, Current={checksum}");
                }

                Console.WriteLine($"[SKIP] {migration.Id} already applied at {existing.AppliedAt}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(migration.CheckScript))
            {
                throw new InvalidOperationException(
                    $"Cannot reconcile '{migration.Id}' because it has no check script. Use normal --mode up on a clean database.");
            }

            AssetContent check = ResolveAsset(options, migration.CheckScript!);
            Console.WriteLine($"[CHECK] {migration.Id} -> {migration.CheckScript}");

            try
            {
                ExecuteSqlAsset(options, check, $"reconcile_{migration.Id}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Cannot baseline '{migration.Id}' because its check script failed. " +
                    "Stop here and do not run --mode up until the database state is fixed. Detail: " + ex.Message, ex);
            }

            RegisterMigration(options, migration, checksum, 0);
            applied[migration.Id] = new AppliedMigration(checksum, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            baselinedCount++;
            Console.WriteLine($"[BASELINE] {migration.Id}");
        }

        Console.WriteLine($"[OK] Reconciled migration history. Baselined={baselinedCount}.");
    }

    private static void RebaselineChecksum(AppOptions options, Manifest manifest)
    {
        if (string.IsNullOrWhiteSpace(options.MigrationId))
        {
            throw new InvalidOperationException(
                "--mode rebaselinechecksum requires --migration-id <id>.");
        }

        MigrationItem? migration = manifest.Migrations
            .FirstOrDefault(m => string.Equals(m.Id, options.MigrationId, StringComparison.OrdinalIgnoreCase));
        if (migration is null)
        {
            throw new InvalidOperationException($"Migration '{options.MigrationId}' not found in manifest.");
        }

        Dictionary<string, AppliedMigration> applied = GetAppliedMigrations(options);
        if (!applied.TryGetValue(migration.Id, out AppliedMigration? existing))
        {
            throw new InvalidOperationException(
                $"'{migration.Id}' is not recorded as applied. Use --mode up or --mode reconcilehistory instead.");
        }

        AssetContent script = ResolveAsset(options, migration.Script);
        string currentChecksum = ComputeSha256(script.Content);

        if (string.Equals(existing.Checksum, currentChecksum, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[SKIP] '{migration.Id}' recorded checksum already matches the current script.");
            return;
        }

        if (options.Reexecute)
        {
            Console.WriteLine($"[REEXECUTE] {migration.Id} -> {migration.Script}");
            ExecuteSqlAsset(options, script, $"reexecute_{migration.Id}");
            ExecuteMigrationCheckIfPresent(options, migration, $"recheck_{migration.Id}");

            Console.WriteLine(
                $"[REBASELINE] {migration.Id}: {existing.Checksum} -> {currentChecksum} " +
                "(current script re-executed against this server before rebaselining)");
        }
        else
        {
            Console.WriteLine(
                $"[REBASELINE] {migration.Id}: {existing.Checksum} -> {currentChecksum} " +
                "(script content changed after this migration was applied; no SQL is re-executed)");
        }

        UpdateMigrationChecksum(options, migration.Id, currentChecksum);
        Console.WriteLine($"[OK] Rebaselined checksum for '{migration.Id}'.");
    }

    private static void ShowCompileErrors(AppOptions options)
    {
        string[] names = string.IsNullOrWhiteSpace(options.ObjectName)
            ? ["ACCT_REPORT_ENGINE_V1", "ACCT_LAPORAN_V2"]
            : options.ObjectName.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        string nameList = string.Join(",", names.Select(n => $"'{EscapeSqlLiteral(n.ToUpperInvariant())}'"));

        const string columns = "NAME || '|' || TYPE || '|' || LINE || '|' || POSITION || '|' || TEXT";
        string sql = $"""
SET SERVEROUTPUT ON
SET HEADING OFF
SET FEEDBACK OFF
SET PAGESIZE 0
SET LINESIZE 32767
SET TRIMSPOOL ON
BEGIN
    FOR obj IN (
        SELECT OBJECT_NAME, OBJECT_TYPE
          FROM USER_OBJECTS
         WHERE OBJECT_NAME IN ({nameList})
           AND OBJECT_TYPE IN ('PACKAGE', 'PACKAGE BODY')
           AND STATUS = 'INVALID'
    ) LOOP
        BEGIN
            IF obj.OBJECT_TYPE = 'PACKAGE' THEN
                EXECUTE IMMEDIATE 'ALTER PACKAGE ' || obj.OBJECT_NAME || ' COMPILE';
            ELSE
                EXECUTE IMMEDIATE 'ALTER PACKAGE ' || obj.OBJECT_NAME || ' COMPILE BODY';
            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                NULL;
        END;
    END LOOP;
END;
/
SELECT {columns}
  FROM USER_ERRORS
 WHERE NAME IN ({nameList})
 ORDER BY NAME, SEQUENCE;
EXIT
""";

        SqlExecutionResult result = ExecuteSqlInline(options, sql, "show_compile_errors");
        Console.WriteLine(result.Output.Trim());
    }

    private static void ReconcileCoa(AppOptions options)
    {
        List<string> missing = [];
        if (string.IsNullOrWhiteSpace(options.IdData)) missing.Add("--iddata");
        if (string.IsNullOrWhiteSpace(options.Periode)) missing.Add("--periode");
        if (options.Tahun is null) missing.Add("--tahun");
        if (options.Bulan is null) missing.Add("--bulan");

        if (missing.Count > 0)
        {
            throw new ArgumentException(
                $"--mode reconcilecoa requires {string.Join(", ", missing)}. Example: --iddata FSLFKM --periode 01/2026 --tahun 2026 --bulan 1");
        }

        string idData = EscapeSqlLiteral(options.IdData);
        string periode = EscapeSqlLiteral(options.Periode);
        int tahun = options.Tahun!.Value;
        int bulan = options.Bulan!.Value;

        string sql = $"""
SET SERVEROUTPUT ON
SET HEADING ON
SET FEEDBACK OFF
SET LINESIZE 32767
SET TRIMSPOOL ON
COLUMN KODEACC FORMAT A30;
COLUMN NAMAACC FORMAT A60;
COLUMN COA_DEBET FORMAT 999999999999990.99;
COLUMN SRC_DEBET FORMAT 999999999999990.99;
COLUMN DIFF_DEBET FORMAT 999999999999990.99;
COLUMN COA_KREDIT FORMAT 999999999999990.99;
COLUMN SRC_KREDIT FORMAT 999999999999990.99;
COLUMN DIFF_KREDIT FORMAT 999999999999990.99;
WITH coa_values AS (
    SELECT c.IDDATA,
           c.TAHUN,
           c.KODEACC,
           c.NAMAACC,
           NVL(c."{bulan}D", 0) AS COA_DEBET,
           NVL(c."{bulan}K", 0) AS COA_KREDIT
      FROM ACCT_COA c
     WHERE c.IDDATA = '{idData}'
       AND c.TAHUN = {tahun}
), coa_scope AS (
    SELECT c.KODEACC,
           c.PARENTACC
      FROM ACCT_COA c
     WHERE c.IDDATA = '{idData}'
       AND c.TAHUN = {tahun}
), posted_accounts AS (
    SELECT d.KODE AS KODEACC,
           SUM(NVL(d.DEBET, 0)) AS DEBET,
           SUM(NVL(d.KREDIT, 0)) AS KREDIT
      FROM ACCT_JURNAL_DTL d
     WHERE d.IDDATA = '{idData}'
       AND d.PERIODE = '{periode}'
       AND d.KODE IS NOT NULL
     GROUP BY d.KODE
), account_ancestors AS (
    SELECT CONNECT_BY_ROOT cs.KODEACC AS POSTED_KODEACC,
           cs.KODEACC AS KODEACC
      FROM coa_scope cs
     START WITH cs.KODEACC IN (SELECT KODEACC FROM posted_accounts)
     CONNECT BY NOCYCLE cs.KODEACC = PRIOR cs.PARENTACC
), source_values AS (
    SELECT aa.KODEACC,
           SUM(pa.DEBET) AS SRC_DEBET,
           SUM(pa.KREDIT) AS SRC_KREDIT
      FROM account_ancestors aa
      JOIN posted_accounts pa ON pa.KODEACC = aa.POSTED_KODEACC
     GROUP BY aa.KODEACC
)
SELECT c.KODEACC,
       c.NAMAACC,
       c.COA_DEBET,
       NVL(s.SRC_DEBET, 0) AS SRC_DEBET,
       c.COA_DEBET - NVL(s.SRC_DEBET, 0) AS DIFF_DEBET,
       c.COA_KREDIT,
       NVL(s.SRC_KREDIT, 0) AS SRC_KREDIT,
       c.COA_KREDIT - NVL(s.SRC_KREDIT, 0) AS DIFF_KREDIT
  FROM coa_values c
  LEFT JOIN source_values s ON s.KODEACC = c.KODEACC
 WHERE ABS(c.COA_DEBET - NVL(s.SRC_DEBET, 0)) > 0.005
    OR ABS(c.COA_KREDIT - NVL(s.SRC_KREDIT, 0)) > 0.005
 ORDER BY c.KODEACC;

COLUMN NOJURNAL FORMAT A30;
COLUMN TOTAL_DEBET FORMAT 999999999999990.99;
COLUMN TOTAL_KREDIT FORMAT 999999999999990.99;
COLUMN JURNAL_DIFF FORMAT 999999999999990.99;
SELECT d.REFFID,
       d.NOJURNAL,
       SUM(NVL(d.DEBET, 0)) AS TOTAL_DEBET,
       SUM(NVL(d.KREDIT, 0)) AS TOTAL_KREDIT,
       SUM(NVL(d.DEBET, 0)) - SUM(NVL(d.KREDIT, 0)) AS JURNAL_DIFF
  FROM ACCT_JURNAL_DTL d
 WHERE d.IDDATA = '{idData}'
   AND d.PERIODE = '{periode}'
 GROUP BY d.REFFID, d.NOJURNAL
HAVING ABS(SUM(NVL(d.DEBET, 0)) - SUM(NVL(d.KREDIT, 0))) > 0.005
 ORDER BY ABS(SUM(NVL(d.DEBET, 0)) - SUM(NVL(d.KREDIT, 0))) DESC;

SELECT SUM(NVL(d.DEBET, 0)) AS TOTAL_DEBET,
       SUM(NVL(d.KREDIT, 0)) AS TOTAL_KREDIT,
       SUM(NVL(d.DEBET, 0)) - SUM(NVL(d.KREDIT, 0)) AS GRAND_DIFF
  FROM ACCT_JURNAL_DTL d
 WHERE d.IDDATA = '{idData}'
   AND d.PERIODE = '{periode}';

COLUMN LVL FORMAT 999;
COLUMN ROW_COUNT FORMAT 999;
SELECT c.KODEACC,
       c.NAMAACC,
       c.LVL,
       COUNT(*) AS ROW_COUNT,
       NVL(c."{bulan}D", 0) AS COA_DEBET,
       NVL(c."{bulan}K", 0) AS COA_KREDIT
  FROM ACCT_COA c
 WHERE c.IDDATA = '{idData}'
   AND c.TAHUN = {tahun}
 GROUP BY c.KODEACC, c.NAMAACC, c.LVL, c."{bulan}D", c."{bulan}K"
HAVING COUNT(*) > 1
 ORDER BY c.KODEACC;

SELECT c.LVL,
       COUNT(*) AS ROW_COUNT,
       SUM(NVL(c."{bulan}D", 0)) AS SUM_DEBET,
       SUM(NVL(c."{bulan}K", 0)) AS SUM_KREDIT,
       SUM(NVL(c."{bulan}D", 0)) - SUM(NVL(c."{bulan}K", 0)) AS LVL_DIFF
  FROM ACCT_COA c
 WHERE c.IDDATA = '{idData}'
   AND c.TAHUN = {tahun}
 GROUP BY c.LVL
 ORDER BY c.LVL;

COLUMN PARENTACC FORMAT A30;
SELECT c.KODEACC,
       c.NAMAACC,
       c.PARENTACC,
       NVL(c."{bulan}D", 0) AS COA_DEBET,
       NVL(c."{bulan}K", 0) AS COA_KREDIT,
       NVL(c."{bulan}D", 0) - NVL(c."{bulan}K", 0) AS NET_DIFF
  FROM ACCT_COA c
 WHERE c.IDDATA = '{idData}'
   AND c.TAHUN = {tahun}
   AND c.LVL = 1
 ORDER BY ABS(NVL(c."{bulan}D", 0) - NVL(c."{bulan}K", 0)) DESC;

COLUMN NAMAACC_PRIORYEAR FORMAT A40;
COLUMN POSTED_DEBET FORMAT 999999999999990.99;
COLUMN POSTED_KREDIT FORMAT 999999999999990.99;
COLUMN POSTED_DIFF FORMAT 999999999999990.99;
SELECT d.KODE,
       (SELECT MAX(p.NAMAACC) FROM ACCT_COA p WHERE p.IDDATA = '{idData}' AND p.KODEACC = d.KODE AND p.TAHUN = {tahun} - 1) AS NAMAACC_PRIORYEAR,
       SUM(NVL(d.DEBET, 0)) AS POSTED_DEBET,
       SUM(NVL(d.KREDIT, 0)) AS POSTED_KREDIT,
       SUM(NVL(d.DEBET, 0)) - SUM(NVL(d.KREDIT, 0)) AS POSTED_DIFF
  FROM ACCT_JURNAL_DTL d
 WHERE d.IDDATA = '{idData}'
   AND d.PERIODE = '{periode}'
   AND d.KODE IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM ACCT_COA c
        WHERE c.IDDATA = d.IDDATA
          AND c.TAHUN = {tahun}
          AND c.KODEACC = d.KODE
   )
 GROUP BY d.KODE
 ORDER BY ABS(SUM(NVL(d.DEBET, 0)) - SUM(NVL(d.KREDIT, 0))) DESC
 FETCH FIRST 15 ROWS ONLY;

SELECT COUNT(*) AS MISSING_ACCOUNT_COUNT,
       SUM(NVL(d.DEBET, 0)) AS SUM_DEBET,
       SUM(NVL(d.KREDIT, 0)) AS SUM_KREDIT,
       SUM(NVL(d.DEBET, 0)) - SUM(NVL(d.KREDIT, 0)) AS SUM_DIFF
  FROM (
       SELECT d.KODE, SUM(NVL(d.DEBET,0)) AS DEBET, SUM(NVL(d.KREDIT,0)) AS KREDIT
         FROM ACCT_JURNAL_DTL d
        WHERE d.IDDATA = '{idData}'
          AND d.PERIODE = '{periode}'
          AND d.KODE IS NOT NULL
          AND NOT EXISTS (
              SELECT 1 FROM ACCT_COA c
               WHERE c.IDDATA = d.IDDATA
                 AND c.TAHUN = {tahun}
                 AND c.KODEACC = d.KODE
          )
        GROUP BY d.KODE
  ) d;

COLUMN MISSING_PARENT FORMAT A30;
SELECT DISTINCT p2025.PARENTACC AS MISSING_PARENT
  FROM ACCT_COA p2025
 WHERE p2025.IDDATA = '{idData}'
   AND p2025.TAHUN = {tahun} - 1
   AND p2025.PARENTACC IS NOT NULL
   AND EXISTS (
       SELECT 1 FROM ACCT_COA c2026
        WHERE c2026.IDDATA = p2025.IDDATA AND c2026.TAHUN = {tahun} AND c2026.KODEACC = p2025.KODEACC
   )
   AND NOT EXISTS (
       SELECT 1 FROM ACCT_COA parent2026
        WHERE parent2026.IDDATA = p2025.IDDATA AND parent2026.TAHUN = {tahun} AND parent2026.KODEACC = p2025.PARENTACC
   );

COLUMN ISHEADER FORMAT A10;
SELECT p.ISHEADER,
       COUNT(*) AS ACCOUNT_COUNT
  FROM ACCT_COA p
 WHERE p.IDDATA = '{idData}'
   AND p.TAHUN = {tahun} - 1
   AND p.KODEACC IN (
       SELECT DISTINCT d.KODE
         FROM ACCT_JURNAL_DTL d
        WHERE d.IDDATA = '{idData}'
          AND d.PERIODE = '{periode}'
          AND d.KODE IS NOT NULL
          AND NOT EXISTS (
              SELECT 1 FROM ACCT_COA c
               WHERE c.IDDATA = d.IDDATA
                 AND c.TAHUN = {tahun}
                 AND c.KODEACC = d.KODE
          )
   )
 GROUP BY p.ISHEADER;

COLUMN PERIODE FORMAT A10;
SELECT d.PERIODE,
       COUNT(DISTINCT d.KODE) AS DISTINCT_ACCOUNTS,
       SUM(NVL(d.DEBET, 0)) AS PERIODE_DEBET,
       SUM(NVL(d.KREDIT, 0)) AS PERIODE_KREDIT
  FROM ACCT_JURNAL_DTL d
 WHERE d.IDDATA = '{idData}'
   AND d.PERIODE LIKE '%/{tahun}'
   AND d.KODE IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM ACCT_COA c
        WHERE c.IDDATA = d.IDDATA
          AND c.TAHUN = {tahun}
          AND c.KODEACC = d.KODE
   )
 GROUP BY d.PERIODE
 ORDER BY d.PERIODE;
EXIT
""";

        SqlExecutionResult result = ExecuteSqlInline(options, sql, "reconcile_coa");
        Console.WriteLine(result.Output.Trim());
    }

    private static void ShowSource(AppOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ObjectName))
        {
            throw new ArgumentException("--mode showsource requires --object-name PACKAGE_NAME (optionally PACKAGE_NAME.PROCEDURE_NAME to filter by TEXT).");
        }

        string objectName = options.ObjectName.Trim().ToUpperInvariant();
        string packageName = objectName;
        string procedureFilter = string.Empty;
        int dotIndex = objectName.IndexOf('.');
        if (dotIndex > 0)
        {
            packageName = objectName[..dotIndex];
            procedureFilter = objectName[(dotIndex + 1)..];
        }

        string escapedPackage = EscapeSqlLiteral(packageName);

        string sql = $"""
SET HEADING OFF
SET FEEDBACK OFF
SET PAGESIZE 0
SET LINESIZE 32767
SET TRIMSPOOL ON
SELECT LINE || ': ' || TEXT
  FROM USER_SOURCE
 WHERE NAME = '{escapedPackage}'
   AND TYPE = 'PACKAGE BODY'
 ORDER BY LINE;
EXIT
""";

        SqlExecutionResult result = ExecuteSqlInline(options, sql, "show_source");
        string output = result.Output.Trim();

        if (!string.IsNullOrWhiteSpace(procedureFilter))
        {
            string[] lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            List<string> matching = [];
            bool inside = false;
            foreach (string line in lines)
            {
                string upper = line.ToUpperInvariant();
                bool startsProcOrFunc = upper.Contains($"PROCEDURE {procedureFilter}") || upper.Contains($"FUNCTION {procedureFilter}");
                if (startsProcOrFunc)
                {
                    inside = true;
                }
                else if (inside && (upper.Contains("PROCEDURE ") || upper.Contains("FUNCTION ")) && !startsProcOrFunc)
                {
                    inside = false;
                }

                if (inside)
                {
                    matching.Add(line);
                }
            }

            output = matching.Count > 0 ? string.Join(Environment.NewLine, matching) : output;
        }

        Console.WriteLine(output);
    }

    private static void RepairMissingCoa(AppOptions options)
    {
        List<string> missing = [];
        if (string.IsNullOrWhiteSpace(options.IdData)) missing.Add("--iddata");
        if (options.Tahun is null) missing.Add("--tahun");
        if (string.IsNullOrWhiteSpace(options.BulanList)) missing.Add("--bulan-list");
        if (string.IsNullOrWhiteSpace(options.UserId)) missing.Add("--userid");

        if (missing.Count > 0)
        {
            throw new ArgumentException(
                $"--mode repairmissingcoa requires {string.Join(", ", missing)}. Example: --iddata FSLFKM --tahun 2026 --bulan-list 1,2,3,4 --userid ADMIN [--apply]");
        }

        string idData = EscapeSqlLiteral(options.IdData);
        int tahun = options.Tahun!.Value;
        string userId = EscapeSqlLiteral(options.UserId);

        int[] bulanValues = options.BulanList
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(b =>
            {
                if (!int.TryParse(b, out int v) || v is < 1 or > 12)
                {
                    throw new ArgumentException($"Invalid month '{b}' in --bulan-list. Each value must be an integer 1-12.");
                }
                return v;
            })
            .ToArray();

        string previewSql = $"""
SET HEADING ON
SET FEEDBACK OFF
SET LINESIZE 32767
SET TRIMSPOOL ON
COLUMN KODEACC FORMAT A20;
COLUMN NAMAACC FORMAT A50;
COLUMN GRP FORMAT A5;
COLUMN ISHEADER FORMAT A5;
COLUMN SALDOAWAL FORMAT 999999999999990.99;
WITH needed_leaf AS (
    SELECT DISTINCT d.KODE AS KODEACC
      FROM ACCT_JURNAL_DTL d
     WHERE d.IDDATA = '{idData}'
       AND d.PERIODE LIKE '%/{tahun}'
       AND d.KODE IS NOT NULL
), prior_year AS (
    SELECT p.KODEACC, p.PARENTACC, p.NAMAACC, p.GRP, p.ISHEADER, p."12S" AS SALDOAKHIR
      FROM ACCT_COA p
     WHERE p.IDDATA = '{idData}'
       AND p.TAHUN = {tahun} - 1
), needed_closure AS (
    SELECT KODEACC FROM needed_leaf
    UNION
    SELECT py.KODEACC
      FROM prior_year py
     START WITH py.KODEACC IN (SELECT KODEACC FROM needed_leaf)
     CONNECT BY NOCYCLE py.KODEACC = PRIOR py.PARENTACC
)
SELECT py.KODEACC,
       py.NAMAACC,
       py.GRP,
       py.ISHEADER,
       CASE WHEN py.GRP IN ('11','12','13','14','15','16','17','18','19','20') THEN 0 ELSE NVL(py.SALDOAKHIR, 0) END AS SALDOAWAL
  FROM prior_year py
 WHERE py.KODEACC IN (SELECT KODEACC FROM needed_closure)
   AND NOT EXISTS (
       SELECT 1 FROM ACCT_COA c WHERE c.IDDATA = '{idData}' AND c.TAHUN = {tahun} AND c.KODEACC = py.KODEACC
   )
 ORDER BY py.KODEACC;
EXIT
""";

        Console.WriteLine(options.Apply
            ? "[INFO] Preview of accounts about to be inserted into ACCT_COA (before --apply changes anything):"
            : "[DRY RUN] Accounts that WOULD be inserted into ACCT_COA (pass --apply to execute):");
        SqlExecutionResult previewResult = ExecuteSqlInline(options, previewSql, "repair_missing_coa_preview");
        Console.WriteLine(previewResult.Output.Trim());

        if (!options.Apply)
        {
            Console.WriteLine();
            Console.WriteLine("[DRY RUN] No changes were made. Re-run with --apply to insert the rows above and recalc: " +
                               string.Join(",", bulanValues));
            return;
        }

        string insertSql = $"""
SET SERVEROUTPUT ON
SET FEEDBACK ON
SET LINESIZE 32767
INSERT INTO ACCT_COA (ACCTCOAID, IDDATA, TAHUN, KODEACC, PARENTACC, ISHEADER, NAMAACC, POSISI, GRP, LVL, ISAKTIF, SALDOAWAL, BLOK, DIVISI, TAHUNTANAM)
WITH needed_leaf AS (
    SELECT DISTINCT d.KODE AS KODEACC
      FROM ACCT_JURNAL_DTL d
     WHERE d.IDDATA = '{idData}'
       AND d.PERIODE LIKE '%/{tahun}'
       AND d.KODE IS NOT NULL
), prior_year AS (
    SELECT p.KODEACC, p.PARENTACC, p.ISHEADER, p.NAMAACC, p.POSISI, p.GRP, p.LVL, p.ISAKTIF, p."12S" AS SALDOAKHIR, p.BLOK, p.DIVISI, p.TAHUNTANAM
      FROM ACCT_COA p
     WHERE p.IDDATA = '{idData}'
       AND p.TAHUN = {tahun} - 1
), needed_closure AS (
    SELECT KODEACC FROM needed_leaf
    UNION
    SELECT py.KODEACC
      FROM prior_year py
     START WITH py.KODEACC IN (SELECT KODEACC FROM needed_leaf)
     CONNECT BY NOCYCLE py.KODEACC = PRIOR py.PARENTACC
)
SELECT '{idData}' || {tahun} || py.KODEACC,
       '{idData}',
       {tahun},
       py.KODEACC,
       py.PARENTACC,
       py.ISHEADER,
       py.NAMAACC,
       py.POSISI,
       py.GRP,
       py.LVL,
       py.ISAKTIF,
       CASE WHEN py.GRP IN ('11','12','13','14','15','16','17','18','19','20') THEN 0 ELSE NVL(py.SALDOAKHIR, 0) END,
       py.BLOK,
       py.DIVISI,
       py.TAHUNTANAM
  FROM prior_year py
 WHERE py.KODEACC IN (SELECT KODEACC FROM needed_closure)
   AND NOT EXISTS (
       SELECT 1 FROM ACCT_COA c WHERE c.IDDATA = '{idData}' AND c.TAHUN = {tahun} AND c.KODEACC = py.KODEACC
   );
COMMIT;
EXIT
""";

        Console.WriteLine();
        Console.WriteLine("[APPLY] Inserting missing ACCT_COA rows...");
        SqlExecutionResult insertResult = ExecuteSqlInline(options, insertSql, "repair_missing_coa_insert");
        Console.WriteLine(insertResult.Output.Trim());

        string recalcBlock = string.Join(Environment.NewLine, bulanValues.Select(b =>
            $"    ACCT_RECALLCULATIONS_V2.ReCalcPeriod('{idData}', {b}, {tahun}, '{b:00}/{tahun}', '{userId}');"));

        string recalcSql = $"""
SET SERVEROUTPUT ON
SET FEEDBACK OFF
BEGIN
{recalcBlock}
END;
/
EXIT
""";

        Console.WriteLine();
        Console.WriteLine("[APPLY] Recalculating periods: " + string.Join(",", bulanValues.Select(b => $"{b:00}/{tahun}")));
        SqlExecutionResult recalcResult = ExecuteSqlInline(options, recalcSql, "repair_missing_coa_recalc");
        Console.WriteLine(recalcResult.Output.Trim());

        // Per-period verification: reuse the LVL=1 Balanced_Check formula for each recalculated month.
        string verifyUnion = string.Join(Environment.NewLine + "UNION ALL" + Environment.NewLine, bulanValues.Select(b => $"""
SELECT '{b:00}/{tahun}' AS PERIODE,
       ROUND(NVL(SUM(NVL(c."{b}D", 0)), 0) - NVL(SUM(NVL(c."{b}K", 0)), 0), 2) AS SELISIH
  FROM ACCT_COA c
 WHERE c.IDDATA = '{idData}' AND c.TAHUN = {tahun} AND c.LVL = 1
"""));

        string finalVerifySql = $"""
SET HEADING ON
SET FEEDBACK OFF
SET LINESIZE 32767
COLUMN PERIODE FORMAT A10;
COLUMN SELISIH FORMAT 999999999999990.99;
{verifyUnion}
ORDER BY 1;
EXIT
""";

        Console.WriteLine();
        Console.WriteLine("[VERIFY] Balanced_Check-equivalent selisih per recalculated period (should all be 0):");
        SqlExecutionResult verifyResult = ExecuteSqlInline(options, finalVerifySql, "repair_missing_coa_verify");
        Console.WriteLine(verifyResult.Output.Trim());
    }

    private static void EnsureConnection(AppOptions options)
    {
        Console.WriteLine("[INFO] Checking database connection...");

        const string sql = """
SET HEADING OFF
SET FEEDBACK OFF
SET PAGESIZE 0
SELECT 'CONN_OK' FROM DUAL;
EXIT
""";

        try
        {
            SqlExecutionResult result = ExecuteSqlInline(options, sql, "conn_check", options.ConnTimeoutMs);
            if (!result.Output.Contains("CONN_OK", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Connection probe tidak mengembalikan hasil yang diharapkan. Output: " + result.Output.Trim());
            }

            Console.WriteLine("[INFO] Database connection OK.");
        }
        catch (Exception ex)
        {
            int seconds = Math.Max(1, options.ConnTimeoutMs / 1000);
            throw new InvalidOperationException(
                $"Tidak bisa connect ke database (atau timeout {seconds}s)." + Environment.NewLine +
                "        Periksa: config.json (Host/Port/ServiceName/UserId/Password)," + Environment.NewLine +
                "        sqlnet.ora (set SQLNET.AUTHENTICATION_SERVICES=(NONE) bila ORA-12638)," + Environment.NewLine +
                "        atau pakai --connection \"USER/PASS@//HOST:PORT/SERVICE\"." + Environment.NewLine +
                "        Detail: " + ex.Message);
        }
    }

    private static void EnsureHistoryTable(AppOptions options)
    {
        Console.WriteLine("[INFO] Ensuring GL_MIGRATION_HISTORY exists...");
        ExecuteSqlAsset(options, ResolveBootstrapAsset(options), "bootstrap");
    }

    private static Dictionary<string, AppliedMigration> GetAppliedMigrations(AppOptions options)
    {
        const string sql = """
SET HEADING OFF
SET FEEDBACK OFF
SET PAGESIZE 0
SET LINESIZE 32767
SET VERIFY OFF
SET ECHO OFF
SET TRIMSPOOL ON
SELECT MIGRATION_ID || '|' || CHECKSUM_SHA256 || '|' || TO_CHAR(APPLIED_AT, 'YYYY-MM-DD HH24:MI:SS')
  FROM GL_MIGRATION_HISTORY
 ORDER BY APPLIED_AT;
EXIT
""";

        SqlExecutionResult result = ExecuteSqlInline(options, sql, "status_query");
        string[] lines = result.Output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, AppliedMigration> map = new(StringComparer.OrdinalIgnoreCase);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (!line.Contains('|'))
            {
                continue;
            }

            string[] parts = line.Split('|');
            if (parts.Length < 3)
            {
                continue;
            }

            string migrationId = parts[0].Trim();
            string checksum = parts[1].Trim();
            string appliedAt = parts[2].Trim();

            if (string.IsNullOrWhiteSpace(migrationId))
            {
                continue;
            }

            map[migrationId] = new AppliedMigration(checksum, appliedAt);
        }

        return map;
    }

    private static void RegisterMigration(AppOptions options, MigrationItem migration, string checksum, long executionMs)
    {
        string migrationId = EscapeSqlLiteral(migration.Id);
        string migrationChecksum = EscapeSqlLiteral(checksum);
        string migrationDescription = EscapeSqlLiteral(migration.Description ?? migration.Id);
        string migrationScriptPath = EscapeSqlLiteral(migration.Script);

        string sql = $"""
INSERT INTO GL_MIGRATION_HISTORY
    (MIGRATION_ID, CHECKSUM_SHA256, DESCRIPTION, SCRIPT_PATH, STATUS, EXECUTION_MS)
VALUES
    ('{migrationId}', '{migrationChecksum}', '{migrationDescription}', '{migrationScriptPath}', 'SUCCESS', {executionMs});
COMMIT;
EXIT
""";

        ExecuteSqlInline(options, sql, $"register_{migration.Id}");
    }

    private static void UpdateMigrationChecksum(AppOptions options, string migrationId, string newChecksum)
    {
        string escapedId = EscapeSqlLiteral(migrationId);
        string escapedChecksum = EscapeSqlLiteral(newChecksum);
        string sql = $"""
UPDATE GL_MIGRATION_HISTORY
   SET CHECKSUM_SHA256 = '{escapedChecksum}'
 WHERE MIGRATION_ID = '{escapedId}';
COMMIT;
EXIT
""";

        ExecuteSqlInline(options, sql, $"rebaseline_{migrationId}");
    }

    private static void RemoveMigrationHistory(AppOptions options, string migrationId)
    {
        string escapedId = EscapeSqlLiteral(migrationId);
        string sql = $"""
DELETE FROM GL_MIGRATION_HISTORY WHERE MIGRATION_ID = '{escapedId}';
COMMIT;
EXIT
""";

        ExecuteSqlInline(options, sql, $"unregister_{migrationId}");
    }

    private static SqlExecutionResult ExecuteSqlInline(AppOptions options, string sql, string logPrefix, int? timeoutMsOverride = null)
    {
        return ExecuteSqlAsset(options, AssetContent.FromText($"inline/{logPrefix}.sql", sql), logPrefix, timeoutMsOverride);
    }

    private static SqlExecutionResult ExecuteSqlAsset(AppOptions options, AssetContent asset, string logPrefix, int? timeoutMsOverride = null)
    {
        string tempFilePath = WriteAssetToTempFile(asset);
        try
        {
            return ExecuteSqlFile(options, tempFilePath, logPrefix, timeoutMsOverride);
        }
        finally
        {
            TryDeleteFile(tempFilePath);
        }
    }

    private static SqlExecutionResult ExecuteSqlFile(AppOptions options, string sqlPath, string logPrefix, int? timeoutMsOverride = null)
    {
        int timeoutMs = timeoutMsOverride ?? options.ScriptTimeoutMs;
        if (!File.Exists(sqlPath))
        {
            throw new FileNotFoundException("SQL file not found.", sqlPath);
        }

        string absoluteSqlPath = Path.GetFullPath(sqlPath).Replace('\\', '/');
        string wrapperSql = $$"""
WHENEVER OSERROR EXIT FAILURE ROLLBACK
WHENEVER SQLERROR EXIT FAILURE ROLLBACK
SET SERVEROUTPUT ON
SET DEFINE ON
@{{absoluteSqlPath}}
EXIT
""";

        string wrapperPath = Path.Combine(Path.GetTempPath(), $"glmigrator_wrapper_{Guid.NewGuid():N}.sql");
        File.WriteAllText(wrapperPath, wrapperSql, Encoding.ASCII);

        string logName = $"{SanitizeFileName(logPrefix)}_{DateTime.Now:yyyyMMdd_HHmmss}.log";
        string logPath = Path.Combine(options.LogDirectory, logName);

        try
        {
            ProcessStartInfo psi = new("sqlplus")
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("-s");
            psi.ArgumentList.Add(options.Connection);
            psi.ArgumentList.Add("@" + wrapperPath);

            using Process process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start sqlplus process.");

            // Close stdin so sqlplus receives EOF on any unexpected prompt
            // (e.g. bad credentials -> "Enter user-name:") instead of hanging forever.
            try { process.StandardInput.Close(); } catch { /* ignore */ }

            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();

            bool exited = process.WaitForExit(timeoutMs);
            if (!exited)
            {
                try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
            }

            string stdout = TryGetStreamResult(stdoutTask);
            string stderr = TryGetStreamResult(stderrTask);
            string output = string.IsNullOrWhiteSpace(stderr) ? stdout : stdout + Environment.NewLine + stderr;
            File.WriteAllText(logPath, output, Encoding.ASCII);

            if (!exited)
            {
                int seconds = Math.Max(1, timeoutMs / 1000);
                throw new InvalidOperationException(
                    $"SQL execution timed out after {seconds}s ({logPrefix}). Proses sqlplus dihentikan paksa. See log: {logPath}");
            }

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"SQL execution failed ({logPrefix}). See log: {logPath}");
            }

            return new SqlExecutionResult(output, logPath);
        }
        finally
        {
            TryDeleteFile(wrapperPath);
        }
    }

    private static Manifest LoadManifest(AppOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ManifestPathOverride))
        {
            string fullPath = Path.GetFullPath(options.ManifestPathOverride);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Manifest not found.", fullPath);
            }

            string externalJson = File.ReadAllText(fullPath, Encoding.UTF8);
            return DeserializeManifest(externalJson, fullPath);
        }

        AssetContent manifestAsset = AssetStore.GetRequired("migrations.manifest.json");
        return DeserializeManifest(manifestAsset.ReadAsString(), manifestAsset.DisplayName);
    }

    private static Manifest DeserializeManifest(string json, string displayName)
    {
        Manifest? manifest = JsonSerializer.Deserialize<Manifest>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (manifest is null)
        {
            throw new InvalidOperationException($"Failed to parse manifest: {displayName}");
        }

        manifest.Migrations ??= [];
        return manifest;
    }

    private static AssetContent ResolveBootstrapAsset(AppOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.BootstrapScriptPathOverride))
        {
            string path = Path.GetFullPath(options.BootstrapScriptPathOverride);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Bootstrap SQL not found.", path);
            }

            return AssetContent.FromFile(path);
        }

        return AssetStore.GetRequired("000_bootstrap_gl_migration_history.sql");
    }

    private static AssetContent ResolveAsset(AppOptions options, string relativePath)
    {
        if (!string.IsNullOrWhiteSpace(options.RootDirectoryOverride))
        {
            string candidate = Path.GetFullPath(Path.Combine(options.RootDirectoryOverride, relativePath));
            if (!File.Exists(candidate))
            {
                throw new FileNotFoundException("Migration file not found.", candidate);
            }

            return AssetContent.FromFile(candidate);
        }

        return AssetStore.GetRequired(relativePath);
    }

    private static string WriteAssetToTempFile(AssetContent asset)
    {
        string extension = Path.GetExtension(asset.DisplayName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".sql";
        }

        string tempFilePath = Path.Combine(Path.GetTempPath(), $"glmigrator_asset_{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(tempFilePath, asset.Content);
        return tempFilePath;
    }

    private static string ComputeSha256(byte[] content)
    {
        byte[] hash = SHA256.HashData(content);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string EscapeSqlLiteral(string text) => text.Replace("'", "''");

    private static string SanitizeFileName(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        StringBuilder sb = new(value.Length);
        foreach (char c in value)
        {
            sb.Append(invalid.Contains(c) ? '_' : c);
        }

        return sb.ToString();
    }

    private static string TryGetStreamResult(Task<string> task)
    {
        try
        {
            return task.GetAwaiter().GetResult();
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // best effort cleanup
        }
    }

    private static void EnsureSqlPlusAvailable()
    {
        try
        {
            ProcessStartInfo psi = new("sqlplus")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("-v");

            using Process process = Process.Start(psi)
                ?? throw new InvalidOperationException("Unable to start sqlplus.");

            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("sqlplus returned non-zero exit code. Ensure Oracle Client is installed.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("sqlplus command not found or unavailable in PATH.", ex);
        }
    }

    private sealed record AppliedMigration(string Checksum, string AppliedAt);
    private sealed record SqlExecutionResult(string Output, string LogPath);
}

internal static class ConnectionResolver
{
    private const string ActiveServerKeyEnvironmentVariable = "ACCOUNTING_DB_ACTIVE_SERVER_KEY";
    private const string HostEnvironmentVariable = "ACCOUNTING_DB_HOST";
    private const string PortEnvironmentVariable = "ACCOUNTING_DB_PORT";
    private const string ServiceNameEnvironmentVariable = "ACCOUNTING_DB_SERVICE_NAME";
    private const string UserIdEnvironmentVariable = "ACCOUNTING_DB_USER_ID";
    private const string PasswordEnvironmentVariable = "ACCOUNTING_DB_PASSWORD";

    public static string Resolve(AppOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Connection))
        {
            return options.Connection.Trim();
        }

        string configPath = ResolveConfigPath(options.ConfigPathOverride);
        AppConfig config = LoadConfig(configPath);

        string activeServerKey = ResolveActiveServerKey(config, options.ServerKeyOverride);
        OracleServerConfig? serverConfig = ResolveServerConfig(config, activeServerKey);
        if (serverConfig is null && !config.AllowEnvironmentFallback)
        {
            throw new InvalidOperationException($"Server key '{activeServerKey}' was not found in config '{configPath}'.");
        }

        string dbHost = serverConfig?.Host?.Trim() ?? string.Empty;
        string dbServiceName = serverConfig?.ServiceName?.Trim() ?? string.Empty;
        string dbUserId = serverConfig?.UserId?.Trim() ?? string.Empty;
        string dbPassword = serverConfig?.Password ?? string.Empty;
        int dbPort = serverConfig?.Port ?? 0;

        if (config.AllowEnvironmentFallback)
        {
            dbHost = GetConfigValueOrFallback(dbHost, HostEnvironmentVariable);
            dbServiceName = GetConfigValueOrFallback(dbServiceName, ServiceNameEnvironmentVariable);
            dbUserId = GetConfigValueOrFallback(dbUserId, UserIdEnvironmentVariable);
            dbPassword = GetConfigValueOrFallback(dbPassword, PasswordEnvironmentVariable);

            if (dbPort <= 0)
            {
                string rawPort = GetEnvironmentValue(PortEnvironmentVariable);
                if (!string.IsNullOrWhiteSpace(rawPort))
                {
                    if (!int.TryParse(rawPort, out dbPort) || dbPort <= 0)
                    {
                        throw new InvalidOperationException($"Environment variable {PortEnvironmentVariable} is invalid: '{rawPort}'");
                    }
                }
            }
        }

        List<string> missingFields = [];
        if (string.IsNullOrWhiteSpace(dbHost)) { missingFields.Add("Host"); }
        if (dbPort <= 0) { missingFields.Add("Port"); }
        if (string.IsNullOrWhiteSpace(dbServiceName)) { missingFields.Add("ServiceName"); }
        if (string.IsNullOrWhiteSpace(dbUserId)) { missingFields.Add("UserId"); }
        if (string.IsNullOrWhiteSpace(dbPassword)) { missingFields.Add("Password"); }

        if (missingFields.Count > 0)
        {
            throw new InvalidOperationException(
                $"Server configuration '{activeServerKey}' is incomplete. Missing: {string.Join(", ", missingFields)}");
        }

        return $"{dbUserId}/{dbPassword}@//{dbHost}:{dbPort}/{dbServiceName}";
    }

    private static string ResolveConfigPath(string configPathOverride)
    {
        if (!string.IsNullOrWhiteSpace(configPathOverride))
        {
            string explicitPath = Path.GetFullPath(configPathOverride);
            if (!File.Exists(explicitPath))
            {
                throw new FileNotFoundException("Config file not found.", explicitPath);
            }

            return explicitPath;
        }

        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "config.json"),
            Path.Combine(AppContext.BaseDirectory, "Utilities", "config.json"),
            Path.Combine(AppContext.BaseDirectory, "Accounting", "Utilities", "config.json"),
            Path.Combine(Environment.CurrentDirectory, "config.json"),
            Path.Combine(Environment.CurrentDirectory, "Utilities", "config.json"),
            Path.Combine(Environment.CurrentDirectory, "Accounting", "Utilities", "config.json")
        ];

        foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        throw new FileNotFoundException(
            "No config.json file was found. Use --config <path> or --connection <USER/PASS@//HOST:PORT/SERVICE>.");
    }

    private static AppConfig LoadConfig(string configPath)
    {
        string json = string.Join(
            Environment.NewLine,
            File.ReadLines(configPath).Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));

        AppConfig? config = JsonSerializer.Deserialize<AppConfig>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return config ?? throw new InvalidOperationException($"Unable to parse config file '{configPath}'.");
    }

    private static string ResolveActiveServerKey(AppConfig config, string serverKeyOverride)
    {
        if (!string.IsNullOrWhiteSpace(serverKeyOverride))
        {
            return serverKeyOverride.Trim();
        }

        if (config.AllowEnvironmentFallback)
        {
            string environmentServerKey = GetEnvironmentValue(ActiveServerKeyEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(environmentServerKey))
            {
                return environmentServerKey;
            }
        }

        if (!string.IsNullOrWhiteSpace(config.ActiveServerKey))
        {
            return config.ActiveServerKey.Trim();
        }

        throw new InvalidOperationException("ActiveServerKey is missing in config and no --server-key override was provided.");
    }

    private static OracleServerConfig? ResolveServerConfig(AppConfig config, string activeServerKey)
    {
        if (config.Servers is null)
        {
            return null;
        }

        foreach ((string key, OracleServerConfig value) in config.Servers)
        {
            if (string.Equals(key, activeServerKey, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return null;
    }

    private static string GetConfigValueOrFallback(string currentValue, string environmentVariableName)
    {
        if (!string.IsNullOrWhiteSpace(currentValue))
        {
            return currentValue.Trim();
        }

        return GetEnvironmentValue(environmentVariableName);
    }

    private static string GetEnvironmentValue(string environmentVariableName)
    {
        return Environment.GetEnvironmentVariable(environmentVariableName)?.Trim() ?? string.Empty;
    }
}

internal enum MigrationMode
{
    Up,
    Down,
    Status,
    Verify,
    CheckConn,
    ReconcileHistory,
    RebaselineChecksum,
    ShowCompileErrors,
    ReconcileCoa,
    ShowSource,
    RepairMissingCoa
}

internal sealed class AppOptions
{
    public string Connection { get; set; } = string.Empty;
    public MigrationMode Mode { get; private init; } = MigrationMode.Up;
    public int Steps { get; private init; } = 1;
    public string RootDirectoryOverride { get; private init; } = string.Empty;
    public string ManifestPathOverride { get; private init; } = string.Empty;
    public string BootstrapScriptPathOverride { get; private init; } = string.Empty;
    public string LogDirectory { get; private init; } = string.Empty;
    public string ConfigPathOverride { get; private init; } = string.Empty;
    public string ServerKeyOverride { get; private init; } = string.Empty;
    public string MigrationId { get; private init; } = string.Empty;
    public bool Reexecute { get; private init; }
    public string ObjectName { get; private init; } = string.Empty;
    public string IdData { get; private init; } = string.Empty;
    public string Periode { get; private init; } = string.Empty;
    public int? Tahun { get; private init; }
    public int? Bulan { get; private init; }
    public string BulanList { get; private init; } = string.Empty;
    public string UserId { get; private init; } = string.Empty;
    public bool Apply { get; private init; }
    public int ScriptTimeoutMs { get; private init; } = 600000;
    public int ConnTimeoutMs { get; private init; } = 30000;
    public bool ShowHelp { get; private init; }

    public static string HelpText => """
GLMigrator.exe [--connection <USER/PASS@//HOST:PORT/SERVICE>] [--config <path>] [--server-key KEY] [--mode up|down|status|verify|checkconn|reconcilehistory|rebaselinechecksum|showcompileerrors|reconcilecoa] [--steps N] [--migration-id ID]

Options:
  --connection   Oracle SQL*Plus connection string. If omitted, the executable reads config.json.
  --config       Optional config.json path. If omitted, the executable probes common config.json locations.
  --server-key   Optional server key override for config.json resolution
  --mode         up (default), down, status, verify, checkconn, reconcilehistory, rebaselinechecksum,
                  showcompileerrors, reconcilecoa
  --migration-id Required for --mode rebaselinechecksum: the manifest id whose recorded checksum should be
                  updated to match the current script content (no SQL is re-executed unless --reexecute is
                  also given; use only when the script content change is a confirmed no-op on this server).
  --reexecute    With --mode rebaselinechecksum: re-run the migration's current script (and check script)
                  against this server before updating the checksum. Use when the script gained real new
                  logic since it was applied here, not just a refactor. Scripts must be idempotent.
  --object-name  With --mode showcompileerrors: comma-separated PL/SQL object name(s) to inspect via
                  USER_ERRORS. Defaults to ACCT_REPORT_ENGINE_V1,ACCT_LAPORAN_V2.
  --iddata       Required for --mode reconcilecoa: IDDATA company code.
  --periode      Required for --mode reconcilecoa: periode string as stored on ACCT_JURNAL_DTL (e.g. 01/2026).
  --tahun        Required for --mode reconcilecoa: fiscal year (e.g. 2026).
  --bulan        Required for --mode reconcilecoa: month number 1-12.
  --object-name  With --mode showsource: PACKAGE_NAME or PACKAGE_NAME.PROCEDURE_NAME to print USER_SOURCE
                  for (package body). Without a procedure suffix, prints the whole package body.
  --bulan-list   Required for --mode repairmissingcoa: comma-separated month numbers to recalc after
                  inserting missing ACCT_COA rows (e.g. 1,2,3,4).
  --userid       Required for --mode repairmissingcoa: USERID to pass to ACCT_RECALLCULATIONS_V2.ReCalcPeriod.
  --apply        With --mode repairmissingcoa: actually INSERT/COMMIT and run the recalc. Without it, the
                  mode only prints what would be inserted (dry run, no writes).
  --steps        Number of steps for down mode (default: 1)
  --timeout      Max milliseconds per SQL script before sqlplus is killed (default: 600000)
  --conn-timeout Max milliseconds for the startup connection check (default: 30000)
  --root         Optional external root folder to override embedded migrations
  --manifest     Optional custom manifest path to override embedded manifest
  --bootstrap    Optional custom bootstrap sql path to override embedded bootstrap
  --log-dir      Optional custom log directory
  --help         Show usage

Default behavior:
  The executable uses embedded manifest/bootstrap/migration SQL resources.
  If --connection is omitted, it resolves the Oracle connection from config.json.
""";

    public static AppOptions Parse(string[] args)
    {
        Dictionary<string, string?> map = new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            string key = arg[2..];
            string? value = null;
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                value = args[++i];
            }

            map[key] = value;
        }

        bool help = map.ContainsKey("help") || map.ContainsKey("h");
        string root = map.TryGetValue("root", out string? rootArg) && !string.IsNullOrWhiteSpace(rootArg)
            ? Path.GetFullPath(rootArg)
            : string.Empty;

        string manifest = map.TryGetValue("manifest", out string? manifestArg) && !string.IsNullOrWhiteSpace(manifestArg)
            ? Path.GetFullPath(manifestArg)
            : string.Empty;

        string bootstrap = map.TryGetValue("bootstrap", out string? bootstrapArg) && !string.IsNullOrWhiteSpace(bootstrapArg)
            ? Path.GetFullPath(bootstrapArg)
            : string.Empty;

        string logDir = map.TryGetValue("log-dir", out string? logArg) && !string.IsNullOrWhiteSpace(logArg)
            ? Path.GetFullPath(logArg)
            : Path.Combine(AppContext.BaseDirectory, "logs");

        string config = map.TryGetValue("config", out string? configArg) && !string.IsNullOrWhiteSpace(configArg)
            ? Path.GetFullPath(configArg)
            : string.Empty;

        string serverKey = map.TryGetValue("server-key", out string? serverKeyArg) && !string.IsNullOrWhiteSpace(serverKeyArg)
            ? serverKeyArg.Trim()
            : string.Empty;

        MigrationMode mode = MigrationMode.Up;
        if (map.TryGetValue("mode", out string? modeArg) && !string.IsNullOrWhiteSpace(modeArg))
        {
            if (!Enum.TryParse(modeArg, true, out mode))
            {
                throw new ArgumentException($"Invalid mode '{modeArg}'. Valid: up, down, status, verify, checkconn, reconcilehistory, rebaselinechecksum, showcompileerrors, reconcilecoa, showsource, repairmissingcoa.");
            }
        }

        string migrationId = map.TryGetValue("migration-id", out string? migrationIdArg) && !string.IsNullOrWhiteSpace(migrationIdArg)
            ? migrationIdArg.Trim()
            : string.Empty;

        bool reexecute = map.ContainsKey("reexecute");

        string objectName = map.TryGetValue("object-name", out string? objectNameArg) && !string.IsNullOrWhiteSpace(objectNameArg)
            ? objectNameArg.Trim()
            : string.Empty;

        string idData = map.TryGetValue("iddata", out string? idDataArg) && !string.IsNullOrWhiteSpace(idDataArg)
            ? idDataArg.Trim()
            : string.Empty;

        string periode = map.TryGetValue("periode", out string? periodeArg) && !string.IsNullOrWhiteSpace(periodeArg)
            ? periodeArg.Trim()
            : string.Empty;

        int? tahun = null;
        if (map.TryGetValue("tahun", out string? tahunArg) && !string.IsNullOrWhiteSpace(tahunArg))
        {
            if (!int.TryParse(tahunArg, out int tahunValue) || tahunValue < 1)
            {
                throw new ArgumentException("Invalid --tahun value. Must be a positive integer year.");
            }

            tahun = tahunValue;
        }

        int? bulan = null;
        if (map.TryGetValue("bulan", out string? bulanArg) && !string.IsNullOrWhiteSpace(bulanArg))
        {
            if (!int.TryParse(bulanArg, out int bulanValue) || bulanValue is < 1 or > 12)
            {
                throw new ArgumentException("Invalid --bulan value. Must be an integer between 1 and 12.");
            }

            bulan = bulanValue;
        }

        string bulanList = map.TryGetValue("bulan-list", out string? bulanListArg) && !string.IsNullOrWhiteSpace(bulanListArg)
            ? bulanListArg.Trim()
            : string.Empty;

        string userId = map.TryGetValue("userid", out string? userIdArg) && !string.IsNullOrWhiteSpace(userIdArg)
            ? userIdArg.Trim()
            : string.Empty;

        bool apply = map.ContainsKey("apply");

        int steps = 1;
        if (map.TryGetValue("steps", out string? stepsArg) && !string.IsNullOrWhiteSpace(stepsArg))
        {
            if (!int.TryParse(stepsArg, out steps) || steps < 1)
            {
                throw new ArgumentException("Invalid --steps value. Must be integer >= 1.");
            }
        }

        int scriptTimeoutMs = 600000;
        if (map.TryGetValue("timeout", out string? timeoutArg) && !string.IsNullOrWhiteSpace(timeoutArg))
        {
            if (!int.TryParse(timeoutArg, out scriptTimeoutMs) || scriptTimeoutMs < 1000)
            {
                throw new ArgumentException("Invalid --timeout value. Must be integer >= 1000 (milliseconds).");
            }
        }

        int connTimeoutMs = 30000;
        if (map.TryGetValue("conn-timeout", out string? connTimeoutArg) && !string.IsNullOrWhiteSpace(connTimeoutArg))
        {
            if (!int.TryParse(connTimeoutArg, out connTimeoutMs) || connTimeoutMs < 1000)
            {
                throw new ArgumentException("Invalid --conn-timeout value. Must be integer >= 1000 (milliseconds).");
            }
        }

        string connection = map.TryGetValue("connection", out string? connArg) && !string.IsNullOrWhiteSpace(connArg)
            ? connArg.Trim()
            : string.Empty;

        return new AppOptions
        {
            Connection = connection,
            Mode = mode,
            Steps = steps,
            RootDirectoryOverride = root,
            ManifestPathOverride = manifest,
            BootstrapScriptPathOverride = bootstrap,
            LogDirectory = logDir,
            ConfigPathOverride = config,
            ServerKeyOverride = serverKey,
            MigrationId = migrationId,
            Reexecute = reexecute,
            ObjectName = objectName,
            IdData = idData,
            Periode = periode,
            Tahun = tahun,
            Bulan = bulan,
            BulanList = bulanList,
            UserId = userId,
            Apply = apply,
            ScriptTimeoutMs = scriptTimeoutMs,
            ConnTimeoutMs = connTimeoutMs,
            ShowHelp = help
        };
    }
}

internal sealed class AppConfig
{
    public string ActiveServerKey { get; set; } = string.Empty;
    public bool AllowEnvironmentFallback { get; set; }
    public Dictionary<string, OracleServerConfig> Servers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class OracleServerConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1521;
    public string ServiceName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

internal sealed class Manifest
{
    public string PackageName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<MigrationItem> Migrations { get; set; } = [];
}

internal sealed class MigrationItem
{
    public string Id { get; set; } = string.Empty;
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Script { get; set; } = string.Empty;
    public string? RollbackScript { get; set; }
    public string? CheckScript { get; set; }
}

internal sealed class EmbeddedAssetStore
{
    private const string ResourcePrefix = "EmbeddedAssets/";
    private readonly Dictionary<string, AssetContent> assets;

    private EmbeddedAssetStore(Dictionary<string, AssetContent> assets)
    {
        this.assets = assets;
    }

    public static EmbeddedAssetStore Create()
    {
        Assembly assembly = typeof(Program).Assembly;
        Dictionary<string, AssetContent> assets = new(StringComparer.OrdinalIgnoreCase);

        foreach (string resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            {
                continue;
            }

            using Stream stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded resource not found: {resourceName}");
            using MemoryStream ms = new();
            stream.CopyTo(ms);

            string logicalPath = resourceName[ResourcePrefix.Length..];
            logicalPath = logicalPath.Replace('\\', '/');
            assets[logicalPath] = new AssetContent(logicalPath, ms.ToArray());
        }

        return new EmbeddedAssetStore(assets);
    }

    public AssetContent GetRequired(string logicalPath)
    {
        string normalized = logicalPath.Replace('\\', '/');
        if (assets.TryGetValue(normalized, out AssetContent? asset))
        {
            return asset;
        }

        throw new FileNotFoundException("Embedded asset not found.", normalized);
    }
}

internal sealed class AssetContent
{
    public AssetContent(string displayName, byte[] content)
    {
        DisplayName = displayName;
        Content = content;
    }

    public string DisplayName { get; }
    public byte[] Content { get; }

    public static AssetContent FromFile(string path)
    {
        return new AssetContent(path, File.ReadAllBytes(path));
    }

    public static AssetContent FromText(string displayName, string text)
    {
        return new AssetContent(displayName, Encoding.ASCII.GetBytes(text));
    }

    public string ReadAsString()
    {
        return Encoding.UTF8.GetString(Content);
    }
}
