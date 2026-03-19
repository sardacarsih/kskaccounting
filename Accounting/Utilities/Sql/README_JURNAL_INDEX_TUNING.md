# Jurnal DB Tuning Runbook (Oracle)

## Files
- `20260318_jurnal_index_tuning_oracle.sql`: create indexes + refresh table stats.
- `20260318_jurnal_explain_plan_check.sql`: capture explain plan for key jurnal queries.
- `20260318_jurnal_index_tuning_rollback.sql`: rollback indexes.
- `20260318_jurnal_oracle_text_apply.sql`: create Oracle Text preferences and CONTEXT indexes for `NOJURNAL`/`KETERANGAN`.
- `20260318_jurnal_oracle_text_explain_check.sql`: compare explain plan for `CONTAINS` vs legacy `LIKE`.
- `20260318_jurnal_oracle_text_rollback.sql`: rollback Oracle Text indexes/preferences.

## Deployment Order
1. Run `20260318_jurnal_explain_plan_check.sql` and save output as baseline.
2. Run `20260318_jurnal_index_tuning_oracle.sql` in the target schema.
3. Run `20260318_jurnal_oracle_text_apply.sql` in the target schema (only if keyword search still slow at scale).
4. Run `20260318_jurnal_explain_plan_check.sql` again and compare cost/plan path.
5. Run `20260318_jurnal_oracle_text_explain_check.sql` and validate `CONTAINS` plan path.
6. Run aplikasi smoke test pada `FrmJurnal` (periode switch, search, AIS/HRIS).
7. If regression happens, run rollback script:
   - B-tree rollback: `20260318_jurnal_index_tuning_rollback.sql`
   - Oracle Text rollback: `20260318_jurnal_oracle_text_rollback.sql`

## Success Criteria
- Key queries avoid full table scan for common filters.
- Query cost and response time improve on period/header/detail loads.
- Oracle Text `CONTAINS` query path is used for `NOJURNAL` and `KETERANGAN` keyword search.
- No functional change to jurnal result set.
