using Accounting._1.Interface;
using Accounting._2.DataAcces;
using Accounting.BusinessLayer;
using Accounting.Model;
using Accounting.UC.Jurnal;
using DevExpress.Data;
using DevExpress.Data.ODataLinq.Helpers;
using DevExpress.Mvvm.Native;
using DevExpress.Utils.DragDrop;
using DevExpress.Utils.Menu;
using DevExpress.Xpf.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace Accounting.Form
{
    public partial class FrmJurnal : XtraForm
    {
        private bool isEnsuringInputGridState;
        private System.Windows.Forms.Timer? resizeDebounceTimer;

        private void InitializeTabHosts()
        {
            HostTabControls(xtraTabPage1, inputTabHost);
            HostTabControls(xtraTabPage2, daftarTabHost);
            HostTabControls(xtraTabPage3, cariTabHost);
            HostTabControls(xtraTabKasir, kasirTabHost);
            HostTabControls(xtraTabAIS, aisTabHost);
            HostTabControls(xtraTabInventori, inventoryTabHost);
            HostTabControls(xtraTabHR, hrisTabHost);
        }

        private static void HostTabControls(DevExpress.XtraTab.XtraTabPage page, UcJurnalTabHostBase host)
        {
            if (page.Controls.Contains(host))
            {
                return;
            }

            List<Control> existingControls = page.Controls.Cast<Control>().ToList();
            page.Controls.Clear();
            host.AttachExistingControls(existingControls);
            page.Controls.Add(host);
        }

        private void InitializeFormState()
        {
            JDgridView.OptionsNavigation.EnterMoveNextColumn = true;
            _ = new GridNewRowHelper(JDgridView);

            PopulateList();
            HandleBehaviorDragDropEvents();
        }

        private void RegisterEventHandlers()
        {
            JDgridView.ShownEditor += JDgridView_ShownEditor;
            JDgridView.ValidateRow += JDgridView_ValidateRow;
            GCJurnal.DataSourceChanged += GCJurnal_DataSourceChanged;
            TABJurnal.SelectedPageChanged += TABJurnal_SelectedPageChanged;

            repdebet.KeyDown += repdebet_KeyDown;
            repkredit.KeyDown += Repkredit_KeyDown;
            gridViewAISheader.FocusedRowChanged += GridViewAISheader_FocusedRowChanged;
            GCJurnal.Resize += GCJurnal_Resize;
        }

        private void UnregisterEventHandlers()
        {
            JDgridView.ShownEditor -= JDgridView_ShownEditor;
            JDgridView.ValidateRow -= JDgridView_ValidateRow;
            GCJurnal.DataSourceChanged -= GCJurnal_DataSourceChanged;
            TABJurnal.SelectedPageChanged -= TABJurnal_SelectedPageChanged;

            repdebet.KeyDown -= repdebet_KeyDown;
            repkredit.KeyDown -= Repkredit_KeyDown;
            gridViewAISheader.FocusedRowChanged -= GridViewAISheader_FocusedRowChanged;
            GCJurnal.Resize -= GCJurnal_Resize;

            if (repositoryItemTextEditKode != null)
            {
                repositoryItemTextEditKode.Leave -= txtkodeperkiran_Leave;
            }
        }

        private void InitializeRepositoryItems()
        {
            // Style 1 only: single editor for KODE.
            repositoryItemTextEditKode = new RepositoryItemTextEdit();
            repositoryItemTextEditKode.MaxLength = 12;
            repositoryItemTextEditKode.Mask.MaskType = MaskType.None;
            GCJurnal.RepositoryItems.Add(repositoryItemTextEditKode);
            repositoryItemTextEditKode.Leave -= txtkodeperkiran_Leave;
            repositoryItemTextEditKode.Leave += txtkodeperkiran_Leave;
            SetColumnEdit(repositoryItemTextEditKode);
        }


        private void ApplyInputGridEditorConfiguration()
        {
            SetColumnEdit(repositoryItemTextEditKode);
        }

        private void allperiode()
        {
            try
            {
                // Get the period data as a DataTable
                DataTable prdTable = PeriodeListAll(p_iddata);  // Adjust to your actual method

                // Check if the DataTable has rows
                if (prdTable != null && prdTable.Rows.Count > 0)
                {
                    // Bind the DataTable as the DataSource
                    leallperiode.Properties.DataSource = prdTable;

                    // Set the ValueMember and DisplayMember properties
                    leallperiode.Properties.ValueMember = "PERIODE";    // Adjust if necessary
                    leallperiode.Properties.DisplayMember = "PERIODE";  // Adjust if necessary

                    // Select the last row
                    leallperiode.EditValue = prdTable.Rows[prdTable.Rows.Count - 1]["PERIODE"];

                }

            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it or show a message to the user)
                XtraMessageBox.Show("An error occurred while loading periods: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private async void FrmJurnal_Load(object sender, EventArgs e)
        {
            int coaCount = 0;
            int periodeAllCount = 0;
            int periodeListCount = 0;
            using var perf = BeginPerfMeasurement(
                "FrmJurnal.Load",
                () => $"coaCount={coaCount};periodeAllCount={periodeAllCount};periodeListCount={periodeListCount}");

            isInitializing = true;
            try
            {
                // Phase 1: parallel DB calls - PeriodeListAll + MaxTahunCOA
                p_iddata = CompanyInfo.IDDATA;
                var periodeAllTask = jurnalRepository.PeriodeListAllAsync(p_iddata);
                var maxTahunTask = jurnalRepository.MaxTahunCOAAsync(p_iddata);
                await Task.WhenAll(periodeAllTask, maxTahunTask);

                BindAllPeriode(periodeAllTask.Result);
                periodeAllCount = periodeAllTask.Result?.Rows.Count ?? 0;
                Acct.TahunMax = maxTahunTask.Result;

                SetupComboBoxes();
                SetupYearRanges();
                SetInitialSelections();

                // Phase 2: parallel DB calls - KodeUntukJurnal + PeriodeList (depend on TahunMax)
                var coaTask = jurnalRepository.KodeUntukJurnalAsync(p_iddata, Acct.TahunMax);
                var periodeListTask = jurnalRepository.PeriodeListAsync(p_iddata, DateTime.Today.Year.ToString());
                await Task.WhenAll(coaTask, periodeListTask);

                ListCoaAktif = coaTask.Result;
                coaCount = ListCoaAktif?.Count() ?? 0;
                periodeListCount = periodeListTask.Result?.Rows.Count ?? 0;
                InitializeRepositoryItems();
                ApplyStaticGridAppearance();
                ApplyInputGridEditorConfiguration();

                NoJurnaltxt.Select();

                if (!HasValidAccountData())
                {
                    XtraMessageBox.Show("Daftar perkiraan belum ada", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Bind periode list
                leperiode.Properties.DataSource = periodeListTask.Result;
                leperiode.Properties.ValueMember = "PERIODE";
                leperiode.Properties.DisplayMember = "PERIODE";
                string periodetujuan = FormatPeriod(pbulan, ptahun);
                leperiode.EditValue = periodetujuan;

                PilihanPeriodeAkuntansi();
                lblrecordbulan.Visible = false;
                SetTanggalharini();
                EnsureInputGridReadyForEntry();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Error saat memuat form jurnal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isInitializing = false;
            }
        }

        private void BindAllPeriode(DataTable prdTable)
        {
            if (prdTable != null && prdTable.Rows.Count > 0)
            {
                leallperiode.Properties.DataSource = prdTable;
                leallperiode.Properties.ValueMember = "PERIODE";
                leallperiode.Properties.DisplayMember = "PERIODE";
                leallperiode.EditValue = prdTable.Rows[prdTable.Rows.Count - 1]["PERIODE"];
            }
        }

        private void EnsureInputGridReadyForEntry(bool focusFirstCell = true)
        {
            if (isEnsuringInputGridState)
            {
                return;
            }

            isEnsuringInputGridState = true;
            try
            {
                UpdateGridEnabledState();
                EnsureSeedRowForInputGrid();
                ResetJurnalInputGridLayoutToDefault();
                if (focusFirstCell)
                {
                    FocusFirstInputCell();
                }
            }
            finally
            {
                isEnsuringInputGridState = false;
            }
        }

        private void ApplyStaticGridAppearance()
        {
            ApplyModernInputGridAppearance();
        }

        private void GCJurnal_DataSourceChanged(object? sender, EventArgs e)
        {
            if (isInitializing) return;
            bool shouldFocus = TABJurnal.SelectedTabPage == xtraTabPage1;
            EnsureInputGridReadyForEntry(shouldFocus);
        }

        private async void TABJurnal_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (e.Page == xtraTabPage1)
            {
                EnsureInputGridReadyForEntry();
            }
            else if (e.Page == xtraTabKasir && !kasirLoaded)
            {
                Load_Kode_Kasir();
                kasirLoaded = true;
            }
            else if (e.Page == xtraTabAIS && !aisLoaded)
            {
                await Load_Kode_AISAsync();
                aisLoaded = true;
            }
            else if (e.Page == xtraTabInventori && !inventoryLoaded)
            {
                Load_Kode_Inv();
                inventoryLoaded = true;
            }
            else if (e.Page == xtraTabHR && !hrisLoaded)
            {
                await Load_Kode_HRISAsync();
                hrisLoaded = true;
            }

            // Release search result memory when leaving search tabs
            if (e.PrevPage == xtraTabPage3)
            {
                PencarianJurnal = Enumerable.Empty<JurnalDetailDTO>();
                ExportPencarian = Enumerable.Empty<JurnalDetailDTO>();
                PencarianJurnal_Bulan = Enumerable.Empty<JurnalDetailDTO>();
                ExportPencarian_Bulan = Enumerable.Empty<JurnalDetailDTO>();
            }
        }

        private void EnsureSeedRowForInputGrid()
        {
            if (InputJurnalDetail == null)
            {
                InputJurnalDetail = new BindingList<JurnalDetailAdd>();
            }

            if (!ReferenceEquals(GCJurnal.DataSource, InputJurnalDetail))
            {
                GCJurnal.DataSource = InputJurnalDetail;
            }

            if (InputJurnalDetail.Count > 0)
            {
                return;
            }

            InputJurnalDetail.Add(new JurnalDetailAdd
            {
                BARIS = 1,
                Kode = string.Empty,
                Rekening = string.Empty,
                Debet = 0,
                Kredit = 0,
                Keterangan = string.Empty
            });

            InputJurnalDetail.Add(new JurnalDetailAdd
            {
                BARIS = 2,
                Kode = string.Empty,
                Rekening = string.Empty,
                Debet = 0,
                Kredit = 0,
                Keterangan = string.Empty
            });

            InputJurnalDetail.AllowNew = true;
            x = Math.Max(0, InputJurnalDetail.Count - 1);
        }

        private int ScaleX(int basePixels)
        {
            float dpiScale = DeviceDpi / 96f;
            return (int)(basePixels * dpiScale);
        }

        private void ResetJurnalInputGridLayoutToDefault()
        {
            JDgridView.BeginUpdate();
            try
            {
                JDgridView.OptionsView.ShowColumnHeaders = true;
                JDgridView.OptionsView.ShowFooter = true;
                JDgridView.OptionsView.ShowGroupPanel = false;
                JDgridView.OptionsView.ColumnAutoWidth = true;
                JDgridView.OptionsBehavior.Editable = true;
                JDgridView.ClearColumnsFilter();
                JDgridView.ActiveFilterString = string.Empty;
                JDgridView.ClearGrouping();

                int totalWidth = GCJurnal.ClientSize.Width;
                if (totalWidth <= 0) totalWidth = GCJurnal.Width;

                int fixedNoWidth = ScaleX(35);
                int fixedHapusWidth = ScaleX(54);
                int flexibleWidth = totalWidth - fixedNoWidth - fixedHapusWidth;
                if (flexibleWidth < 200) flexibleWidth = 200;

                int kodeWidth = Math.Max(ScaleX(120), (int)(flexibleWidth * 0.12));
                int rekeningWidth = Math.Max(ScaleX(120), (int)(flexibleWidth * 0.16));
                int debetWidth = Math.Max(ScaleX(80), (int)(flexibleWidth * 0.10));
                int kreditWidth = Math.Max(ScaleX(80), (int)(flexibleWidth * 0.10));
                int keteranganWidth = Math.Max(ScaleX(150), flexibleWidth - kodeWidth - rekeningWidth - debetWidth - kreditWidth);

                ApplyDefaultColumnLayout(NO, 0, fixedNoWidth);
                ApplyDefaultColumnLayout(KODE, 1, kodeWidth);
                ApplyDefaultColumnLayout(REKENING, 2, rekeningWidth);
                ApplyDefaultColumnLayout(DEBET, 3, debetWidth);
                ApplyDefaultColumnLayout(KREDIT, 4, kreditWidth);
                ApplyDefaultColumnLayout(KETERANGAN, 5, keteranganWidth);
                ApplyDefaultColumnLayout(Hapus, 6, fixedHapusWidth);
            }
            finally
            {
                JDgridView.EndUpdate();
            }

            GCJurnal.ForceInitialize();
            JDgridView.LayoutChanged();
        }

        private void ApplyModernInputGridAppearance()
        {
            JDgridView.BeginUpdate();
            try
            {
                JDgridView.OptionsSelection.EnableAppearanceFocusedCell = true;
                JDgridView.OptionsSelection.EnableAppearanceFocusedRow = true;
                JDgridView.OptionsSelection.InvertSelection = false;
                JDgridView.OptionsView.EnableAppearanceOddRow = true;
                JDgridView.OptionsView.EnableAppearanceEvenRow = true;
                JDgridView.OptionsView.ShowIndicator = false;
                JDgridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.True;
                JDgridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.True;
                JDgridView.PaintStyleName = "Skin";

                JDgridView.RowHeight = 26;
                JDgridView.ColumnPanelRowHeight = 30;

                JDgridView.Appearance.HeaderPanel.BackColor = Color.FromArgb(238, 242, 247);
                JDgridView.Appearance.HeaderPanel.BackColor2 = Color.FromArgb(238, 242, 247);
                JDgridView.Appearance.HeaderPanel.ForeColor = Color.FromArgb(45, 52, 64);
                JDgridView.Appearance.HeaderPanel.Font = new Font("Segoe UI Semibold", 9.25F, FontStyle.Bold);
                JDgridView.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

                JDgridView.Appearance.OddRow.BackColor = Color.White;
                JDgridView.Appearance.EvenRow.BackColor = Color.FromArgb(250, 252, 255);
                JDgridView.Appearance.Row.ForeColor = Color.FromArgb(35, 39, 47);

                JDgridView.Appearance.FocusedRow.BackColor = Color.FromArgb(224, 236, 255);
                JDgridView.Appearance.FocusedRow.BackColor2 = Color.FromArgb(224, 236, 255);
                JDgridView.Appearance.FocusedCell.BackColor = Color.FromArgb(255, 245, 217);
                JDgridView.Appearance.FocusedCell.BackColor2 = Color.FromArgb(255, 245, 217);
                JDgridView.Appearance.FocusedCell.ForeColor = Color.FromArgb(29, 35, 45);

                DEBET.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                KREDIT.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                Hapus.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                NO.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

                DEBET.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                DEBET.DisplayFormat.FormatString = "n2";
                KREDIT.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                KREDIT.DisplayFormat.FormatString = "n2";
            }
            finally
            {
                JDgridView.EndUpdate();
            }
        }

        private static void ApplyDefaultColumnLayout(GridColumn column, int visibleIndex, int width)
        {
            if (column == null)
            {
                return;
            }

            column.MaxWidth = 0;
            column.Visible = true;
            column.VisibleIndex = visibleIndex;
            column.Width = width;
        }

        private void GCJurnal_Resize(object? sender, EventArgs e)
        {
            if (isEnsuringInputGridState || GCJurnal.ClientSize.Width <= 0)
                return;

            resizeDebounceTimer?.Stop();
            resizeDebounceTimer ??= new System.Windows.Forms.Timer { Interval = 100 };
            resizeDebounceTimer.Tick -= ResizeDebounceTimer_Tick;
            resizeDebounceTimer.Tick += ResizeDebounceTimer_Tick;
            resizeDebounceTimer.Start();
        }

        private void ResizeDebounceTimer_Tick(object? sender, EventArgs e)
        {
            resizeDebounceTimer?.Stop();
            ResetJurnalInputGridLayoutToDefault();
        }

        private void FocusFirstInputCell()
        {
            if (JDgridView.RowCount <= 0)
            {
                return;
            }

            JDgridView.FocusedRowHandle = 0;
            JDgridView.FocusedColumn = KODE;
            JDgridView.ShowEditor();
        }


        private void InitializeData()
        {
            p_iddata = CompanyInfo.IDDATA;
        }

        private void SetupComboBoxes()
        {
            PopulateComboBox(cmbbulan, Bulan);
            PopulateComboBox(cmbbulan2, Bulan);
            PopulateComboBox(cmbbulankasir, Bulan);
            PopulateComboBox(cmbbulanAIS, Bulan);
            PopulateComboBox(CMBBULANINV, Bulan);
            PopulateComboBox(cmbbulanhris, Bulan);
            PopulateComboBox(cmbbulanumum, Bulan);

            var bl = FormatPeriod(DateTime.Today.Month, DateTime.Today.Year);
            leallperiode.EditValue = bl;
        }

        private void PopulateComboBox(ComboBoxEdit comboBox, IEnumerable<string> items)
        {
            if (items != null && items.Any())
            {
                comboBox.Properties.Items.AddRange(items.ToList());
            }
        }


        private void SetupYearRanges()
        {
            SetYearRange(daritahun);
            SetYearRange(sampaitahun);
            SetYearRange(setahunkasir);
            SetYearRange(setahunAIS);
            SetYearRange(SETAHUNINV);
            SetYearRange(setahunhris);
            SetYearRange(setahunumum);
        }

        private void SetYearRange(SpinEdit yearControl)
        {
            yearControl.Properties.MinValue = Acct.TahunMin;
            yearControl.Properties.MaxValue = Acct.TahunMax;
        }

        private void SetInitialSelections()
        {
            var bulan = int.Parse(Acct.PeriodeMax.ToString().Substring(Acct.PeriodeMax.ToString().Length - 2, 2)) - 1;

            cmbbulan.SelectedIndex = bulan;
            cmbbulan2.SelectedIndex = bulan;
            cmbbulankasir.SelectedIndex = bulan;
            cmbbulanAIS.SelectedIndex = bulan;
            CMBBULANINV.SelectedIndex = bulan;
            cmbbulanhris.SelectedIndex = bulan;
            cmbbulanumum.SelectedIndex = bulan;

            daritahun.Value = Acct.TahunMax;
            sampaitahun.Value = Acct.TahunMax;
            setahunkasir.Value = Acct.TahunMax;
            setahunAIS.Value = Acct.TahunMax;
            SETAHUNINV.Value = Acct.TahunMax;
            setahunhris.Value = Acct.TahunMax;
            setahunumum.Value = Acct.TahunMax;
        }

        private bool HasValidAccountData()
        {
            return Acct.TahunMax > 0;
        }

        private void UpdateGridEnabledState()
        {
            bool hasCoaData = HasValidAccountData();
            bool hasValidYear = true;

            if (deJurnal.EditValue is DateTime dt)
            {
                hasValidYear = Acct.TahunMax >= dt.Year;
            }

            GCJurnal.Enabled = hasCoaData && hasValidYear;
        }

        private void LoadAndSetPeriode()
        {
            DataTable dt2 = new();
            PopulatePeriode(dt2);

            string periodetujuan = FormatPeriod(pbulan, ptahun);
            leperiode.EditValue = periodetujuan;
        }

        private async Task LoadAdditionalDataAsync()
        {
            if (!kasirLoaded) { Load_Kode_Kasir(); kasirLoaded = true; }
            if (!aisLoaded) { await Load_Kode_AISAsync(); aisLoaded = true; }
            if (!hrisLoaded) { await Load_Kode_HRISAsync(); hrisLoaded = true; }
            if (!inventoryLoaded) { Load_Kode_Inv(); inventoryLoaded = true; }
        }

        private async Task Load_Kode_HRISAsync()
        {
            var estate = await jurnalRepository.GetEstateAsync(CompanyInfo.IDDATA);
            lookUpEditestatehris.Properties.DataSource = estate;
            lookUpEditestatehris.Properties.ValueMember = "ID";
            lookUpEditestatehris.Properties.DisplayMember = "NAMA";


            Dictionary<int, string> Remises = new()
            {
                { 1, "1" },
                { 2, "2" }
            };
            leremiseumum.Properties.DataSource = Remises;
            leremiseumum.EditValue = 1;
        }

        private void Load_COA_Dapper()
        {
            Acct.TahunMax = jurnalRepository.MaxTahunCOA(p_iddata);
            ListCoaAktif = jurnalRepository.KodeUntukJurnal(p_iddata, Acct.TahunMax);
        }


        private void repositoryItemGridLookUpEditkode_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
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


        private void SetTanggalharini()
        {
            deJurnal.EditValue = DateTime.Today;
            deJurnal.Properties.Mask.MaskType = MaskType.DateTime;
            deJurnal.Properties.Mask.EditMask = "dd/MM/yyyy";
        }

        private void JDgridView_RowCountChanged(object sender, EventArgs e)
        {

            for (int i = 0; i < JDgridView.RowCount; i++)
            {

                JDgridView.SetRowCellValue(i, JDgridView.Columns["BARIS"], i + 1);
            }
        }
    }
}
