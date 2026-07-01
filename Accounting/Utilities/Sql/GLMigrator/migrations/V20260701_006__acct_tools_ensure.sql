-- Purpose: Ensure ACCT_TOOLS carries the current procedures -- notably FUNCTION Analisa_kesalahan_COA, used by
--   the closing forms' COA pre-check. Some environments (e.g. FSKPKS) have a stale ACCT_TOOLS missing this
--   function, so Tutup Tahun fails at the pre-check with:
--     ORA-06550 / PLS-00201: identifier 'ACCT_TOOLS.ANALISA_KESALAHAN_COA' must be declared.
--   This deploys the canonical package spec + body (schema qualifier neutralized). CREATE OR REPLACE is idempotent.
-- WARNING: this replaces the entire ACCT_TOOLS package. If a target environment has environment-specific
--   ACCT_TOOLS procedures that are not in this canonical version, diff before applying.
-- Date: 2026-07-01


  CREATE OR REPLACE PACKAGE "ACCT_TOOLS" AS

PROCEDURE AddIDDATA(p_IDDATA IN VARCHAR2,apps IN VARCHAR2,userid in VARCHAR2);
PROCEDURE AddUserLogin(p_IDDATA IN VARCHAR2,p_apps IN VARCHAR2,p_userid in VARCHAR2,p_nama in VARCHAR2, p_dept IN VARCHAR2,p_pass in VARCHAR2,p_roleid in int,p_jabatan in VARCHAR2);
PROCEDURE DuplikatRole(ROLENAME IN VARCHAR2,dariroleid IN int,apps IN VARCHAR2);
PROCEDURE AddorUpdateDivisi(p_divisiid IN VARCHAR2,p_IDDATA IN VARCHAR2,p_kode IN VARCHAR2,p_nama IN VARCHAR2);
PROCEDURE AddorUpdateBlok(p_blokid IN VARCHAR2,p_IDDATA IN VARCHAR2,p_kode IN VARCHAR2,p_nama IN VARCHAR2,p_divid in VARCHAR2,p_luas in NUMBER
, p_ttanam IN VARCHAR2,p_status in VARCHAR2,p_TM IN VARCHAR2,p_AKTIF in VARCHAR2);
PROCEDURE AddorUpdateMasterAkun(p_status IN VARCHAR2,p_kode IN VARCHAR2,p_perkiraan IN VARCHAR2,p_jenis IN VARCHAR2,p_level IN VARCHAR2,p_induk in VARCHAR2,p_gd in VARCHAR2,p_kategori in VARCHAR2);
FUNCTION Analisa_kesalahan_COA (p_IDDATA IN VARCHAR2,p_tahun in int) RETURN SYS_REFCURSOR;
FUNCTION GetBlokList (p_IDDATA IN VARCHAR2) RETURN SYS_REFCURSOR;
FUNCTION CekAkunBlok (p_IDDATA IN VARCHAR2,p_tahun in int,p_divisi IN VARCHAR2,p_blok IN VARCHAR2) RETURN NUMBER;
FUNCTION GetPerkiraanBlokList (p_IDDATA IN VARCHAR2,p_tahun in int,p_divisi IN VARCHAR2,p_blok IN VARCHAR2) RETURN SYS_REFCURSOR;
PROCEDURE GenerateAkunTBM(p_IDDATA IN VARCHAR2,p_tahun in int,p_status IN VARCHAR2,p_divisiid IN VARCHAR2,p_divisi IN VARCHAR2,p_blokid IN VARCHAR2,p_blok IN VARCHAR2,p_ttanam IN VARCHAR2,p_mode IN VARCHAR2);
PROCEDURE GenerateAkunTM(p_IDDATA IN VARCHAR2,p_tahun in int,p_status IN VARCHAR2,p_divisiid IN VARCHAR2,p_divisi IN VARCHAR2,p_blokid IN VARCHAR2,p_blok IN VARCHAR2,p_ttanam IN VARCHAR2);
PROCEDURE UPDATE_LUASAN_DIVISI(p_IDDATA IN VARCHAR2);
END ACCT_TOOLS;
/

