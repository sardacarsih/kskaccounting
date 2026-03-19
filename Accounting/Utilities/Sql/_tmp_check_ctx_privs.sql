set pages 200 lines 220
col privilege format a40
select privilege from user_sys_privs where privilege in ('CREATE INDEX','CREATE ANY INDEX');
select owner, table_name from all_tables where owner='CTXSYS' and table_name in ('CTX_INDEXES','CTX_INDEX_ERRORS');
exit
