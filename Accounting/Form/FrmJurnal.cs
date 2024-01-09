using Accounting.BusinessLayer;
using Accounting.Controllers;
using Accounting.Model;
using Accounting.Repositories;
using Dapper;
using DevExpress.Data;
using DevExpress.Data.ODataLinq.Helpers;
using DevExpress.Mvvm.Native;
using DevExpress.Utils.DragDrop;
using DevExpress.Utils.Menu;
using DevExpress.Xpf.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraSplashScreen;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Color = System.Drawing.Color;

namespace Accounting.Form
{
    public partial class FrmJurnal : XtraForm
    {
        private readonly JurnalController _jurnalController;

        bool editjurnal,filter = false;
        string periodetujuan, p_iddata, p_nomorHID, P_IDDATA, P_NOMOR, P_PERIODE;
        int pbulan, ptahun;
        double selisihD, selisihK, nilai, nilai2, new_jurnalid,old_JurnalID;
        private readonly SoundPlayer Player = new ();
        private readonly OracleConnection conn = new(Acct.OracleConnString);
        DataTable dtJurnalKasir, dtAISHeader, dtAISDetail, dtJurnalInventory = new();

        List<DTOCOAAktif> ListCoaAktif;
        IQueryable<JurnalHeaderDTO>? JurnalHeader = null;
        IQueryable<JurnalDetailDTO>? JurnalDetail = null;
        IQueryable<JurnalInventoryHeaderDTO>? InventoryJurnalHeader = null;
        IQueryable<JurnalKasirHeaderDTO>? KasirJurnalHeader = null;
        IEnumerable<JurnalDetailDTO> PencarianJurnal;
        IEnumerable<JurnalDetailDTO> ExportPencarian;
        IEnumerable<JurnalDetailDTO> PencarianJurnal_Bulan;
        IEnumerable<JurnalDetailDTO> ExportPencarian_Bulan;
        List<JurnalDetailReffID> ReffID = new();
        List<JurnalHeaderDTO> JurnalHeader_Filtered = new();
        IEnumerable<JurnalKasirDetailDTO> JurnalFromKasir ;
        BindingList<JurnalDetailAdd> InputJurnalDetail;

