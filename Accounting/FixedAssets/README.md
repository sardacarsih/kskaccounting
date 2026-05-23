# Fixed Asset Module (Core Implementation)

## Scope implemented
- Clean layer structure in current codebase:
  - `FixedAssets/Domain`
  - `FixedAssets/Application`
  - `FixedAssets/Infrastructure`
  - service entrypoint in `3.Services`
- Oracle schema migration for:
  - master data (`ACCT_FA_ASSET`, category/group, account mapping)
  - lifecycle transaction + approval (`ACCT_FA_TRX_HDR`, `ACCT_FA_APPROVAL_DTL`)
  - depreciation run + history (`ACCT_FA_DEPR_RUN`, `ACCT_FA_DEPR_RUN_DTL`, `ACCT_FA_DEPR_HISTORY`)
  - CIP tables, attachment metadata, audit log, document sequence
- Monthly depreciation flow:
  - preview and draft run
  - posting to `ACCT_JURNAL_TMP`
  - import to GL via `ACCT_JURNAL_IMPORT_V2.ImportJurnalParsial`
  - update depreciation history
  - idempotent posting by `RUN_ID` with deterministic `NOJURNAL` format `FA/YYYYMM/{RUN_ID:000000}`
  - accept import success return code `1` or `99`
  - fallback insert to `ACCT_JURNAL_TMP` without `SUMBER` column for legacy schema
- Lifecycle service baseline:
  - transaction creation
  - submit/approve/reject workflow hooks
  - approval transition guard (`SUBMITTED -> APPROVED/REJECTED` only)
  - apply transaction impact to asset when approved:
    - `IMPROVEMENT`: increase `ACQUISITION_COST` and `IMPROVEMENT_TOTAL`
    - `REVALUATION`: update `ACQUISITION_COST` and accumulate `REVALUATION_DELTA_TOTAL`
    - `FULL_DISPOSAL`/`SALE`/`WRITE_OFF`: update asset `STATUS` so depreciation will stop
  - auto journal posting to GL on approval for supported lifecycle types (`IMPROVEMENT`, `REVALUATION`, `FULL_DISPOSAL`, `SALE`, `WRITE_OFF`) and update trx `NOJURNAL/JURNALID`
  - audit log for key value changes at transaction and asset level

## Main service entrypoints
- `Accounting.BusinessLayer.FixedAssetServices`
  - `PreviewDepreciationAsync(...)`
  - `PostDepreciationAsync(...)`
- `Accounting.BusinessLayer.FixedAssetLifecycleServices`
  - `CreateTransactionAsync(...)`
  - `ApproveAsync(...)`
  - `RejectAsync(...)`
- `Accounting.BusinessLayer.FixedAssetQueryServices`
  - `GetAssetMaster(...)`
  - `GetCipSummary(...)`
  - `GetAssetRegisterReport(...)`
  - `GetDepreciationExpenseReport(...)`
  - `GetMovementReport(...)`

## UI forms
- `FrmFixedAssetMaster`
  - list/filter asset
  - create/update/soft delete master asset
  - category/group lookup
  - asset card (depreciation history + lifecycle transaction history)
  - quick lifecycle actions from selected asset (improvement/revaluation/transfer/dispose/sale/write-off)
- `FrmFixedAssetCip`
  - CIP summary list/filter
  - CIP cost detail
  - CIP capitalization history
  - shortcut open lifecycle transaction with default type `CIP_CAPITALIZATION`
- `FrmFixedAssetReports`
  - asset register
  - depreciation expense
  - asset movement
  - disposal report
  - revaluation report
  - summary by category
  - fully depreciated assets
  - idle/inactive assets
  - export CSV dan XLSX per tab aktif
  - post/reverse transaksi lifecycle dari tab disposal/revaluation
- `FrmFixedAssetDepreciation`
- `FrmFixedAssetLifecycle`
- `FrmFixedAssetApproval`
  - pending approval inbox (period filter)
  - worklist status-aware (`SUBMITTED`, `APPROVED`, `POSTED`, `REVERSED`)
  - approve/reject selected transaction
  - post approved transaction
  - reverse posted transaction

## Access control in UI
- `FixedAssetUiRoleHelper` diterapkan pada tombol aksi utama:
  - master save/delete
  - lifecycle quick actions
  - CIP capitalization action
  - approval action buttons
  - post/reverse report actions

## Session scope
- Seluruh form Fixed Asset menggunakan `CompanyInfo.IDDATA` dari session login.
- Field `IDDATA` di UI bersifat display-only (tidak dapat diubah user).

## Migration files
- `Utilities/Sql/GLMigrator/migrations/V20260319_008__fixed_asset_core.sql`
- `Utilities/Sql/GLMigrator/migrations/R20260319_008__fixed_asset_core.sql`
- `Utilities/Sql/GLMigrator/migrations/C20260319_008__fixed_asset_core_check.sql`

## Sample SQL
- report queries:
  - `DatabaseScripts/20260319_fixed_asset_report_samples.sql`
- seed data:
  - `DatabaseScripts/20260319_fixed_asset_seed_samples.sql`
- cleanup test artifact:
  - `DatabaseScripts/20260319_fixed_asset_cleanup_testdata.sql`
- smoke test:
  - `DatabaseScripts/20260319_fixed_asset_smoke_test.sql`
  - `DatabaseScripts/20260319_fixed_asset_smoke_pack.sql`
- release note & UAT:
  - `DatabaseScripts/20260319_fixed_asset_release_notes.md`
  - `DatabaseScripts/20260319_fixed_asset_uat_checklist.md`
