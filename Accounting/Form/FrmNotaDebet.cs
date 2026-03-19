

using Accounting.BusinessLayer;
using DevExpress.XtraSplashScreen;
using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Diagnostics;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Configuration;
using Accounting.Laporan;
using DevExpress.XtraReports.UI;
using System.Media;
using DevExpress.Export;
using DevExpress.XtraPrinting;
using System.Drawing;
using DevExpress.XtraGrid.Views.Grid;

namespace Accounting.Form
{
    public partial class FrmNotaDebet : DevExpress.XtraEditors.XtraForm
    {

        string pperiode;
        public FrmNotaDebet()
        {
            InitializeComponent();
        }
        OracleConnection con = new OracleConnection( LoginInfo.OracleConnString);
        private void FrmNotaDebet_Load(object sender, EventArgs e)
        {
            cmbbulan.Properties.Items.AddRange(new[] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" });
            int x= int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length-2, 2));
           
            cmbbulan.SelectedIndex = x-1;
            setahun.Properties.MinValue = Acct.TahunMin;
            setahun.Properties.MaxValue = Acct.TahunMax;
            setahun.Value = Acct.TahunMax;
           
            Load_AkunNotaDebet();
           // Load_JurnalNotaDebet();
            gridView2.Focus();
        }
        DataSet _ds;
        private void Load_JurnalNotaDebet()
        {
            var rowhandle = gridView2.FocusedRowHandle;
            var PKODE = gridView2.GetRowCellValue(rowhandle, "KODE").ToString();
            var PPERIODE = (cmbbulan.SelectedIndex + 1).ToString("00") + "/" + setahun.Value;            
            _ds = JurnalServices.GetNotaDebet(CompanyInfo.IDDATA, PPERIODE, PKODE);
            gridControl1.DataSource = _ds.Tables[0];
            gridViewND.Columns[0].Visible = false;
            gridViewND.Columns[1].Visible = false;
            gridViewND.Columns[3].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            gridViewND.Columns[3].DisplayFormat.FormatString = "dd-MMM-yyyy";
            gridViewND.Columns[6].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridViewND.Columns[6].DisplayFormat.FormatString = "n2";
            gridViewND.Columns[6].Summary.Clear();
            gridViewND.Columns[6].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "DEBET", "{0:N2}");
            gridViewND.BestFitColumns();
           
        }

        private void Load_AkunNotaDebet()
        {
            var PPERIODE = (cmbbulan.SelectedIndex + 1).ToString("00") + "/" + setahun.Value;
            var PTAHUN = (int)setahun.Value;
            // string query = "select dictinct J.KODE,A.NAMAACC FROM ACCT_JURNAL_DTL J JOIN ACCT_COA A WHERE JIDDATA=:PIDDATA AND TAHUN=:PTAHUN AND SUBSTR(KODEACC,0,3)='12.' AND ISHEADER='D'";
            string query = "select DISTINCT J.KODE,A.NAMAACC PERKIRAAN FROM ACCT_JURNAL_DTL J " +
                 "JOIN ACCT_COA A ON J.KODE = A.KODEACC AND A.IDDATA =:PIDDATA AND A.TAHUN =:PTAHUN " +
                "WHERE J.IDDATA =:PIDDATA AND J.PERIODE =:PPERIODE AND SUBSTR(J.KODE, 0, 3) = '12.' AND J.DEBET>0 "+
                "AND (J.NOJURNAL NOT LIKE '%/NK%' and J.NOJURNAL NOT LIKE '%/ND%') ORDER BY KODE ASC";

            OracleCommand cmd = new OracleCommand(query, con)
            {
                CommandType = CommandType.Text
            };
           
            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            cmd.Parameters.Add(":PIDDATA", OracleDbType.Varchar2, 20).Value =CompanyInfo.IDDATA;
            cmd.Parameters.Add(":PTAHUN", OracleDbType.Int16).Value = PTAHUN;
            cmd.Parameters.Add(":PIDDATA", OracleDbType.Varchar2, 20).Value =CompanyInfo.IDDATA;
            cmd.Parameters.Add(":PPERIODE", OracleDbType.Varchar2, 7).Value = PPERIODE;
            OracleDataReader dr;
            dr = cmd.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            con.Close();
            gridControl2.DataSource = _dt;

           
        }

        private void setahun_EditValueChanged(object sender, EventArgs e)
        {
          pperiode = (cmbbulan.SelectedIndex + 1).ToString("00") + "/" + setahun.Value;
            Load_AkunNotaDebet();
            gridControl1.DataSource = null;
        }

        private void gridControl2_Click(object sender, EventArgs e)
        {
            Load_JurnalNotaDebet();
            btncetak.Enabled = true;
            btnexport.Enabled = true;
            sbtandai.Enabled = true;
        }

        private void gridControl2_KeyUp(object sender, KeyEventArgs e)
        {
            Load_JurnalNotaDebet();
        }

        private void cmbbulan_SelectedIndexChanged(object sender, EventArgs e)
        {
            pperiode = (cmbbulan.SelectedIndex + 1).ToString("00") + "/" + setahun.Value;
            Load_AkunNotaDebet();
            gridControl1.DataSource = null;
        }

        private SoundPlayer Player = new SoundPlayer();

        private void btnexport_Click(object sender, EventArgs e)
        {
            bool akses = LevelAksesServices.CetakExport(15, LoginInfo.userID);
            if (akses == false)
            {
                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                this.Player.Play();
                XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                var rowhandle = gridView2.FocusedRowHandle;
                var x = gridView2.GetRowCellValue(rowhandle, "PERKIRAAN").ToString();
                var kepada = x;
               

                string file = @"NOTADEBET.xlsx";

                SplashScreenManager.ShowForm(typeof(WaitForm_Exporting));
                Cursor.Current = Cursors.WaitCursor;
               
                // Ensure that the data-aware export mode is enabled.
                ExportSettings.DefaultExportType = ExportType.DataAware;
                // Create a new object defining how a document is exported to the XLSX format.
                var options = new XlsxExportOptionsEx
                {
                    // Specify a name of the sheet in the created XLSX file.
                    SheetName = kepada
                };

                // Subscribe to export customization events. 
                //options.CustomizeSheetSettings += options_CustomizeSheetSettings;
                //options.CustomizeSheetHeader += options_CustomizeSheetHeader;
                //options.CustomizeCell += options_CustomizeCell;
                //options.CustomizeSheetFooter += options_CustomizeSheetFooter;
                //options.AfterAddRow += options_AfterAddRow;

                // Export the grid data to the XLSX format.
                gridControl1.ExportToXlsx(file, options);

                Cursor.Current = Cursors.Default;
                SplashScreenManager.CloseForm();

                Process.Start(file);
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Default;
                SplashScreenManager.CloseForm();
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void sbtandai_Click(object sender, EventArgs e)
        {
            //cek nota debet yg belum ditandai terkirim/true
            int belumdikirim = 0;
            for (int i = 0; i < gridViewND.DataRowCount; i++)
            {
                var p_status = gridViewND.GetRowCellValue(i, "POSTED").ToString().Trim();
                if (p_status != "True")
                {
                    belumdikirim = belumdikirim + 1;
                }
            }

            if (belumdikirim == 0)
            {
                XtraMessageBox.Show("Semua Nota Debet telah dikirim", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

           if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            for (int i = 0; i < gridViewND.DataRowCount; i++)
            {
               // string query = "update acct_jurnal_dtl set posted='True' where did=:p_did and posted<>'True'";

                OracleCommand cmd = new OracleCommand("ACCT_JURNAL.UpdateStatusND", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                var p_did = gridViewND.GetRowCellValue(i, "DID").ToString();
                cmd.Parameters.Add(":p_did", OracleDbType.Varchar2, 50).Value = p_did;
                cmd.ExecuteNonQuery();
            }
            con.Close();
            Load_JurnalNotaDebet();
            XtraMessageBox.Show("Status Pengiriman Nota Debet telah diupdate"+
                "\nJumlah Record :"+belumdikirim, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void gridViewND_RowStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                string priority = View.GetRowCellDisplayText(e.RowHandle, View.Columns["POSTED"]);
                if (priority == "False")
                {
                    e.Appearance.BackColor = Color.FromArgb(150, Color.LightCoral);
                    e.Appearance.BackColor2 = Color.White;
                }
            }
        }

        private void btncetak_Click(object sender, EventArgs e)
        {
            bool akses = LevelAksesServices.CetakExport(15, LoginInfo.userID);
            if (akses == false)
            {
                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                this.Player.Play();
                XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int pbulan = cmbbulan.SelectedIndex + 1;
            int ptahun = Convert.ToInt32(setahun.Value);
            var lastDayOfMonth = DateTime.DaysInMonth(ptahun, pbulan);
            var NDBULAN = lastDayOfMonth + " " + cmbbulan.Text + "-" + setahun.Value.ToString();

            var rowhandle = gridView2.FocusedRowHandle;
            var x = gridView2.GetRowCellValue(rowhandle, "PERKIRAAN").ToString();
            var kepada = x;
            //_ds.WriteXmlSchema("NotaDebet.xsd");
            NotaDebet laporan = new NotaDebet
            {
                DataSource = _ds
            };
            laporan.Parameters["USERID"].Value = LoginInfo.userID;
            laporan.Parameters["TANGGAL"].Value = NDBULAN;
            laporan.Parameters["KEPADA"].Value = kepada;
            laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
            laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
            laporan.RequestParameters = true;
            ReportPrintTool tool = new ReportPrintTool(laporan);
            tool.ShowPreview();
        }

       
    }
}
