set pages 200 lines 220
col comp_name format a40
col version format a15
select comp_name, version, status from dba_registry where comp_id = 'CONTEXT';
exit
