using Accounting.BusinessLayer;
using Accounting.Laporan;
using Accounting.Model;
using DevExpress.Charts.Native;
using DevExpress.Data.Helpers;
using DevExpress.Data.Linq;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using DevExpress.XtraSplashScreen;
using OfficeOpenXml;
using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accounting.Services;

namespace Accounting.Form
{
    public partial class FrmAkunEF_Luas : DevExpress.XtraEditors.XtraForm
    {
        int pbulan, p_sampaibulan, ptahun;
        string fileName;
        private readonly Timer recalcStatusTimer = new() { Interval = 3000 };
        private readonly SimpleButton refreshManualButton = new();
        private readonly SimpleButton previousPeriodButton = new();
        private readonly SimpleButton nextPeriodButton = new();
        private CoaHeaderHandle headerLayout;
        private CoaPeriodNavigator? periodNavigator;
        private long? monitoredRecalcJobId;
        private DateTime monitoredRecalcJobStartUtc;
        private bool isStatusCheckInProgress;
        private bool isSynchronizingPeriodControls;
        private const int RecalcPollingTimeoutSeconds = 180;

        public FrmAkunEF_Luas()
        {
            InitializeComponent();
            InitializePeriodNavigationButtons();
            InitializeManualRefreshButton();
            ConfigureResponsiveLayout();
            recalcStatusTimer.Tick += RecalcStatusTimer_Tick;
            JurnalRekalkulasiNotifier.JobQueued += OnJurnalRekalkulasiJobQueued;
            FormClosed += FrmAkunEF_Luas_FormClosed;
        }
        DataSet DSGL;

        private void UpdateFromNew(object sender, FrmAkunAdd.UpdateEventArgs args)
        {
            Load_COA();
        }
        private void UpdateFromEdit(object sender, FrmAkunEdit.UpdateEventArgs args)
        {
            Load_COA();
        }

        private void ApplyAuthorizationState()
        {
            sbadd.Enabled = AuthorizationService.CanCreateCoa();
            sbubah.Enabled = AuthorizationService.CanUpdateCoa();
            sbhapus.Enabled = AuthorizationService.CanDeleteCoa();
            sbexport.Enabled = AuthorizationService.CanExportCoa();
        }

        private void FrmAkunEF_Luas_Load(object sender, EventArgs e)
        {
            try
            {
                if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanViewCoaWorkspace))
                {
                    Close();
                    return;
                }
                InitializePeriodNavigation();
                Load_COA();
                ApplyAuthorizationState();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
           
        }

        private int ActiveMonth => periodNavigator?.Current.Month ?? 0;

        private int ActiveYear => periodNavigator?.Current.Year ?? 0;

        private CoaPeriod ActivePeriod =>
            periodNavigator?.Current ?? throw new InvalidOperationException("Periode COA belum diinisialisasi.");

        private void InitializePeriodNavigationButtons()
        {
            previousPeriodButton.Name = "sbPeriodPrev";
            previousPeriodButton.Text = "Prev";
            previousPeriodButton.Enabled = false;
            previousPeriodButton.Click += (_, _) => NavigatePeriod(-1);

            nextPeriodButton.Name = "sbPeriodNext";
            nextPeriodButton.Text = "Next";
            nextPeriodButton.Enabled = false;
            nextPeriodButton.Click += (_, _) => NavigatePeriod(1);
        }

        private void InitializePeriodNavigation()
        {
            periodNavigator = new CoaPeriodNavigator(Acct.PeriodeMin, Acct.PeriodeMax);

            cmbbulan.Properties.Items.Clear();
            cmbbulan.Properties.Items.AddRange(CoaPeriod.IndonesianMonthNames.ToArray());
            setahun.Properties.MinValue = periodNavigator.Minimum.Year;
            setahun.Properties.MaxValue = periodNavigator.Maximum.Year;

            SynchronizePeriodControls();
        }

        private void NavigatePeriod(int months)
        {
            if (periodNavigator?.TryMoveByMonths(months) != true)
            {
                return;
            }

            SynchronizePeriodControls();
            Load_COA();
        }