CREATE OR REPLACE PACKAGE BODY "ACCT_TOOLS" AS

PROCEDURE AddUserLogin(p_IDDATA IN VARCHAR2,p_apps IN VARCHAR2,p_userid in VARCHAR2,p_nama in VARCHAR2, p_dept IN VARCHAR2,p_pass in VARCHAR2,p_roleid in int,p_jabatan in VARCHAR2) AS
  MaxID int;
  ada int;
  BEGIN
  select max(appsdetailid) into MaxID from master_apps_detail;
  if MaxID is null then
  MaxID :=0;
  else
  MaxID :=MaxID ;
  end if;

 --  insert into master_login(userid,nama,dept,"PASSWORD",levelid,jabatan) values
  --                          (userid,nama,dept,pass,roleid,jabatan);
MERGE INTO master_login pt
   USING (SELECT p_userid  AS person_id,
                 p_nama AS nama,
                 p_dept AS departement,
                 p_pass AS passwrd,
                 p_roleid AS roleid,
                 p_jabatan AS jabatan
                 FROM DUAL) ps
   ON (pt.USERID = ps.person_id)
WHEN MATCHED THEN UPDATE
SET pt.nama = ps.nama,
    pt.dept = ps.departement,
    pt.password = ps.passwrd,
    pt.levelid=ps.roleid,
    pt.jabatan=ps.jabatan
WHEN NOT MATCHED THEN INSERT
    (pt.userid,pt.nama,pt.dept,pt."PASSWORD",pt.levelid,pt.jabatan) values
    (ps.person_id, ps.nama, ps.departement,ps.passwrd,ps.roleid,ps.jabatan);

select count(1) into ada from master_apps_detail where userid=p_userid and iddata=p_IDDATA;

if(ada=0) then
   insert into master_apps_detail(appsdetailid,appid,iddata,userid) values
                                (MaxID+1,p_apps,p_IDDATA,p_userid);
end if;


  END AddUserLogin;

  PROCEDURE AddIDDATA(p_IDDATA IN VARCHAR2,apps IN VARCHAR2,userid in VARCHAR2) AS
  MaxID int;
  BEGIN
  select max(appsdetailid) into MaxID from master_apps_detail;
  if MaxID is null then
  MaxID :=0;
  else
  MaxID :=MaxID ;
  end if;

   insert into master_apps_detail(appsdetailid,appid,iddata,userid) values
                                (MaxID+1,apps,p_IDDATA,userid);

  END AddIDDATA;

  PROCEDURE DuplikatRole(ROLENAME IN VARCHAR2,dariroleid IN int,apps IN VARCHAR2)AS
  MaxID int;
  NewLevelID int;
  iaksesid int;
  ilevelid int;
  iaksiid int;
  iinduk int;
  sketerangan varchar2(100);
  cbuka char(1);
  cbaru char(1);
  csimpan char(1);
  cubah char(1);
  ccetak char(1);
  chapus char(1);

  recData master_akses%ROWTYPE;
BEGIN
select max(LEVELID) into MaxID from MASTER_LOGIN_LEVEL;
  if MaxID is null then
  MaxID :=0;
  else
  MaxID :=MaxID ;
  end if;
  NewLevelID :=MaxID+1;
  INSERT INTO MASTER_LOGIN_LEVEL(LEVELID,NAMA)VALUES(NewLevelID,ROLENAME);COMMIT;



 select max(aksesid) into MaxID from master_akses;
  if MaxID is null then
  MaxID :=0;
  else
  MaxID :=MaxID ;
  end if;
FOR recData IN (SELECT aksiid,induk,keterangan,buka,baru,simpan,ubah,cetak,hapus
			FROM master_akses
            WHERE levelid = dariroleid and appsid=apps
			ORDER BY aksiid) LOOP

            MaxID       :=MaxID+1;
            iaksiid     := recData.aksiid;	
            iinduk      := recData.induk;
            sketerangan := recData.keterangan;
            cbuka       := recData.buka;
            cbaru       := recData.baru;
            csimpan     := recData.simpan;
            cubah       := recData.ubah;
            ccetak      := recData.cetak;
            chapus      := recData.hapus;

                INSERT INTO master_akses VALUES(MaxID,NewLevelID,apps,iaksiid,iinduk,sketerangan,cbuka,cbaru,csimpan,cubah,ccetak,chapus);
            END loop;
