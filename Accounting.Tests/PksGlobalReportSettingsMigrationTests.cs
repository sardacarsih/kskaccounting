using System.Text.Json;

namespace Accounting.Tests;

public sealed class PksGlobalReportSettingsMigrationTests
{
    private const string MigrationId = "20260729_001_pks_global_report_settings";

    [Fact]
    public void Manifest_RegistersPksGlobalReportSettingsAsLatestMigration()
    {
        string migratorDirectory = FindMigratorDirectory();
        using JsonDocument manifest = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(migratorDirectory, "migrations.manifest.json")));

        JsonElement root = manifest.RootElement;
        JsonElement[] migrations = root.GetProperty("migrations").EnumerateArray().ToArray();
        JsonElement migration = migrations.Single(entry => entry.GetProperty("id").GetString() == MigrationId);

        Assert.Equal("2026.10.08.2", root.GetProperty("version").GetString());
        Assert.Equal(68, migration.GetProperty("order").GetInt32());
        Assert.Equal(69, migrations.Max(entry => entry.GetProperty("order").GetInt32()));

        AssertMigrationFileExists(migratorDirectory, migration, "script");
        AssertMigrationFileExists(migratorDirectory, migration, "rollbackScript");
        AssertMigrationFileExists(migratorDirectory, migration, "checkScript");
    }

    [Fact]
    public void RepairMigration_SeedsAndCanRemoveAllThirteenGlobalLabaRugiMappings()
    {
        string migratorDirectory = FindMigratorDirectory();
        string migrationSql = ReadMigration(migratorDirectory, "V20260729_002__pks_global_labarugi_repair.sql");
        string rollbackSql = ReadMigration(migratorDirectory, "R20260729_002__pks_global_labarugi_repair.sql");
        string checkSql = ReadMigration(migratorDirectory, "C20260729_002__pks_global_labarugi_repair_check.sql");

        Assert.Contains("MERGE INTO ACCT_REPORT_SECTION_ACCOUNT", migrationSql);
        Assert.Contains("CAST(NULL AS VARCHAR2(20)) IDDATA", migrationSql);
        Assert.Contains("IF v_count <> 13", migrationSql);
        Assert.Contains("IF v_count <> 13", checkSql);
        Assert.Contains("DELETE FROM ACCT_REPORT_SECTION_ACCOUNT", rollbackSql);

        string[] expectedRoots =
        [
            "GRP:11", "GRP:12", "89.11007.000", "89.12007.000", "89.13000.000",
            "89.21007.000", "89.22007.000", "89.23007.000", "85.01007.000",
            "GRP:16", "GRP:17", "GRP:15", "GRP:18"
        ];

        foreach (string root in expectedRoots)
        {
            Assert.Contains(root, migrationSql);
            Assert.Contains(root, rollbackSql);
        }
    }

    [Fact]
    public void Migration_SeedsKskpksSettingsAsGlobalPksMappings()
    {
        string migratorDirectory = FindMigratorDirectory();
        string migrationSql = ReadMigration(migratorDirectory, "V20260729_001__pks_global_report_settings.sql");
        string checkSql = ReadMigration(migratorDirectory, "C20260729_001__pks_global_report_settings_check.sql");

        Assert.Contains("'PKS_P1' SECTION_CODE, 4 DISPLAY_LVL", migrationSql);
        Assert.Contains("'PKS_HPP', 3", migrationSql);
        Assert.Contains("'PKS_B1', 5", migrationSql);
        Assert.Contains("'PKS_B5', 4", migrationSql);
        Assert.Contains("'PKS_P2', 4", migrationSql);
        Assert.Contains("'PKS_B6', 4", migrationSql);
        Assert.Contains("CAST(NULL AS VARCHAR2(20)) IDDATA", migrationSql);
        Assert.Contains("account.IDDATA IS NULL", checkSql);

        string[] expectedNeracaRoots =
        [
            "10.00000.000", "11.00000.000", "12.00000.000", "13.00000.000", "14.00000.000",
            "22.00000.000", "23.00000.000", "24.00000.000", "25.00000.000", "26.00000.000",
            "27.00000.000", "29.00000.000", "30.00000.000", "31.00000.000", "32.00000.000",
            "33.00000.000", "39.00000.000", "40.00000.000", "58.00000.000", "59.00000.000"
        ];

        foreach (string root in expectedNeracaRoots)
        {
            Assert.Contains(root, migrationSql);
        }

        Assert.Contains("IF v_count <> 20", checkSql);
        Assert.Contains("IF v_count <> 13", checkSql);
    }

    [Fact]
    public void Migration_BacksUpScopedPksMappingsBeforeDeletingThem()
    {
        string migratorDirectory = FindMigratorDirectory();
        string migrationSql = ReadMigration(migratorDirectory, "V20260729_001__pks_global_report_settings.sql");
        string checkSql = ReadMigration(migratorDirectory, "C20260729_001__pks_global_report_settings_check.sql");

        int backupPosition = migrationSql.IndexOf(
            "MERGE INTO ACCT_RPT_PKS_SCOPE_BAK",
            StringComparison.Ordinal);
        int deletePosition = migrationSql.IndexOf(
            "DELETE FROM ACCT_REPORT_SECTION_ACCOUNT",
            StringComparison.Ordinal);

        Assert.True(backupPosition >= 0, "Scoped PKS backup statement was not found.");
        Assert.True(deletePosition > backupPosition, "Scoped PKS mappings must be backed up before deletion.");
        Assert.Contains("ACCT_RPT_PKS_SCOPE_BAK", migrationSql);
        Assert.Contains("UPPER(TRIM(JENIS_AKUNTING)) = 'PKS'", migrationSql);
        Assert.Contains("TRIM(IDDATA) IS NOT NULL", migrationSql);
        Assert.Contains("v_live_count <> 74", migrationSql);
        Assert.Contains("v_labarugi_count <> 36", migrationSql);
        Assert.Contains("v_neraca_count <> 38", migrationSql);
        Assert.Contains("v_active_count <> 34", migrationSql);
        Assert.Contains("v_inactive_count <> 40", migrationSql);
        Assert.Contains("Scoped PKS report mappings still exist", checkSql);
        Assert.Contains("v_count <> 74", checkSql);
    }

    [Fact]
    public void Rollback_RestoresEveryOriginalScopedPksColumn()
    {
        string migratorDirectory = FindMigratorDirectory();
        string rollbackSql = ReadMigration(migratorDirectory, "R20260729_001__pks_global_report_settings.sql");

        Assert.Contains("MERGE INTO ACCT_REPORT_SECTION_ACCOUNT", rollbackSql);
        Assert.Contains("FROM ACCT_RPT_PKS_SCOPE_BAK", rollbackSql);
        Assert.Contains("SECTION_ACCOUNT_ID", rollbackSql);
        Assert.Contains("SECTION_ID", rollbackSql);
        Assert.Contains("JENIS_AKUNTING", rollbackSql);
        Assert.Contains("IDDATA", rollbackSql);
        Assert.Contains("KODEACC_ROOT", rollbackSql);
        Assert.Contains("DISPLAY_ORDER", rollbackSql);
        Assert.Contains("INCLUDE_CHILDREN", rollbackSql);
        Assert.Contains("IS_ACTIVE", rollbackSql);
        Assert.Contains("CREATED_AT", rollbackSql);
        Assert.Contains("MATCH_MODE", rollbackSql);
        Assert.Contains("GRP_CODE", rollbackSql);
        Assert.Contains("v_restored_count <> v_backup_count", rollbackSql);
    }

    [Fact]
    public void Migration_SelectsPksNeracaMappingsWithoutGeneralMappingDuplicates()
    {
        string migratorDirectory = FindMigratorDirectory();
        string migrationSql = ReadMigration(migratorDirectory, "V20260729_001__pks_global_report_settings.sql");

        Assert.Contains("FROM MASTER_PT_DTL pks_detail", migrationSql);
        Assert.Contains("TRIM(pks_detail.JENIS_AKUNTANSI) = 'PKS'", migrationSql);
        Assert.Contains("account.JENIS_AKUNTING = 'PKS'", migrationSql);
        Assert.Contains("SELECT MAX(TRIM(detail.JENIS_AKUNTANSI))", migrationSql);
    }

    private static string ReadMigration(string migratorDirectory, string fileName)
    {
        return File.ReadAllText(Path.Combine(migratorDirectory, "migrations", fileName));
    }

    private static void AssertMigrationFileExists(
        string migratorDirectory,
        JsonElement migration,
        string propertyName)
    {
        string relativePath = migration.GetProperty(propertyName).GetString()!;
        string fullPath = Path.Combine(migratorDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(fullPath), $"Migration asset was not found: {fullPath}");
    }

    private static string FindMigratorDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory != null)
        {
            string candidate = Path.Combine(directory.FullName, "Accounting", "Utilities", "Sql", "GLMigrator");
            if (File.Exists(Path.Combine(candidate, "migrations.manifest.json")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Accounting/Utilities/Sql/GLMigrator.");
    }
}
