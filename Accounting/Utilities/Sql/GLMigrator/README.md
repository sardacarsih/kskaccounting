# GL Migrator Package

`GLMigrator.exe` is the only supported migration runner for Oracle database changes in this repository.

## Contents

- `GLMigrator.exe`: migration runner (`Up`, `Down`, `Status`, `Verify`)
- `migrations.manifest.json`: ordered migration list and script mapping
- `migrations/*.sql`: embedded SQL assets in the executable source
- `Build-GLMigratorExePackage.ps1`: builds the executable package

## Prerequisites

- PowerShell 5+ or PowerShell 7+ to run build/package scripts
- `sqlplus` available in `PATH`
- Either:
  - `Accounting/Utilities/config.json` in the application format, or
  - Oracle credential in SQL*Plus format: `USER/PASSWORD@//HOST:PORT/SERVICE`

## Usage

From the repository root:

```powershell
# 1) Check migration status using config.json active server
.\Accounting\Utilities\Sql\GLMigrator\dist-exe\<package>\GLMigrator.exe --mode status

# 2) Check status using a specific server key from config.json
.\Accounting\Utilities\Sql\GLMigrator\dist-exe\<package>\GLMigrator.exe --server-key MSLKEBUN --mode status

# 3) Apply all pending migrations using config.json
.\Accounting\Utilities\Sql\GLMigrator\dist-exe\<package>\GLMigrator.exe --mode up

# 4) Verify using an explicit SQL*Plus connection string
.\Accounting\Utilities\Sql\GLMigrator\dist-exe\<package>\GLMigrator.exe --connection "USER/PASSWORD@//HOST:1521/SERVICE_NAME" --mode verify

# 5) Rollback latest migration
.\Accounting\Utilities\Sql\GLMigrator\dist-exe\<package>\GLMigrator.exe --connection "USER/PASSWORD@//HOST:1521/SERVICE_NAME" --mode down --steps 1
```

Connection resolution:

- `--connection` bypasses config resolution entirely
- `--config <path>` points to a specific `config.json`
- `--server-key <key>` overrides `ActiveServerKey`
- If `--connection` is omitted, `GLMigrator.exe` probes common `config.json` locations near the executable and current working directory

When `AllowEnvironmentFallback` is `true`, the executable also honors:

- `ACCOUNTING_DB_ACTIVE_SERVER_KEY`
- `ACCOUNTING_DB_HOST`
- `ACCOUNTING_DB_PORT`
- `ACCOUNTING_DB_SERVICE_NAME`
- `ACCOUNTING_DB_USER_ID`
- `ACCOUNTING_DB_PASSWORD`

## Build EXE Package

```powershell
powershell -ExecutionPolicy Bypass -File .\Accounting\Utilities\Sql\GLMigrator\Build-GLMigratorExePackage.ps1
```

Output:

- `dist-exe/<PackageName>Exe_<Version>_<Timestamp>/`
- `dist-exe/<PackageName>Exe_<Version>_<Timestamp>.zip`
- Entry point executable: `GLMigrator.exe`

Default build is framework-dependent (`GLMigrator.exe` + `.dll/.runtimeconfig/.deps` files).  
For self-contained single-file build:

```powershell
powershell -ExecutionPolicy Bypass -File .\Accounting\Utilities\Sql\GLMigrator\Build-GLMigratorExePackage.ps1 -SelfContained -Runtime win-x64
```

## Notes

- Migration state is tracked in `GL_MIGRATION_HISTORY`
- The executable uses embedded manifest/bootstrap/migration SQL resources by default
- SQL execution logs are written to the `logs` folder beside the executable unless `--log-dir` is provided
- RBAC migrations are additive through `20260413_004_standard_role_catalog_alignment`. The destructive legacy RBAC cleanup migration is intentionally excluded from the active manifest so legacy roles (`MANAGER`, `KABAG`, `ASISTEN`, `TAMU`, `AUDIT`) and the legacy `ACCOUNTING/ALL` permission remain available during coexistence.
- If an environment already applied `20260413_005_remove_legacy_rbac_data`, restore legacy RBAC data with the rollback script or a controlled data restore before deploying this package. Do not re-add the cleanup entry to the manifest unless every legacy user has been remapped.
