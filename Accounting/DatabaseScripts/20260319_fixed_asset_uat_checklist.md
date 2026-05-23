# Fixed Asset UAT Checklist (Enterprise)

## Scope
- Module: Fixed Asset
- Company/Branch scope: multi-company ready (`IDDATA`)
- Accounting period rule: monthly with lock validation
- Integration: General Ledger via `ACCT_JURNAL_IMPORT_V2.ImportJurnalParsial`

## Pre-Condition
- Migration `V20260319_008__fixed_asset_core.sql` sudah `APPLIED`.
- Mapping account category fixed asset terisi:
  - `ASSET_ACCT`
  - `ACC_DEPR_ACCT`
  - `DEPR_EXP_ACCT`
  - `GAIN_DISP_ACCT`
  - `LOSS_DISP_ACCT`
  - `REVAL_SURPLUS_ACCT`
  - `REVAL_DEFICIT_ACCT`
  - `CIP_ACCT`
- Minimal 1 asset status `ACTIVE` tersedia.
- Period target masih open (tidak locked).

## UAT Flow
1. Master Data
- Create asset baru (`ACTIVE`) dengan depreciation method `SL` atau `DB`.
- Expected:
  - `ACCT_FA_ASSET` terisi lengkap.
  - `ASSET_CODE` unik per `IDDATA`.

2. Depreciation Preview + Post
- Jalankan `Preview` lalu `Post Run`.
- Expected:
  - `ACCT_FA_DEPR_RUN` status `POSTED`.
  - `ACCT_FA_DEPR_HISTORY` terisi tanpa duplikasi per asset+period.
  - Jurnal `FA/YYYYMM/{RUN_ID}` terbentuk dan balanced.

3. Improvement Transaction
- Create transaction type `IMPROVEMENT`, submit, lalu approve.
- Expected:
  - Header lifecycle status berubah `DRAFT -> SUBMITTED -> APPROVED -> POSTED`.
  - `ACCT_FA_ASSET.ACQUISITION_COST` naik.
  - `ACCT_FA_ASSET.IMPROVEMENT_TOTAL` naik.
  - Jurnal lifecycle terbentuk (`FA/YYYYMM/TX{TRX_ID}`) dan balanced.

4. Revaluation Transaction
- Create transaction type `REVALUATION`, isi old/new value, submit, approve.
- Expected:
  - Selisih revaluation tersimpan di `REVALUATION_DELTA_TOTAL`.
  - `ACCT_FA_ASSET.ACQUISITION_COST` updated.
  - Jurnal surplus/defisit terbentuk sesuai delta.

5. Disposal / Sale / Write-Off
- Create transaction type `FULL_DISPOSAL` / `SALE` / `WRITE_OFF`, submit, approve.
- Expected:
  - Asset status berubah (`DISPOSED`/`SOLD`/`WRITTEN_OFF`).
  - Asset tersebut tidak lagi masuk perhitungan depresiasi period berikut.
  - Jurnal disposal/write-off/sale terbentuk dan balanced.

6. Period Lock Guard
- Lock period target.
- Coba post depreciation / create lifecycle transaction.
- Expected:
  - Proses ditolak dengan pesan period locked.

7. Role-Based Menu Guard
- Uji login role `Viewer`, `Maker`, `Checker`, `Approver`, `Admin`.
- Expected:
  - `Depreciation Run`: Maker/Checker/Approver/Admin.
  - `Lifecycle Transaction`: Maker/Admin.
  - `Approval Action`: Checker/Approver/Admin.

## SQL Verification
- Jalankan `20260319_fixed_asset_smoke_pack.sql`.
- Semua section harus tanpa error Oracle dan hasil anomali harus `no rows selected`.
