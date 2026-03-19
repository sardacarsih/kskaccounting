# GL Migrator Package

`GL Migrator` is a lightweight SQL*Plus-based migration runner for Oracle database changes in this repository.

## Contents

- `Run-GLMigrator.ps1`: migration runner (`Up`, `Down`, `Status`, `Verify`).
- `000_bootstrap_gl_migration_history.sql`: creates migration history table if needed.
- `migrations.manifest.json`: ordered migration list and script mapping.
- `migrations/*.sql`: migration, rollback, and verification SQL scripts.
- `Build-GLMigratorPackage.ps1`: builds distribution folder and `.zip` package.

## Prerequisites

- PowerShell 5+ or PowerShell 7+.
- `sqlplus` available in `PATH`.
- Oracle credential in SQL*Plus format:
  - `USER/PASSWORD@//HOST:PORT/SERVICE`

## Usage

From `Accounting/Utilities/Sql/GLMigrator`:

```powershell
# 1) Check migration status
powershell -ExecutionPolicy Bypass -File .\Run-GLMigrator.ps1 `
  -Connection "MSL/MSLboss@//localhost:1521/MSLKEBUN" `
  -Mode Status

# 2) Apply all pending migrations
powershell -ExecutionPolicy Bypass -File .\Run-GLMigrator.ps1 `
  -Connection "MSL/MSLboss@//localhost:1521/MSLKEBUN" `
  -Mode Up

# 3) Run verification scripts
powershell -ExecutionPolicy Bypass -File .\Run-GLMigrator.ps1 `
  -Connection "MSL/MSLboss@//localhost:1521/MSLKEBUN" `
  -Mode Verify

# 4) Rollback latest migration
powershell -ExecutionPolicy Bypass -File .\Run-GLMigrator.ps1 `
  -Connection "MSL/MSLboss@//localhost:1521/MSLKEBUN" `
  -Mode Down -Steps 1
```

## Build Package

```powershell
powershell -ExecutionPolicy Bypass -File .\Build-GLMigratorPackage.ps1
```

Output:

- `dist/<PackageName>_<Version>_<Timestamp>/`
- `dist/<PackageName>_<Version>_<Timestamp>.zip`

## Build EXE Package

```powershell
powershell -ExecutionPolicy Bypass -File .\Build-GLMigratorExePackage.ps1
```

Output:

- `dist-exe/<PackageName>Exe_<Version>_<Timestamp>/`
- `dist-exe/<PackageName>Exe_<Version>_<Timestamp>.zip`
- Entry point executable: `GLMigrator.exe`

Default build is framework-dependent (`GLMigrator.exe` + `.dll/.runtimeconfig/.deps` files).  
For self-contained single-file build (requires runtime pack availability):

```powershell
powershell -ExecutionPolicy Bypass -File .\Build-GLMigratorExePackage.ps1 -SelfContained -Runtime win-x64
```

## Notes

- Migration state is tracked in `GL_MIGRATION_HISTORY`.
- Runner validates SHA-256 checksum for already-applied migrations.
- SQL execution logs are written to `GLMigrator/logs`.