END DuplikatRole;

FUNCTION Analisa_kesalahan_COA (p_IDDATA IN VARCHAR2,p_tahun in int) RETURN SYS_REFCURSOR
AS CUR SYS_REFCURSOR;

Err_KodeAkun int;
BEGIN

--KODE AKUN LEVEL SATU TIDAK BOLEH ADA INDUK ( HARUS NULL)
select count(1) into Err_KodeAkun  from acct_coa where iddata=p_IDDATA AND TAHUN=p_tahun AND LVL=1 AND PARENTACC IS NOT NULL;
if(Err_KodeAkun>0) then
open CUR for select KODEACC,NAMAACC,PARENTACC,LVL,'KODE AKUN LEVEL SATU TIDAK BOLEH ADA INDUK' KET from acct_coa where iddata=p_IDDATA AND TAHUN=p_tahun AND LVL=1 AND PARENTACC IS NOT NULL;
end if;

--KODE AKUN LEVEL DUA KETATAS HARUS ADA INDUK
select count(1) into Err_KodeAkun  from acct_coa where iddata=p_IDDATA AND TAHUN=p_tahun AND LVL>1 AND PARENTACC IS NULL;
if(Err_KodeAkun>0) then
open CUR for select KODEACC,NAMAACC,PARENTACC,LVL,'KODE AKUN LEVEL DUA KEATAS HARUS ADA INDUK' KET from acct_coa where iddata=p_IDDATA AND TAHUN=p_tahun AND LVL>1 AND PARENTACC IS NULL;
--DBMS_SQL.RETURN_RESULT(v_list);
end if;

--KODE INDUK SAMA DENGAN AKUN
select count(1) into Err_KodeAkun  from acct_coa where iddata=p_IDDATA AND TAHUN=p_tahun AND KODEACC=PARENTACC;
if(Err_KodeAkun>0) then
open CUR for select KODEACC,NAMAACC,PARENTACC,LVL,'KODE AKUN TIDAK BOLEH SAMA SAMA DENGAN KODE INDUK' KET from acct_coa where iddata=p_IDDATA AND TAHUN=p_tahun AND KODEACC=PARENTACC;
--DBMS_SQL.RETURN_RESULT(v_list);
end if;

--Akun detail tidak memiliki induk akun' keterangan
select count(1) into Err_KodeAkun  from acct_coa where iddata=p_IDDATA AND TAHUN=p_tahun AND isheader='D' and parentacc is null;
if(Err_KodeAkun>0) then
open CUR for select KODEACC,NAMAACC,PARENTACC,LVL,'KODE AKUN DETAIL TIDAK MEMILIKI INDUK AKUN' KET from acct_coa where iddata=p_IDDATA AND TAHUN=p_tahun AND isheader='D' and parentacc is null;
--DBMS_SQL.RETURN_RESULT(v_list);
end if;

RETURN CUR;
 CLOSE CUR;
END Analisa_kesalahan_COA;

PROCEDURE AddorUpdateDivisi(p_divisiid IN VARCHAR2,p_IDDATA IN VARCHAR2,p_kode IN VARCHAR2,p_nama IN VARCHAR2) AS
  sBlockID VARCHAR2(30);
sBlockIDOld VARCHAR2(30);
sDivID VARCHAR2(30);
  BEGIN
   sDivID   :=p_IDDATA||p_kode;

  if(p_divisiid='New') then
   insert into master_divisi(divisiid,iddata,kode,divisi) values
                            (sDivID,p_IDDATA,p_kode,p_nama);
  else

  update master_divisi
  set
  divisiid=sDivID
  ,kode=p_kode
  ,divisi=p_nama
  where divisiid=p_divisiid;
  commit;

  end if;
  END AddorUpdateDivisi;

