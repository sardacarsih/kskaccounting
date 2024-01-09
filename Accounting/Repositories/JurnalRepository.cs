using Accounting.BusinessLayer;
using Accounting.Interface;
using Accounting.Model;
using Dapper;
using DevExpress.Utils.DragDrop;
using DevExpress.XtraCharts.Native;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.Repositories
{
    public class JurnalRepository : IJurnalRepository
    {
        public void InsertJurnalMasterDetail(JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail)
        {
            using var connection = new OracleConnection(Acct.OracleConnString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var masterInsertQuery = @"INSERT INTO ACCT_JURNAL_HDR (IDDATA, NOJURNAL, TANGGAL, PERIODE, SUMBER, USERID, HID, PC, IP_ADD, ISRE)
                VALUES(:IDData, :NoJurnal, :Tanggal, :Periode, :Sumber, :UserID,:HID, :PC, :IP_Add, :ISRE) 
                RETURNING JURNALID INTO :jurnalid";

                var masterParameters = new DynamicParameters(jurnalHeader);
                masterParameters.Add("jurnalid", dbType: DbType.Double, direction: ParameterDirection.Output);

                int rowsAffected = connection.Execute(masterInsertQuery, masterParameters, transaction);

                double newjurnalid = masterParameters.Get<double>("jurnalid");

                var detailInsertQuery = @"INSERT INTO ACCT_JURNAL_DTL
             (NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT, KETERANGAN, POSTED, PERIODE, IDDATA, USERID, SUMBER, DID, GLYEAR, GLMONTH, HIDREFF, REFFID)
             VALUES (:NOJURNAL, :TANGGAL, :BARIS, :KODE, :REKENING, :DEBET, :KREDIT, :KETERANGAN, :POSTED, :PERIODE, :IDDATA, :USERID, :SUMBER, :DID, :GLYEAR, :GLMONTH, :HIDREFF, :REFFID)";

                foreach (var detailData in jurnalDetail)
                {
                    detailData.NoJurnal = jurnalHeader.NOJURNAL;
                    detailData.Tanggal = jurnalHeader.TANGGAL;
                    detailData.Periode = jurnalHeader.PERIODE;
                    detailData.IDDATA = jurnalHeader.IDDATA;
                    detailData.USERID = jurnalHeader.USERID;
                    detailData.SUMBER = jurnalHeader.SUMBER;
                    detailData.GLMONTH = int.Parse(jurnalHeader.PERIODE[..2]);
                    detailData.GLYEAR = int.Parse(jurnalHeader.PERIODE.Substring(3, 4));
                    detailData.DID = newjurnalid.ToString() + detailData.BARIS;
                    detailData.Posted = "True";
                    detailData.REFFID = newjurnalid; // Set the foreign key reference to the master record
                    detailData.HIDREFF = jurnalHeader.HID;

                    var detailParameters = new DynamicParameters(detailData);
                    connection.Execute(detailInsertQuery, detailParameters, transaction);
                }

                transaction.Commit(); // Commit the transaction to save the changes
               
                AccountServices.RekalkulasiByJurnalID(jurnalHeader.IDDATA, int.Parse(jurnalHeader.PERIODE.Substring(0, 2)), int.Parse(jurnalHeader.PERIODE[^4..]), newjurnalid, jurnalHeader.PERIODE, LoginInfo.userID);
            }
            catch
            {
                transaction.Rollback(); // Rollback the transaction in case of any exception
                throw;
            }
        }
        public void HapusJurnal(double p_JurnalID)
        {
            using var connection = new OracleConnection(Acct.OracleConnString);
            using OracleCommand cmd = new("DELETE FROM ACCT_JURNAL_HDR WHERE JURNALID=:p_JurnalID ", connection)
            {
                CommandType = CommandType.Text
            };
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            cmd.Parameters.Add(":p_JurnalID", OracleDbType.Double).Value = p_JurnalID;
            cmd.ExecuteNonQuery();
        }

        public void HapusJurnalRange(List<double> selectedValues)
        {
            // Create a connection to Oracle
            using var connection = new OracleConnection(Acct.OracleConnString);
            connection.Open();

            // Construct the SQL query with the list of record IDs
            var sql = "DELETE FROM ACCT_JURNAL_HDR WHERE JURNALID IN :RecordIds";

            // Execute the query using Dapper
            connection.Execute(sql, new { RecordIds = selectedValues }, commandType: CommandType.Text);
        }
        public void PerformDragAndDrop(GridView targetGrid, GridView sourceGrid, DragDropEventArgs e)
        {
            if (e.Action == DragDropActions.None || targetGrid != sourceGrid)
                return;

            BindingList<JurnalDetailAdd> sourceTable = sourceGrid.GridControl.DataSource as BindingList<JurnalDetailAdd>;

            Point hitPoint = targetGrid.GridControl.PointToClient(Cursor.Position);
            GridHitInfo hitInfo = targetGrid.CalcHitInfo(hitPoint);

            int[] sourceHandles = e.GetData<int[]>();

            int targetRowHandle = hitInfo.RowHandle;
            int targetRowIndex = targetGrid.GetDataSourceRowIndex(targetRowHandle);

            List<JurnalDetailAdd> draggedRows = new List<JurnalDetailAdd>();
            foreach (int sourceHandle in sourceHandles)
            {
                int oldRowIndex = sourceGrid.GetDataSourceRowIndex(sourceHandle);
                JurnalDetailAdd oldRow = sourceTable[oldRowIndex];
                draggedRows.Add(oldRow);
            }

            int newRowIndex;

            switch (e.InsertType)
            {
                case InsertType.Before:
                    newRowIndex = targetRowIndex > sourceHandles[sourceHandles.Length - 1] ? targetRowIndex - 1 : targetRowIndex;
                    for (int i = draggedRows.Count - 1; i >= 0; i--)
                    {
                        JurnalDetailAdd oldRow = draggedRows[i];
                        JurnalDetailAdd newRow = new JurnalDetailAdd()
                        {
                            Kode = oldRow.Kode,
                            Rekening = oldRow.Rekening,
                            Debet = oldRow.Debet,
                            Kredit = oldRow.Kredit,
                            Keterangan = oldRow.Keterangan
                        };
                        sourceTable.Remove(oldRow);
                        sourceTable.Insert(newRowIndex, newRow);
                    }
                    break;
                case InsertType.After:
                    newRowIndex = targetRowIndex < sourceHandles[0] ? targetRowIndex + 1 : targetRowIndex;
                    for (int i = 0; i < draggedRows.Count; i++)
                    {
                        JurnalDetailAdd oldRow = draggedRows[i];
                        JurnalDetailAdd newRow = new JurnalDetailAdd()
                        {
                            Kode = oldRow.Kode,
                            Rekening = oldRow.Rekening,
                            Debet = oldRow.Debet,
                            Kredit = oldRow.Kredit,
                            Keterangan = oldRow.Keterangan
                        };
                        sourceTable.Remove(oldRow);
                        sourceTable.Insert(newRowIndex, newRow);
                    }
                    break;
                default:
                    newRowIndex = -1;
                    break;
            }

            int insertedIndex = targetGrid.GetRowHandle(newRowIndex);
            targetGrid.FocusedRowHandle = insertedIndex;
            targetGrid.SelectRow(targetGrid.FocusedRowHandle);
        }

        public List<DTOCOAAktif> KodeUntukJurnal(string piddata, int ptahun)
        {
            using var connection = new OracleConnection(Acct.OracleConnString);
            string sql1 = "select kodeacc as KODE, namaacc as PERKIRAAN from acct_coa where iddata=:p_iddata and tahun=:p_tahun AND ISHEADER = 'D' AND isAKTIF <>'T' order by kodeacc asc";

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var parameters = new { p_iddata = piddata, p_tahun = ptahun };

            List<DTOCOAAktif> resultList = connection.Query<DTOCOAAktif>(sql1, parameters).ToList();

            connection.Close();

            return resultList;
        }
    }

}