        string[] Bulan = { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" };
        public FrmJurnal()
        {
            InitializeComponent();
            // Instantiate the controller with the repository implementation
            var jurnalRepository = new JurnalRepository();
            _jurnalController = new JurnalController(jurnalRepository);

            _ = new GridNewRowHelper(JDgridView);
            PopulateList();
            HandleBehaviorDragDropEvents();
            JDgridView.OptionsClipboard.PasteMode = DevExpress.Export.PasteMode.Update;
        }


        private void HandleBehaviorDragDropEvents()
        {
            DragDropBehavior gridControlBehavior = behaviorManager1.GetBehavior<DragDropBehavior>(this.JDgridView);
            gridControlBehavior.DragDrop += Behavior_DragDrop;
            gridControlBehavior.DragOver += Behavior_DragOver;
        }

        private void Behavior_DragOver(object sender, DragOverEventArgs e)
        {

            DragOverGridEventArgs args = DragOverGridEventArgs.GetDragOverGridEventArgs(e);
            e.InsertType = args.InsertType;
            e.InsertIndicatorLocation = args.InsertIndicatorLocation;
            e.Action = args.Action;
            Cursor.Current = args.Cursor;
            args.Handled = true;
        }

        private void Behavior_DragDrop(object sender, DevExpress.Utils.DragDrop.DragDropEventArgs e)
        {
            GridView targetGrid = e.Target as GridView;
            GridView sourceGrid = e.Source as GridView;
            if (e.Action == DragDropActions.None || targetGrid != sourceGrid)
                return;
            //DataTable sourceTable = sourceGrid.GridControl.DataSource as DataTable;
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
                        JurnalDetailAdd newRow = new()
                        {
                            //newRow.RowNo = oldRow.RowNo;
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
                        JurnalDetailAdd newRow = new()
                        {
                            //newRow.RowNo = oldRow.RowNo;
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

            //GridView targetGrid = sender as GridView;
            //GridView sourceGrid = e.Source as GridView;

            //_jurnalController.PerformDragAndDrop(targetGrid, sourceGrid, e);
        }
        private void PopulateList()
        {

            InputJurnalDetail = new BindingList<JurnalDetailAdd>
            {new JurnalDetailAdd
                {
                    BARIS = 1,
                    Kode = "",
                    Rekening = "",
                    Debet = 0,
                    Kredit = 0,
                    Keterangan = ""
                }
            };
            InputJurnalDetail.AllowNew = true;
            GCJurnal.DataSource = InputJurnalDetail;
        }

        private void txtkodeperkiran_Enter(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.TextEdit textEditor = (DevExpress.XtraEditors.TextEdit)sender;
            if (!string.IsNullOrEmpty(textEditor.Text))
            {
                string kode = textEditor.Text;
                var kodeacct = ListCoaAktif.Where(t => t.KODE == kode).FirstOrDefault();
                if (kodeacct != null)
                {
                    // Set the column value in the GridView
                    //JDgridView.SetRowCellValue(JDgridView.FocusedRowHandle, "Perkiraan", "ccxxx");
                    JDgridView.SetFocusedRowCellValue("Perkiraan", kodeacct.PERKIRAAN);
                    //XtraMessageBox.Show("" + kodeacct.KODE + " " + kodeacct.PERKIRAAN, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    XtraMessageBox.Show("Show Form", "Info", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private void txtkodeperkiran_EditValueChanged(object sender, EventArgs e)
        {
            
        }

        private void Load_PeriodeList(string piddata, string ptahun)
        {
            var periodelist = JurnalServices.PeriodeList(piddata, ptahun);

            cmbperiode.DataSource = periodelist;
            cmbperiode.ValueMember = "PERIODE";
            cmbperiode.DisplayMember = "PERIODE";
        }


        int x = 0;
        private void JDgridView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            try
            {
                // belumsimpan = true;
                var n = x++;
                var ket = JDgridView.GetRowCellValue(n, JDgridView.Columns[5]);

                GridView view = sender as GridView;
                view.SetRowCellValue(e.RowHandle, view.Columns["Kode"], "");
                view.SetRowCellValue(e.RowHandle, view.Columns["Rekening"], "");
                view.SetRowCellValue(e.RowHandle, view.Columns["Debet"], 0);
                view.SetRowCellValue(e.RowHandle, view.Columns["Kredit"], 0);
                view.SetRowCellValue(e.RowHandle, view.Columns["Keterangan"], ket);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SBSimpan_Click(object sender, EventArgs e)
        {
            bool akses = LevelAksesServices.Simpan(16, LoginInfo.userID);
            if (akses == false)
            {
                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                this.Player.Play();
                XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.INIT, cmbperiode.Text);
                if (Acct.KunciPeriode == "Y")
                {
                    XtraMessageBox.Show("Transaksi tidak dapat disimpan diperiode ini...!!!\nPeriode Akuntansi : " + cmbperiode.Text + " Telah Dikunci.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                P_IDDATA = CompanyInfo.INIT;
                P_NOMOR = NoJurnaltxt.Text;
                P_PERIODE = cmbperiode.Text;
                p_nomorHID = P_IDDATA + P_PERIODE + P_NOMOR + Convert.ToDateTime(deJurnal.Text).ToString("yyMMdd");

                if (string.IsNullOrEmpty(P_NOMOR))
                {

                    XtraMessageBox.Show("Nomor Jurnal tidak boleh kosong", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    NoJurnaltxt.Focus();
                    return;
                }
                if (P_NOMOR.Length>30)
                {

                    XtraMessageBox.Show("Nomor Jurnal MAX 30 Karakter,panjang karakter sekarang : "+ P_NOMOR.Length.ToString(), "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    NoJurnaltxt.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(deJurnal.Text))
                {

                    XtraMessageBox.Show("Tanggal tidak boleh kosong", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    deJurnal.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(cmbperiode.Text))
                {

                    XtraMessageBox.Show("Periode tidak boleh kosong", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmbperiode.Focus();
                    return;
                }

                var cekdetail = InputJurnalDetail.ToList();
                if (cekdetail.Count > 1)
                {
                    var kodekosong = cekdetail.Where(x => string.IsNullOrEmpty(x.Kode) && (x.Debet > 0 || x.Kredit > 0));
                    if (kodekosong.Any())
                    {
                        XtraMessageBox.Show("Baris \n" + string.Join(Environment.NewLine, kodekosong.Select(x => x.BARIS)) + "\nBelum ada Kode", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        JDgridView.Focus();
                        return;
                    }
                    var ketmax200 = cekdetail.Where(x => !string.IsNullOrEmpty(x.Kode) && x.Keterangan.Length>200);
                    if (ketmax200.Any())
                    {
                        XtraMessageBox.Show(string.Join(Environment.NewLine, ketmax200.Select(x => new { x.BARIS,x.Keterangan.Length}).ToList()) + "\nKeterangan Lebih dari 200 Karakter", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        JDgridView.Focus();
                        return;
                    }
                }


                var Debet = Convert.ToDecimal(JDgridView.Columns["Debet"].SummaryItem.SummaryValue.ToString());
                var Kredit = Convert.ToDecimal(JDgridView.Columns["Kredit"].SummaryItem.SummaryValue.ToString());
                if (Debet == 0 && Kredit == 0)
                {
                    XtraMessageBox.Show("Nilai Transaksi Jurnal Belum diisi", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                decimal selisih;
                if (Debet > Kredit)
                {
                    selisih = Debet - Kredit;
                }
                else
                {
                    selisih = Kredit - Debet;
                }

                if (Debet != Kredit)
                {
                    XtraMessageBox.Show($"Jurnal tidak seimbang,\nselisih {selisih.ToString("N2", CultureInfo.InvariantCulture)}", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //SIMPAN
                if (editjurnal)
                {
                    //jika mode edit true, hapus data lama dan simpan data baru

                    _jurnalController.HapusJurnal(old_JurnalID);
                    SimpanJurnal();
                    if (dariModule_AIS == "AIS")
                    {
                        //UpdatenNomorJurnal_AIS_EDITED(Nomor, Periode);
                    }
                    editjurnal = false;
                }
                else
                {
                    //jurnal baru langsung simpan 

                    if (!NoJurnaltxt.Text.Contains("/ND") && !NoJurnaltxt.Text.Contains("/NK"))
                    {
                        bool nomorexist = JurnalServices.CekNoJurnalExist_input(CompanyInfo.INIT, NoJurnaltxt.Text.ToUpper(), cmbperiode.Text);
                        if (nomorexist)
                        {
                            XtraMessageBox.Show("Nomor Jurnal : " + NoJurnaltxt.Text + " Sudah ada...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            NoJurnaltxt.Focus();
                            return;
                        }
                        SimpanJurnal();
                    }
                    else
                    {
                        SimpanJurnal();
                    }
                }
                //proses rekalkulasi dipindahkan langsung pada saat proses simpan
                //AccountServices.RekalkulasiByJurnalID(P_IDDATA, int.Parse(P_PERIODE.Substring(0, 2)), int.Parse(P_PERIODE[^4..]), new_jurnalid, P_PERIODE, LoginInfo.userID);
                PilihanPeriodeAkuntansi();
                bersih();
                if (addAISJurnal)
                {
                    UpdateStatusJurnal_AIS_ADD(P_NOMOR, P_PERIODE, noAIS_Bukti);
                    addAISJurnal = false;
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Simpan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatenNomorJurnal_AIS_EDITED(string p_NOMOR, string P_PERIODE)
        {
            using OracleCommand _command = new("UPDATE BKM_JURNAL@DATABASE_LINK SET NOJURNAL=:p_NOMOR,PERIODE_ACCT=:P_PERIODE,JURNALSTATUS='Y',JURNAL_AT=SYSDATE WHERE NOMOR=:noAIS_Bukti", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
                _command.Parameters.Add(":p_NOMOR", OracleDbType.Varchar2, 30).Value = p_NOMOR;
                _command.Parameters.Add(":P_PERIODE", OracleDbType.Varchar2, 7).Value = P_PERIODE;
                _command.Parameters.Add(":noAIS_Bukti", OracleDbType.Varchar2, 50).Value = noAIS_Bukti;
                _command.ExecuteNonQuery();
                conn.Close();

            }
        }

        private void UpdateStatusJurnal_AIS_ADD(string p_NOMOR, string P_PERIODE, string noAIS_Bukti)
        {
            using OracleCommand _command = new("UPDATE BKM_JURNAL@DATABASE_LINK SET NOJURNAL=:p_NOMOR,PERIODE_ACCT=:P_PERIODE,JURNALSTATUS='Y',JURNAL_AT=SYSDATE WHERE NOMOR=:noAIS_Bukti", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
                _command.Parameters.Add(":p_NOMOR", OracleDbType.Varchar2, 30).Value = p_NOMOR;
                _command.Parameters.Add(":P_PERIODE", OracleDbType.Varchar2, 7).Value = P_PERIODE;
                _command.Parameters.Add(":noAIS_Bukti", OracleDbType.Varchar2, 50).Value = noAIS_Bukti;
                _command.ExecuteNonQuery();
                conn.Close();

            }
        }

        bool fromModule_AIS = false;
        bool fromModule_Kasir = false;
        bool fromModule_Inv = false;

        private void SimpanJurnal()
        {
            try
            {
                string hostName = System.Net.Dns.GetHostName();
                string IP46 = ToolsServices.GetLocalIPAddress();

                //insert new jurnal header

                var P_SUMBER = "JV";
                if (fromModule_AIS)
                {

                    P_SUMBER = "AIS";
                    fromModule_AIS = false;
                }
                if (fromModule_Kasir)
                {

                    P_SUMBER = "KASIR";
                    fromModule_Kasir = false;
                }
                if (fromModule_Inv)
                {

                    P_SUMBER = "INV";
                    fromModule_Inv = false;
                }
                var isre = "T";
                if (jurnalbalik.Checked == true)
                {
                    isre = "Y";
                }
                

                JurnalHeaderAdd AddJurnalHeader = new()
                {
                    HID = p_nomorHID,
                    NOJURNAL = P_NOMOR,
                    TANGGAL = Convert.ToDateTime(deJurnal.EditValue),
                    PERIODE = P_PERIODE,
                    IDDATA = P_IDDATA,
                    USERID = LoginInfo.userID,
                    SUMBER = P_SUMBER,
                    ISRE = isre,
                    PC=hostName,
                    IP_ADD= IP46                    
                };                

                var jurnalDetailAdd = InputJurnalDetail.Where(i=>!string.IsNullOrEmpty(i.Kode)).ToList();
               
                // Call the controller method to insert jurnal data
                _jurnalController.InsertJurnal(AddJurnalHeader, jurnalDetailAdd);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    XtraMessageBox.Show("Duplikasi Data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error on Simpan Jurnal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void PilihanPeriodeAkuntansi()
        {
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);

                var Periode_daftar_Jurnal = cmballperiode.Text;
                if (!string.IsNullOrEmpty(defiltertanggal.Text) || Int32.Parse(txtfilterjumlah.Text) > 0 || !string.IsNullOrEmpty(txtfilterkode.Text) || !string.IsNullOrEmpty(txtfilternojurnal.Text) || !string.IsNullOrEmpty(txtfilterketerangan.Text))
                {
                    JurnalDetail = JurnalServices.GetJurnalDetails_Dapper(CompanyInfo.INIT, Periode_daftar_Jurnal);
                    CariJurnal_Bulan();
                    GCHeader.Focus();
                }
                else
                {
                    //Stopwatch watch = new Stopwatch();
                    //watch.Start();                  

                    JurnalHeader = JurnalServices.GetJurnalHeader_Dapper(CompanyInfo.INIT, Periode_daftar_Jurnal);
                    JurnalDetail = JurnalServices.GetJurnalDetails_Dapper(CompanyInfo.INIT, Periode_daftar_Jurnal);

                    GCHeader.DataSource = JurnalHeader;
                    GCDetails.DataSource = null;
                    //watch.Stop();
                    //XtraMessageBox.Show(watch.ElapsedMilliseconds.ToString());

                    GVHeader.Columns["JURNALID"].Visible = false;
                    GVHeader.Columns["HID"].Visible = false;
                    //GVHeader.Columns["Tanggal"].OptionsColumn.FixedWidth = true;
                    //GVHeader.Columns["Tanggal"].Width = 90;
                    GVHeader.Columns["Tanggal"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    GVHeader.Columns["Tanggal"].DisplayFormat.FormatString = "dd-MMM-yyyy";
                    GVHeader.BestFitColumns();
                }

                GVHeader.Focus();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error Pilihan Periode", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void GVHeader_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            try
            {
                GridView view = sender as GridView;
                if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
                {
                    int rowHandle = e.HitInfo.RowHandle;
                    //hapus menu jika ada
                    e.Menu.Items.Clear();

                    DXMenuItem hapus = CreateMenuItemHapus(view, rowHandle);
                    DXMenuItem exportselected = CreateMenuExportSelected(view, rowHandle);


                    hapus.BeginGroup = true;
                    exportselected.BeginGroup = true;

                    e.Menu.Items.Add(hapus);
                    e.Menu.Items.Add(exportselected);
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Popup Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DXMenuItem CreateMenuExportSelected(DevExpress.XtraGrid.Views.Grid.GridView view, int rowHandle)
        {
            DXMenuItem checkItem = new("Export Terpilih", new EventHandler(OnExportClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }

        private void OnExportClick(object? sender, EventArgs e)
        {
            List<double> selectedValues = new();

            // Iterate over the selected rows
            for (int i = 0; i < GVHeader.SelectedRowsCount; i++)
            {
                // Get the selected row handle
                int rowHandle = GVHeader.GetSelectedRows()[i];

                // Get the value from a specific column (replace "ColumnName" with the actual column name)
                double value = Convert.ToDouble(GVHeader.GetRowCellValue(rowHandle, "JURNALID").ToString());

                // Add the value to the list
                selectedValues.Add(value);
            }
            // Create a formatted string containing the selected values
            // string message = "Selected Values:\n\n" + string.Join("\n", selectedValues);

            if (selectedValues.Any())
            {
                ExportJurnalDipilih(selectedValues);
            }
            // Display the MessageBox with the selected values
           // XtraMessageBox.Show(message, "Selected Values", MessageBoxButtons.OK, MessageBoxIcon.Information);
            

        }

        private void ExportJurnalDipilih(List<double> selectedValues)
        {
            // If you use EPPlus in a noncommercial context
            // according to the Polyform Noncommercial license:
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            string filename = string.Empty;
            using (ExcelPackage package = new())
            {
                List<JurnalDetailDTO> selectedJurnalItems = JurnalDetail.Where(j => selectedValues.Contains(j.REFFID)).ToList();
                var wsDt = package.Workbook.Worksheets.Add("Jurnal Entries");

                //Load the datatable and set the number formats...
                wsDt.Cells["A1"].LoadFromCollection(selectedJurnalItems, true);

                //wsDt.Cells["A1"].LoadFromCollection(mydata);
                wsDt.DeleteColumn(1, 2);

                //Add the headers
                wsDt.Cells[1, 1].Value = "NoJurnal";
                wsDt.Cells[1, 2].Value = "Tanggal";
                wsDt.Cells[1, 3].Value = "RowNo";
                wsDt.Cells[1, 4].Value = "Kode";
                wsDt.Cells[1, 5].Value = "Rekening";
                wsDt.Cells[1, 6].Value = "Debet";
                wsDt.Cells[1, 7].Value = "Kredit";
                wsDt.Cells[1, 8].Value = "Keterangan";
                wsDt.Cells[1, 9].Value = "Posted";
                wsDt.Cells[1, 10].Value = "Periode";

                wsDt.Cells[2, 2, selectedJurnalItems.Count + 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                //=IF(A2<>A1,1,C1+1)
                wsDt.Cells[2, 3, selectedJurnalItems.Count + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, selectedJurnalItems.Count + 1, 3).Address);

                // number formats
                string positiveFormat = "#,##0.00_)";
                string negativeFormat = "(#,##0.00)";
                string zeroFormat = "-_)";
                string numberFormat = positiveFormat + ";" + negativeFormat;
                string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                wsDt.Cells[2, 6, selectedJurnalItems.Count + 1, 7].Style.Numberformat.Format = fullNumberFormat;
                //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                // 
                wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                //package.Save();
                // package.Dispose();

                // Obtain the Excel file data as a byte array
                byte[] excelData = package.GetAsByteArray();

                // Generate a temporary file path
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

                // Write the byte array to the temporary file
                File.WriteAllBytes(tempFilePath, excelData);

                // Open the temporary file with the default associated Excel program
                ProcessStartInfo psi = new(tempFilePath)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
        }

        private void Ubah_jurnal()
        {
            try
            {
                //4 kode daftar jurnal
                bool akses = LevelAksesServices.Ubah(16, LoginInfo.userID);
                if (akses == false)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                var Periode_daftar_Jurnal = cmballperiode.Text;
                Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.INIT, Periode_daftar_Jurnal);
                if (Acct.KunciPeriode == "Y")
                {
                    XtraMessageBox.Show("Transaksi ini tidak dapat diubah...!!!\nPeriode Akuntansi : " + Periode_daftar_Jurnal + " Telah Dikunci.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                editjurnal = true;
                var periodelist = JurnalServices.PeriodeList(CompanyInfo.INIT, cmballperiode.Text[^4..]);

                cmbperiode.DataSource = periodelist;
                cmbperiode.ValueMember = "PERIODE";
                cmbperiode.DisplayMember = "PERIODE";
                if (this.GVHeader.GetFocusedRowCellValue("JURNALID") == null)
                    return;
                var rowhandle = GVHeader.FocusedRowHandle;
                old_JurnalID = Convert.ToDouble(GVHeader.GetRowCellValue(rowhandle, "JURNALID"));
                p_nomorHID = GVHeader.GetRowCellValue(rowhandle, "HID").ToString();
                var NomorJurnal = GVHeader.GetRowCellValue(rowhandle, "NoJurnal").ToString();
                var TanggalJurnal = Convert.ToDateTime(GVHeader.GetRowCellValue(rowhandle, "Tanggal"));
                //if (NomorJurnal == "001/CLOSE" || NomorJurnal == "001/LABA")
                //{
                //    XtraMessageBox.Show("Jurnal Closing tidak dapat diubah... manual!!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}

                //header
                NoJurnaltxt.Text = NomorJurnal;
                deJurnal.EditValue = TanggalJurnal;
                cmbperiode.SelectedValue = cmballperiode.Text;
                List<JurnalDetailAdd> query = JurnalDetail.Where(d => d.REFFID == old_JurnalID).Select(n =>
                                new JurnalDetailAdd { BARIS = n.BARIS, Kode = n.Kode, Rekening = n.Rekening, Debet = n.Debet, Kredit = n.Kredit, Keterangan = n.Keterangan }).ToList();
                InputJurnalDetail = new BindingList<JurnalDetailAdd>(query);

                this.GCJurnal.DataSource = InputJurnalDetail;
                InputJurnalDetail.AllowNew = true;
                // this.gridControl1.DataSource = InputJurnalDetail;
                //CEK BARANGKALI NOJURNAL DIUBAH, UBAH JUGA PADA MODULE IMPORT DI AIS
                dariModule_AIS = JurnalFromModuleServices.CekSumber_Jurnal(old_JurnalID);               
                

                // cmbperiode.SelectedValue = dperiode;
                TABJurnal.SelectedTabPage = xtraTabPage1;
                //x = JDgridView.RowCount;
                x = InputJurnalDetail.Count()-1;
                var jre = JurnalServices.CekjURNALRJE(old_JurnalID);
                if (jre)
                {
                    jurnalbalik.Checked= true;
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on ubah jurnal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DXMenuItem CreateMenuItemHapus(DevExpress.XtraGrid.Views.Grid.GridView view, int rowHandle)
        {
            DXMenuItem checkItem = new("Hapus Terpilih", new EventHandler(OnHapusClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[0];
            return checkItem;
        }

        private void OnHapusClick(object sender, EventArgs e)
        {
            Hapus_Jurnal_Selected();
        }

        private void Hapus_Jurnal_Selected()
        {
            try
            {
                //4 kode daftar jurnal
                bool akses = LevelAksesServices.Hapus(16, LoginInfo.userID);
                if (akses == false)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                var Periode = cmballperiode.Text;

                Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.INIT, Periode);
                if (Acct.KunciPeriode == "Y")
                {
                    XtraMessageBox.Show("Transaksi ini tidak dapat diHapus...!!!\nPeriode Akuntansi : " + Periode + " Telah Dikunci.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                List<string> selectedNoJurnal = new();
                List<double> selectedValues = new();

                // Iterate over the selected rows
                for (int i = 0; i < GVHeader.SelectedRowsCount; i++)
                {
                    // Get the selected row handle
                    int rowHandle = GVHeader.GetSelectedRows()[i];

                    // Get the value from a specific column (replace "ColumnName" with the actual column name)
                    string value = GVHeader.GetRowCellValue(rowHandle, "NoJurnal").ToString();
                    double value1 = Convert.ToDouble(GVHeader.GetRowCellValue(rowHandle, "JURNALID").ToString());
                    

                    // Add the value to the list
                    selectedNoJurnal.Add(value);
                    selectedValues.Add(value1);
                }
                // Create a formatted string containing the selected values
                 string NoJurnal = string.Join("\n", selectedNoJurnal);

                if (selectedNoJurnal.Any())
                {
                    if (XtraMessageBox.Show("Hapus Transaksi Jurnal ? \n\nNomor : \n" + NoJurnal+ "\n", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    _jurnalController.HapusJurnalRange(selectedValues);
                    PilihanPeriodeAkuntansi();
                    XtraMessageBox.Show("Jurnal telah dihapus", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on hapus jurnal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        

        string dariModule_AIS = string.Empty;
        private void Hapus_jurnal()
        {
            try
            {
                //4 kode daftar jurnal
                bool akses = LevelAksesServices.Hapus(16, LoginInfo.userID);
                if (akses == false)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\maaf_noakses.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (this.GVHeader.GetFocusedRowCellValue("HID") == null)
                    return;
                var Periode = cmballperiode.Text;
                var rowhandle = GVHeader.FocusedRowHandle;
                var Nomor = GVHeader.GetRowCellValue(rowhandle, "NoJurnal").ToString();
                old_JurnalID = Convert.ToDouble(GVHeader.GetRowCellValue(rowhandle, "JURNALID"));

               
                Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.INIT, Periode);
                if (Acct.KunciPeriode == "Y")
                {
                    XtraMessageBox.Show("Transaksi ini tidak dapat diHapus...!!!\nPeriode Akuntansi : " + Periode + " Telah Dikunci.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                //if (Nomor == "001/CLOSE" || Nomor == "001/LABA")
                //{
                //    XtraMessageBox.Show("Jurnal Closing tidak dapat dihapus... manual!!!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    return;
                //}
                if (XtraMessageBox.Show("Hapus Transaksi Jurnal ? \n\nNomor : " + Nomor + "\nPeriode : " + Periode, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                dariModule_AIS = JurnalFromModuleServices.CekSumber_Jurnal(old_JurnalID);
                _jurnalController.HapusJurnal(old_JurnalID);
                if (dariModule_AIS=="AIS")
                {
                    UpdateStatusJurnal_AIS_DELETED(Nomor, Periode);
                }
                PilihanPeriodeAkuntansi();
                XtraMessageBox.Show("Jurnal telah dihapus", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on hapus jurnal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatusJurnal_AIS_DELETED(string p_NOMOR, string P_PERIODE)
        {
            using (OracleCommand _command = new("UPDATE BKM_JURNAL@DATABASE_LINK SET JURNALSTATUS='T' WHERE NOJURNAL=:p_NOMOR AND PERIODE_ACCT=:P_PERIODE", conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                    _command.Parameters.Add(":p_NOMOR", OracleDbType.Varchar2, 30).Value = p_NOMOR;
                    _command.Parameters.Add(":P_PERIODE", OracleDbType.Varchar2, 7).Value = P_PERIODE;
                    _command.ExecuteNonQuery();
                    conn.Close();

                }
            }
        }
        private void NoJurnaltxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                deJurnal.Select();
            }
        }


        private void cmbperiode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }
        private void FrmJurnal_KeyDown(object sender, KeyEventArgs e)
        {
            if (JDgridView.PostEditor())
                JDgridView.UpdateCurrentRow();

            if (e.KeyCode == Keys.F1)
            {
                TABJurnal.SelectedTabPage = xtraTabPage1;
                NoJurnaltxt.Select();
            }
            else if(e.KeyCode == Keys.F8)
            {
                TABJurnal.SelectedTabPage = xtraTabPage2;
            }
            else if (e.KeyCode == Keys.F9)
            {
                TABJurnal.SelectedTabPage = xtraTabPage3;
                txtcarinomor.Select();
            }

            if (TABJurnal.SelectedTabPage == xtraTabPage1)
            {
                    if (e.KeyCode == Keys.F3)
                    {
                    if (JDgridView.PostEditor())
                        JDgridView.UpdateCurrentRow();
                    NoJurnaltxt.Focus();
                    }                   
                    if (e.KeyCode == Keys.F4)
                    {
                        SBSimpan.PerformClick();
                    }
                    else if (e.KeyCode == Keys.F5)
                    {
                        sbbatal.PerformClick();
                    }
            }

            if(TABJurnal.SelectedTabPage == xtraTabPage2)
            {
                if (e.KeyCode == Keys.F6)
                {
                    sbubah.PerformClick();
                }
                else if (e.KeyCode == Keys.F7)
                {
                    sbhapus.PerformClick();
                }
            }

            
        }

        private void sbubah_Click(object sender, EventArgs e)
        {
            Ubah_jurnal();
        }

        private void sbhapus_Click(object sender, EventArgs e)
        {
            Hapus_jurnal();
        }


        private void sbbatal_Click(object sender, EventArgs e)
        {
            if (XtraMessageBox.Show("Batalkan Transaksi Jurnal ? ", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            bersih();           

        }
        private void bersih()
        {
            //SetTanggalharini();
            PopulateList();
            NoJurnaltxt.Text = "";

            editjurnal = false;
            //belumsimpan = false;
            x = 0;
            SBSimpan.Enabled = true;
            jurnalbalik.Checked = false;
            NoJurnaltxt.Select();
        }

        private void JDgridView_KeyDown(object sender, KeyEventArgs e)
        {
            GridView view = sender as GridView;

            if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.Control)
            {
                if (MessageBox.Show("Hapus Baris?", "Konfirmasi", MessageBoxButtons.YesNo) !=
                  DialogResult.Yes)
                    return;
                GridView view2 = sender as GridView;
                view2.DeleteRow(view2.FocusedRowHandle);
                x--;
            }           
            else if (e.Control && e.KeyCode == Keys.C)
            {
                if (view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn) != null && view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString() != String.Empty)
                    Clipboard.SetText(view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString());
                else
                    MessageBox.Show("The value in the selected cell is null or empty!");
                e.Handled = true;
            }
        }

        private void JDgridView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            GridView gridView = (GridView)sender;
            if (gridView == null) return;
            if (e.Column.Caption != "DEBET") return;
            string cellValue = e.Value + "0" + gridView.GetRowCellValue(e.RowHandle, gridView.Columns["KREDIT"]);
            gridView.SetRowCellValue(e.RowHandle, gridView.Columns["DEBET"], cellValue);

            //if (e.Column.Caption != "KODE") return;

            //// Retrieve the cell value and perform a lookup
            //string kodeakun = e.Value.ToString();
            //string selectedValue = ListCoaAktif.Where(x => x.KODE == kodeakun).Select(x => x.PERKIRAAN).FirstOrDefault();

            //gridView.SetRowCellValue(e.RowHandle, gridView.Columns["PERKIRAAN"], selectedValue);
           
        }

        private string GetNamaPerkiraan(string? changedValue)
        {
            string namakun=ListCoaAktif.Where(x=>x.KODE== changedValue).Select(x => x.PERKIRAAN).FirstOrDefault();
            return namakun;
        }

        private void repdebet_EditValueChanged(object sender, EventArgs e)
        {
            JDgridView.SetRowCellValue(JDgridView.FocusedRowHandle, "Kredit", 0);
            HitungSelisih();

        }

        private void HitungSelisih()
        {
            var Debet = Convert.ToDecimal(JDgridView.Columns["Debet"].SummaryItem.SummaryValue.ToString());
            var Kredit = Convert.ToDecimal(JDgridView.Columns["Kredit"].SummaryItem.SummaryValue.ToString());

            decimal selisih;
            if (Debet > Kredit)
            {
                selisih = Debet - Kredit;
            }
            else
            {
                selisih = Kredit - Debet;
            }
            //txtselisih.Text = selisih.ToString();
        }

        private void repkredit_EditValueChanged(object sender, EventArgs e)
        {
            JDgridView.SetRowCellValue(JDgridView.FocusedRowHandle, "Debet", 0);
            HitungSelisih();
        }


        private void PopulatePeriode(DataTable dt2)
        {
            dt2 = JurnalServices.PeriodeList(CompanyInfo.INIT, DateTime.Today.Year.ToString());

            cmbperiode.DataSource = dt2;
            cmbperiode.ValueMember = "PERIODE";
            cmbperiode.DisplayMember = "PERIODE";
        }

        private void JDgridView_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName == "Debet")
                if (Convert.ToDecimal(e.Value) == 0) e.DisplayText = "0";
            if (e.Column.FieldName == "Kredit")
                if (Convert.ToDecimal(e.Value) == 0) e.DisplayText = "0";
        }
        private void simpleButton4_Click(object sender, EventArgs e)
        {
            try
            {                

                if (cmballperiode.SelectedIndex != -1)
                {

                    cmballperiode.SelectedIndex--;
                   GCHeader.Focus();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            try
            {
               

                if (cmballperiode.SelectedIndex != -1)
                {
                    cmballperiode.SelectedIndex++;
                    GCHeader.Focus();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("SelectedIndex"))
                {
                    XtraMessageBox.Show("Periode terakhir", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }                    
               
            }

        }


        private void GCHeader_Click(object sender, EventArgs e)
        {
            if (this.GVHeader.GetFocusedRowCellValue("NoJurnal") == null)
                return;
            FilterNomorJurnal();
        }

        private void FilterNomorJurnal()
        {

            try
            {
                var filter = Convert.ToDouble(GVHeader.GetRowCellValue(GVHeader.FocusedRowHandle, "JURNALID").ToString());
                var filtered = JurnalDetail.Where(x => x.REFFID == filter).ToList();
                GCDetails.DataSource = filtered;
                GVDetail.Columns[0].Visible = false;
                GVDetail.Columns[1].Visible = false;
                GVDetail.Columns[2].Visible = false;
                GVDetail.Columns[3].OptionsColumn.FixedWidth = true;
                GVDetail.Columns[3].Width = 40;
                GVDetail.Columns[4].OptionsColumn.FixedWidth = true;
                GVDetail.Columns[4].Width = 120;
                GVDetail.Columns[5].OptionsColumn.FixedWidth = true;
                GVDetail.Columns[5].Width = 300;
                GVDetail.Columns[6].OptionsColumn.FixedWidth = true;
                GVDetail.Columns[6].Width = 170;
                GVDetail.Columns[7].OptionsColumn.FixedWidth = true;
                GVDetail.Columns[7].Width = 170;
                GVDetail.Columns[6].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                GVDetail.Columns[6].DisplayFormat.FormatString = "n2";
                GVDetail.Columns[7].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                GVDetail.Columns[7].DisplayFormat.FormatString = "n2";
                GVDetail.Columns[6].Summary.Clear();
                GVDetail.Columns[7].Summary.Clear();
                GVDetail.Columns[6].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "Debet", "{0:N2}");
                GVDetail.Columns[7].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "Kredit", "{0:N2}");
                GVDetail.Columns[9].Visible = false;
                GVDetail.Columns[10].Visible = false;
                GVDetail.BestFitColumns();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on filter no jurnal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private void GCHeader_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.GVHeader.GetFocusedRowCellValue("NoJurnal") == null)
                return;
            FilterNomorJurnal();
        }






        private void GVHeader_GotFocus(object sender, EventArgs e)
        {
            if (this.GVHeader.GetFocusedRowCellValue("NoJurnal") == null)
                return;
            FilterNomorJurnal();
        }

        private void txtfilterjurnal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
            else if (e.KeyCode == Keys.Down)
            {
                GCHeader.Focus();
            }
        }



        private void txtfilterket_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
            else if (e.KeyCode == Keys.Down)
            {
                GCHeader.Focus();
            }
        }

        GridHitInfo downHitInfo = null;
        private void JDgridView_MouseDown(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            downHitInfo = null;

            GridHitInfo hitInfo = view.CalcHitInfo(new Point(e.X, e.Y));
            if (Control.ModifierKeys != Keys.None)
                return;
            if (e.Button == MouseButtons.Left && hitInfo.InRow && hitInfo.RowHandle != GridControl.NewItemRowHandle)
                downHitInfo = hitInfo;
        }

        private void JDgridView_MouseMove(object sender, MouseEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.Button == MouseButtons.Left && downHitInfo != null)
            {
                Size dragSize = SystemInformation.DragSize;
                Rectangle dragRect = new Rectangle(new Point(downHitInfo.HitPoint.X - dragSize.Width / 2,
                    downHitInfo.HitPoint.Y - dragSize.Height / 2), dragSize);

                if (!dragRect.Contains(new Point(e.X, e.Y)))
                {
                    view.GridControl.DoDragDrop(downHitInfo, DragDropEffects.All);
                    downHitInfo = null;
                }
            }
        }

        private void GCJurnal_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GridHitInfo)))
            {
                GridHitInfo downHitInfo = e.Data.GetData(typeof(GridHitInfo)) as GridHitInfo;
                if (downHitInfo == null)
                    return;

                GridControl grid = sender as GridControl;
                GridView view = grid.MainView as GridView;
                GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
                if (hitInfo.InRow && hitInfo.RowHandle != downHitInfo.RowHandle && hitInfo.RowHandle != GridControl.NewItemRowHandle)
                    e.Effect = DragDropEffects.Move;
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        private void GCJurnal_DragDrop(object sender, DragEventArgs e)
        {
            GridControl grid = sender as GridControl;
            GridView view = grid.MainView as GridView;
            GridHitInfo srcHitInfo = e.Data.GetData(typeof(GridHitInfo)) as GridHitInfo;
            GridHitInfo hitInfo = view.CalcHitInfo(grid.PointToClient(new Point(e.X, e.Y)));
            int sourceRow = srcHitInfo.RowHandle;
            int targetRow = hitInfo.RowHandle;
            MoveRow(sourceRow, targetRow);
        }
        private void MoveRow(int sourceRow, int targetRow)
        {
            if (sourceRow == targetRow || sourceRow == targetRow + 1)
                return;

            GridView view = JDgridView;
            DataRow row1 = view.GetDataRow(targetRow);
            DataRow row2 = view.GetDataRow(targetRow + 1);
            DataRow dragRow = view.GetDataRow(sourceRow);
            int val1 = (int)row1["OrderFieldName"];
            if (row2 == null)
                dragRow["OrderFieldName"] = val1 + 1;
            else
            {
                int val2 = (int)row2["OrderFieldName"];
                dragRow["OrderFieldName"] = (val1 + val2) / 2;
            }
        }
        private void allperiode()
        {
            var prd = PeriodeListAll(p_iddata);
            cmballperiode.DataSource = prd;
            cmballperiode.ValueMember = "PERIODE";
            cmballperiode.DisplayMember = "PERIODE";
        }
        private void FrmJurnal_Load(object sender, EventArgs e)
        {
           
            //try
            //{
                p_iddata = CompanyInfo.INIT;
                allperiode();

                var bl = DateTime.Today.Month.ToString("0#") + "/" + DateTime.Today.Year.ToString();
                cmballperiode.SelectedValue = bl;

               
                cmbbulan.Properties.Items.AddRange(Bulan);
                cmbbulan2.Properties.Items.AddRange(Bulan);
               
                var bulan = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 2, 2));

                cmbbulan.SelectedIndex = bulan - 1;
                cmbbulan2.SelectedIndex = bulan - 1;
                daritahun.Properties.MinValue = Acct.TahunMin;
                daritahun.Properties.MaxValue = Acct.TahunMax;
                sampaitahun.Properties.MinValue = Acct.TahunMin;
                sampaitahun.Properties.MaxValue = Acct.TahunMax;
                daritahun.Value = Acct.TahunMax;
                sampaitahun.Value = Acct.TahunMax;

                cmbbulankasir.Properties.Items.AddRange(Bulan);
                cmbbulankasir.SelectedIndex = bulan - 1;
                setahunkasir.Properties.MinValue = Acct.TahunMin;
                setahunkasir.Properties.MaxValue = Acct.TahunMax;
                setahunkasir.Value = Acct.TahunMax;

                cmbbulanAIS.Properties.Items.AddRange(Bulan);
                cmbbulanAIS.SelectedIndex = bulan - 1;
                setahunAIS.Properties.MinValue = Acct.TahunMin;
                setahunAIS.Properties.MaxValue = Acct.TahunMax;
                setahunAIS.Value = Acct.TahunMax;


                CMBBULANINV.Properties.Items.AddRange(Bulan);
                CMBBULANINV.SelectedIndex = bulan - 1;
                SETAHUNINV.Properties.MinValue = Acct.TahunMin;
                SETAHUNINV.Properties.MaxValue = Acct.TahunMax;
                SETAHUNINV.Value = Acct.TahunMax;

                NoJurnaltxt.Select();
                if (Acct.TahunMax == 0)
                {
                    XtraMessageBox.Show("Daftar perkiraan belum ada", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    GCJurnal.Enabled = false;
                    return;
                }
                Load_COA_Dapper();
                DataTable dt2 = new ();
                PopulatePeriode(dt2);

                //Load_PeriodeList(CompanyInfo.INIT, DateTime.Today.Year.ToString());
                periodetujuan = pbulan.ToString("00") + "/" + ptahun;
                cmbperiode.Text = periodetujuan;

                PilihanPeriodeAkuntansi();
                lblrecordbulan.Visible = false;
                SetTanggalharini();

                //load jurnal from other
                string serverip = Acct.OracleConnString.Substring(54, 11);
                if(serverip != "10.10.10.41")
                {
                    Load_Kode_Kasir();
                    Load_Kode_AIS();
                    Load_Kode_Inv();
                }
           

            //}
            //catch (SystemException ex)
            //{

            //    XtraMessageBox.Show(ex.Message, "Error on first load", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

        }

        private void Load_COA_Dapper()
        {
            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            Acct.TahunMax = AccountServices.MaxTahunCOA(p_iddata);
            ListCoaAktif= _jurnalController.KodeUntukJurnal(p_iddata, Acct.TahunMax);    
            
            Load_COA_Style();

            //watch.Stop();
            //XtraMessageBox.Show(watch.ElapsedMilliseconds.ToString());

        }


        private void repositoryItemGridLookUpEditkode_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                ////from datatable
                //GridLookUpEdit editor = ((GridLookUpEdit)sender);
                //DataRowView row = (DataRowView)editor.Properties.GetRowByKeyValue(editor.EditValue);
                //JDgridView.SetFocusedRowCellValue("Rekening", row["PERKIRAAN"].ToString());

                // from List
                GridLookUpEdit editor = (GridLookUpEdit)sender;
                DTOCOAAktif selectedListItem = (DTOCOAAktif)editor.GetSelectedDataRow();

                if (selectedListItem != null)
                {
                    string rekeningValue = selectedListItem.PERKIRAAN.ToString();
                    JDgridView.SetFocusedRowCellValue("Rekening", rekeningValue);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void repositoryItemLookUpEditkode_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                ////from datatable
                //LookUpEdit editor = (sender as LookUpEdit);
                //DataRowView row = editor.Properties.GetDataSourceRowByKeyValue(editor.EditValue) as DataRowView;
                //JDgridView.SetFocusedRowCellValue("Rekening", row["PERKIRAAN"].ToString());
                // from List
                LookUpEdit editor = (LookUpEdit)sender;
                DTOCOAAktif selectedListItem = (DTOCOAAktif)editor.GetSelectedDataRow();

                if (selectedListItem != null)
                {
                    string rekeningValue = selectedListItem.PERKIRAAN.ToString();
                    JDgridView.SetFocusedRowCellValue("Rekening", rekeningValue);
                }

            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void SetTanggalharini()
        {
            deJurnal.EditValue = DateTime.Today;
            deJurnal.Properties.Mask.MaskType = MaskType.DateTime;
            deJurnal.Properties.Mask.EditMask = "dd/MM/yyyy";
        }

        private void repositoryItemSearchLookUpEditKODE_QueryCloseUp(object sender, CancelEventArgs e)
        {
            SendKeys.Send("{TAB}");
        }

        private void JDgridView_RowCountChanged(object sender, EventArgs e)
        {
           
            for (int i = 0; i < JDgridView.RowCount; i++)
            {
               
                JDgridView.SetRowCellValue(i, JDgridView.Columns["BARIS"], i + 1);
            }
        }
        private void CariJurnal_Bulan()
        {
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);
                var dperiode = cmballperiode.Text;


                if (!string.IsNullOrEmpty(defiltertanggal.Text) || decimal.Parse(txtfilterjumlah.Text) > 0 || !string.IsNullOrEmpty(txtfilterkode.Text) || !string.IsNullOrEmpty(txtfilternojurnal.Text) || !string.IsNullOrEmpty(txtfilterketerangan.Text))
                {
                    filter = true;
                    lblrecordbulan.Visible = true;
                    PencarianJurnal_Bulan = JurnalServices.SearchJurnal_Bulan(CompanyInfo.INIT, dperiode, txtfilternojurnal.Text.ToLower(), defiltertanggal.Text, txtfilterkode.Text.ToLower(), txtfilterketerangan.Text.ToLower(), decimal.Parse(txtfilterjumlah.Text));
                    if (PencarianJurnal_Bulan.Count() > 0)
                    {
                        JurnalHeader_Filtered = PencarianJurnal_Bulan.GroupBy(g => new { g.REFFID, g.HIDREFF, g.NoJurnal, g.Tanggal })
                                                .Select(x => new JurnalHeaderDTO { JURNALID = x.Key.REFFID, HID = x.Key.HIDREFF, NoJurnal = x.Key.NoJurnal, Tanggal = x.Key.Tanggal }).ToList();


                        GCHeader.DataSource = JurnalHeader_Filtered;
                        GVHeader.Columns["JURNALID"].Visible = false;
                        //GVHeader.Columns["Tanggal"].OptionsColumn.FixedWidth = true;
                        //GVHeader.Columns["Tanggal"].Width = 90;
                        GVHeader.Columns["Tanggal"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        GVHeader.Columns["Tanggal"].DisplayFormat.FormatString = "dd-MMM-yyyy";
                        GVHeader.BestFitColumns();
                        lblrecordbulan.Text = "Filter Record : " + PencarianJurnal_Bulan.Count().ToString();
                        GCHeader.Focus();
                    }
                    else
                    {
                        GCHeader.DataSource = null;
                        GCDetails.DataSource = null;
                        lblrecordbulan.Text = "Filter Record : 0";

                        PencarianJurnal_Bulan = Enumerable.Empty<JurnalDetailDTO>();
                        // XtraMessageBox.Show("Data tidak ditemukan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
                else
                {
                    filter = true;
                    lblrecordbulan.Visible = false;
                    lblrecordbulan.Text = "Filter Record : 0";
                    GCHeader.DataSource = JurnalHeader;
                    PencarianJurnal_Bulan = Enumerable.Empty<JurnalDetailDTO>();
                }
                GCHeader.Focus();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CariJurnal_Tahun()
        {

            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);

                if (!string.IsNullOrEmpty(decaritanggal.Text) || decimal.Parse(txtcarijumlah.Text) > 0 || !string.IsNullOrEmpty(txtcarikode.Text) || !string.IsNullOrEmpty(txtcarinomor.Text) || !string.IsNullOrEmpty(txtcariketerangan.Text))
                {
                    var daritahunbulan = Convert.ToInt32(daritahun.Value.ToString() + (cmbbulan.SelectedIndex + 1).ToString("00"));
                    var sampaitahunbulan = Convert.ToInt32(sampaitahun.Value.ToString() + (cmbbulan2.SelectedIndex + 1).ToString("00"));

                    if(daritahunbulan>sampaitahunbulan)
                    {
                        XtraMessageBox.Show("Pilihan Periode Salah", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    PencarianJurnal = JurnalServices.SearchJurnal(CompanyInfo.INIT, daritahunbulan,sampaitahunbulan, txtcarinomor.Text.ToLower(), decaritanggal.Text, txtcarikode.Text.ToLower(), txtcariketerangan.Text.ToLower(), decimal.Parse(txtcarijumlah.Text));
                    if (PencarianJurnal.Count() > 0)
                    {
                        cmbcariperiode.Properties.Items.Clear();
                        var qperiode = PencarianJurnal.Select(x => x.Periode).Distinct().OrderBy(x => x).ToList();
                        foreach (string peri in qperiode)
                            cmbcariperiode.Properties.Items.Add(peri);

                        gridControl1.DataSource = PencarianJurnal;
                        gridView1.Columns["REFFID"].Visible = false; //reffid
                        gridView1.Columns["HIDREFF"].Visible = false; //reffid
                        gridView1.Columns["Posted"].Visible = false; //reffid
                        gridView1.Columns["Tanggal"].OptionsColumn.FixedWidth = true;
                        gridView1.Columns["Tanggal"].Width = 120;
                        gridView1.Columns["Tanggal"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        gridView1.Columns["Tanggal"].DisplayFormat.FormatString = "dd-MMM-yyyy";
                        gridView1.Columns["BARIS"].OptionsColumn.FixedWidth = true;
                        gridView1.Columns["BARIS"].Width = 50;
                        gridView1.Columns["Kode"].OptionsColumn.FixedWidth = true;
                        gridView1.Columns["Kode"].Width = 100;
                        gridView1.Columns["Rekening"].OptionsColumn.FixedWidth = true;
                        gridView1.Columns["Rekening"].Width = 250;
                        gridView1.Columns["Debet"].OptionsColumn.FixedWidth = true;
                        gridView1.Columns["Debet"].Width = 100;
                        gridView1.Columns["Kredit"].OptionsColumn.FixedWidth = true;
                        gridView1.Columns["Kredit"].Width = 100;
                        gridView1.Columns["Debet"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gridView1.Columns["Debet"].DisplayFormat.FormatString = "n2";
                        gridView1.Columns["Kredit"].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        gridView1.Columns["Kredit"].DisplayFormat.FormatString = "n2";

                        gridView1.Columns["Periode"].GroupIndex = 0;
                        gridView1.Columns["NoJurnal"].GroupIndex = 1;
                        gridView1.Columns["BARIS"].SortIndex = 1;
                        gridView1.ExpandAllGroups();
                        gridView1.BestFitColumns();
                        lblrecord.Text = "Jumlah Record : " + PencarianJurnal.Count().ToString();
                    }
                    else
                    {
                        lblrecord.Text = "Jumlah Record : 0";
                        cmbcariperiode.Properties.Items.Clear();
                        gridControl1.DataSource = null;
                        PencarianJurnal = Enumerable.Empty<JurnalDetailDTO>();
                        // XtraMessageBox.Show("Data tidak ditemukan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                }
                else
                {
                    lblrecord.Text = "Jumlah Record : 0";
                    gridControl1.DataSource = null;
                    PencarianJurnal = Enumerable.Empty<JurnalDetailDTO>();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtcarinomor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Tahun();
            }
        }

        private void txtcarikode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Tahun();
            }
        }

        private void txtcarijumlah_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Tahun();
            }
        }

        private void txcariketerangan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Tahun();
            }
        }

        private void sbclear_Click(object sender, EventArgs e)
        {
            txtcarinomor.Text = "";
            lblrecord.Text = "Jumlah Record : 0";
            txtcarikode.Text = "";
            txtcarijumlah.Text = "0";
            txtcariketerangan.Text = "";           
            cmbcariperiode.Properties.Items.Clear();
            cmbcariperiode.Text = "";
            decaritanggal.EditValue = null;
            gridControl1.DataSource = null;
            PencarianJurnal = Enumerable.Empty<JurnalDetailDTO>();
            ExportPencarian = Enumerable.Empty<JurnalDetailDTO>();
        }

        private void decaritanggal_EditValueChanged(object sender, EventArgs e)
        {
            if (decaritanggal.EditValue != null)
            {
                CariJurnal_Tahun();
            }

        }
        private IEnumerable<JurnalDetailDTO> GetJurnalLengkap(List<JurnalDetailReffID> ReffID)
        {
           
                IEnumerable<JurnalDetailDTO> SearchJurnal;
                using (var contol = new OracleConnection(Acct.OracleConnString))
                {
                    string sql = "SELECT REFFID,NOJURNAL,TANGGAL,BARIS,KODE,REKENING,DEBET,KREDIT,KETERANGAN,Posted,Periode FROM ACCT_JURNAL_DTL  WHERE REFFID IN :p_reffid order by periode,nojurnal,baris";

                    if (contol.State == ConnectionState.Closed)
                        contol.Open();
                    try
                    {
                        SearchJurnal = contol.Query<JurnalDetailDTO>(sql, param: new { p_reffid = ReffID.Select(d => d.REFFID) });
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        if (contol.State == ConnectionState.Open)
                            contol.Close();
                    }
                }
                return SearchJurnal;           
        }


        private void txtfilternojurnal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Bulan();
            }
        }

        private void defiltertanggal_EditValueChanged(object sender, EventArgs e)
        {
            if (defiltertanggal.EditValue != null)
            {
                CariJurnal_Bulan();
            }
        }

        private void txtfilterkode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Bulan();
            }
        }

        private void Sbfilterexport_Click(object sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            try
            {                
                if (filter)
                {
                    if (cefilterlengkap.Checked == true)
                    {
                        ExportPencarian_Bulan = from detail in JurnalDetail
                                                join header in JurnalHeader_Filtered on detail.REFFID equals header.JURNALID
                                                select detail;                    
                    }
                    else
                    {
                        ExportPencarian_Bulan = PencarianJurnal_Bulan;
                    }
                    // If you use EPPlus in a noncommercial context
                    // according to the Polyform Noncommercial license:
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    string filename = string.Empty;
                    using (ExcelPackage package = new())
                    {

                        if (ExportPencarian_Bulan.Any())
                        {
                            var wsDt = package.Workbook.Worksheets.Add("Jurnal Entries");

                            //Load the datatable and set the number formats...
                            wsDt.Cells["A1"].LoadFromCollection(ExportPencarian_Bulan, true);

                            //wsDt.Cells["A1"].LoadFromCollection(mydata);
                            wsDt.DeleteColumn(1, 2);

                            //Add the headers
                            wsDt.Cells[1, 1].Value = "NoJurnal";
                            wsDt.Cells[1, 2].Value = "Tanggal";
                            wsDt.Cells[1, 3].Value = "RowNo";
                            wsDt.Cells[1, 4].Value = "Kode";
                            wsDt.Cells[1, 5].Value = "Rekening";
                            wsDt.Cells[1, 6].Value = "Debet";
                            wsDt.Cells[1, 7].Value = "Kredit";
                            wsDt.Cells[1, 8].Value = "Keterangan";
                            wsDt.Cells[1, 9].Value = "Posted";
                            wsDt.Cells[1, 10].Value = "Periode";

                            wsDt.Cells[2, 2, ExportPencarian_Bulan.Count() + 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                            //=IF(A2<>A1,1,C1+1)
                            wsDt.Cells[2, 3, ExportPencarian_Bulan.Count() + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, ExportPencarian_Bulan.Count() + 1, 3).Address);

                            // number formats
                            string positiveFormat = "#,##0.00_)";
                            string negativeFormat = "(#,##0.00)";
                            string zeroFormat = "-_)";
                            string numberFormat = positiveFormat + ";" + negativeFormat;
                            string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                            wsDt.Cells[2, 6, ExportPencarian_Bulan.Count() + 1, 7].Style.Numberformat.Format = fullNumberFormat;
                            //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                            // 
                            wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                            //package.Save();
                            // package.Dispose();

                            // Obtain the Excel file data as a byte array
                            byte[] excelData = package.GetAsByteArray();

                            // Generate a temporary file path
                            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

                            // Write the byte array to the temporary file
                            File.WriteAllBytes(tempFilePath, excelData);

                            // Open the temporary file with the default associated Excel program
                            ProcessStartInfo psi = new(tempFilePath)
                            {
                                UseShellExecute = true
                            };
                            Process.Start(psi);

                        }
                    }
                }
                else
                {
                    ExportJurnal_Periode();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        private void ExportJurnaldtExcel(DataTable dt)
        {
            try
            {
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                string filename = string.Empty;
                using (ExcelPackage package = new())
                {

                    if (dt.Rows.Count > 0)
                    {
                        var wsDt = package.Workbook.Worksheets.Add("Jurnal Entries");

                        //Load the datatable and set the number formats...
                        wsDt.Cells["A1"].LoadFromDataTable(dt, true);

                        //wsDt.Cells["A1"].LoadFromCollection(mydata);
                        wsDt.DeleteColumn(11, 14);

                        //Add the headers
                        wsDt.Cells[1, 1].Value = "NoJurnal";
                        wsDt.Cells[1, 2].Value = "Tanggal";
                        wsDt.Cells[1, 3].Value = "RowNo";
                        wsDt.Cells[1, 4].Value = "Kode";
                        wsDt.Cells[1, 5].Value = "Rekening";
                        wsDt.Cells[1, 6].Value = "Debet";
                        wsDt.Cells[1, 7].Value = "Kredit";
                        wsDt.Cells[1, 8].Value = "Keterangan";
                        wsDt.Cells[1, 9].Value = "Posted";
                        wsDt.Cells[1, 10].Value = "Periode";

                        wsDt.Cells[2, 2, dt.Rows.Count + 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                        //=IF(A2<>A1,1,C1+1)
                        wsDt.Cells[2, 3, dt.Rows.Count + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, dt.Rows.Count + 1, 3).Address);

                        // number formats
                        string positiveFormat = "#,##0.00_)";
                        string negativeFormat = "(#,##0.00)";
                        string zeroFormat = "-_)";
                        string numberFormat = positiveFormat + ";" + negativeFormat;
                        string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                        wsDt.Cells[2, 6, dt.Rows.Count + 1, 7].Style.Numberformat.Format = fullNumberFormat;
                        //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                        // 
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                        // Obtain the Excel file data as a byte array
                        byte[] excelData = package.GetAsByteArray();

                        // Generate a temporary file path
                        string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

                        // Write the byte array to the temporary file
                        File.WriteAllBytes(tempFilePath, excelData);

                        // Open the temporary file with the default associated Excel program
                        ProcessStartInfo psi = new(tempFilePath)
                        {
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }
                    else
                    {
                        MessageBox.Show("Data tidak ditemukan", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void ExportJurnal_AIS_dtExcel(DataTable dt)
        {
            try
            {
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                string filename = string.Empty;
                using (ExcelPackage package = new())
                {

                    if (dt.Rows.Count > 0)
                    {
                        var wsDt = package.Workbook.Worksheets.Add("Jurnal Entries");

                        //Load the datatable and set the number formats...
                        wsDt.Cells["A1"].LoadFromDataTable(dt, true);

                        //wsDt.Cells["A1"].LoadFromCollection(mydata);
                        //wsDt.DeleteColumn(11, 14);

                        //Add the headers
                        wsDt.Cells[1, 1].Value = "NoJurnal";
                        wsDt.Cells[1, 2].Value = "Tanggal";
                        wsDt.Cells[1, 3].Value = "RowNo";
                        wsDt.Cells[1, 4].Value = "Kode";
                        wsDt.Cells[1, 5].Value = "Rekening";
                        wsDt.Cells[1, 6].Value = "Debet";
                        wsDt.Cells[1, 7].Value = "Kredit";
                        wsDt.Cells[1, 8].Value = "Keterangan";
                        wsDt.Cells[1, 9].Value = "Posted";
                        wsDt.Cells[1, 10].Value = "Periode";

                        wsDt.Cells[2, 2, dt.Rows.Count + 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                        //=IF(A2<>A1,1,C1+1)
                        wsDt.Cells[2, 3, dt.Rows.Count + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, dt.Rows.Count + 1, 3).Address);

                        // number formats
                        string positiveFormat = "#,##0.00_)";
                        string negativeFormat = "(#,##0.00)";
                        string zeroFormat = "-_)";
                        string numberFormat = positiveFormat + ";" + negativeFormat;
                        string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                        wsDt.Cells[2, 6, dt.Rows.Count + 1, 7].Style.Numberformat.Format = fullNumberFormat;
                        //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                        // 
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                        //package.Save();
                        // package.Dispose();

                        // Obtain the Excel file data as a byte array
                        byte[] excelData = package.GetAsByteArray();

                        // Generate a temporary file path
                        string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

                        // Write the byte array to the temporary file
                        File.WriteAllBytes(tempFilePath, excelData);

                        // Open the temporary file with the default associated Excel program
                        ProcessStartInfo psi = new(tempFilePath)
                        {
                            UseShellExecute = true
                        };
                        Process.Start(psi);

                    }
                    else
                    {
                        MessageBox.Show("Data tidak ditemukan", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
        private void repositoryItemSearchLookUpEditKODE_EditValueChanged(object sender, EventArgs e)
        {
            try
            {   ////from datatable
                //SearchLookUpEdit editor = (SearchLookUpEdit)sender;
                //DataRowView row = editor.Properties.GetRowByKeyValue(editor.EditValue) as DataRowView;
                //if (row != null)
                //{
                //    JDgridView.SetFocusedRowCellValue("Rekening", row["PERKIRAAN"].ToString());
                //}
                // from List
                SearchLookUpEdit editor = (SearchLookUpEdit)sender;
                DTOCOAAktif selectedListItem = (DTOCOAAktif)editor.GetSelectedDataRow();

                if (selectedListItem != null)
                {
                    string rekeningValue = selectedListItem.PERKIRAAN.ToString();
                    JDgridView.SetFocusedRowCellValue("Rekening", rekeningValue);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void radioGroup1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Load_COA_Style();
        }

        private void Load_COA_Style()
        {
            if (radioGroup1.SelectedIndex == 0)
            {
                repositoryItemSearchLookUpEditKODE.DataSource = ListCoaAktif;
                repositoryItemSearchLookUpEditKODE.ValueMember = "KODE";
                repositoryItemSearchLookUpEditKODE.DisplayMember = "KODE";
                repositoryItemSearchLookUpEditKODE.PopulateViewColumns();
                repositoryItemSearchLookUpEditKODE.PopupFormSize = new Size(600, 350);
                repositoryItemSearchLookUpEditKODE.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFit;
                GCJurnal.RepositoryItems.Add(repositoryItemSearchLookUpEditKODE);
                JDgridView.Columns["Kode"].ColumnEdit = repositoryItemSearchLookUpEditKODE;

                  
            }
            else if (radioGroup1.SelectedIndex == 1)
            {
                repositoryItemLookUpEditkode.DataSource = ListCoaAktif;
                repositoryItemLookUpEditkode.ValueMember = "KODE";
                repositoryItemLookUpEditkode.DisplayMember = "KODE";
                repositoryItemLookUpEditkode.PopupFormSize = new Size(600, 350);
                repositoryItemLookUpEditkode.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFit;
                GCJurnal.RepositoryItems.Add(repositoryItemLookUpEditkode);
                JDgridView.Columns["Kode"].ColumnEdit = repositoryItemLookUpEditkode;

            }
            else
            {
                // Create a repository item
                RepositoryItemTextEdit repositoryItemTextEdit = new()
                {
                    MaxLength = 12
                };
                // Regular API
                //var settings = repositoryItemTextEdit.Properties.MaskSettings.Configure<MaskSettings.Simple>();
                //settings.MaskExpression = "00.00000.000";
                // Assign the repository item to the desired column
                JDgridView.Columns["Kode"].ColumnEdit = repositoryItemTextEdit;
                //repositoryItemTextEdit.EditValueChanged += txtkodeperkiran_EditValueChanged;
                repositoryItemTextEdit.Leave += txtkodeperkiran_Leave;
            }
            JDgridView.Focus();
        }

        private void txtkodeperkiran_Leave(object? sender, EventArgs e)
        {
            DevExpress.XtraEditors.TextEdit textEditor = (DevExpress.XtraEditors.TextEdit)sender;
            if (!string.IsNullOrEmpty(textEditor.Text))
            {
                string kode = textEditor.Text;
                var kodeacct = ListCoaAktif.Where(t => t.KODE == kode).FirstOrDefault();
                if (kodeacct != null)
                {
                    // Set the column value in the GridView
                    //JDgridView.SetRowCellValue(JDgridView.FocusedRowHandle, "Perkiraan", "ccxxx");
                    JDgridView.SetFocusedRowCellValue("Perkiraan", kodeacct.PERKIRAAN);
                    //XtraMessageBox.Show("" + kodeacct.KODE + " " + kodeacct.PERKIRAAN, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    XtraMessageBox.Show("Show Form", "Info", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private void ExportJurnal_Periode()
        {

                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                string filename = string.Empty;
                using (ExcelPackage package = new())
                {

                    if (JurnalDetail.Any())
                    {
                        var wsDt = package.Workbook.Worksheets.Add("Jurnal Entries");

                        //Load the datatable and set the number formats...
                        wsDt.Cells["A1"].LoadFromCollection(JurnalDetail, true);

                    //wsDt.Cells["A1"].LoadFromCollection(mydata);
                    wsDt.DeleteColumn(1, 2);

                    //Add the headers
                    wsDt.Cells[1, 1].Value = "NoJurnal";
                        wsDt.Cells[1, 2].Value = "Tanggal";
                        wsDt.Cells[1, 3].Value = "RowNo";
                        wsDt.Cells[1, 4].Value = "Kode";
                        wsDt.Cells[1, 5].Value = "Rekening";
                        wsDt.Cells[1, 6].Value = "Debet";
                        wsDt.Cells[1, 7].Value = "Kredit";
                        wsDt.Cells[1, 8].Value = "Keterangan";
                        wsDt.Cells[1, 9].Value = "Posted";
                        wsDt.Cells[1, 10].Value = "Periode";

                        wsDt.Cells[2, 2, JurnalDetail.Count() + 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                        //=IF(A2<>A1,1,C1+1)
                        wsDt.Cells[2, 3, JurnalDetail.Count() + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, JurnalDetail.Count() + 1, 3).Address);

                        // number formats
                        string positiveFormat = "#,##0.00_)";
                        string negativeFormat = "(#,##0.00)";
                        string zeroFormat = "-_)";
                        string numberFormat = positiveFormat + ";" + negativeFormat;
                        string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                        wsDt.Cells[2, 6, JurnalDetail.Count() + 1, 7].Style.Numberformat.Format = fullNumberFormat;
                        //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                        // 
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                    // Obtain the Excel file data as a byte array
                    byte[] excelData = package.GetAsByteArray();

                    // Generate a temporary file path
                    string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

                    // Write the byte array to the temporary file
                    File.WriteAllBytes(tempFilePath, excelData);

                    // Open the temporary file with the default associated Excel program
                    ProcessStartInfo psi = new(tempFilePath)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(psi);

                    }
                }
                
        }

        private void cmballperiode_SelectedIndexChanged(object sender, EventArgs e)
        {           
            PilihanPeriodeAkuntansi();
        }

        private void repositoryItemLookUpEditkode_QueryCloseUp(object sender, CancelEventArgs e)
        {
            SendKeys.Send("{TAB}");
        }

        private void deJurnal_Enter(object sender, EventArgs e)
        {
            DateEdit edit = sender as DateEdit;
            BeginInvoke(new MethodInvoker(() =>
            {
                edit.SelectionStart = 0;

                edit.SelectionLength = 2;
            }));
        }

        private void GVDetail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                GridView view = sender as GridView;
                object value = view.GetFocusedValue();
                Clipboard.SetText(value.ToString());
            }
        }

        private void cmbbulan_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            if (Int32.Parse(txtcarijumlah.Text) > 0 || !string.IsNullOrEmpty(txtcarikode.Text) || !string.IsNullOrEmpty(txtcarinomor.Text) || !string.IsNullOrEmpty(txtcariketerangan.Text))
            {
                CariJurnal_Tahun();
            }
        }

        private void daritahun_EditValueChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            if (Int32.Parse(txtcarijumlah.Text) > 0 || !string.IsNullOrEmpty(txtcarikode.Text) || !string.IsNullOrEmpty(txtcarinomor.Text) || !string.IsNullOrEmpty(txtcariketerangan.Text))
            {
                CariJurnal_Tahun();
            }
        }

        private void cmbbulan2_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            if (Int32.Parse(txtcarijumlah.Text) > 0 || !string.IsNullOrEmpty(txtcarikode.Text) || !string.IsNullOrEmpty(txtcarinomor.Text) || !string.IsNullOrEmpty(txtcariketerangan.Text))
            {
                CariJurnal_Tahun();
            }
        }

        private void sampaitahun_EditValueChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            if (Int32.Parse(txtcarijumlah.Text) > 0 || !string.IsNullOrEmpty(txtcarikode.Text) || !string.IsNullOrEmpty(txtcarinomor.Text) || !string.IsNullOrEmpty(txtcariketerangan.Text))
            {
                CariJurnal_Tahun();
            }
        }


        private void Load_Kode_Kasir()
        {
            lookUpEditkasir.Properties.DataSource = GetKasirKode(CompanyInfo.INIT);
            lookUpEditkasir.Properties.ValueMember = "PTLOKASI";
            lookUpEditkasir.Properties.DisplayMember = "KASIR";
        }

        private DataTable GetKasirKode(string p_iddata)
        {
            using OracleCommand _command = new("select DISTINCT KASIR,PTLOKASI,  KETERANGAN FROM ACCT_CONV_PTLOKASI WHERE IDDATA =:p_iddata and KASIR IS NOT NULL", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new DataTable();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

      

        private void Load_Kode_Inv()
        {
            lookUpEditINV.Properties.DataSource = GetINVKode(CompanyInfo.INIT);
            lookUpEditINV.Properties.ValueMember = "PTLOKASI";
            lookUpEditINV.Properties.DisplayMember = "INV";
        }

        private object GetINVKode(string p_iddata)
        {
            using OracleCommand _command = new("select DISTINCT PTLOKASI, INV  FROM ACCT_CONV_PTLOKASI WHERE IDDATA =:p_iddata AND INV IS NOT NULL", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }

        private void Load_Kode_AIS()
        {
            lookUpEditAIS.Properties.DataSource = GetAISKode(CompanyInfo.INIT);
            lookUpEditAIS.Properties.ValueMember = "AIS_ESTATE";
            lookUpEditAIS.Properties.DisplayMember = "KETERANGAN";

            Dictionary<int, string> Remises = new Dictionary<int, string>
            {
                { 1, "1" },
                { 2, "2" }
            };
            leremiseAIS.Properties.DataSource = Remises;
            leremiseAIS.EditValue = 1;
        }

        private object GetAISKode(string p_iddata)
        {
            using OracleCommand _command = new("select DISTINCT AIS_ESTATE,PTLOKASI,AIS_KET KETERANGAN FROM ACCT_CONV_PTLOKASI WHERE IDDATA =:p_iddata AND AIS_ESTATE IS NOT NULL", conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = p_iddata;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new ();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            return _dt;
        }
        private void cmbbulanimport_SelectedIndexChanged(object sender, EventArgs e)
        {
            Load_Jurnal_Kasir_Header();
            Load_Jurnal_Kasir_Detail();
        }

        private void Load_Jurnal_Kasir_Detail()
        {
            try
            {
                if (lookUpEditkasir.EditValue != null)
                {
                    if (setahunkasir.Value >= 2022)
                    {
                        int ptahun = Convert.ToInt32(setahunkasir.Value);
                        int pbulan = cmbbulankasir.SelectedIndex + 1;

                        var lastDayOfMonth = DateTime.DaysInMonth(ptahun, pbulan);
                        DateTime tglakhir = new (ptahun, pbulan, lastDayOfMonth);
                        var akhirbulan = Convert.ToDateTime(tglakhir.ToString("dd-MM-yyyy")).Date;

                        DateTime dari = new (ptahun, pbulan, 1);
                        DateTime sampai = new (ptahun, pbulan, akhirbulan.Day);
                        var aliasptlokasi = lookUpEditkasir.EditValue.ToString();
                        var iddata = CompanyInfo.INIT;
                        var aliaskodekasir = lookUpEditkasir.Text;



                        //jika periode telah dikunci,  batalkan proses import jurnal
                        var p_periode = pbulan.ToString("0#") + "/" + ptahun.ToString();

                        //JurnalFromKasir= JurnalFromModuleServices.GetJurnalDetails_DapperKasir(dari, sampai, aliasptlokasi, aliaskodekasir);
                        dtJurnalKasir = JurnalFromModuleServices.JurnalKasirDetail_DapperKasir(dari, sampai, aliasptlokasi, iddata, aliaskodekasir, "True", p_periode, LoginInfo.userID, ptahun, pbulan);

                    }

                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error Load_Jurnal_Kasir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void setahunkasir_EditValueChanged(object sender, EventArgs e)
        {
            Load_Jurnal_Kasir_Header();
            Load_Jurnal_Kasir_Detail();
        }

        private void sbbuatjurnalkasir_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalKasir == null || dtJurnalKasir.Rows.Count==0)
                {
                    return;
                }
                if (checkEditKAS.Checked==true || checkEditBANK.Checked == true)
                {
                    checkEditKAS.Checked = false;
                    checkEditBANK.Checked = false;
                }
                pbulan = cmbbulankasir.SelectedIndex + 1;
                ptahun = Convert.ToInt32(setahunkasir.Value);

                //jika periode telah dikunci,  batalkan proses import jurnal
                var p_periode = pbulan.ToString("0#") + "/" + ptahun.ToString();
                Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.INIT, p_periode);
                var Periode = cmbbulankasir.Text + " - " + setahunkasir.Value.ToString();
                if (Acct.KunciPeriode == "Y")
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\periode_dikunci.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Tidak dapat melakukan proses import Jurnal pada periode ini...!!!\nPeriode Akuntansi : " + Periode + " Telah Dikunci.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var Bulan = cmbbulankasir.Text + " - " + setahunkasir.Value.ToString();
                if (XtraMessageBox.Show("Lanjutkan Proses Import Jurnal KAS dan BANK ? " +
                    "\n\nPeriode : " + Bulan + " " +
                    "\nLokasi Data :" + CompanyInfo.INIT
                    , "Confirm Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);

                Stopwatch watch = new();
                watch.Start();

                //jika periode belum ada, buat periode
                int pexist = JurnalServices.CekPeriodeExist(CompanyInfo.INIT, p_periode);
                if (pexist == 0)
                {
                    AccountServices.CreateNextPeriode(CompanyInfo.INIT, pbulan - 1, ptahun);
                }


                //Kosongkan Data Table Oracle
                Hapus_Data_Table_Tmp();

                //Copy Data dari DataTable ke Oracle
                JurnalServices.SaveUsingOracleBulkCopy("ACCT_JURNAL_TMP", dtJurnalKasir);

                //CEK KODE NULL
                var KODENULL = JurnalServices.CekJurnal_KODENULL();
                if (KODENULL.Rows.Count > 0)
                {                   
                    List<string> list = KODENULL.AsEnumerable()
                           .Select(r => r.Field<string>("NOJURNAL"))
                           .ToList();
                    var daftarKODENULL = string.Join(Environment.NewLine, list);
                    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                        "\nKode Jurnal belum diisi sebanyak " + KODENULL.Rows.Count.ToString("##,###") + " Nomor." +
                        "\n" + daftarKODENULL, "Kode Jurnal Kosong", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //cek duplikasi nojurnal pada periode dan lokasi data yang sama
                var dup_nojurnal = JurnalServices.CekNoJurnalExist();
                if (dup_nojurnal.Rows.Count > 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_duplikasi.wav";
                    this.Player.Play();
                    List<string> list = dup_nojurnal.AsEnumerable()
                           .Select(r => r.Field<string>("NOJURNAL"))
                           .ToList();
                    var daftarnojurnalexist = string.Join(Environment.NewLine, list);
                    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                        "\nDuplikasi NoJurnal sebanyak " + dup_nojurnal.Rows.Count.ToString("##,###") + " Nomor." +
                        "\n" + daftarnojurnalexist, "Nomor Jurnal Telah digunakan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ////cek duplikasi nomor jurnal, nomor jurnal sama tanggal beda, jika ada proses import batal

                var duplikasi = JurnalServices.CekDuplikasiJurnal();
                if (duplikasi.Rows.Count > 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_duplikasi.wav";
                    this.Player.Play();
                    List<string> list = duplikasi.AsEnumerable()
                           .Select(r => r.Field<string>("NOJURNAL"))
                           .ToList();
                    var daftarnojurnal = string.Join(Environment.NewLine, list);
                    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                        "\nDuplikasi NoJurnal pada nomor jurnal yang sama tetapi beda tanggal sebanyak " + duplikasi.Rows.Count.ToString("##,###") + " Nomor." +
                        "\nBerikut ini NoJurnalnya : \n\n" + daftarnojurnal, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                  
                    return;
                }


                //cek aku master sudah ada ?
                var akun = JurnalServices.CekAkunMaster(Convert.ToInt32(setahunkasir.Value));
                if (akun.Rows.Count > 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_daftarperk.wav";
                    this.Player.Play();
                    List<string> list = akun.AsEnumerable()
                           .Select(r => r.Field<string>("ASAL"))
                           .ToList();
                    var daftarakun = string.Join(Environment.NewLine, list);
                    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                        "\nJumlah Kode tidak terdaftar sebanyak " + akun.Rows.Count.ToString("##,###") + " Akun." +
                        "\nKode Akun dibawah ini belum ada di Daftar Perkiraan \n" + daftarakun, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }


                //Copy data dari table tmp ke table Acct_Jurnal_Dtl
                var sukses = JurnalServices.ImportJurnalParsial(CompanyInfo.INIT, pbulan, ptahun, p_periode);
                if (sukses == 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_bedaperiode.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Import Jurnal di Batalkan \nCek Periode Pada Lembar Excel Double ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                if (sukses == 1)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_imbang.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Import Jurnal di Batalkan \nJurnal Tidak Seimbang ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                AccountServices.RekalkulasiSaldo(CompanyInfo.INIT, pbulan, ptahun, LoginInfo.userID);
                watch.Stop();

                TimeSpan timeSpan = watch.Elapsed;
                string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
               
                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_selesai.wav";
                this.Player.Play();
                allperiode();
                XtraMessageBox.Show("Import Jurnal Selesai \n " + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Hapus_Data_Table_Tmp()
        {
            try
            {
                string query = "TRUNCATE TABLE ACCT_JURNAL_TMP";
                conn.Open();
                OracleCommand cmd = new OracleCommand(query, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lookUpEditkasir_EditValueChanged(object sender, EventArgs e)
        {
            Load_Jurnal_Kasir_Header();
            Load_Jurnal_Kasir_Detail();

        }

        private void Load_Jurnal_Kasir_Header()
        {
            if (lookUpEditkasir.EditValue != null)
            {
                if (setahunkasir.Value >= 2022)
                {
                    int ptahun = Convert.ToInt32(setahunkasir.Value);
                    int pbulan = cmbbulankasir.SelectedIndex + 1;
                    var p_ptlokasi = lookUpEditkasir.EditValue.ToString();
                    var p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("0#"));
                    KasirJurnalHeader = JurnalFromModuleServices.GetJurnalHeader_Kasir(p_periode_int, p_ptlokasi);
                    GC_KasirHeader.DataSource = KasirJurnalHeader;
                    gridView_KasirHeader.Columns[1].OptionsColumn.FixedWidth = true;
                    gridView_KasirHeader.Columns[1].Width = 80;


                }
            }
        }

        private void sbbuatjurnalAIS_Click(object sender, EventArgs e)
        {
            BuatJurnalAIS();
        }
        string AISptlokasi;

        private void gridViewAISheader_Click(object sender, EventArgs e)
        {
            if (this.gridViewAISheader.GetFocusedRowCellValue("AISJURNALID") == null)
                return;
            Call_AIS_JurnalDetail();
        }
        DataTable dt = null;
        private void Call_AIS_JurnalDetail()
        {
            try
            {               
                var filter = Convert.ToDecimal(gridViewAISheader.GetRowCellValue(gridViewAISheader.FocusedRowHandle, "AISJURNALID").ToString());
                var filtered = dtAISDetail.AsEnumerable().Where(y => y.Field<decimal>("REFFID") == filter);
                if (filtered.Any())
                {
                    dt = filtered.CopyToDataTable();
                }
                else
                {
                    dt = dtAISDetail.Clone();
                }                  

                gcAISdetail.DataSource = dt;

                gridViewAISdetail.Columns[0].Visible = false;
                gridViewAISdetail.Columns[1].OptionsColumn.FixedWidth = true;
                gridViewAISdetail.Columns[1].Width = 50;
                gridViewAISdetail.Columns[2].OptionsColumn.FixedWidth = true;
                gridViewAISdetail.Columns[2].Width = 90;
                gridViewAISdetail.Columns[4].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gridViewAISdetail.Columns[4].DisplayFormat.FormatString = "n2";
                gridViewAISdetail.Columns[5].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gridViewAISdetail.Columns[5].DisplayFormat.FormatString = "n2";
                gridViewAISdetail.Columns[4].Summary.Clear();
                gridViewAISdetail.Columns[5].Summary.Clear();
                gridViewAISdetail.Columns[4].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "DEBET", "{0:N2}");
                gridViewAISdetail.Columns[5].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "KREDIT", "{0:N2}");
                gridViewAISdetail.BestFitColumns();
            }
            catch (Exception ex)
            {
                gcAISdetail.DataSource = null;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void cmbbulanAIS_SelectedIndexChanged(object sender, EventArgs e)
        {
            Load_JurnalAIS_Periode();
        }

        private void setahunAIS_EditValueChanged(object sender, EventArgs e)
        {
            Load_JurnalAIS_Periode();
        }

        private void gridViewAISheader_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            try
            {
                GridView view = sender as GridView;
                if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
                {
                    int rowHandle = e.HitInfo.RowHandle;
                    //hapus menu jika ada
                    e.Menu.Items.Clear();
                    DXMenuItem expexcelALL_BORONGAN = CreateMenuItemExpExcelALL_BORONGAN(view, rowHandle);
                    DXMenuItem expexcelALL_HARIAN = CreateMenuItemExpExcelALL_HARIAN(view, rowHandle);



                    expexcelALL_BORONGAN.BeginGroup = true;
                    expexcelALL_HARIAN.BeginGroup = true;

                    e.Menu.Items.Add(expexcelALL_BORONGAN);
                    e.Menu.Items.Add(expexcelALL_HARIAN);
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DXMenuItem CreateMenuItemExpExcelALL_HARIAN(DevExpress.XtraGrid.Views.Grid.GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Export ke Excel, SEMUA HARIAN remise ini", new EventHandler(OnexpexcelALLHARIANClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }

        private void OnexpexcelALLHARIANClick(object? sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            handle.QueueFocus(IntPtr.Zero);
            int ptahun = Convert.ToInt32(setahunAIS.Value);
            int pbulan = cmbbulanAIS.SelectedIndex + 1;


            var p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("00"));
            var p_periode_str = pbulan.ToString("00") + "/" + ptahun.ToString();
            var p_iddata = CompanyInfo.INIT;
            var p_estate = lookUpEditAIS.EditValue.ToString();
            var pnomor = gridViewAISheader.GetFocusedRowCellValue("NOMOR").ToString();
            var p_remise = Convert.ToInt32(leremiseAIS.EditValue.ToString());

            DateTime TanggalJurnal;
            if (p_remise == 1)
            {
                TanggalJurnal = new DateTime(ptahun, pbulan, 15);
            }
            else
            {
                // Get the last day of month
                DateTime lastDayOfmonth = new DateTime(ptahun, pbulan, 1).AddMonths(1).AddDays(-1);
                TanggalJurnal = lastDayOfmonth;
            }

            DataTable dt = JurnalFromModuleServices.AIS_Jurnal_Detail_ALL_HARIAN(TanggalJurnal, p_periode_int, p_periode_str, AISptlokasi, p_estate, p_remise, p_iddata);
            ExportJurnal_AIS_dtExcel(dt);
        }

        private DXMenuItem CreateMenuItemExpExcelALL_BORONGAN(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Export ke Excel, SEMUA BORONGAN remise ini", new EventHandler(OnexpexcelALLBORONGANClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }

        private void OnexpexcelALLBORONGANClick(object? sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            handle.QueueFocus(IntPtr.Zero);
            int ptahun = Convert.ToInt32(setahunAIS.Value);
            int pbulan = cmbbulanAIS.SelectedIndex + 1;


            var p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("00"));
            var p_periode_str = pbulan.ToString("00") + "/" + ptahun.ToString();
            var p_iddata = CompanyInfo.INIT;
            var p_estate = lookUpEditAIS.EditValue.ToString();
            var pnomor = gridViewAISheader.GetFocusedRowCellValue("NOMOR").ToString();
            var p_remise = Convert.ToInt32(leremiseAIS.EditValue.ToString());

            DateTime TanggalJurnal;
            if (p_remise == 1)
            {
                TanggalJurnal = new DateTime(ptahun, pbulan, 15);
            }
            else
            {
                // Get the last day of month
                DateTime lastDayOfmonth = new DateTime(ptahun, pbulan, 1).AddMonths(1).AddDays(-1);
                TanggalJurnal = lastDayOfmonth;
            }

            DataTable dt = JurnalFromModuleServices.AIS_Jurnal_Detail_ALL_BORONGAN(TanggalJurnal, p_periode_int, p_periode_str, AISptlokasi, p_estate, p_remise, p_iddata);
            ExportJurnal_AIS_dtExcel(dt);
        }

        bool addAISJurnal;
        string noAIS_Bukti;
        private void OnaddjurnalClick(object? sender, EventArgs e)
        {
            BuatJurnalAIS();
        
        }

        private void BuatJurnalAIS()
        {
            if (dtAISDetail != null)
            {

                var nojurnalexist = gridViewAISheader.GetRowCellValue(gridViewAISheader.FocusedRowHandle, "NOJURNAL").ToString();
                noAIS_Bukti = gridViewAISheader.GetRowCellValue(gridViewAISheader.FocusedRowHandle, "NOMOR").ToString();
                addAISJurnal = true;
                fromModule_AIS = true;
                DateTime TanggalJurnal;
                var estate = lookUpEditAIS.EditValue.ToString();
                var remise = Convert.ToInt16(leremiseAIS.EditValue.ToString());
                var bl = cmbbulanAIS.SelectedIndex + 1;
                var th = (int)setahunAIS.Value;

                if (remise == 1)
                {
                    TanggalJurnal = new DateTime(th, bl, 15);
                }
                else
                {
                    // Get the last day of month
                    DateTime lastDayOfmonth = new DateTime(th, bl, 1).AddMonths(1).AddDays(-1);
                    TanggalJurnal = lastDayOfmonth;
                }
                var filter = gridViewAISheader.GetRowCellValue(gridViewAISheader.FocusedRowHandle, "NOMOR").ToString();
                var filtered = dtAISDetail.AsEnumerable().Where(y => y.Field<string>("NOBUKTI") == filter);
                if (filtered.Any())
                {
                    //header
                    if (string.IsNullOrEmpty(nojurnalexist))
                    {
                        NoJurnaltxt.Text = "XXX/MK-AGRO-" + estate;
                    }
                    else
                    {
                        NoJurnaltxt.Text = nojurnalexist;
                    }

                    deJurnal.EditValue = TanggalJurnal;

                    List<JurnalDetailAdd> query = dtAISDetail.AsEnumerable().Where(d => d.Field<string>("NOBUKTI") == filter).Select(n =>
                                    new JurnalDetailAdd { BARIS = Convert.ToInt32(n.Field<decimal>("BARIS")), Kode = n.Field<string>("KODE"), Rekening = n.Field<string>("REKENING"), Debet = n.Field<decimal>("DEBET"), Kredit = n.Field<decimal>("KREDIT"), Keterangan = n.Field<string>("KETERANGAN") }).ToList();
                    InputJurnalDetail = new BindingList<JurnalDetailAdd>(query);

                    this.GCJurnal.DataSource = InputJurnalDetail;
                    InputJurnalDetail.AllowNew = true;
                    // this.gridControl1.DataSource = InputJurnalDetail;

                    // cmbperiode.SelectedValue = dperiode;
                    TABJurnal.SelectedTabPage = xtraTabPage1;
                    //x = JDgridView.RowCount;
                    x = InputJurnalDetail.Count - 1;
                }
                else
                {
                    MessageBox.Show("Tidak ada transaksi untuk dibuat jurnal", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Pilih Nomor untuk membuat Jurnal MK-AGRO", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void lookUpEditINV_EditValueChanged(object sender, EventArgs e)
        {
            Load_inv_header();
            Load_Jurnal_INV();
            
        }

        private void Load_inv_header()
        {
            if (lookUpEditINV.EditValue!=null  && SETAHUNINV.Value >= 2022)
            {
                int ptahun = Convert.ToInt32(SETAHUNINV.Value);
                int pbulan = CMBBULANINV.SelectedIndex + 1;
                var p_ptlokasi = lookUpEditINV.EditValue.ToString();
                var p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("0#"));
                InventoryJurnalHeader = JurnalFromModuleServices.GetJurnalHeader_Inventory(p_periode_int, p_ptlokasi);
                gc_inv_header.DataSource = InventoryJurnalHeader;
                gridView_inv_header.Columns[1].OptionsColumn.FixedWidth = true;
                gridView_inv_header.Columns[1].Width = 80;
            }

        }

            private void Load_Jurnal_INV()
        {
            if (lookUpEditINV.EditValue != null)
            {
                if (SETAHUNINV.Value >=2022)
                {
                    int ptahun = Convert.ToInt32(SETAHUNINV.Value);
                    int pbulan = CMBBULANINV.SelectedIndex + 1;

                 
                    var p_ptlokasi = lookUpEditINV.EditValue.ToString();
                    var p_iddata = CompanyInfo.INIT;

                    //jika periode telah dikunci,  batalkan proses import jurnal
                    var p_periode_int = Convert.ToInt32(ptahun.ToString()+pbulan.ToString("0#")) ;
                    var p_periode_str = pbulan.ToString("0#") + "/" + ptahun.ToString();

                    var TOTALNILAI = JurnalFromModuleServices.CEK_TOTAL_TRANSAKSI(p_periode_int, p_ptlokasi, "INVENTORY");

                   dtJurnalInventory = JurnalFromModuleServices.Jurnal_Inventori(p_periode_int,p_ptlokasi, p_iddata, "True", p_periode_str, LoginInfo.userID, ptahun, pbulan);
                    LBLTOTALTRANSAKSI.Text = string.Format("{0:#,##}", TOTALNILAI);
                    
                }

            }
        }

        private void CMBBULANINV_SelectedIndexChanged(object sender, EventArgs e)
        {
            Load_inv_header();
            Load_Jurnal_INV();
        }

        private void SBJURNALINV_Click(object sender, EventArgs e)
        {
            try
            {
                if(dtJurnalInventory == null || dtJurnalInventory.Rows.Count==0)
                {
                    return;
                }
                if (checkEditlk.Checked == true || checkEditlt.Checked == true)
                {
                    checkEditlk.Checked = false;
                    checkEditlt.Checked = false;
                }
                pbulan = CMBBULANINV.SelectedIndex + 1;
                ptahun = Convert.ToInt32(SETAHUNINV.Value);

                //jika periode telah dikunci,  batalkan proses import jurnal
                var p_periode = pbulan.ToString("0#") + "/" + ptahun.ToString();
                Acct.KunciPeriode = JurnalServices.GetLockStatus(CompanyInfo.INIT, p_periode);
                var Periode = CMBBULANINV.Text + " - " + SETAHUNINV.Value.ToString();
                if (Acct.KunciPeriode == "Y")
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\periode_dikunci.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Tidak dapat melakukan proses import Jurnal pada periode ini...!!!\nPeriode Akuntansi : " + Periode + " Telah Dikunci.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var Bulan = CMBBULANINV.Text + " - " + SETAHUNINV.Value.ToString();
                if (XtraMessageBox.Show("Lanjutkan Proses Import Jurnal LT dan LK ? " +
                    "\n\nPeriode : " + Bulan + " " +
                    "\nLokasi Data :" + CompanyInfo.INIT
                    , "Confirm Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);

                Stopwatch watch = new();
                watch.Start();

                //jika periode belum ada, buat periode
                int pexist = JurnalServices.CekPeriodeExist(CompanyInfo.INIT, p_periode);
                if (pexist == 0)
                {
                    AccountServices.CreateNextPeriode(CompanyInfo.INIT, pbulan - 1, ptahun);
                }


                //Kosongkan Data Table Oracle
                Hapus_Data_Table_Tmp();

                //Copy Data dari DataTable ke Oracle
                JurnalServices.SaveUsingOracleBulkCopy("ACCT_JURNAL_TMP", dtJurnalInventory);

                //CEK KODE NULL
                var KODENULL = JurnalServices.CekJurnal_KODENULL();
                if (KODENULL.Rows.Count > 0)
                {
                    List<string> list = KODENULL.AsEnumerable()
                           .Select(r => r.Field<string>("NOJURNAL"))
                           .ToList();
                    var daftarKODENULL = string.Join(Environment.NewLine, list);
                    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                        "\nKode Jurnal belum diisi sebanyak " + KODENULL.Rows.Count.ToString("##,###") + " Nomor." +
                        "\n" + daftarKODENULL, "Kode Jurnal Kosong", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //cek duplikasi nojurnal pada periode dan lokasi data yang sama
                var dup_nojurnal = JurnalServices.CekNoJurnalExist();
                if (dup_nojurnal.Rows.Count > 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_duplikasi.wav";
                    this.Player.Play();
                    List<string> list = dup_nojurnal.AsEnumerable()
                           .Select(r => r.Field<string>("NOJURNAL"))
                           .ToList();
                    var daftarnojurnalexist = string.Join(Environment.NewLine, list);
                    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                        "\nDuplikasi NoJurnal sebanyak " + dup_nojurnal.Rows.Count.ToString("##,###") + " Nomor." +
                        "\n" + daftarnojurnalexist, "Nomor Jurnal Telah digunakan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                ////cek duplikasi nomor jurnal, nomor jurnal sama tanggal beda, jika ada proses import batal

                var duplikasi = JurnalServices.CekDuplikasiJurnal();
                if (duplikasi.Rows.Count > 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_duplikasi.wav";
                    this.Player.Play();
                    List<string> list = duplikasi.AsEnumerable()
                           .Select(r => r.Field<string>("NOJURNAL"))
                           .ToList();
                    var daftarnojurnal = string.Join(Environment.NewLine, list);
                    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                        "\nDuplikasi NoJurnal pada nomor jurnal yang sama tetapi beda tanggal sebanyak " + duplikasi.Rows.Count.ToString("##,###") + " Nomor." +
                        "\nBerikut ini NoJurnalnya : \n\n" + daftarnojurnal, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }


                //cek aku master sudah ada ?
                var akun = JurnalServices.CekAkunMaster(ptahun);
                if (akun.Rows.Count > 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_daftarperk.wav";
                    this.Player.Play();
                    List<string> list = akun.AsEnumerable()
                           .Select(r => r.Field<string>("ASAL"))
                           .ToList();
                    var daftarakun = string.Join(Environment.NewLine, list);
                    XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                        "\nJumlah Kode tidak terdaftar sebanyak " + akun.Rows.Count.ToString("##,###") + " Akun." +
                        "\nKode Akun dibawah ini belum ada di Daftar Perkiraan \n" + daftarakun, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }


                //Copy data dari table tmp ke table Acct_Jurnal_Dtl
                var sukses = JurnalServices.ImportJurnalParsial(CompanyInfo.INIT, pbulan, ptahun, p_periode);
                if (sukses == 0)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_bedaperiode.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Import Jurnal di Batalkan \nCek Periode Pada Lembar Excel Double ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                if (sukses == 1)
                {
                    this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_imbang.wav";
                    this.Player.Play();
                    XtraMessageBox.Show("Import Jurnal di Batalkan \nJurnal Tidak Seimbang ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                AccountServices.RekalkulasiSaldo(CompanyInfo.INIT, pbulan, ptahun, LoginInfo.userID);
                watch.Stop();

                TimeSpan timeSpan = watch.Elapsed;
                string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_selesai.wav";
                this.Player.Play();
                allperiode();
                XtraMessageBox.Show("Import Jurnal dari Inventori Selesai \n " + waktuproses, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sbexportinv_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalInventory == null || dtJurnalInventory.Rows.Count == 0)
                {
                    return;
                }
                if (dtJurnalInventory.Rows.Count > 0)
                {
                    DataTable dtnew = new();
                    if (checkEditlt.Checked == false && checkEditlk.Checked == false)
                    {
                        dtnew = dtJurnalInventory;
                    }
                    else if (checkEditlt.Checked == true)
                    {
                        var filtered = dtJurnalInventory.AsEnumerable().Where(y => y.Field<string>("NOJURNAL").Contains("/LT"));
                        if (filtered.Any())
                        {
                            dtnew = filtered.CopyToDataTable();
                        }
                        else
                        {
                            dtnew = dtJurnalInventory.Clone();
                        }
                    }
                    else if (checkEditlk.Checked == true)
                    {
                        var filtered = dtJurnalInventory.AsEnumerable().Where(y => y.Field<string>("NOJURNAL").Contains("/LK"));
                        if (filtered.Any())
                        {
                            dtnew = filtered.CopyToDataTable();
                        }
                        else
                        {
                            dtnew = dtJurnalInventory.Clone();
                        }
                    }

                    ExportJurnaldtExcel(dtnew);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void checkEditlt_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalInventory.Rows.Count > 0)
                {
                    if (checkEditlt.Checked == true)
                    {

                        checkEditlk.Checked = false;
                        ColumnView view = gridView_inv_header;
                        GridColumn colCategory = view.Columns["NOMOR"];
                        ColumnFilterInfo filter = new("Contains([NOMOR], '/LT')", string.Empty);
                        view.ActiveFilter.Add(colCategory, filter);
                    }
                    else
                    {

                        gridView_inv_header.Columns["NOMOR"].ClearFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void checkEditlk_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalInventory.Rows.Count > 0)
                {
                    if (checkEditlk.Checked == true)
                    {

                        checkEditlt.Checked = false;
                        ColumnView view = gridView_inv_header;
                        GridColumn colCategory = view.Columns["NOMOR"];
                        ColumnFilterInfo filter = new("Contains([NOMOR], '/LK')", string.Empty);
                        view.ActiveFilter.Add(colCategory, filter);
                    }
                    else
                    {

                        gridView_inv_header.Columns["NOMOR"].ClearFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        private void checkEditKAS_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalKasir.Rows.Count >0)
                {
                    if (checkEditKAS.Checked == true)
                    {

                        checkEditBANK.Checked = false;
                        ColumnView view = gridView_KasirHeader;
                        GridColumn colCategory = view.Columns["NOMOR"];
                        ColumnFilterInfo filter = new("Contains([NOMOR], '/KK') OR Contains([NOMOR], '/KT') ", string.Empty);
                        view.ActiveFilter.Add(colCategory, filter);
                    }
                    else
                    {

                        gridView_KasirHeader.Columns["NOMOR"].ClearFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void checkEditBANK_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalKasir.Rows.Count > 0)
                {
                    if (checkEditBANK.Checked == true)
                    {

                        checkEditKAS.Checked = false;
                        ColumnView view = gridView_KasirHeader;
                        GridColumn colCategory = view.Columns["NOMOR"];
                        ColumnFilterInfo filter = new("Contains([NOMOR], '/BK') OR Contains([NOMOR], '/BT')", string.Empty);
                        view.ActiveFilter.Add(colCategory, filter);
                    }
                    else
                    {

                        gridView_KasirHeader.Columns["NOMOR"].ClearFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        private void SETAHUNINV_EditValueChanged(object sender, EventArgs e)
        {
            Load_inv_header();
            Load_Jurnal_INV();
        }

        private void gcAISheader_Click(object sender, EventArgs e)
        {
            ShowJurnal_AIS_Detail();
        }

        private void ShowJurnal_AIS_Detail()
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            handle.QueueFocus(IntPtr.Zero);
            if (this.gridViewAISheader.GetFocusedRowCellValue("NOMOR") == null)
                return;
            if (this.gridViewAISheader.GetFocusedRowCellValue("TIPE_BOR").ToString() == "Y")
            {
                ShowDetailJurnal_AIS_BORONGAN();
            }
            else
            {
                //gcAISdetail.DataSource = null;
                ShowDetailJurnal_AIS_HARIAN();
            }
        }

        private void ShowDetailJurnal_AIS_HARIAN()
        {
            if (this.gridViewAISheader.GetFocusedRowCellValue("NOMOR") == null)
                return;
            
            int ptahun = Convert.ToInt32(setahunAIS.Value);
            int pbulan = cmbbulanAIS.SelectedIndex + 1;


            var p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("00"));
            var p_periode_str = pbulan.ToString("00")+"/"+ptahun.ToString() ;
            var p_iddata = CompanyInfo.INIT;
            var p_estate = lookUpEditAIS.EditValue.ToString();
            var pnomor = gridViewAISheader.GetFocusedRowCellValue("NOMOR").ToString();
            var p_remise = Convert.ToInt32(leremiseAIS.EditValue.ToString());
            var pDIVISI = gridViewAISheader.GetFocusedRowCellValue("DIVISI").ToString();

            DateTime TanggalJurnal;
            if (p_remise == 1)
            {
                TanggalJurnal = new DateTime(ptahun, pbulan, 15);
            }
            else
            {
                // Get the last day of month
                DateTime lastDayOfmonth = new DateTime(ptahun, pbulan, 1).AddMonths(1).AddDays(-1);
                TanggalJurnal = lastDayOfmonth;
            }
            
            dtAISDetail = JurnalFromModuleServices.AIS_Jurnal_Detail_HARIAN(pnomor, TanggalJurnal,p_periode_int, p_periode_str, AISptlokasi, p_estate,p_remise, p_iddata, pDIVISI);
            gcAISdetail.DataSource = dtAISDetail;
            gridViewAISdetail.Columns[0].Visible = false;
            gridViewAISdetail.Columns[1].Visible = false;
            gridViewAISdetail.Columns[8].Visible = false;
            gridViewAISdetail.Columns[9].Visible = false;
            gridViewAISdetail.Columns[5].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridViewAISdetail.Columns[5].DisplayFormat.FormatString = "n2";
            gridViewAISdetail.Columns[6].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridViewAISdetail.Columns[6].DisplayFormat.FormatString = "n2";
            gridViewAISdetail.Columns[5].Summary.Clear();
            gridViewAISdetail.Columns[6].Summary.Clear();
            gridViewAISdetail.Columns[5].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "DEBET", "{0:N2}");
            gridViewAISdetail.Columns[6].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "KREDIT", "{0:N2}");
            gridViewAISdetail.BestFitColumns();
        }

        private void ShowDetailJurnal_AIS_BORONGAN()
        {
            if (this.gridViewAISheader.GetFocusedRowCellValue("NOMOR") == null)
                return;

            int ptahun = Convert.ToInt32(setahunAIS.Value);
            int pbulan = cmbbulanAIS.SelectedIndex + 1;


            var p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("00"));
            var p_periode_str = pbulan.ToString("00") + "/" + ptahun.ToString();
            var p_iddata = CompanyInfo.INIT;
            var p_estate = lookUpEditAIS.EditValue.ToString();
            var pnomor = gridViewAISheader.GetFocusedRowCellValue("NOMOR").ToString();
            var p_remise = Convert.ToInt32(leremiseAIS.EditValue.ToString());
            var pDIVISI = gridViewAISheader.GetFocusedRowCellValue("DIVISI").ToString();

            DateTime TanggalJurnal;
            if (p_remise == 1)
            {
                TanggalJurnal = new DateTime(ptahun, pbulan, 15);
            }
            else
            {
                // Get the last day of month
                DateTime lastDayOfmonth = new DateTime(ptahun, pbulan, 1).AddMonths(1).AddDays(-1);
                TanggalJurnal = lastDayOfmonth;
            }

            dtAISDetail = JurnalFromModuleServices.AIS_Jurnal_Detail_BOR(pnomor, TanggalJurnal, p_periode_int, p_periode_str, AISptlokasi, p_estate, p_remise, p_iddata, pDIVISI);
            gcAISdetail.DataSource = dtAISDetail;
            gridViewAISdetail.Columns[0].Visible = false;
            gridViewAISdetail.Columns[1].Visible = false;
            gridViewAISdetail.Columns[8].Visible = false;
            gridViewAISdetail.Columns[9].Visible = false;
            gridViewAISdetail.Columns[5].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridViewAISdetail.Columns[5].DisplayFormat.FormatString = "n2";
            gridViewAISdetail.Columns[6].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridViewAISdetail.Columns[6].DisplayFormat.FormatString = "n2";
            gridViewAISdetail.Columns[5].Summary.Clear();
            gridViewAISdetail.Columns[6].Summary.Clear();
            gridViewAISdetail.Columns[5].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "DEBET", "{0:N2}");
            gridViewAISdetail.Columns[6].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "KREDIT", "{0:N2}");
            gridViewAISdetail.BestFitColumns();
            }

        private void gcAISheader_KeyUp(object sender, KeyEventArgs e)
        {
            ShowJurnal_AIS_Detail();
        }

        private void gridViewAISheader_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (IsJurnalTrue(gridViewAISheader, e.RowHandle))
            {
                e.Appearance.BackColor = Color.LightGray;
            }
            else if (IsJurnalTrueRevised(gridViewAISheader, e.RowHandle))
            {
                e.Appearance.BackColor = Color.LightCoral;
            }
        }

        private bool IsJurnalTrueRevised(GridView gridViewAISheader, int rowHandle)
        {
            try
            {
                string val = Convert.ToString(gridViewAISheader.GetRowCellValue(rowHandle, "JURNAL"));
                var nojurnal = Convert.ToString(gridViewAISheader.GetRowCellValue(rowHandle, "NOJURNAL"));
                return (val == "T" && !string.IsNullOrEmpty(nojurnal));
            }
            catch
            {
                return false;
            }
        }

        private bool IsJurnalTrue(GridView gridViewAISheader, int rowHandle)
        {
            try
            {
                string val = Convert.ToString(gridViewAISheader.GetRowCellValue(rowHandle, "JURNAL"));
                return (val == "Y");
            }
            catch
            {
                return false;
            }
        }

        private void leremiseAIS_EditValueChanged(object sender, EventArgs e)
        {
            Load_JurnalAIS_Periode();
        }

        private void sbExportExcelAIS_Click(object sender, EventArgs e)
        {
            if (dtAISDetail != null)
            {
                ExportJurnal_AIS_dtExcel(dtAISDetail);
            }
            else
            {
                MessageBox.Show("Pilih Nomor untuk mengExport ke Excel", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void gckasir_Detail_Click_1(object sender, EventArgs e)
        {

        }

        private void gridView_inv_header_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            try
            {
                GridView view = sender as GridView;
                if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
                {
                    int rowHandle = e.HitInfo.RowHandle;
                    //hapus menu jika ada
                    e.Menu.Items.Clear();

                    DXMenuItem JurnalSelected = CreateMenuItemJurnalInvSelected(view, rowHandle);
                    DXMenuItem exportselected = CreateMenuExportInvSelected(view, rowHandle);


                    JurnalSelected.BeginGroup = true;
                    exportselected.BeginGroup = true;

                    e.Menu.Items.Add(JurnalSelected);
                    e.Menu.Items.Add(exportselected);
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Popup Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DXMenuItem CreateMenuItemJurnalInvSelected(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Jurnal Item Terpilih", new EventHandler(OnInvJurnalClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[2];
            return checkItem;
        }

        private void OnInvJurnalClick(object? sender, EventArgs e)
        {
            List<string> selectedValues = new();

            // Iterate over the selected rows
            for (int i = 0; i < gridView_inv_header.SelectedRowsCount; i++)
            {
                // Get the selected row handle
                int rowHandle = gridView_inv_header.GetSelectedRows()[i];

                // Get the value from a specific column (replace "ColumnName" with the actual column name)
                string value = gridView_inv_header.GetRowCellValue(rowHandle, "NOMOR").ToString();

                // Add the value to the list
                selectedValues.Add(value);
            }
            // Create a formatted string containing the selected values
            // string message = "Selected Values:\n\n" + string.Join("\n", selectedValues);

            if (selectedValues.Any())
            {
                if (selectedValues.Count == 1)
                {
                    var periode=(CMBBULANINV.SelectedIndex+1).ToString("00")+"/"+SETAHUNINV.Value.ToString();
                    NoJurnaltxt.Text = gridView_inv_header.GetRowCellValue(gridView_inv_header.FocusedRowHandle, "NOMOR").ToString(); 
                    deJurnal.EditValue = Convert.ToDateTime(gridView_inv_header.GetRowCellValue(gridView_inv_header.FocusedRowHandle, "TANGGAL"));
                    cmbperiode.SelectedValue = periode;

                    List<JurnalDetailAdd> query = dtJurnalInventory.AsEnumerable()
                                                    .Where(d => selectedValues.Contains(d.Field<string>("NOJURNAL")))
                                                    .Select(n => new JurnalDetailAdd
                                                    {
                                                        BARIS = Convert.ToInt32(n.Field<decimal>("BARIS")),
                                                        Kode = n.Field<string>("KODE"),
                                                        Rekening = n.Field<string>("REKENING"),
                                                        Debet = n.Field<decimal>("DEBET"),
                                                        Kredit = n.Field<decimal>("KREDIT"),
                                                        Keterangan = n.Field<string>("KETERANGAN"),
                                                    })
                                                    .ToList();

                    InputJurnalDetail = new BindingList<JurnalDetailAdd>(query);

                    this.GCJurnal.DataSource = InputJurnalDetail;
                    InputJurnalDetail.AllowNew = true;
                    TABJurnal.SelectedTabPageIndex = 0;
                }
                else
                {
                    XtraMessageBox.Show("Pilih hanya satu Nomor untuk menjurnal ini", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            // Display the MessageBox with the selected values
            // XtraMessageBox.Show(message, "Selected Values", MessageBoxButtons.OK, MessageBoxIcon.Information);


            
        }

        private DXMenuItem CreateMenuExportInvSelected(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Export Item Terpilih", new EventHandler(OnInvExportClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }

        private void OnInvExportClick(object? sender, EventArgs e)
        {
            List<string> selectedValues = new();

            // Iterate over the selected rows
            for (int i = 0; i < gridView_inv_header.SelectedRowsCount; i++)
            {
                // Get the selected row handle
                int rowHandle = gridView_inv_header.GetSelectedRows()[i];

                // Get the value from a specific column (replace "ColumnName" with the actual column name)
                string value = gridView_inv_header.GetRowCellValue(rowHandle, "NOMOR").ToString();

                // Add the value to the list
                selectedValues.Add(value);
            }
            // Create a formatted string containing the selected values
            // string message = "Selected Values:\n\n" + string.Join("\n", selectedValues);

            if (selectedValues.Any())
            {
                ExportNomorInvetoryDipilih(selectedValues);
            }
            // Display the MessageBox with the selected values
            // XtraMessageBox.Show(message, "Selected Values", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void ExportNomorInvetoryDipilih(List<string> selectedValues)
        {
            try
            {
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (ExcelPackage package = new ExcelPackage())
                {
                    List<JurnalDetailDTO> selectedJurnalItems = dtJurnalInventory
                        .AsEnumerable()
                        .Where(j => selectedValues.Contains(j.Field<string>("NOJURNAL")))
                        .Select(j => new JurnalDetailDTO
                        {
                            // Map the properties of JurnalDetailDTO from the DataRow j
                            NoJurnal = j.Field<string>("NOJURNAL"),
                            Tanggal = j.Field<DateTime>("TANGGAL"),
                            BARIS = Convert.ToInt32(j.Field<decimal>("BARIS")),
                            Kode = j.Field<string>("KODE"),
                            Rekening = j.Field<string>("REKENING"),
                            Debet = j.Field<decimal>("DEBET"),
                            Kredit = j.Field<decimal>("KREDIT"),
                            Keterangan = j.Field<string>("KETERANGAN"),
                            Posted = j.Field<string>("POSTED"),
                            Periode = j.Field<string>("PERIODE")
                            // ...
                        })
                        .ToList();

                    var wsDt = package.Workbook.Worksheets.Add("Jurnal Entries");

                    // Load the datatable and set the number formats...
                    wsDt.Cells["A1"].LoadFromCollection(selectedJurnalItems, true);

                    wsDt.DeleteColumn(1, 2);

                    // Add the headers
                    wsDt.Cells[1, 1].Value = "NoJurnal";
                    wsDt.Cells[1, 2].Value = "Tanggal";
                    wsDt.Cells[1, 3].Value = "RowNo";
                    wsDt.Cells[1, 4].Value = "Kode";
                    wsDt.Cells[1, 5].Value = "Rekening";
                    wsDt.Cells[1, 6].Value = "Debet";
                    wsDt.Cells[1, 7].Value = "Kredit";
                    wsDt.Cells[1, 8].Value = "Keterangan";
                    wsDt.Cells[1, 9].Value = "Posted";
                    wsDt.Cells[1, 10].Value = "Periode";

                    wsDt.Cells[2, 2, selectedJurnalItems.Count + 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                    wsDt.Cells[2, 3, selectedJurnalItems.Count + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, selectedJurnalItems.Count + 1, 3).Address);

                    string positiveFormat = "#,##0.00_)";
                    string negativeFormat = "(#,##0.00)";
                    string zeroFormat = "-_)";
                    string numberFormat = positiveFormat + ";" + negativeFormat;
                    string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                    wsDt.Cells[2, 6, selectedJurnalItems.Count + 1, 7].Style.Numberformat.Format = fullNumberFormat;

                    wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();

                    // Obtain the Excel file data as a byte array
                    byte[] excelData = package.GetAsByteArray();

                    // Generate a temporary file path
                    string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

                    // Write the byte array to the temporary file
                    File.WriteAllBytes(tempFilePath, excelData);

                    // Open the temporary file with the default associated Excel program
                    ProcessStartInfo psi = new(tempFilePath)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Export Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void gc_inv_header_Click(object sender, EventArgs e)
        {
            LoadDataInventoryDetail();
        }

        private void gridView_inv_header_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up | e.KeyCode == Keys.Down)
            {
                LoadDataInventoryDetail();
            }
        }

        private void LoadDataInventoryDetail()
        {
            try
            {
                var filter = gridView_inv_header.GetRowCellValue(gridView_inv_header.FocusedRowHandle, "NOMOR").ToString();
            var filtered = dtJurnalInventory.AsEnumerable().Where(row => row.Field<string>("NOJURNAL") == filter).CopyToDataTable();
            GC_INV.DataSource = filtered;
            gridView_INVDetails.Columns[0].Visible = false;
            gridView_INVDetails.Columns[1].Visible = false;
            gridView_INVDetails.Columns[2].Visible = false;
            gridView_INVDetails.Columns[8].Visible = false;
            gridView_INVDetails.Columns[9].Visible = false;
            gridView_INVDetails.Columns[10].Visible = false;
            gridView_INVDetails.Columns[11].Visible = false;
            gridView_INVDetails.Columns[12].Visible = false;
            gridView_INVDetails.Columns[13].Visible = false;
            gridView_INVDetails.Columns[3].OptionsColumn.FixedWidth = true;
            gridView_INVDetails.Columns[3].Width = 90;
            //gridView_INVDetails.Columns[1].OptionsColumn.FixedWidth = true;
            //gridView_INVDetails.Columns[1].Width = 80;
            //gridView_INVDetails.Columns[3].OptionsColumn.FixedWidth = true;
            //gridView_INVDetails.Columns[3].Width = 90;
            gridView_INVDetails.Columns[5].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridView_INVDetails.Columns[5].DisplayFormat.FormatString = "n2";
            gridView_INVDetails.Columns[6].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            gridView_INVDetails.Columns[6].DisplayFormat.FormatString = "n2";
            gridView_INVDetails.Columns[5].Summary.Clear();
            gridView_INVDetails.Columns[6].Summary.Clear();
            gridView_INVDetails.Columns[5].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "DEBET", "{0:N2}");
            gridView_INVDetails.Columns[6].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "KREDIT", "{0:N2}");
            gridView_INVDetails.BestFitColumns();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on filter Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NoJurnaltxt_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void GC_KasirHeader_Click(object sender, EventArgs e)
        {
            ShowJurnalKasirDetail();
        }

        private void gridView_KasirHeader_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            try
            {
                GridView view = sender as GridView;
                if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
                {
                    int rowHandle = e.HitInfo.RowHandle;
                    //hapus menu jika ada
                    e.Menu.Items.Clear();

                    DXMenuItem JurnalSelected = CreateMenuItemJurnalKasirSelected(view, rowHandle);
                    DXMenuItem exportselected = CreateMenuExportKasirSelected(view, rowHandle);


                    JurnalSelected.BeginGroup = true;
                    exportselected.BeginGroup = true;

                    e.Menu.Items.Add(JurnalSelected);
                    e.Menu.Items.Add(exportselected);
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Popup Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DXMenuItem CreateMenuExportKasirSelected(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Export Terpilih", new EventHandler(OnKasirExportClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }

        private void OnKasirExportClick(object? sender, EventArgs e)
        {
            List<string> selectedValues = new();

            // Iterate over the selected rows
            for (int i = 0; i < gridView_KasirHeader.SelectedRowsCount; i++)
            {
                // Get the selected row handle
                int rowHandle = gridView_KasirHeader.GetSelectedRows()[i];

                // Get the value from a specific column (replace "ColumnName" with the actual column name)
                string value = gridView_KasirHeader.GetRowCellValue(rowHandle, "NOMOR").ToString();

                // Add the value to the list
                selectedValues.Add(value);
            }
            // Create a formatted string containing the selected values
            // string message = "Selected Values:\n\n" + string.Join("\n", selectedValues);

            if (selectedValues.Any())
            {
                ExportNomorKasirDipilih(selectedValues);
            }
            // Display the MessageBox with the selected values
            // XtraMessageBox.Show(message, "Selected Values", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void ExportNomorKasirDipilih(List<string> selectedValues)
        {
            try
            {
                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using ExcelPackage package = new();
                List<JurnalDetailDTO> selectedJurnalItems = dtJurnalKasir
                    .AsEnumerable()
                    .Where(j => selectedValues.Contains(j.Field<string>("NOJURNAL")))
                    .Select(j => new JurnalDetailDTO
                    {
                        // Map the properties of JurnalDetailDTO from the DataRow j
                        NoJurnal = j.Field<string>("NOJURNAL"),
                        Tanggal = j.Field<DateTime>("TANGGAL"),
                        BARIS = Convert.ToInt32(j.Field<decimal>("BARIS")),
                        Kode = j.Field<string>("KODE"),
                        Rekening = j.Field<string>("REKENING"),
                        Debet = j.Field<decimal>("DEBET"),
                        Kredit = j.Field<decimal>("KREDIT"),
                        Keterangan = j.Field<string>("KETERANGAN"),
                        Posted = j.Field<string>("POSTED"),
                        Periode = j.Field<string>("PERIODE")
                        // ...
                    })
                    .ToList();

                var wsDt = package.Workbook.Worksheets.Add("Jurnal Entries");

                // Load the datatable and set the number formats...
                wsDt.Cells["A1"].LoadFromCollection(selectedJurnalItems, true);

                wsDt.DeleteColumn(1, 2);

                // Add the headers
                wsDt.Cells[1, 1].Value = "NoJurnal";
                wsDt.Cells[1, 2].Value = "Tanggal";
                wsDt.Cells[1, 3].Value = "RowNo";
                wsDt.Cells[1, 4].Value = "Kode";
                wsDt.Cells[1, 5].Value = "Rekening";
                wsDt.Cells[1, 6].Value = "Debet";
                wsDt.Cells[1, 7].Value = "Kredit";
                wsDt.Cells[1, 8].Value = "Keterangan";
                wsDt.Cells[1, 9].Value = "Posted";
                wsDt.Cells[1, 10].Value = "Periode";

                wsDt.Cells[2, 2, selectedJurnalItems.Count + 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                wsDt.Cells[2, 3, selectedJurnalItems.Count + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, selectedJurnalItems.Count + 1, 3).Address);

                string positiveFormat = "#,##0.00_)";
                string negativeFormat = "(#,##0.00)";
                string zeroFormat = "-_)";
                string numberFormat = positiveFormat + ";" + negativeFormat;
                string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                wsDt.Cells[2, 6, selectedJurnalItems.Count + 1, 7].Style.Numberformat.Format = fullNumberFormat;

                wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();

                // Obtain the Excel file data as a byte array
                byte[] excelData = package.GetAsByteArray();

                // Generate a temporary file path
                string tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");

                // Write the byte array to the temporary file
                File.WriteAllBytes(tempFilePath, excelData);

                // Open the temporary file with the default associated Excel program
                ProcessStartInfo psi = new(tempFilePath)
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Export Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DXMenuItem CreateMenuItemJurnalKasirSelected(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Jurnal item Terpilih", new EventHandler(OnKasirJurnalClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[2];
            return checkItem;
        }

        private void OnKasirJurnalClick(object? sender, EventArgs e)
        {
            List<string> selectedValues = new();

            // Iterate over the selected rows
            for (int i = 0; i < gridView_KasirHeader.SelectedRowsCount; i++)
            {
                // Get the selected row handle
                int rowHandle = gridView_KasirHeader.GetSelectedRows()[i];

                // Get the value from a specific column (replace "ColumnName" with the actual column name)
                string value = gridView_KasirHeader.GetRowCellValue(rowHandle, "NOMOR").ToString();

                // Add the value to the list
                selectedValues.Add(value);
            }
            // Create a formatted string containing the selected values
            // string message = "Selected Values:\n\n" + string.Join("\n", selectedValues);

            if (selectedValues.Any())
            {
                if (selectedValues.Count == 1)
                {
                    var periode = (cmbbulankasir.SelectedIndex + 1).ToString("00") + "/" + setahunkasir.Value.ToString();
                    NoJurnaltxt.Text = gridView_KasirHeader.GetRowCellValue(gridView_KasirHeader.FocusedRowHandle, "NOMOR").ToString();
                    deJurnal.EditValue = Convert.ToDateTime(gridView_KasirHeader.GetRowCellValue(gridView_KasirHeader.FocusedRowHandle, "TANGGAL"));
                    cmbperiode.SelectedValue = periode;

                    List<JurnalDetailAdd> query = dtJurnalKasir.AsEnumerable()
                                                    .Where(d => selectedValues.Contains(d.Field<string>("NOJURNAL")))
                                                    .Select(n => new JurnalDetailAdd
                                                    {
                                                        BARIS = Convert.ToInt32(n.Field<decimal>("BARIS")),
                                                        Kode = n.Field<string>("KODE"),
                                                        Rekening = n.Field<string>("REKENING"),
                                                        Debet = n.Field<decimal>("DEBET"),
                                                        Kredit = n.Field<decimal>("KREDIT"),
                                                        Keterangan = n.Field<string>("KETERANGAN"),
                                                    })
                                                    .ToList();

                    InputJurnalDetail = new BindingList<JurnalDetailAdd>(query);

                    this.GCJurnal.DataSource = InputJurnalDetail;
                    InputJurnalDetail.AllowNew = true;
                    TABJurnal.SelectedTabPageIndex = 0;
                }
                else
                {
                    XtraMessageBox.Show("Pilih hanya satu Nomor untuk menjurnal ini", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            // Display the MessageBox with the selected values
            // XtraMessageBox.Show(message, "Selected Values", MessageBoxButtons.OK, MessageBoxIcon.Information);


        }

        private void repositoryItemTextEdit_KODE_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter )
            {
                
            }
        }


        private void gridView_KasirHeader_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up | e.KeyCode == Keys.Down)
            {
                ShowJurnalKasirDetail();
            }
        }

        private void ShowJurnalKasirDetail()
        {
            try
            {
                var filter = gridView_KasirHeader.GetRowCellValue(gridView_KasirHeader.FocusedRowHandle, "NOMOR").ToString();
                var filtered = dtJurnalKasir.AsEnumerable().Where(row => row.Field<string>("NOJURNAL") == filter).CopyToDataTable();
                gckasir_Detail.DataSource = filtered;
                gridView_kasir.Columns[0].Visible = false;
                gridView_kasir.Columns[1].Visible = false;
                gridView_kasir.Columns[2].Visible = false;
                gridView_kasir.Columns[8].Visible = false;
                gridView_kasir.Columns[9].Visible = false;
                gridView_kasir.Columns[10].Visible = false;
                gridView_kasir.Columns[11].Visible = false;
                gridView_kasir.Columns[12].Visible = false;
                gridView_kasir.Columns[13].Visible = false;
                gridView_kasir.Columns[3].OptionsColumn.FixedWidth = true;
                gridView_kasir.Columns[3].Width = 90;
                ////gridView_kasir.Columns[1].OptionsColumn.FixedWidth = true;
                ////gridView_kasir.Columns[1].Width = 80;
                ////gridView_kasir.Columns[3].OptionsColumn.FixedWidth = true;
                ////gridView_kasir.Columns[3].Width = 90;
                gridView_kasir.Columns[5].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gridView_kasir.Columns[5].DisplayFormat.FormatString = "n2";
                gridView_kasir.Columns[6].DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gridView_kasir.Columns[6].DisplayFormat.FormatString = "n2";
                gridView_kasir.Columns[5].Summary.Clear();
                gridView_kasir.Columns[6].Summary.Clear();
                gridView_kasir.Columns[5].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "DEBET", "{0:N2}");
                gridView_kasir.Columns[6].Summary.Add(DevExpress.Data.SummaryItemType.Sum, "KREDIT", "{0:N2}");
                gridView_kasir.BestFitColumns();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on filter Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lookUpEditAIS_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);
                LookUpEdit editor = ((LookUpEdit)sender);
                DataRowView row = (DataRowView)editor.Properties.GetDataSourceRowByKeyValue(editor.EditValue);
                AISptlokasi = row["PTLOKASI"].ToString();
                Load_JurnalAIS_Periode();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void Load_JurnalAIS_Periode()
        {
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);
                if (lookUpEditAIS.EditValue != null)
                {
                    if (setahunAIS.Value != 0)
                    {
                        int ptahun = Convert.ToInt32(setahunAIS.Value);
                        int pbulan = cmbbulanAIS.SelectedIndex + 1;
                        int p_remise = Convert.ToInt16(leremiseAIS.EditValue.ToString());

                        var p_periode_int = ptahun.ToString() + pbulan.ToString("00");
                        var p_estate = lookUpEditAIS.EditValue.ToString();
                        var p_iddata = CompanyInfo.INIT;



                        //jika periode telah dikunci,  batalkan proses import jurnal
                        var p_periode_str = pbulan.ToString("0#") + "/" + ptahun.ToString();
                        dtAISHeader = JurnalFromModuleServices.AIS_Jurnal_Header(Convert.ToInt32(p_periode_int), AISptlokasi, p_estate, p_remise);
                        gcAISheader.DataSource = dtAISHeader;
                        gridViewAISheader.Columns[3].Visible = false;
                        gridViewAISheader.Columns[4].Visible = false;
                        gridViewAISheader.BestFitColumns();
                        gcAISdetail.DataSource = null;

                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Load_JurnalAIS_Periode", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
        
        private void sbexportexcel_Click(object sender, EventArgs e)
        {
            try
            {

                if (dtJurnalKasir == null || dtJurnalKasir.Rows.Count==0)
                {
                    return;
                }
                if (dtJurnalKasir.Rows.Count > 0)
                {
                    DataTable dtnew = new();
                    if (checkEditKAS.Checked == false && checkEditBANK.Checked == false)
                    {
                        dtnew = dtJurnalKasir;
                    }
                    else if (checkEditKAS.Checked == true)
                    {
                        var filtered = dtJurnalKasir.AsEnumerable().Where(y => y.Field<string>("NOJURNAL").Contains("/KK") || y.Field<string>("NOJURNAL").Contains("/KT"));
                        if (filtered.Any())
                        {
                            dtnew = filtered.CopyToDataTable();
                        }
                        else
                        {
                            dtnew = dtJurnalKasir.Clone();
                        }
                    }
                    else if (checkEditBANK.Checked == true)
                    {
                        var filtered = dtJurnalKasir.AsEnumerable().Where(y => y.Field<string>("NOJURNAL").Contains("/BK") || y.Field<string>("NOJURNAL").Contains("/BT"));
                        if (filtered.Any())
                        {
                            dtnew = filtered.CopyToDataTable();
                        }
                        else
                        {
                            dtnew = dtJurnalKasir.Clone();
                        }
                    }

                    ExportJurnaldtExcel(dtnew);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void Sbfilterclear_Click(object sender, EventArgs e)
        {
            try
            {
                txtfilternojurnal.Text = "";
                defiltertanggal.Text = "";
                txtfilterkode.Text = "";
                txtfilterjumlah.Text = "0";
                txtfilterketerangan.Text = "";
                lblrecordbulan.Visible = false;
                filter = false;
                PencarianJurnal_Bulan = Enumerable.Empty<JurnalDetailDTO>();
                ExportPencarian_Bulan = Enumerable.Empty<JurnalDetailDTO>();
                // GCHeader.DataSource = JurnalHeader;
                // GCDetails.DataSource = null;
                PilihanPeriodeAkuntansi();
                GCHeader.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void txtfilterjumlah_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Bulan();
            }
        }

        private void txtfilterketerangan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Bulan();
            }
        }

        private void sbexport_Click(object sender, EventArgs e)
        {
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            try
            {                
                if (PencarianJurnal.Any())
                {


                    if (!string.IsNullOrEmpty(cmbcariperiode.Text))
                    {
                        ExportPencarian = PencarianJurnal.Where(p => p.Periode == cmbcariperiode.Text);
                        ReffID = ExportPencarian.Select(x => new JurnalDetailReffID { REFFID = x.REFFID }).Distinct().ToList();
                    }
                    else
                    {
                        ExportPencarian = PencarianJurnal;
                        ReffID = ExportPencarian.Select(x => new JurnalDetailReffID { REFFID = x.REFFID }).Distinct().ToList();
                    }

                    if (celengkap.Checked == true)
                    {
                        if (ReffID.Count() > 999)
                        {
                            
                            MessageBox.Show("Record Jurnal lengkap terlalu banyak, silahkan persempit filter pencarian", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            SplashScreenManager.CloseOverlayForm(handle);
                            return;
                        }
                   
                        ExportPencarian = GetJurnalLengkap(ReffID);
                    }


                    // If you use EPPlus in a noncommercial context
                    // according to the Polyform Noncommercial license:
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    string filename = string.Empty;
                    using (ExcelPackage package = new())
                    {


                        var wsDt = package.Workbook.Worksheets.Add("Jurnal Entries");

                        //Load the datatable and set the number formats...
                        wsDt.Cells["A1"].LoadFromCollection(ExportPencarian, true);
                        //wsDt.Cells["A1"].LoadFromCollection(GetJurnalLengkap(ReffID), true);

                        //wsDt.Cells["A1"].LoadFromCollection(mydata);
                        wsDt.DeleteColumn(1,2);

                        //Add the headers
                        wsDt.Cells[1, 1].Value = "NoJurnal";
                        wsDt.Cells[1, 2].Value = "Tanggal";
                        wsDt.Cells[1, 3].Value = "RowNo";
                        wsDt.Cells[1, 4].Value = "Kode";
                        wsDt.Cells[1, 5].Value = "Rekening";
                        wsDt.Cells[1, 6].Value = "Debet";
                        wsDt.Cells[1, 7].Value = "Kredit";
                        wsDt.Cells[1, 8].Value = "Keterangan";
                        wsDt.Cells[1, 9].Value = "Posted";
                        wsDt.Cells[1, 10].Value = "Periode";

                        wsDt.Cells[2, 2, ExportPencarian.Count() + 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
                        //=IF(A2<>A1,1,C1+1)
                        wsDt.Cells[2, 3, ExportPencarian.Count() + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, ExportPencarian.Count() + 1, 3).Address);

                        // number formats
                        string positiveFormat = "#,##0.00_)";
                        string negativeFormat = "(#,##0.00)";
                        string zeroFormat = "-_)";
                        string numberFormat = positiveFormat + ";" + negativeFormat;
                        string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                        wsDt.Cells[2, 6, ExportPencarian.Count() + 1, 7].Style.Numberformat.Format = fullNumberFormat;
                        //wsDt.Cells[2, 7, dt.Rows.Count + 1, 7].Style.Numberformat.Format = "#,##0.00";

                        // 
                        wsDt.Cells[wsDt.Dimension.Address].AutoFitColumns();
                        //package.Save();
                        // package.Dispose();

                        Byte[] bin = package.GetAsByteArray();
                        string tempPath = Path.GetTempPath();
                        filename = tempPath + CompanyInfo.INIT + "SearchJurnal.xlsx";
                        File.WriteAllBytes(@filename, bin);

                    }
                    ProcessStartInfo psi = new(@filename)
                    {
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                else
                {
                    MessageBox.Show("Data tidak tersedia", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void JDgridView_ClipboardRowPasting(object sender, ClipboardRowPastingEventArgs e)
        {
            var view = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            var cells = view.GetSelectedCells() as DevExpress.XtraGrid.Views.Base.GridCell[];

            if (cells.Length <= 1 || e.Values.Count > 1 || System.Windows.Forms.Clipboard.GetText().Contains(System.Environment.NewLine))
                return;

            e.Cancel = true;
            for (int i = 0; i < cells.Length; i++)
            {
                if (!cells[i].Column.ReadOnly)
                    view.SetRowCellValue(cells[i].RowHandle, cells[i].Column, e.OriginalValues[0]);
            }
        }


        private void deJurnal_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {


                if (e.KeyCode == Keys.Enter)
                {
                    if (string.IsNullOrEmpty(deJurnal.Text)) return;
                    if (Acct.TahunMax < Convert.ToDateTime(deJurnal.Text).Year)
                    {
                        XtraMessageBox.Show("Daftar Perkiraan Akun belum ada...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        GCJurnal.Enabled = false;
                        return;
                    }
                    GCJurnal.Enabled = true;
                    if (editjurnal == false)
                    {
                        if (!NoJurnaltxt.Text.Contains("/ND") && !NoJurnaltxt.Text.Contains("/NK"))
                        {
                            bool nomorexist = JurnalServices.CekNoJurnalExist_input(CompanyInfo.INIT, NoJurnaltxt.Text.ToUpper(), cmbperiode.Text);
                            if (nomorexist)
                            {
                                XtraMessageBox.Show("Nomor Jurnal : " + NoJurnaltxt.Text + " Sudah ada...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                NoJurnaltxt.Select();
                                return;
                            }
                        }
                    }

                    SendKeys.Send("{TAB}");
                    // Obtain created columns.
                    //GridColumn colKODE = JDgridView.Columns["KODE"];

                    // Focus a specific cell.
                    JDgridView.FocusedRowHandle = 0;
                    JDgridView.FocusedColumn = JDgridView.Columns["Kode"];
                }
            }
            catch (SystemException ex)
            {
                if(ex.Message.Contains("valid DateTime"))
                {
                    XtraMessageBox.Show("Tanggal Salah", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
               
            }
        }

        private void deJurnal_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (deJurnal.EditValue != null)
                {
                    string[] bulanbi = { "Bulan", "Januari", "Pebruari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" };

                    pbulan = DateTime.Parse(deJurnal.Text).Month;
                    ptahun = DateTime.Parse(deJurnal.Text).Year;
                    Load_PeriodeList(CompanyInfo.INIT, ptahun.ToString());

                    // cmbbulan.SelectedIndex = pbulan - 1;
                    // setahun.Value = ptahun;
                    periodetujuan = pbulan.ToString("00") + "/" + ptahun;
                    cmbperiode.SelectedValue = periodetujuan;


                    var Periode = bulanbi[pbulan].ToString() + " - " + ptahun.ToString();
                    //cek apakah periode yg dipilih ada, jika ada tombol proses aktif, jika tidak tombol proses disable
                    var ada = AccountServices.CekPeriodeExist(CompanyInfo.INIT, pbulan, ptahun);
                    if (ada == 0)
                    {
                        XtraMessageBox.Show("Periode Akuntansi: " + Periode + "\nPilihan Periode belum tersedia...!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SBSimpan.Enabled = false;
                    }
                    else
                    {
                        SBSimpan.Enabled = true;
                    }

                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void JDgridView_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            GridView view = sender as GridView;
            // Get the summary ID. 
            int summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);

            // Initialization. 
            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {

                selisihD = 0;
                selisihK = 0;
            }
            // Calculation.
            if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            {


                switch (summaryID)
                {
                    case 1: // The total summary calculated against the 'UnitPrice' column. 
                        var Debet = Convert.ToDouble(JDgridView.Columns["Debet"].SummaryItem.SummaryValue.ToString());
                        var Kredit = Convert.ToDouble(JDgridView.Columns["Kredit"].SummaryItem.SummaryValue.ToString());
                        //double nilai,nilai2;
                        if (Debet > Kredit)
                        {
                            nilai = 0;
                            nilai2 = Debet - Kredit;
                        }
                        else
                        {
                            nilai = Kredit - Debet;
                            nilai2 = 0;
                        }
                        selisihD = nilai;
                        break;
                    case 2: // The group summary. 

                        //selisihD = 0;
                        selisihK = nilai2;
                        break;
                }
            }
            // Finalization. 
            if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            {
                switch (summaryID)
                {
                    case 1:
                        e.TotalValue = selisihD;
                        break;
                    case 2:
                        e.TotalValue = selisihK;
                        break;
                }
            }
        }

    

        private void cmbbulan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }



        private void ribhapus_Click(object sender, EventArgs e)
        {
            JDgridView.DeleteRow(JDgridView.FocusedRowHandle);
            x--;
            XtraMessageBox.Show("Deleted");
        }
        private DataTable PeriodeListAll(string piddata)
        {
            using (OracleCommand _command = new ("select tahun,periode from acct_periode where iddata=:piddata order by tahun,bulan,periode desc ", conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":piddata", OracleDbType.Varchar2, 40).Value = piddata;
                OracleDataReader dr;
                dr = _command.ExecuteReader();
                DataTable _dt = new ();
                _dt.Load(dr);
                dr.Close();
                conn.Close();
                return _dt;
            }
        }

    }
}