PROCEDURE AddorUpdateBlok(p_blokid IN VARCHAR2,p_IDDATA IN VARCHAR2,p_kode IN VARCHAR2,p_nama IN VARCHAR2,p_divid in VARCHAR2,p_luas in NUMBER
, p_ttanam IN VARCHAR2,p_status in VARCHAR2,p_TM IN VARCHAR2,p_AKTIF in VARCHAR2) AS
sBlockID VARCHAR2(30);
sBlockIDOld VARCHAR2(30);
sDivID VARCHAR2(30);
  BEGIN

  sBlockID :=p_IDDATA||p_divid||p_kode;
  sDivID   :=p_IDDATA||p_divid;

  if(p_blokid='New') then
   insert into master_blok(blokid,divisiid,kode,blok,luas,ttanam,status,TM,AKTIF) values
                            (sBlockID,sDivID,p_kode,p_nama,p_luas,p_ttanam,p_status,p_TM,p_AKTIF);
  else

  update master_blok
  set
  blokid=sBlockID
  ,divisiid=sDivID
  ,kode=p_kode
  ,blok=p_nama
  ,luas=p_luas
  ,ttanam=p_ttanam
  ,status=p_status
  ,TM=p_TM
  ,AKTIF=p_AKTIF
  where blokid=p_blokid;
  commit;

  --update kode perkiraan nama divisi baru
  --GenerateAkunTBM(p_IDDATA,p_tahun ,p_status ,p_divid,p_divisi ,p_blokid,p_blok ,p_ttanam);
  end if;

  END AddorUpdateBlok;

PROCEDURE AddorUpdateMasterAkun(p_status IN VARCHAR2,p_kode IN VARCHAR2,p_perkiraan IN VARCHAR2,p_jenis IN VARCHAR2,p_level IN VARCHAR2,p_induk in VARCHAR2,p_gd in VARCHAR2,p_kategori in VARCHAR2) AS

  BEGIN
  if(p_status='New') then
   insert into master_akun(account,perkiraan,jenis,lvl,induk,gd,posisi,kategori) values
                            (p_kode,p_perkiraan,p_jenis,p_level,p_induk,p_gd,'D',p_kategori);
  else

  update master_akun
  set
 perkiraan=p_perkiraan
 ,jenis=p_jenis
 ,lvl=p_level
 ,induk=p_induk
 ,gd=p_gd
 ,kategori=p_kategori
  where account=p_kode;
  commit;

  end if;
  END AddorUpdateMasterAkun;

FUNCTION GetBlokList(p_IDDATA IN VARCHAR2) RETURN SYS_REFCURSOR AS CUR SYS_REFCURSOR;

BEGIN
open CUR for
SELECT D.DIVISI,D.KODE KODEDIV,B.BLOKID,B.KODE,B.BLOK,B.TTANAM,B.LUAS,B.STATUS,B.POKOK,B.TM,B.AKTIF FROM MASTER_BLOK B
JOIN MASTER_DIVISI D ON B.DIVISIID=D.DIVISIID WHERE D.IDDATA=p_IDDATA
ORDER BY D.KODE ASC;
RETURN CUR;
CLOSE CUR;
END GetBlokList;

FUNCTION CekAkunBlok(p_IDDATA IN VARCHAR2,p_tahun in int,p_divisi IN VARCHAR2,p_blok IN VARCHAR2) RETURN NUMBER IS
iada int;
BEGIN
SELECT count(acctcoaid)into  iada  FROM ACCT_COA WHERE IDDATA=p_IDDATA AND TAHUN=p_tahun AND UPPER(DIVISI)=p_divisi AND UPPER(BLOK)=p_blok ORDER BY KODEACC ASC;

RETURN iada;
END CekAkunBlok;

FUNCTION GetPerkiraanBlokList(p_IDDATA IN VARCHAR2,p_tahun in int,p_divisi IN VARCHAR2,p_blok IN VARCHAR2) RETURN SYS_REFCURSOR AS CUR SYS_REFCURSOR;