        private void ApplyManualPeriodSelection()
        {
            if (isSynchronizingPeriodControls || periodNavigator == null || cmbbulan.SelectedIndex < 0 || setahun.Value == 0)
            {
                return;
            }

            CoaPeriod candidate = new(Convert.ToInt32(setahun.Value), cmbbulan.SelectedIndex + 1);
            if (!periodNavigator.TrySetCurrent(candidate, out bool changed))
            {
                SynchronizePeriodControls();
                return;
            }

            UpdatePeriodNavigationButtons();
            if (changed)
            {
                Load_COA();
            }
        }

        private void SynchronizePeriodControls()
        {
            if (periodNavigator == null)
            {
                return;
            }

            isSynchronizingPeriodControls = true;
            try
            {
                cmbbulan.SelectedIndex = periodNavigator.Current.Month - 1;
                setahun.Value = periodNavigator.Current.Year;
            }
            finally
            {
                isSynchronizingPeriodControls = false;
            }

            UpdatePeriodNavigationButtons();
        }

        private void UpdatePeriodNavigationButtons()
        {
            previousPeriodButton.Enabled = periodNavigator?.CanMovePrevious == true;
            nextPeriodButton.Enabled = periodNavigator?.CanMoveNext == true;
        }

        private void InitializeManualRefreshButton()
        {
            refreshManualButton.Name = "sbRefreshManual";
            refreshManualButton.Text = "Refresh";
            refreshManualButton.Appearance.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            refreshManualButton.Appearance.Options.UseFont = true;
            if (imageCollection1.Images.Count > 1)
            {
                refreshManualButton.ImageOptions.Image = imageCollection1.Images[1];
            }
            refreshManualButton.Click += (_, _) => Load_COA();
        }

        private void ConfigureResponsiveLayout()
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            panelControl1.Dock = DockStyle.Fill;
            gridControl1.Dock = DockStyle.Fill;
            gridControl2.Visible = false;

