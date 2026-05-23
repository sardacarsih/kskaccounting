# Release Notes - Fixed Asset Module

Release Date: 2026-03-19  
Module: Accounting - Fixed Asset  
Target: Enterprise production deployment (multi-company/multi-branch)

## Highlights
- Added enterprise Fixed Asset core schema and service layer.
- Added depreciation run engine with journal posting integration to GL.
- Added lifecycle transaction + approval flow.
- Added audit trail for key field changes and status transitions.
- Added role-based menu guard for Fixed Asset menus in main ribbon.

## Functional Changes
1. Fixed Asset menu in Ribbon
- Location: `Daftar` page, group `Fixed Asset`.
- Menus:
  - `Depreciation Run`
  - `Lifecycle Transaction`
  - `Approval Action`

2. Depreciation posting hardening
- Idempotent per `RUN_ID`.
- Deterministic journal number: `FA/YYYYMM/{RUN_ID}`.
- Import success code accepts `1` and `99`.
- Compatibility fallback if `ACCT_JURNAL_TMP.SUMBER` absent.

3. Lifecycle approval hardening
- Status transition guard: `SUBMITTED -> APPROVED/REJECTED` only.
- Approval updates asset impact:
  - Improvement: `ACQUISITION_COST`, `IMPROVEMENT_TOTAL`
  - Revaluation: `ACQUISITION_COST`, `REVALUATION_DELTA_TOTAL`
  - Disposal/Sale/Write-Off: asset status mutation
- Auto posting lifecycle journal and set trx status `POSTED` with `NOJURNAL/JURNALID`.

4. Audit & traceability
- Transaction and asset key changes logged to `ACCT_FA_AUDIT_LOG`.
- Document number trace maintained for every transaction.

## Database / Script Pack
- Migration:
  - `V20260319_008__fixed_asset_core.sql`
  - `C20260319_008__fixed_asset_core_check.sql`
  - `R20260319_008__fixed_asset_core.sql`
- Operational scripts:
  - `20260319_fixed_asset_smoke_test.sql`
  - `20260319_fixed_asset_smoke_pack.sql`
  - `20260319_fixed_asset_cleanup_testdata.sql`
  - `20260319_fixed_asset_uat_checklist.md`

## Deployment Checklist
1. Run migration and verify status `APPLIED`.
2. Execute smoke pack SQL.
3. Execute UAT checklist flow for at least one `IDDATA`.
4. Validate role access matrix in UI.
5. Sign-off by Accounting + IT Control.

## Known Environment Constraint
- Build/restore may fail in restricted sandbox due external NuGet TLS/access policy.
- No impact to Oracle migration and SQL validation scripts.