BEGIN
open CUR for
SELECT CASE WHEN SUBSTR(KODEACC,1,2)='20' THEN 'TBM' WHEN SUBSTR(KODEACC,1,2)='80' THEN 'TM PANEN' WHEN SUBSTR(KODEACC,1,2)='81' THEN 'TM PERAWATAN' END KELOMPOK,
KODEACC KODE ,NAMAACC PERKIRAAN ,GRP JENIS,LVL,PARENTACC INDUK,ISHEADER GD,POSISI,ISAKTIF FROM ACCT_COA WHERE IDDATA=p_IDDATA AND TAHUN=p_tahun AND
UPPER(DIVISI)=p_divisi AND UPPER(BLOK)=p_blok;
RETURN CUR;
CLOSE CUR;
END GetPerkiraanBlokList;

PROCEDURE GenerateAkunTBM(p_IDDATA IN VARCHAR2,p_tahun in int,p_status IN VARCHAR2,p_divisiid IN VARCHAR2,p_divisi IN VARCHAR2,p_blokid IN VARCHAR2,p_blok IN VARCHAR2,p_ttanam IN VARCHAR2,p_mode IN VARCHAR2) AS
sCOAID VARCHAR2(50);
sKodeAcc VARCHAR2(20);
sRekeningAcc VARCHAR2(100);
sJenis char(2);
sLevel char(1);
sInduk VARCHAR2(20);
sGD char(1);
sPosisi char(1);

sKodeAccNew VARCHAR2(20);
sRekeningAccNew VARCHAR2(100);
sIndukNew VARCHAR2(20);

sDigit45 char(2);
s4DigitAkirKode char(4);
s4DigitAkirInduk char(4);

iRecord integer;
recData MASTER_AKUN%ROWTYPE;
  BEGIN
DELETE FROM ACC_AKUN_BLOK;

IF(p_status='INTI') THEN
sDigit45 :='11';
ELSIF(p_status='KKPA') THEN
sDigit45 :='21';
END IF;

FOR recData IN (SELECT ACCOUNT,PERKIRAAN,JENIS,LVL,INDUK,GD,POSISI FROM   MASTER_AKUN WHERE KATEGORI='TBM' ORDER BY ACCOUNT ASC)
			LOOP

            sKodeAcc        := recData.ACCOUNT;
            sRekeningAcc    := recData.PERKIRAAN;
            sJenis          := recData.JENIS;
            sLevel          := recData.LVL;
            sInduk          := recData.INDUK;
            sGD             := recData.GD;
            sPosisi         := recData.POSISI;

            s4DigitAkirKode     :=SUBSTR(sKodeAcc,9,4);
            s4DigitAkirInduk    :=SUBSTR(sInduk,9,4);

            sKodeAccNew     :='20.'||sDigit45||p_blokid||s4DigitAkirKode;

            IF(sLevel='3') THEN
                sIndukNew :='20.'||sDigit45||'000.000';
            ELSE
                sIndukNew :='20.'||sDigit45||p_blokid||s4DigitAkirInduk;
            END IF;

            IF(sGD='G') THEN
                sRekeningAccNew :=sRekeningAcc||' '||p_divisi||' '||p_blok;
            ELSE
                sRekeningAccNew :='('||p_divisi||' '||p_blok||') '||sRekeningAcc;
            END IF;


            sCOAID          :=p_IDDATA||p_tahun||sKodeAccNew;
            --DBMS_OUTPUT.ENABLE(100000);
            --DBMS_OUTPUT.PUT_LINE('Kode: '||sKodeAccNew||' Induk: '||sIndukNew||' '||sRekeningAccNew );
            INSERT INTO ACC_AKUN_BLOK (ACCTCOAID,ACCOUNT,PERKIRAAN,JENIS,LVL,INDUK,GD,POSISI,DIVISIID,DIVISI,BLOKID,BLOK,TTANAM)
                                VALUES(sCOAID,sKodeAccNew,sRekeningAccNew,sJenis,sLevel,sIndukNew,sGD,sPosisi,p_divisiid,p_divisi,p_blokid,p_blok,p_ttanam);
            END LOOP;
            COMMIT;