            headerLayout = CoaHeaderLayout.Apply(
                this,
                sidePanel1,
                cmbbulan, setahun,
                previousPeriodButton, nextPeriodButton,
                sbadd, sbubah, sbhapus, sbexport, refreshManualButton,
                gridView1);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            headerLayout?.Relayout();
        }

        protected override void OnDpiChangedAfterParent(EventArgs e)
        {
            base.OnDpiChangedAfterParent(e);
            headerLayout?.Relayout();
        }

        private void FrmAkunEF_Luas_FormClosed(object? sender, FormClosedEventArgs e)
        {
            headerLayout?.Dispose();
            recalcStatusTimer.Stop();
            recalcStatusTimer.Tick -= RecalcStatusTimer_Tick;
            JurnalRekalkulasiNotifier.JobQueued -= OnJurnalRekalkulasiJobQueued;
        }

        private void OnJurnalRekalkulasiJobQueued(object? sender, JurnalRekalkulasiQueuedEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnJurnalRekalkulasiJobQueued(sender, e)));
                return;
            }

            if (!string.Equals(e.IdData, CompanyInfo.IDDATA, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!IsPeriodeMatch(e.Periode))
            {
                Log.Debug(
                    "COA AutoRefresh skipped_context_mismatch form=FrmAkunEF_Luas job_id={JobId} event_periode={EventPeriode} active_bulan={ActiveBulan} active_tahun={ActiveTahun}",
                    e.JobId,
                    e.Periode,
                    ActiveMonth,
                    ActiveYear);
                return;
            }

            monitoredRecalcJobId = e.JobId;
            monitoredRecalcJobStartUtc = DateTime.UtcNow;
            Log.Information(
                "COA AutoRefresh watch_started form=FrmAkunEF_Luas job_id={JobId} periode={Periode} impacted_count={ImpactedCount}",
                e.JobId,
                e.Periode,
                e.ImpactedAccountCodes?.Count ?? 0);
            recalcStatusTimer.Start();
        }

        private bool IsPeriodeMatch(string periode)
        {
            if (string.IsNullOrWhiteSpace(periode))
            {
                return false;
            }

            if (!DateTime.TryParseExact(periode, "MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedPeriode))
            {
                return false;
            }

            return parsedPeriode.Month == ActiveMonth && parsedPeriode.Year == ActiveYear;
        }

        private async void RecalcStatusTimer_Tick(object? sender, EventArgs e)
        {
            if (isStatusCheckInProgress)
            {
                return;
            }

            if (!monitoredRecalcJobId.HasValue)
            {
                recalcStatusTimer.Stop();
                return;
            }

            if ((DateTime.UtcNow - monitoredRecalcJobStartUtc).TotalSeconds > RecalcPollingTimeoutSeconds)
            {
                recalcStatusTimer.Stop();
                long timeoutJobId = monitoredRecalcJobId.Value;
                monitoredRecalcJobId = null;
                Log.Warning(
                    "COA AutoRefresh watch_timeout form=FrmAkunEF_Luas job_id={JobId} timeout_seconds={TimeoutSeconds}",
                    timeoutJobId,
                    RecalcPollingTimeoutSeconds);
                XtraMessageBox.Show(
                    "Rekalkulasi belum selesai dalam batas waktu. Silakan gunakan tombol Refresh.",
                    "Info Rekalkulasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            isStatusCheckInProgress = true;
            try
            {
                long jobId = monitoredRecalcJobId.Value;
                RekalkulasiJobStatusSnapshot? status = await Task.Run(() => JurnalInputOperationService.GetRekalkulasiJobStatus(jobId));
                if (status == null)
                {
                    Log.Debug("COA AutoRefresh status_not_found form=FrmAkunEF_Luas job_id={JobId}", jobId);
                    return;
                }

                string normalizedStatus = (status.Status ?? string.Empty).Trim().ToUpperInvariant();
                Log.Debug(
                    "COA AutoRefresh status_polled form=FrmAkunEF_Luas job_id={JobId} status={Status}",
                    jobId,
                    normalizedStatus);
                if (normalizedStatus == "DONE")
                {
                    recalcStatusTimer.Stop();
                    monitoredRecalcJobId = null;
                    Log.Information("COA AutoRefresh status_done form=FrmAkunEF_Luas job_id={JobId}", jobId);
                    Load_COA();
                    return;
                }

                if (normalizedStatus == "FAILED")
                {
                    recalcStatusTimer.Stop();
                    monitoredRecalcJobId = null;
                    Log.Warning(
                        "COA AutoRefresh status_failed form=FrmAkunEF_Luas job_id={JobId} error={LastError}",
                        jobId,
                        status.LastError);
                    string pesan = string.IsNullOrWhiteSpace(status.LastError)
                        ? "Rekalkulasi gagal. Silakan refresh manual."
                        : $"Rekalkulasi gagal: {status.LastError}";
                    XtraMessageBox.Show(pesan, "Info Rekalkulasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                isStatusCheckInProgress = false;
            }
        }

        DataTable tbl_Coa = new();
        private void Load_COA()
        {
            if (CompanyInfo.IDDATA == "KSKINTI")
            {
                EntityInstantFeedbackSource eifs = new();
                eifs.KeyExpression = "ID";
                eifs.GetQueryable += Dapper_GetQueryable;
                gridControl1.DataSource = eifs;
            }
            else
            {
                var p_iddata =CompanyInfo.IDDATA;
                var p_tahun = ActiveYear;
                var p_bulan = ActiveMonth;
                if (p_tahun != 0 && p_bulan != 0)
                {
                    var data = AccountServices.GetPerkiraanSaldo_ADO(p_iddata, p_tahun, p_bulan);
                    gridControl1.DataSource = data;
                }
            }
            
        }

        private void Dapper_GetQueryable(object sender, GetQueryableEventArgs e)
        {
            var p_iddata =CompanyInfo.IDDATA;
            var p_tahun = ActiveYear;
            var p_bulan = ActiveMonth;
            if (p_tahun != 0 && p_bulan != 0)
            {
                var data = AccountServices.GetPerkiraanSaldo_Dapper(p_iddata, p_tahun, p_bulan);
                e.QueryableSource =data;
            }
        }


        List<BlokAccount> GetDataRows(ColumnView view)
        {
            if (view == null) return null;

            List<BlokAccount> rowList = new List<BlokAccount>();
            for (int i = 0; i < view.DataRowCount; i++)
                rowList.Add(view.GetRow(i) as BlokAccount);

            return rowList;
        }
        private void sbexport_Click(object sender, EventArgs e)
        {
                if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanExportCoa))
                {
                    return;
                }
               

                fileName =CompanyInfo.IDDATA + "DaftarPerkiraan.xlsx";

                if (CompanyInfo.JENIS_AKUNTING == "KEBUN")
                {
                    gridView1.Columns["AWALTAHUN"].VisibleIndex = 7;
                    gridView1.Columns["SALDOAWAL"].VisibleIndex = 8;
                    gridView1.Columns["DEBET"].VisibleIndex = 9;
                    gridView1.Columns["KREDIT"].VisibleIndex = 10;
                    gridView1.Columns["MUTASI"].VisibleIndex = 11;
                    gridView1.Columns["SALDOAKHIR"].VisibleIndex = 12;
                    gridView1.Columns["DIVISI"].VisibleIndex = 13;
                    gridView1.Columns["BLOK"].VisibleIndex = 14;
                    gridView1.Columns["TAHUNTANAM"].VisibleIndex = 15;

                    gridView1.Columns["AWALTAHUN"].Visible = true;
                    gridView1.Columns["MUTASI"].Visible = true;
                    gridView1.Columns["DIVISI"].Visible = true;
                    gridView1.Columns["BLOK"].Visible = true;
                    gridView1.Columns["TAHUNTANAM"].Visible = true;

                   



                }
                else
                {
                    gridView1.Columns["AWALTAHUN"].VisibleIndex = 7;
                    gridView1.Columns["SALDOAWAL"].VisibleIndex = 8;
                    gridView1.Columns["DEBET"].VisibleIndex = 9;
                    gridView1.Columns["KREDIT"].VisibleIndex = 10;
                    gridView1.Columns["MUTASI"].VisibleIndex = 11;
                    gridView1.Columns["SALDOAKHIR"].VisibleIndex = 12;
                    gridView1.Columns["AWALTAHUN"].Visible = true;
                    gridView1.Columns["MUTASI"].Visible = true;
                }

                //gridView1.BestFitColumns();
                string sheetname;
                CoaPeriod exportPeriod = ActivePeriod;
                sheetname = CompanyInfo.IDDATA + exportPeriod.MonthName + exportPeriod.Year;

                XlsxExportOptionsEx xlsxOptions = new XlsxExportOptionsEx
                {
                    ShowGridLines = true,
                    SheetName = sheetname,
                    ExportType = DevExpress.Export.ExportType.Default,    // ExportType
                    TextExportMode = TextExportMode.Value,
                    RawDataMode = true
                };
                gridControl1.ExportToXlsx(@fileName, xlsxOptions);


                gridView1.Columns["AWALTAHUN"].Visible = false;
                gridView1.Columns["MUTASI"].Visible = false;
                gridView1.Columns["DIVISI"].Visible = false;
                gridView1.Columns["BLOK"].Visible = false;
                gridView1.Columns["TAHUNTANAM"].Visible = false;

                // number formats
                string positiveFormat = "#,##0.00_)";
                string negativeFormat = "[Red](#,##0.00)";
                string zeroFormat = "-_)";
                string numberFormat = positiveFormat + ";" + negativeFormat;
                string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;

                //Opening an existing Excel file
                FileInfo fi = new FileInfo(@fileName);


                // If you use EPPlus in a noncommercial context
                // according to the Polyform Noncommercial license:
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage excelPackage = new(fi))
                {

                    ExcelWorksheet namedWorksheet = excelPackage.Workbook.Worksheets[0];

                    namedWorksheet.Column(1).Width = 12;
                    namedWorksheet.Column(2).Width = 60;
                    namedWorksheet.Column(8).Style.Numberformat.Format = fullNumberFormat;
                    namedWorksheet.Column(9).Style.Numberformat.Format = fullNumberFormat;
                    namedWorksheet.Column(10).Style.Numberformat.Format = fullNumberFormat;
                    namedWorksheet.Column(11).Style.Numberformat.Format = fullNumberFormat;
                    namedWorksheet.Column(12).Style.Numberformat.Format = fullNumberFormat;
                    namedWorksheet.Column(13).Style.Numberformat.Format = fullNumberFormat;

                    namedWorksheet.Cells[1, 3, 150, 17].AutoFitColumns();
                    //Save your file
                    excelPackage.Save();
                    excelPackage.Dispose();
                }
                ProcessStartInfo psi = new ProcessStartInfo(@fileName);
                psi.UseShellExecute = true;
                Process.Start(psi);
               
           
        }


        private void sbadd_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanCreateCoa))
            {
                return;
            }
            //try
            //{
            //    //2 kode buka kode perkiraan
            //    bool akses = LevelAksesServices.BaruImport(2, LoginInfo.userID);
            //    if (akses == false)
            //    {
            //        XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //    FrmAkunAdd form = new FrmAkunAdd(this)
            //    {
            //        //MdiParent = this,
            //        StartPosition = FormStartPosition.CenterScreen
            //    };
            //    form.UpdateEventHandler += UpdateFromNew;
            //    form.ShowDialog();
            //}
            //catch (SystemException ex)
            //{
            //    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

        }
        private void sbubah_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanUpdateCoa))
            {
                return;
            }
            //try
            //{
            //    //2 kode buka kode perkiraan
            //    bool akses = LevelAksesServices.Ubah(2, LoginInfo.userID);
            //    if (akses == false)
            //    {
            //        XtraMessageBox.Show("UserID : " + LoginInfo.userID + "\nAnda Tidak memiliki Akses...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //        return;
            //    }
            //    if (this.gridView1.GetFocusedRowCellValue("ID") == null)
            //        return;
            //    if (this.gridView1.GetFocusedRowCellValue("INDUK") == null)
            //    {
            //        XtraMessageBox.Show("Perkiraan Level 1 tidak dapat diubah", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        return;
            //    }

            //    var rowHandle = gridView1.FocusedRowHandle;
            //    EditCOA.COAID = gridView1.GetRowCellValue(rowHandle, "ID").ToString();
            //    EditCOA.TAHUN = Convert.ToInt32(setahun.Value);
            //    EditCOA.JENIS = gridView1.GetRowCellValue(rowHandle, "GRP").ToString();
            //    EditCOA.INDUK = gridView1.GetRowCellValue(rowHandle, "INDUK").ToString();
            //    EditCOA.GD = Convert.ToChar(gridView1.GetRowCellValue(rowHandle, "GD").ToString());
            //    EditCOA.KODE = gridView1.GetRowCellValue(rowHandle, "KODEACC").ToString();
            //    EditCOA.LEVEL = Convert.ToInt32(gridView1.GetRowCellValue(rowHandle, "LVL").ToString());
            //    EditCOA.DK = Convert.ToChar(gridView1.GetRowCellValue(rowHandle, "POSISI").ToString());
            //    EditCOA.PERKIRAAN = gridView1.GetRowCellValue(rowHandle, "NAMAACC").ToString();
            //    EditCOA.AKTIF = Convert.ToChar(gridView1.GetRowCellValue(rowHandle, "ISAKTIF").ToString());

            //    FrmAkunEdit form = new(this)
            //    {
            //        //MdiParent = this,
            //        StartPosition = FormStartPosition.CenterScreen
            //    };
            //    form.UpdateEventHandler += UpdateFromEdit;
            //    form.ShowDialog();

            //}
            //catch (SystemException ex)
            //{
            //    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }
        private void sbhapus_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanDeleteCoa))
            {
                return;
            }
            try
            {
                var rowhandle = gridView1.FocusedRowHandle;
                var ID = gridView1.GetRowCellValue(rowhandle, "ID").ToString();

                var KODE = gridView1.GetRowCellValue(rowhandle, "KODEACC").ToString();
                var NAMA = gridView1.GetRowCellValue(rowhandle, "NAMAACC").ToString();
                var GD = gridView1.GetRowCellValue(rowhandle, "GD").ToString();

                if(GD=="G" )
                {
                    if (XtraMessageBox.Show("Hapus Group Kode Perkiraan ?\n" + KODE + " " + NAMA +
                        "\nSemua kode dibawah group ini akan dihapus jika tidak memiliki tansaksi" , "Confirm Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                }
                else
                {
                    if (XtraMessageBox.Show("Hapus Detail Kode Perkiraan ? \n" + KODE + " " + NAMA, "Confirm Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                }
                DELETEAKUN(ID);
            }
            catch (Exception ex)
            {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DELETEAKUN(string iD)
        {
            try
            {
                AccountServices.DeleteCOA(iD);
                Load_COA();
                XtraMessageBox.Show("Account Deleted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-02292"))
                {
                    XtraMessageBox.Show("Kode Perkiraan Telah diGunakan,tidak dapat dihapus.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void gridView1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                int rowHandle = e.HitInfo.RowHandle;
                //hapus menu jika ada
                e.Menu.Items.Clear();

                DXMenuItem detail = CreateMenuItemDetail(view, rowHandle);
                DXMenuItem segar = CreateMenuItemSegar(view, rowHandle);

                detail.BeginGroup = true;
                segar.BeginGroup = true;

                e.Menu.Items.Add(detail);
                e.Menu.Items.Add(segar);

            }
        }

        private DXMenuItem CreateMenuItemSegar(GridView view, int rowHandle)
        {
            DXMenuItem checkItem = new DXMenuItem("Refresh Data", new EventHandler(OnRefreshClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }

        private void OnRefreshClick(object sender, EventArgs e)
        {
            Load_COA();
        }

        private DXMenuItem CreateMenuItemDetail(GridView view, int rowHandle)
        {
            DXMenuItem checkItem = new DXMenuItem("Detail Transactions", new EventHandler(OnDetailClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[0];
            return checkItem;
        }

        private void cmbbulan_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyManualPeriodSelection();
        }



        private void setahun_EditValueChanged(object sender, EventArgs e)
        {
            ApplyManualPeriodSelection();
        }

        private void OnDetailClick(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanViewReports))
            {
                return;
            }
            try
            {
                if (this.gridView1.GetFocusedRowCellValue("GD") == null) return;
                var rowhandle = gridView1.FocusedRowHandle;
                var kode = gridView1.GetRowCellValue(rowhandle, "KODEACC").ToString();
                var nama = gridView1.GetRowCellValue(rowhandle, "NAMAACC").ToString();
                var generation = gridView1.GetRowCellValue(rowhandle, "GD").ToString();
                var posisi = gridView1.GetRowCellValue(rowhandle, "POSISI").ToString();
                var debet = Convert.ToDecimal(gridView1.GetRowCellValue(rowhandle, "DEBET"));
                var kredit = Convert.ToDecimal(gridView1.GetRowCellValue(rowhandle, "KREDIT"));
                CoaPeriod detailPeriod = ActivePeriod;
                pbulan = detailPeriod.Month;
                p_sampaibulan = detailPeriod.Month;
                ptahun = detailPeriod.Year;
                var bulan = detailPeriod.MonthName + "-" + ptahun.ToString();
                var periode = pbulan.ToString("00") + "/" + ptahun.ToString();
                var iddata =CompanyInfo.IDDATA;
                var userid = LoginInfo.userID;

                if (CoaDrillDownPolicy.HasNoDirectTransactions(generation, debet, kredit))
                {
                    MessageBox.Show("Tidak ada transaksi", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (CoaDrillDownPolicy.OpensHierarchy(generation))
                {
                    DataSet dataSet = LaporanServices.ViewCoaDrillDown(iddata, pbulan, ptahun, userid, kode);
                    using FrmCoaDrillDown dialog = new(dataSet.Tables["CoaDrillDown"]!, $"Rincian COA - {kode} {nama}", pbulan, ptahun, iddata, userid);
                    dialog.ShowDialog(this);
                    return;
                }

                DSGL = LaporanServices.ViewLap_BukuBesar_Tree(iddata, ptahun, pbulan, p_sampaibulan, kode);
                    //DSGL.WriteXmlSchema("GeneralLedger.xsd");
                    if (posisi == "D")
                    {

                        GeneralLedgerD2 laporan = new ()
                        {
                            DataSource = DSGL
                        };

                        laporan.Parameters["PBULAN"].Value = pbulan;
                        laporan.Parameters["PTAHUN"].Value = ptahun;
                        laporan.Parameters["BULAN"].Value = bulan;
                        laporan.Parameters["PERIODE"].Value = periode;
                        laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                        laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                        laporan.Parameters["USERID"].Value = userid;
                        laporan.RequestParameters = true;
                        ReportPrintTool tool = new (laporan);
                        tool.ShowPreview();
                    }
                    else
                    {

                        GeneralLedgerK2 laporan = new ()
                        {
                            DataSource = DSGL
                        };

                        laporan.Parameters["PBULAN"].Value = pbulan;
                        laporan.Parameters["PTAHUN"].Value = ptahun;
                        laporan.Parameters["BULAN"].Value = bulan;
                        laporan.Parameters["PERIODE"].Value = periode;
                        laporan.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
                        laporan.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
                        laporan.Parameters["USERID"].Value = userid;
                        laporan.RequestParameters = true;
                        ReportPrintTool tool = new (laporan);
                        tool.ShowPreview();
                    }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            _ = AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanExportCoa);
        }
    }
}