IF(p_mode='CREATE') THEN
MERGE INTO ACCT_COA t1
        --using ACC_AKUN_BLOK t2
        using (SELECT * FROM ACC_AKUN_BLOK ORDER BY ACCTCOAID ASC) t2 --ORDER ASC NEEDED BEFORE INSERT FOR REF INDUK
        on (t1.ACCTCOAID = t2.ACCTCOAID)
        WHEN MATCHED THEN UPDATE SET t1.NAMAACC = t2.PERKIRAAN,t1.divisiID=t2.divisiID,t1.divisi=t2.divisi,t1.blokID=t2.blokID,t1.blok=t2.blok,t1.tahuntanam=t2.ttanam
        WHEN NOT MATCHED THEN INSERT (ACCTCOAID,KODEACC,NAMAACC,GRP,lvl,PARENTACC,isheader,posisi,DIVISIID,DIVISI,BLOKID,BLOK,TAHUNTANAM,IDDATA,TAHUN)
            VALUES (t2.ACCTCOAID,t2.ACCOUNT,t2.PERKIRAAN,t2.JENIS,t2.LVL,t2.INDUK,t2.GD,t2.POSISI,t2.DIVISIID,t2.DIVISI,t2.BLOKID,t2.BLOK,t2.TTANAM,p_IDDATA,p_tahun);

        COMMIT;
ELSIF(p_mode='DISABLE') THEN
MERGE INTO ACCT_COA t1
        using ACC_AKUN_BLOK t2
        on (t1.ACCTCOAID = t2.ACCTCOAID)
        WHEN MATCHED THEN UPDATE SET t1.ISAKTIF ='T';
        COMMIT;
ELSIF(p_mode='ENABLE') THEN
MERGE INTO ACCT_COA t1
        using ACC_AKUN_BLOK t2
        on (t1.ACCTCOAID = t2.ACCTCOAID)
        WHEN MATCHED THEN UPDATE SET t1.ISAKTIF ='-';
        COMMIT;

END IF;

END GenerateAkunTBM;

 PROCEDURE GenerateAkunTM(p_IDDATA IN VARCHAR2,p_tahun in int,p_status IN VARCHAR2,p_divisiid IN VARCHAR2,p_divisi IN VARCHAR2,p_blokid IN VARCHAR2,p_blok IN VARCHAR2,p_ttanam IN VARCHAR2) AS
sCOAID VARCHAR2(50);
sKodeAcc VARCHAR2(20);
sRekeningAcc VARCHAR2(100);
sJenis char(2);
sLevel char(1);
sInduk VARCHAR2(20);
sGD char(1);
sPosisi char(1);

sKodeAccNew VARCHAR2(20);
sRekeningAccNew VARCHAR2(100);
sIndukNew VARCHAR2(20);

sDigit45 char(2);
s3DigitAwal char(3);
s4DigitAkirKode char(4);
s4DigitAkirInduk char(4);

iRecord integer;
recData MASTER_AKUN%ROWTYPE;
  BEGIN
DELETE FROM ACC_AKUN_BLOK;

IF(p_status='INTI') THEN
sDigit45 :='11';
ELSIF(p_status='KKPA') THEN
sDigit45 :='21';
END IF;

FOR recData IN (SELECT ACCOUNT,PERKIRAAN,JENIS,LVL,INDUK,GD,POSISI FROM   MASTER_AKUN WHERE KATEGORI='TM' ORDER BY ACCOUNT ASC)
			LOOP

            sKodeAcc        := recData.ACCOUNT;
            sRekeningAcc    := recData.PERKIRAAN;
            sJenis          := recData.JENIS;
            sLevel          := recData.LVL;
            sInduk          := recData.INDUK;
            sGD             := recData.GD;
            sPosisi         := recData.POSISI;

            s3DigitAwal         :=SUBSTR(sKodeAcc,1,3);
            s4DigitAkirKode     :=SUBSTR(sKodeAcc,9,4);
            s4DigitAkirInduk    :=SUBSTR(sInduk,9,4);

            sKodeAccNew     :=s3DigitAwal||sDigit45||p_blokid||s4DigitAkirKode;

            IF(sLevel='3') THEN
                sIndukNew :=s3DigitAwal||sDigit45||'000.000';
            ELSE
                sIndukNew :=s3DigitAwal||sDigit45||p_blokid||s4DigitAkirInduk;
            END IF;

            IF(sGD='G') THEN
                sRekeningAccNew :=sRekeningAcc||' '||p_divisi||' '||p_blok;
            ELSE
                sRekeningAccNew :='('||p_divisi||' '||p_blok||') '||sRekeningAcc;
            END IF;


            sCOAID          :=p_IDDATA||p_tahun||sKodeAccNew;

            INSERT INTO ACC_AKUN_BLOK (ACCTCOAID,ACCOUNT,PERKIRAAN,JENIS,LVL,INDUK,GD,POSISI,DIVISIID,DIVISI,BLOKID,BLOK,TTANAM)
                                VALUES(sCOAID,sKodeAccNew,sRekeningAccNew,sJenis,sLevel,sIndukNew,sGD,sPosisi,p_divisiid,p_divisi,p_blokid,p_blok,p_ttanam);
            END LOOP;
 MERGE INTO ACCT_COA t1
        --using ACC_AKUN_BLOK t2
         using (SELECT * FROM ACC_AKUN_BLOK ORDER BY ACCTCOAID ASC) t2 --ORDER ASC NEEDED BEFORE INSERT FOR REF INDUK
        on (t1.ACCTCOAID = t2.ACCTCOAID)
        WHEN MATCHED THEN UPDATE SET t1.NAMAACC = t2.PERKIRAAN,t1.divisiID=t2.divisiID,t1.divisi=t2.divisi,t1.blokID=t2.blokID,t1.blok=t2.blok,t1.tahuntanam=t2.ttanam
        WHEN NOT MATCHED THEN INSERT (ACCTCOAID,KODEACC,NAMAACC,GRP,lvl,PARENTACC,isheader,posisi,DIVISIID,DIVISI,BLOKID,BLOK,TAHUNTANAM,IDDATA,TAHUN)
            VALUES (t2.ACCTCOAID,t2.ACCOUNT,t2.PERKIRAAN,t2.JENIS,t2.LVL,t2.INDUK,t2.GD,t2.POSISI,t2.DIVISIID,t2.DIVISI,t2.BLOKID,t2.BLOK,t2.TTANAM,p_IDDATA,p_tahun);

  END GenerateAkunTM;

PROCEDURE UPDATE_LUASAN_DIVISI (p_IDDATA IN VARCHAR2) AS
  BEGIN
  /**
  update luasan divisi dibuat sekaligus dalam lokasi iddata, karena untuk antisipasi penpindahan divisi
  otomatis divisi lama juga akan di recalkulasi luasannya.
  jika dibuat hitungan perdivisi maka divisilama luasannya jadi salah
  **/
  UPDATE MASTER_DIVISI SET luastm=0 where IDDATA=p_IDDATA;
  Merge into MASTER_DIVISI T1 using
( select B.DIVISIID,SUM(B.LUAS) LUAS from MASTER_BLOK B WHERE B.IDDATA=p_IDDATA AND B.AKTIF='Y' AND B.TM='Y' group by B.DIVISIID) T2
   on ( T1.DIVISIID=T2.DIVISIID )
when matched then
  UPDATE SET T1.LUASTM = T2.LUAS ;

commit;

  UPDATE MASTER_DIVISI SET luastBm=0 where IDDATA=p_IDDATA;
Merge into MASTER_DIVISI T1 using
( select B.DIVISIID,SUM(B.LUAS) LUAS from MASTER_BLOK B WHERE B.IDDATA=p_IDDATA AND B.AKTIF='Y' AND B.TM<>'Y' group by B.DIVISIID) T2
   on ( T1.DIVISIID=T2.DIVISIID )
when matched then
  UPDATE SET T1.LUASTBM = T2.LUAS ;

commit;
  END UPDATE_LUASAN_DIVISI;
END ACCT_TOOLS;

/
