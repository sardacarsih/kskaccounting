using Accounting.BusinessLayer;
using Accounting.DataLayer;
using Accounting.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraSplashScreen;
using DevExpress.XtraTab;
using Oracle.ManagedDataAccess.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmSettingRL : DevExpress.XtraEditors.XtraForm
    {
        private readonly ReportSettingRepository reportSettingRepository = new(LoginInfo.OracleConnString);
        private readonly CancellationTokenSource lifetimeCancellation = new();
        private readonly object overlaySync = new();
        private DataSet DSSetup = new();
        private int MAXTAHUN;
        private bool focusedRowEventAttached;
        private bool suppressSectionEvents;
        private bool isInitializing;
        private bool isClosing;
        private bool canManageSetup;
        private int reportLoadVersion;
        private int mappingLoadVersion;
        private CancellationTokenSource? reportLoadCancellation;
        private CancellationTokenSource? mappingLoadCancellation;
        private CancellationTokenSource? validationCancellation;
        private IOverlaySplashScreenHandle? overlayHandle;
        private int overlayDepth;
        private SimpleButton addRootButton;
        private SimpleButton deactivateRootButton;
        private SimpleButton moveRootUpButton;
        private SimpleButton moveRootDownButton;
        private SimpleButton validateSectionsButton;
        private XtraTabControl tabControl;
        private SplitContainerControl settingsSplitContainer;
        private FlowLayoutPanel mappingButtonLayout;
        private string currentReportCode = "LABARUGI";
        private const string AddRootMappingSql = @"
                INSERT INTO ACCT_REPORT_SECTION_ACCOUNT (
                    SECTION_ID,
                    JENIS_AKUNTING,
                    IDDATA,
                    KODEACC_ROOT,
                    DISPLAY_ORDER,
                    INCLUDE_CHILDREN,
                    IS_ACTIVE,
                    MATCH_MODE,
                    GRP_CODE)
                VALUES (
                    :sectionId,
                    :jenisAkunting,
                    :iddata,
                    :kodeAcc,
                    (SELECT NVL(MAX(DISPLAY_ORDER), 0) + 10
                       FROM ACCT_REPORT_SECTION_ACCOUNT
                      WHERE SECTION_ID = :sectionId),
                    'Y',
                    'Y',
                    'TREE',
                    NULL)";

        public FrmSettingRL()
        {
            InitializeComponent();
        }

        private async void FrmSettingRL_Load(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                Close();
                return;
            }

            isInitializing = true;
            try
            {
                string setlr = ConfigurationManager.AppSettings["setLR"];
                checkEdit1.EditValue = setlr == "Ya";

                ConfigureLayout();
                ConfigureMappingButtons();
                ConfigureSectionGrid();
                ConfigureMappingGrid();
                AttachFocusedRowChanged();

                canManageSetup = AuthorizationService.CanManageProfitLossSetup();
                gridView1.OptionsBehavior.Editable = canManageSetup;
                simpleButton1.Enabled = canManageSetup;
                checkEdit1.Enabled = canManageSetup;
                SetMappingActionsEnabled(false);

                MAXTAHUN = await reportSettingRepository.GetMaximumCoaYearAsync(
                    CompanyInfo.IDDATA,
                    lifetimeCancellation.Token);

                if (MAXTAHUN <= 0)
                {
                    throw new InvalidOperationException($"Tahun COA untuk {CompanyInfo.IDDATA} tidak ditemukan.");
                }

                await ReloadReportAsync();
            }
            catch (OperationCanceledException)
            {
                // Expected when the form closes while the initial data is loading.
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize report settings form");
                ShowLoadError("Data pengaturan laporan tidak dapat dimuat.", ex);
            }
            finally
            {
                isInitializing = false;
            }
        }

        private void ConfigureLayout()
        {
            float scale = GetScale(this);

            SuspendLayout();
            Text = "Pengaturan Laporan";
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = ScaleSize(1024, 680, scale);

            tabControl = new XtraTabControl
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty
            };
            tabControl.TabPages.Add("Laba Rugi");
            tabControl.TabPages.Add("Neraca");
            tabControl.SelectedPageChanged += TabControl_SelectedPageChanged;

            TableLayoutPanel rootLayout = CreateRootLayout(scale);
            settingsSplitContainer = new SplitContainerControl
            {
                Dock = DockStyle.Fill,
                Horizontal = true,
                SplitterPosition = Scale(420, scale)
            };
            settingsSplitContainer.Panel1.MinSize = Scale(380, scale);
            settingsSplitContainer.Panel2.MinSize = Scale(520, scale);
            settingsSplitContainer.Panel1.Controls.Add(CreateSectionPanel(scale));
            settingsSplitContainer.Panel2.Controls.Add(CreateMappingPanel(scale));

            rootLayout.Controls.Add(tabControl, 0, 0);
            rootLayout.Controls.Add(settingsSplitContainer, 0, 1);
            Controls.Add(rootLayout);
            rootLayout.BringToFront();

            ResumeLayout(false);
        }

        private TableLayoutPanel CreateRootLayout(float scale)
        {
            TableLayoutPanel rootLayout = new()
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Padding = ScalePadding(8, 8, 8, 8, scale),
                RowCount = 2
            };
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(34, scale)));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            return rootLayout;
        }

        private PanelControl CreateSectionPanel(float scale)
        {
            gridControl1.Dock = DockStyle.Fill;
            gridControl1.Margin = Padding.Empty;

            labelControl1.Dock = DockStyle.Fill;
            labelControl1.AutoSizeMode = LabelAutoSizeMode.None;
            labelControl1.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            labelControl1.Text = "Tampilkan Nilai 0";

            checkEdit1.Dock = DockStyle.Fill;
            checkEdit1.Properties.OffText = "Tidak";
            checkEdit1.Properties.OnText = "Ya";

            simpleButton1.Dock = DockStyle.Fill;
            simpleButton1.MinimumSize = new Size(Scale(116, scale), Scale(30, scale));
            simpleButton1.Text = "Reset Default";

            TableLayoutPanel footerLayout = new()
            {
                ColumnCount = 4,
                Dock = DockStyle.Fill,
                RowCount = 1
            };
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(118, scale)));
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(130, scale)));
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(126, scale)));
            footerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            footerLayout.Controls.Add(labelControl1, 0, 0);
            footerLayout.Controls.Add(checkEdit1, 1, 0);
            footerLayout.Controls.Add(simpleButton1, 3, 0);

            labelControl1.Margin = ScalePadding(0, 3, 6, 3, scale);
            checkEdit1.Margin = ScalePadding(0, 3, 10, 3, scale);
            simpleButton1.Margin = ScalePadding(0, 3, 0, 3, scale);

            TableLayoutPanel sectionLayout = new()
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Padding = ScalePadding(0, 0, 8, 0, scale),
                RowCount = 2
            };
            sectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            sectionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            sectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(46, scale)));
            sectionLayout.Controls.Add(gridControl1, 0, 0);
            sectionLayout.Controls.Add(footerLayout, 0, 1);

            PanelControl sectionPanel = new()
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill
            };
            sectionPanel.Controls.Add(sectionLayout);

            return sectionPanel;
        }

        private PanelControl CreateMappingPanel(float scale)
        {
            mappingButtonLayout = new FlowLayoutPanel
            {
                AutoScroll = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = ScalePadding(0, 0, 0, 4, scale),
                WrapContents = false
            };

            gridControl2.Dock = DockStyle.Fill;
            gridControl2.Margin = Padding.Empty;

            TableLayoutPanel mappingLayout = new()
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 2
            };
            mappingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mappingLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, Scale(38, scale)));
            mappingLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mappingLayout.Controls.Add(mappingButtonLayout, 0, 0);
            mappingLayout.Controls.Add(gridControl2, 0, 1);

            PanelControl mappingPanel = new()
            {
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill
            };
            mappingPanel.Controls.Add(mappingLayout);

            return mappingPanel;
        }

        private async void TabControl_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
        {
            currentReportCode = tabControl.SelectedTabPageIndex == 0 ? "LABARUGI" : "NERACA";
            Text = $"Pengaturan Laporan {(currentReportCode == "LABARUGI" ? "Laba Rugi" : "Neraca")}";

            if (isInitializing || isClosing)
            {
                return;
            }

            await ReloadReportAsync();
        }

        private void ConfigureMappingButtons()
        {
            float scale = GetScale(this);
            addRootButton = CreateMappingButton("Add Root", addRootButton_Click, scale);
            deactivateRootButton = CreateMappingButton("Deactivate", deactivateRootButton_Click, scale);
            moveRootUpButton = CreateMappingButton("Move Up", moveRootUpButton_Click, scale);
            moveRootDownButton = CreateMappingButton("Move Down", moveRootDownButton_Click, scale);
            validateSectionsButton = CreateMappingButton("Validate Section", validateSectionsButton_Click, scale);

            Control.ControlCollection targetControls = mappingButtonLayout?.Controls ?? Controls;
            targetControls.Add(addRootButton);
            targetControls.Add(deactivateRootButton);
            targetControls.Add(moveRootUpButton);
            targetControls.Add(moveRootDownButton);
            targetControls.Add(validateSectionsButton);
        }

        private SimpleButton CreateMappingButton(string text, EventHandler clickHandler, float scale)
        {
            SimpleButton button = new()
            {
                Margin = ScalePadding(0, 2, 8, 2, scale),
                Size = new Size(Scale(104, scale), Scale(30, scale)),
                Text = text
            };
            button.Click += clickHandler;
            return button;
        }
        private void ConfigureSectionGrid()
        {
            gridView1.OptionsView.ShowGroupPanel = false;
            gridView1.OptionsView.ShowAutoFilterRow = true;
            gridView1.OptionsView.EnableAppearanceEvenRow = true;
            gridView1.OptionsView.ColumnAutoWidth = true;
            gridView1.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDownFocused;

            NO.Caption = "URUT";
            NO.FieldName = "DISPLAY_ORDER";
            NO.MinWidth = 50;
            NO.Visible = true;
            NO.VisibleIndex = 0;
            NO.Width = 58;

            KELOMPOK.Caption = "SECTION";
            KELOMPOK.FieldName = "SECTION_NAME";
            KELOMPOK.OptionsColumn.AllowEdit = true;
            KELOMPOK.MinWidth = 170;
            KELOMPOK.Visible = true;
            KELOMPOK.VisibleIndex = 1;
            KELOMPOK.Width = 230;

            LVL.Caption = "LVL";
            LVL.FieldName = "DISPLAY_LVL";
            LVL.OptionsColumn.AllowEdit = true;
            LVL.MinWidth = 44;
            LVL.Visible = true;
            LVL.VisibleIndex = 2;
            LVL.Width = 54;

            TAMPILKAN.Caption = "ZERO";
            TAMPILKAN.FieldName = "SHOW_ZERO";
            TAMPILKAN.MinWidth = 64;
            TAMPILKAN.Visible = true;
            TAMPILKAN.VisibleIndex = 3;
            TAMPILKAN.Width = 72;

            KODE.Caption = "KODE";
            KODE.FieldName = "SECTION_CODE";
            KODE.Visible = false;

            GRP.Caption = "ID";
            GRP.FieldName = "SECTION_ID";
            GRP.Visible = false;
        }

        private void ConfigureMappingGrid()
        {
            gridView2.OptionsBehavior.Editable = false;
            gridView2.OptionsView.ShowGroupPanel = false;
            gridView2.OptionsView.ShowAutoFilterRow = true;
            gridView2.OptionsView.EnableAppearanceEvenRow = true;
            gridView2.OptionsView.ColumnAutoWidth = false;
            gridView2.HorzScrollVisibility = ScrollVisibility.Auto;
        }

        private async Task ReloadReportAsync(int? preferredSectionId = null)
        {
            if (isClosing || IsDisposed)
            {
                return;
            }

            CancelRequest(ref reportLoadCancellation);
            CancelRequest(ref mappingLoadCancellation);
            CancelRequest(ref validationCancellation);

            CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetimeCancellation.Token);
            CancellationToken token = cancellation.Token;
            reportLoadCancellation = cancellation;
            int requestVersion = ++reportLoadVersion;
            string requestedReportCode = currentReportCode;
            Stopwatch stopwatch = Stopwatch.StartNew();
            int sectionCount = 0;

            using IDisposable loadingScope = BeginLoadingScope();
            SetMappingActionsEnabled(false);
            try
            {
                DataTable sections = await reportSettingRepository.LoadSectionsAsync(
                    requestedReportCode,
                    IsPksAccounting(),
                    token);
                sectionCount = sections.Rows.Count;

                if (!IsCurrentReportRequest(cancellation, requestVersion, requestedReportCode))
                {
                    return;
                }

                BindSectionGrid(sections, preferredSectionId);
                int? sectionId = GetFocusedSectionIdOrNull();
                if (sectionId.HasValue)
                {
                    await ReloadMappingAsync(sectionId.Value, token, showLoadingIndicator: false);
                }
                else
                {
                    gridControl2.DataSource = null;
                }

                LogNavigationPerformance(
                    "ReloadReport",
                    stopwatch.ElapsedMilliseconds,
                    requestedReportCode,
                    sectionId,
                    sectionCount);
            }
            catch (OperationCanceledException)
            {
                // Expected when another tab request supersedes this request.
            }
            catch (OracleException ex) when (ex.Number == 1013)
            {
                if (IsCurrentReportRequest(cancellation, requestVersion, requestedReportCode))
                {
                    ShowLoadError("Waktu pemuatan pengaturan laporan habis. Silakan coba kembali.", ex);
                }
            }
            catch (TimeoutException ex)
            {
                if (IsCurrentReportRequest(cancellation, requestVersion, requestedReportCode))
                {
                    ShowLoadError("Waktu pemuatan pengaturan laporan habis. Silakan coba kembali.", ex);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load report settings. ReportCode={ReportCode}", requestedReportCode);
                if (IsCurrentReportRequest(cancellation, requestVersion, requestedReportCode))
                {
                    ShowLoadError("Data pengaturan laporan tidak dapat dimuat.", ex);
                }
            }
            finally
            {
                if (ReferenceEquals(reportLoadCancellation, cancellation))
                {
                    reportLoadCancellation = null;
                    SetMappingActionsEnabled(true);
                }

                cancellation.Dispose();
            }
        }

        private void BindSectionGrid(DataTable sections, int? preferredSectionId)
        {
            suppressSectionEvents = true;
            try
            {
                DSSetup = new DataSet();
                DSSetup.Tables.Add(sections);
                gridControl1.DataSource = DSSetup;
                gridControl1.DataMember = "Setup";
                gridView1.BestFitColumns();
                ConfigureResponsiveGridColumns();

                if (gridView1.RowCount <= 0)
                {
                    return;
                }

                int rowHandle = preferredSectionId.HasValue
                    ? gridView1.LocateByValue("SECTION_ID", preferredSectionId.Value)
                    : 0;
                gridView1.FocusedRowHandle = rowHandle >= 0 ? rowHandle : 0;
            }
            finally
            {
                suppressSectionEvents = false;
            }
        }

        private void AttachFocusedRowChanged()
        {
            if (focusedRowEventAttached)
            {
                return;
            }

            gridView1.FocusedRowChanged += gridView1_FocusedRowChanged;
            focusedRowEventAttached = true;
        }

        private async void gridView1_RowUpdated(object sender, RowObjectEventArgs e)
        {
            if (suppressSectionEvents || isClosing)
            {
                return;
            }

            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                return;
            }

            if (e.Row is not DataRowView rowView)
            {
                return;
            }

            int sectionId = GetInt(rowView.Row, "SECTION_ID");
            if (!SaveSection(rowView.Row))
            {
                await ReloadReportAsync(sectionId);
                return;
            }

            await ReloadReportAsync(sectionId);
        }

        private bool SaveSection(DataRow row)
        {
            int sectionId = GetInt(row, "SECTION_ID");
            int displayLvl = Math.Max(1, GetInt(row, "DISPLAY_LVL"));
            bool displayLevelChanged = !row.HasVersion(DataRowVersion.Original)
                || displayLvl != Math.Max(1, GetInt(row, "DISPLAY_LVL", DataRowVersion.Original));
            if (displayLevelChanged)
            {
                List<SectionLevelValidationResult> validationResults = ValidateSectionLevel(sectionId, displayLvl);
                List<SectionLevelValidationResult> gaps = validationResults.Where(result => !result.IsValid).ToList();
                if (gaps.Count > 0)
                {
                    XtraMessageBox.Show(
                        BuildGapMessage(gaps, displayLvl),
                        "Gap LVL Section",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return false;
                }
            }

            const string updateSql = @"
                UPDATE ACCT_REPORT_SECTION
                   SET SECTION_NAME = :sectionName,
                       DISPLAY_ORDER = :displayOrder,
                       NORMAL_POSISI = :normalPosisi,
                       DISPLAY_LVL = :displayLvl,
                       SHOW_ZERO = :showZero,
                       IS_ACTIVE = :isActive
                 WHERE SECTION_ID = :sectionId";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(updateSql, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };

            command.Parameters.Add("sectionName", OracleDbType.Varchar2, 150).Value = GetString(row, "SECTION_NAME");
            command.Parameters.Add("displayOrder", OracleDbType.Int32).Value = GetInt(row, "DISPLAY_ORDER");
            command.Parameters.Add("normalPosisi", OracleDbType.Char, 1).Value = NormalizePosisi(GetString(row, "NORMAL_POSISI"));
            command.Parameters.Add("displayLvl", OracleDbType.Int32).Value = displayLvl;
            command.Parameters.Add("showZero", OracleDbType.Char, 1).Value = NormalizeFlag(GetString(row, "SHOW_ZERO"));
            command.Parameters.Add("isActive", OracleDbType.Char, 1).Value = NormalizeFlag(GetString(row, "IS_ACTIVE"));
            command.Parameters.Add("sectionId", OracleDbType.Int32).Value = GetInt(row, "SECTION_ID");
            command.ExecuteNonQuery();
            return true;
        }

        private Task ReloadFocusedMappingAsync()
        {
            int? sectionId = GetFocusedSectionIdOrNull();
            if (!sectionId.HasValue)
            {
                gridControl2.DataSource = null;
                return Task.CompletedTask;
            }

            return ReloadMappingAsync(sectionId.Value, lifetimeCancellation.Token, showLoadingIndicator: true);
        }

        private async Task ReloadMappingAsync(
            int sectionId,
            CancellationToken parentToken,
            bool showLoadingIndicator)
        {
            if (isClosing || IsDisposed)
            {
                return;
            }

            CancelRequest(ref mappingLoadCancellation);
            CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                lifetimeCancellation.Token,
                parentToken);
            CancellationToken token = cancellation.Token;
            mappingLoadCancellation = cancellation;
            int requestVersion = ++mappingLoadVersion;
            string requestedReportCode = currentReportCode;
            Stopwatch stopwatch = Stopwatch.StartNew();
            int mappingCount = 0;

            IDisposable? loadingScope = showLoadingIndicator ? BeginLoadingScope() : null;
            SetMappingActionsEnabled(false);
            gridControl2.DataSource = null;
            try
            {
                DataTable mappings = await reportSettingRepository.LoadMappingsAsync(
                    sectionId,
                    CompanyInfo.IDDATA,
                    MAXTAHUN,
                    CompanyInfo.JENIS_AKUNTING,
                    token);
                mappingCount = mappings.Rows.Count;

                if (!IsCurrentMappingRequest(cancellation, requestVersion, requestedReportCode, sectionId))
                {
                    return;
                }

                gridControl2.DataSource = mappings;
                gridView2.BestFitColumns();
                ConfigureResponsiveGridColumns();

                LogNavigationPerformance(
                    "ReloadMapping",
                    stopwatch.ElapsedMilliseconds,
                    requestedReportCode,
                    sectionId,
                    mappingCount);
            }
            catch (OperationCanceledException)
            {
                // Expected while the user moves quickly between report sections.
            }
            catch (OracleException ex) when (ex.Number == 1013)
            {
                if (IsCurrentMappingRequest(cancellation, requestVersion, requestedReportCode, sectionId))
                {
                    ShowLoadError("Waktu pemuatan mapping akun habis. Silakan coba kembali.", ex);
                }
            }
            catch (TimeoutException ex)
            {
                if (IsCurrentMappingRequest(cancellation, requestVersion, requestedReportCode, sectionId))
                {
                    ShowLoadError("Waktu pemuatan mapping akun habis. Silakan coba kembali.", ex);
                }
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Failed to load report section mappings. ReportCode={ReportCode} SectionId={SectionId}",
                    requestedReportCode,
                    sectionId);
                if (IsCurrentMappingRequest(cancellation, requestVersion, requestedReportCode, sectionId))
                {
                    ShowLoadError("Mapping akun section tidak dapat dimuat.", ex);
                }
            }
            finally
            {
                loadingScope?.Dispose();
                if (ReferenceEquals(mappingLoadCancellation, cancellation))
                {
                    mappingLoadCancellation = null;
                    SetMappingActionsEnabled(true);
                }

                cancellation.Dispose();
            }
        }

        private void ConfigureResponsiveGridColumns()
        {
            NO.Width = 58;
            LVL.Width = 54;
            TAMPILKAN.Width = 72;
            KELOMPOK.MinWidth = 170;

            if (gridView2.Columns.Count == 0)
            {
                return;
            }

            gridView2.OptionsView.ColumnAutoWidth = false;
            SetMappingColumnWidth("SECTION_ACCOUNT_ID", 92);
            SetMappingColumnWidth("URUT", 60);
            SetMappingColumnWidth("KODEACC", 120);
            SetMappingColumnWidth("NAMAACC", 220);
            SetMappingColumnWidth("PARENTACC", 120);
            SetMappingColumnWidth("ISHEADER", 80);
            SetMappingColumnWidth("LVL", 60);
            SetMappingColumnWidth("POSISI", 70);
            SetMappingColumnWidth("JENIS_AKUNTING", 120);
            SetMappingColumnWidth("IDDATA_SCOPE", 110);
            SetMappingColumnWidth("MATCH_MODE", 110);
            SetMappingColumnWidth("GRP_CODE", 90);
            SetMappingColumnWidth("INCLUDE_CHILDREN", 130);
            SetMappingColumnWidth("IS_ACTIVE", 90);
            SetMappingColumnWidth("LVL_STATUS", 95);
            SetMappingColumnWidth("LVL_DETAIL", 260);
        }

        private void SetMappingColumnWidth(string fieldName, int minWidth)
        {
            DevExpress.XtraGrid.Columns.GridColumn column = gridView2.Columns.ColumnByFieldName(fieldName);
            if (column == null)
            {
                return;
            }

            column.MinWidth = minWidth;
            column.Width = Math.Max(column.Width, minWidth);
        }

        private static float GetScale(Control control)
        {
            return control.DeviceDpi > 0 ? control.DeviceDpi / 96F : 1F;
        }

        private static int Scale(int value, float scale)
        {
            return (int)Math.Round(value * scale);
        }

        private static Size ScaleSize(int width, int height, float scale)
        {
            return new Size(Scale(width, scale), Scale(height, scale));
        }

        private static Padding ScalePadding(int left, int top, int right, int bottom, float scale)
        {
            return new Padding(Scale(left, scale), Scale(top, scale), Scale(right, scale), Scale(bottom, scale));
        }

        private void checkEdit1_EditValueChanged(object sender, EventArgs e)
        {
            if (!Visible)
            {
                return;
            }

            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                return;
            }

            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("setLR");
                config.AppSettings.Settings.Add("setLR", (bool)checkEdit1.EditValue ? "Ya" : "Tidak");
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception exc)
            {
                MessageBox.Show(@"Saving Error. " + exc.Message);
            }
        }

        private async void simpleButton1_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                return;
            }

            string sectionScopeFilter = BuildSectionScopeFilter();
            string resetSql = $@"
                UPDATE ACCT_REPORT_SECTION
                   SET SHOW_ZERO = 'N',
                       IS_ACTIVE = 'Y'
                  WHERE REPORT_CODE = '{currentReportCode}'
                    {sectionScopeFilter}";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(resetSql, connection)
            {
                CommandType = CommandType.Text
            };
            command.ExecuteNonQuery();

            await ReloadReportAsync(GetFocusedSectionIdOrNull());
        }

        private string BuildSectionScopeFilter()
        {
            if (currentReportCode != "LABARUGI")
            {
                return string.Empty;
            }

            return string.Equals(CompanyInfo.JENIS_AKUNTING, "PKS", StringComparison.OrdinalIgnoreCase)
                ? "AND SECTION_CODE LIKE 'PKS\\_%' ESCAPE '\\'"
                : "AND SECTION_CODE NOT LIKE 'PKS\\_%' ESCAPE '\\'";
        }

        private async void gridView1_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (suppressSectionEvents || isInitializing || isClosing)
            {
                return;
            }

            await ReloadFocusedMappingAsync();
        }

        private async void addRootButton_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                return;
            }

            if (!TryGetFocusedSectionId(out int sectionId))
            {
                return;
            }

            if (!TrySelectRootAccount(out string kodeAcc))
            {
                return;
            }

            if (MappingExists(sectionId, kodeAcc))
            {
                XtraMessageBox.Show("Mapping akun sudah aktif untuk section ini.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int displayLvl = GetFocusedSectionDisplayLevel();
            List<int> availableLevels = GetAvailableLevels(kodeAcc, "TREE", null);
            if (!availableLevels.Contains(displayLvl))
            {
                XtraMessageBox.Show(
                    $"Root {kodeAcc} tidak memiliki akun LVL {displayLvl}. Level tersedia: {FormatLevels(availableLevels)}.",
                    "Gap LVL Section",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!ReactivateInactiveMapping(sectionId, kodeAcc))
            {
                AddRootMapping(sectionId, kodeAcc);
            }

            await ReloadMappingAsync(sectionId, lifetimeCancellation.Token, showLoadingIndicator: true);
        }

        private async void deactivateRootButton_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                return;
            }

            if (!TryGetFocusedMappingId(out int sectionAccountId))
            {
                return;
            }

            DialogResult result = XtraMessageBox.Show("Deactivate mapping akun ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            UpdateMappingActive(sectionAccountId, "N");
            await ReloadFocusedMappingAsync();
        }

        private async void moveRootUpButton_Click(object sender, EventArgs e)
        {
            if (MoveFocusedMapping(-1))
            {
                await ReloadFocusedMappingAsync();
            }
        }

        private async void moveRootDownButton_Click(object sender, EventArgs e)
        {
            if (MoveFocusedMapping(1))
            {
                await ReloadFocusedMappingAsync();
            }
        }

        private async void validateSectionsButton_Click(object sender, EventArgs e)
        {
            await ShowSectionValidationResultsAsync();
        }

        private bool TryGetFocusedSectionId(out int sectionId)
        {
            sectionId = 0;
            if (gridView1.FocusedRowHandle < 0)
            {
                string reportName = currentReportCode == "LABARUGI" ? "Laba Rugi" : "Neraca";
                XtraMessageBox.Show($"Pilih section {reportName} terlebih dahulu.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            object value = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "SECTION_ID");
            if (value == null || value == DBNull.Value)
            {
                XtraMessageBox.Show("Section yang dipilih tidak valid.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            sectionId = Convert.ToInt32(value);
            return true;
        }

        private bool TryGetFocusedMappingId(out int sectionAccountId)
        {
            sectionAccountId = 0;
            if (gridView2.FocusedRowHandle < 0)
            {
                XtraMessageBox.Show("Pilih mapping akun terlebih dahulu.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            object value = gridView2.GetRowCellValue(gridView2.FocusedRowHandle, "SECTION_ACCOUNT_ID");
            if (value == null || value == DBNull.Value)
            {
                XtraMessageBox.Show("Mapping akun yang dipilih tidak valid.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            sectionAccountId = Convert.ToInt32(value);
            return true;
        }

        private bool TrySelectRootAccount(out string kodeAcc)
        {
            kodeAcc = string.Empty;
            DataTable accounts = LoadCoaLookupRows();
            if (accounts.Rows.Count == 0)
            {
                XtraMessageBox.Show($"Tidak ada data ACCT_COA untuk {CompanyInfo.IDDATA} tahun {MAXTAHUN}.", "Lookup ACCT_COA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string selectedKodeAcc = string.Empty;
            using XtraForm dialog = new()
            {
                Text = "Pilih Root Account",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(900, 560),
                MinimizeBox = false,
                MaximizeBox = false
            };

            GridControl accountGrid = new()
            {
                Dock = DockStyle.Fill,
                DataSource = accounts
            };
            GridView accountView = new(accountGrid);
            accountGrid.MainView = accountView;
            accountGrid.ViewCollection.Add(accountView);

            accountView.OptionsBehavior.Editable = false;
            accountView.OptionsSelection.EnableAppearanceFocusedCell = false;
            accountView.OptionsView.ShowGroupPanel = false;
            accountView.OptionsView.ShowAutoFilterRow = true;
            accountView.OptionsView.EnableAppearanceEvenRow = true;
            accountView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;

            PanelControl buttonPanel = new()
            {
                Dock = DockStyle.Bottom,
                Height = 52
            };
            SimpleButton okButton = new()
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(680, 10),
                Size = new Size(92, 30),
                Text = "OK"
            };
            SimpleButton cancelButton = new()
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                DialogResult = DialogResult.Cancel,
                Location = new Point(782, 10),
                Size = new Size(92, 30),
                Text = "Cancel"
            };

            void AcceptSelection()
            {
                if (accountView.FocusedRowHandle < 0)
                {
                    return;
                }

                object value = accountView.GetRowCellValue(accountView.FocusedRowHandle, "KODEACC");
                if (value == null || value == DBNull.Value)
                {
                    return;
                }

                selectedKodeAcc = value.ToString();
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            }

            okButton.Click += (_, _) => AcceptSelection();
            accountView.DoubleClick += (_, _) => AcceptSelection();
            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            dialog.Controls.Add(accountGrid);
            dialog.Controls.Add(buttonPanel);
            dialog.AcceptButton = okButton;
            dialog.CancelButton = cancelButton;

            accountView.BestFitColumns();
            if (dialog.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(selectedKodeAcc))
            {
                return false;
            }

            kodeAcc = selectedKodeAcc;
            return true;
        }

        private DataTable LoadCoaLookupRows()
        {
            const string sql = @"
                SELECT KODEACC,
                       NAMAACC,
                       PARENTACC,
                       ISHEADER,
                       LVL,
                       POSISI
                  FROM ACCT_COA
                 WHERE IDDATA = :iddata
                   AND TAHUN = :tahun
                 ORDER BY KODEACC";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(sql, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };
            command.Parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = CompanyInfo.IDDATA;
            command.Parameters.Add("tahun", OracleDbType.Int32).Value = MAXTAHUN;

            using OracleDataReader reader = command.ExecuteReader();
            DataTable table = new();
            table.Load(reader);
            return table;
        }

        private int GetFocusedSectionDisplayLevel()
        {
            object value = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "DISPLAY_LVL");
            return value == null || value == DBNull.Value ? 1 : Math.Max(1, Convert.ToInt32(value));
        }

        private List<SectionLevelValidationResult> ValidateSectionLevel(int sectionId, int displayLevel)
        {
            return LoadActiveMappings(sectionId)
                .Select(mapping => CreateValidationResult(mapping, displayLevel))
                .ToList();
        }

        private List<SectionAccountMapping> LoadActiveMappings(int sectionId)
        {
            const string sql = @"
                SELECT SECTION_ACCOUNT_ID,
                       KODEACC_ROOT,
                       NVL(MATCH_MODE, 'TREE') MATCH_MODE,
                       GRP_CODE
                  FROM ACCT_REPORT_SECTION_ACCOUNT
                 WHERE SECTION_ID = :sectionId
                   AND IS_ACTIVE = 'Y'
                   AND (
                        (:jenisAkunting = 'PKS' AND JENIS_AKUNTING = 'PKS')
                        OR
                        (:jenisAkunting <> 'PKS' AND JENIS_AKUNTING IN ('*', :jenisAkunting))
                   )
                   AND (IDDATA IS NULL OR IDDATA = :iddata)
                 ORDER BY DISPLAY_ORDER, SECTION_ACCOUNT_ID";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(sql, connection) { BindByName = true };
            command.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
            command.Parameters.Add("jenisAkunting", OracleDbType.Varchar2, 20).Value = CompanyInfo.JENIS_AKUNTING;
            command.Parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = CompanyInfo.IDDATA;

            using OracleDataReader reader = command.ExecuteReader();
            List<SectionAccountMapping> mappings = [];
            while (reader.Read())
            {
                mappings.Add(new SectionAccountMapping(
                    reader.GetInt32(reader.GetOrdinal("SECTION_ACCOUNT_ID")),
                    reader.IsDBNull(reader.GetOrdinal("KODEACC_ROOT")) ? string.Empty : reader.GetString(reader.GetOrdinal("KODEACC_ROOT")),
                    reader.GetString(reader.GetOrdinal("MATCH_MODE")),
                    reader.IsDBNull(reader.GetOrdinal("GRP_CODE")) ? null : reader.GetString(reader.GetOrdinal("GRP_CODE"))));
            }

            return mappings;
        }

        private SectionLevelValidationResult CreateValidationResult(SectionAccountMapping mapping, int displayLevel)
        {
            List<int> availableLevels = GetAvailableLevels(mapping.KodeAccRoot, mapping.MatchMode, mapping.GroupCode);
            return new SectionLevelValidationResult(mapping, displayLevel, availableLevels);
        }

        private List<int> GetAvailableLevels(string kodeAccRoot, string matchMode, string groupCode)
        {
            string normalizedMode = string.IsNullOrWhiteSpace(matchMode) ? "TREE" : matchMode.ToUpperInvariant();
            string sql = normalizedMode switch
            {
                "PARENT" => @"
                    SELECT DISTINCT LVL
                      FROM ACCT_COA
                     WHERE IDDATA = :iddata
                       AND TAHUN = :tahun
                       AND PARENTACC = :kodeAccRoot
                     ORDER BY LVL",
                "GRP_LVL" => @"
                    SELECT DISTINCT LVL
                      FROM ACCT_COA
                     WHERE IDDATA = :iddata
                       AND TAHUN = :tahun
                       AND GRP = :groupCode
                     ORDER BY LVL",
                _ => @"
                    SELECT DISTINCT LVL
                      FROM ACCT_COA
                     WHERE IDDATA = :iddata
                       AND TAHUN = :tahun
                     START WITH KODEACC = :kodeAccRoot
                   CONNECT BY NOCYCLE PRIOR KODEACC = PARENTACC
                          AND PRIOR IDDATA = IDDATA
                          AND PRIOR TAHUN = TAHUN
                     ORDER BY LVL"
            };

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(sql, connection) { BindByName = true };
            command.Parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = CompanyInfo.IDDATA;
            command.Parameters.Add("tahun", OracleDbType.Int32).Value = MAXTAHUN;
            if (normalizedMode == "GRP_LVL")
            {
                command.Parameters.Add("groupCode", OracleDbType.Varchar2, 20).Value = groupCode ?? string.Empty;
            }
            else
            {
                command.Parameters.Add("kodeAccRoot", OracleDbType.Varchar2, 50).Value = kodeAccRoot;
            }

            using OracleDataReader reader = command.ExecuteReader();
            List<int> levels = [];
            while (reader.Read())
            {
                levels.Add(Convert.ToInt32(reader["LVL"]));
            }

            return levels;
        }

        private async Task ShowSectionValidationResultsAsync()
        {
            if (isClosing || IsDisposed)
            {
                return;
            }

            CancelRequest(ref validationCancellation);
            CancellationTokenSource cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetimeCancellation.Token);
            validationCancellation = cancellation;
            string requestedReportCode = currentReportCode;
            Stopwatch stopwatch = Stopwatch.StartNew();

            using IDisposable loadingScope = BeginLoadingScope();
            SetMappingActionsEnabled(false);
            try
            {
                DataTable results = await reportSettingRepository.ValidateSectionsAsync(
                    requestedReportCode,
                    CompanyInfo.IDDATA,
                    MAXTAHUN,
                    CompanyInfo.JENIS_AKUNTING,
                    cancellation.Token);

                if (!ReferenceEquals(validationCancellation, cancellation) ||
                    isClosing ||
                    !string.Equals(currentReportCode, requestedReportCode, StringComparison.Ordinal))
                {
                    return;
                }

                LogNavigationPerformance(
                    "ValidateSections",
                    stopwatch.ElapsedMilliseconds,
                    requestedReportCode,
                    sectionId: null,
                    results.Rows.Count);
                ShowSectionValidationDialog(results, requestedReportCode);
            }
            catch (OperationCanceledException)
            {
                // Expected when the form closes or a newer validation starts.
            }
            catch (OracleException ex) when (ex.Number == 1013)
            {
                if (ReferenceEquals(validationCancellation, cancellation) && !cancellation.IsCancellationRequested)
                {
                    ShowLoadError("Waktu validasi section habis. Silakan coba kembali.", ex);
                }
            }
            catch (TimeoutException ex)
            {
                if (ReferenceEquals(validationCancellation, cancellation) && !cancellation.IsCancellationRequested)
                {
                    ShowLoadError("Waktu validasi section habis. Silakan coba kembali.", ex);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to validate report sections. ReportCode={ReportCode}", requestedReportCode);
                if (ReferenceEquals(validationCancellation, cancellation) && !cancellation.IsCancellationRequested)
                {
                    ShowLoadError("Validasi section tidak dapat diselesaikan.", ex);
                }
            }
            finally
            {
                if (ReferenceEquals(validationCancellation, cancellation))
                {
                    validationCancellation = null;
                    SetMappingActionsEnabled(true);
                }

                cancellation.Dispose();
            }
        }

        private void ShowSectionValidationDialog(DataTable results, string reportCode)
        {
            using XtraForm dialog = new()
            {
                Text = $"Validasi Section {reportCode}",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(980, 560),
                MinimizeBox = false,
                MaximizeBox = false
            };
            GridControl grid = new() { Dock = DockStyle.Fill, DataSource = results };
            GridView view = new(grid);
            grid.MainView = view;
            grid.ViewCollection.Add(view);
            view.OptionsBehavior.Editable = false;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsView.ShowAutoFilterRow = true;
            view.OptionsView.EnableAppearanceEvenRow = true;
            view.BestFitColumns();
            dialog.Controls.Add(grid);
            dialog.ShowDialog(this);
        }

        private static string BuildGapMessage(IEnumerable<SectionLevelValidationResult> gaps, int displayLevel)
        {
            string details = string.Join(Environment.NewLine, gaps.Select(gap =>
                $"• {gap.Mapping.DisplayIdentifier} ({gap.Mapping.MatchMode}): tersedia {FormatLevels(gap.AvailableLevels)}"));
            return $"Section tidak dapat disimpan karena target LVL {displayLevel} tidak tersedia pada root berikut:{Environment.NewLine}{Environment.NewLine}{details}";
        }

        private static string FormatLevels(IEnumerable<int> levels)
        {
            int[] distinctLevels = levels.Distinct().OrderBy(level => level).ToArray();
            return distinctLevels.Length == 0 ? "tidak ada" : string.Join(", ", distinctLevels);
        }
        private bool MappingExists(int sectionId, string kodeAcc)
        {
            const string sql = @"
                SELECT COUNT(1)
                  FROM ACCT_REPORT_SECTION_ACCOUNT
                 WHERE SECTION_ID = :sectionId
                   AND KODEACC_ROOT = :kodeAcc
                   AND IS_ACTIVE = 'Y'
                   AND JENIS_AKUNTING = :jenisAkunting
                   AND (IDDATA IS NULL OR IDDATA = :iddata)";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(sql, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };
            command.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
            command.Parameters.Add("kodeAcc", OracleDbType.Varchar2, 50).Value = kodeAcc;
            command.Parameters.Add("jenisAkunting", OracleDbType.Varchar2, 20).Value = CompanyInfo.JENIS_AKUNTING;
            command.Parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = CompanyInfo.IDDATA;
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        private bool ReactivateInactiveMapping(int sectionId, string kodeAcc)
        {
            const string sql = @"
                UPDATE ACCT_REPORT_SECTION_ACCOUNT
                   SET IS_ACTIVE = 'Y'
                 WHERE SECTION_ID = :sectionId
                   AND KODEACC_ROOT = :kodeAcc
                   AND IS_ACTIVE = 'N'
                   AND JENIS_AKUNTING = :jenisAkunting
                   AND (IDDATA IS NULL OR IDDATA = :iddata)";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(sql, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };
            command.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
            command.Parameters.Add("kodeAcc", OracleDbType.Varchar2, 50).Value = kodeAcc;
            command.Parameters.Add("jenisAkunting", OracleDbType.Varchar2, 20).Value = CompanyInfo.JENIS_AKUNTING;
            command.Parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = CompanyInfo.IDDATA;
            return command.ExecuteNonQuery() > 0;
        }
        private void AddRootMapping(int sectionId, string kodeAcc)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(AddRootMappingSql, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };
            command.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
            command.Parameters.Add("jenisAkunting", OracleDbType.Varchar2, 20).Value = CompanyInfo.JENIS_AKUNTING;
            command.Parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = CompanyInfo.IDDATA;
            command.Parameters.Add("kodeAcc", OracleDbType.Varchar2, 50).Value = kodeAcc;
            command.ExecuteNonQuery();
        }

        private static void UpdateMappingActive(int sectionAccountId, string isActive)
        {
            const string sql = @"
                UPDATE ACCT_REPORT_SECTION_ACCOUNT
                   SET IS_ACTIVE = :isActive
                 WHERE SECTION_ACCOUNT_ID = :sectionAccountId";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(sql, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };
            command.Parameters.Add("isActive", OracleDbType.Char, 1).Value = NormalizeFlag(isActive);
            command.Parameters.Add("sectionAccountId", OracleDbType.Int32).Value = sectionAccountId;
            command.ExecuteNonQuery();
        }

        private bool MoveFocusedMapping(int direction)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                return false;
            }

            if (gridControl2.DataSource is not DataTable table || table.Rows.Count == 0)
            {
                return false;
            }

            int currentIndex = gridView2.GetDataSourceRowIndex(gridView2.FocusedRowHandle);
            int targetIndex = currentIndex + direction;
            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= table.Rows.Count)
            {
                return false;
            }

            DataRow currentRow = table.Rows[currentIndex];
            DataRow targetRow = table.Rows[targetIndex];
            SwapMappingOrder(
                Convert.ToInt32(currentRow["SECTION_ACCOUNT_ID"]),
                Convert.ToInt32(currentRow["URUT"]),
                Convert.ToInt32(targetRow["SECTION_ACCOUNT_ID"]),
                Convert.ToInt32(targetRow["URUT"]));
            return true;
        }

        private static void SwapMappingOrder(int firstId, int firstOrder, int secondId, int secondOrder)
        {
            const string sql = @"
                UPDATE ACCT_REPORT_SECTION_ACCOUNT
                   SET DISPLAY_ORDER = CASE SECTION_ACCOUNT_ID
                       WHEN :firstId THEN :secondOrder
                       WHEN :secondId THEN :firstOrder
                       ELSE DISPLAY_ORDER
                   END
                 WHERE SECTION_ACCOUNT_ID IN (:firstId, :secondId)";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(sql, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };
            command.Parameters.Add("firstId", OracleDbType.Int32).Value = firstId;
            command.Parameters.Add("secondOrder", OracleDbType.Int32).Value = secondOrder;
            command.Parameters.Add("secondId", OracleDbType.Int32).Value = secondId;
            command.Parameters.Add("firstOrder", OracleDbType.Int32).Value = firstOrder;
            command.ExecuteNonQuery();
        }

        private int? GetFocusedSectionIdOrNull()
        {
            if (gridView1.FocusedRowHandle < 0)
            {
                return null;
            }

            object value = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "SECTION_ID");
            return value == null || value == DBNull.Value ? null : Convert.ToInt32(value);
        }

        private bool IsCurrentReportRequest(
            CancellationTokenSource cancellation,
            int requestVersion,
            string requestedReportCode)
        {
            return !isClosing &&
                   !IsDisposed &&
                   !cancellation.IsCancellationRequested &&
                   ReferenceEquals(reportLoadCancellation, cancellation) &&
                   reportLoadVersion == requestVersion &&
                   string.Equals(currentReportCode, requestedReportCode, StringComparison.Ordinal);
        }

        private bool IsCurrentMappingRequest(
            CancellationTokenSource cancellation,
            int requestVersion,
            string requestedReportCode,
            int sectionId)
        {
            return !isClosing &&
                   !IsDisposed &&
                   !cancellation.IsCancellationRequested &&
                   ReferenceEquals(mappingLoadCancellation, cancellation) &&
                   mappingLoadVersion == requestVersion &&
                   string.Equals(currentReportCode, requestedReportCode, StringComparison.Ordinal) &&
                   GetFocusedSectionIdOrNull() == sectionId;
        }

        private bool IsPksAccounting()
        {
            return string.Equals(CompanyInfo.JENIS_AKUNTING, "PKS", StringComparison.OrdinalIgnoreCase);
        }

        private void SetMappingActionsEnabled(bool enabled)
        {
            if (addRootButton == null)
            {
                return;
            }

            bool canEdit = enabled && canManageSetup;
            addRootButton.Enabled = canEdit;
            deactivateRootButton.Enabled = canEdit;
            moveRootUpButton.Enabled = canEdit;
            moveRootDownButton.Enabled = canEdit;
            validateSectionsButton.Enabled = enabled;
        }

        private IDisposable BeginLoadingScope()
        {
            lock (overlaySync)
            {
                overlayDepth++;
                if (overlayDepth == 1 &&
                    !isClosing &&
                    !IsDisposed &&
                    IsHandleCreated &&
                    settingsSplitContainer != null)
                {
                    try
                    {
                        overlayHandle = SplashScreenManager.ShowOverlayForm(settingsSplitContainer);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Unable to show report settings loading overlay");
                        overlayHandle = null;
                    }
                }
            }

            return new LoadingScope(this);
        }

        private void EndLoadingScope()
        {
            lock (overlaySync)
            {
                if (overlayDepth <= 0)
                {
                    return;
                }

                overlayDepth--;
                if (overlayDepth > 0)
                {
                    return;
                }

                CloseLoadingOverlay();
            }
        }

        private void CloseLoadingOverlay()
        {
            if (overlayHandle == null)
            {
                return;
            }

            try
            {
                SplashScreenManager.CloseOverlayForm(overlayHandle);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Unable to close report settings loading overlay");
            }
            finally
            {
                overlayHandle = null;
            }
        }

        private void LogNavigationPerformance(
            string operation,
            long elapsedMilliseconds,
            string reportCode,
            int? sectionId,
            int rowCount)
        {
            int warningThreshold = string.Equals(operation, "ValidateSections", StringComparison.Ordinal)
                ? 3000
                : 500;

            if (elapsedMilliseconds > warningThreshold)
            {
                Log.Warning(
                    "PERF FrmSettingRL.{Operation} slow elapsed_ms={ElapsedMs} report_code={ReportCode} section_id={SectionId} row_count={RowCount}",
                    operation,
                    elapsedMilliseconds,
                    reportCode,
                    sectionId,
                    rowCount);
                return;
            }

            Log.Information(
                "PERF FrmSettingRL.{Operation} elapsed_ms={ElapsedMs} report_code={ReportCode} section_id={SectionId} row_count={RowCount}",
                operation,
                elapsedMilliseconds,
                reportCode,
                sectionId,
                rowCount);
        }

        private void ShowLoadError(string message, Exception exception)
        {
            Log.Warning(exception, "Report settings load error: {Message}", message);
            if (isClosing || IsDisposed)
            {
                return;
            }

            XtraMessageBox.Show(message, "Pengaturan Laporan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void CancelRequest(ref CancellationTokenSource? cancellation)
        {
            CancellationTokenSource? current = cancellation;
            cancellation = null;
            if (current == null)
            {
                return;
            }

            try
            {
                current.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // The completed request already disposed its cancellation source.
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            isClosing = true;
            try
            {
                lifetimeCancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // The form lifetime was already disposed.
            }

            CancelRequest(ref reportLoadCancellation);
            CancelRequest(ref mappingLoadCancellation);
            CancelRequest(ref validationCancellation);

            if (focusedRowEventAttached)
            {
                gridView1.FocusedRowChanged -= gridView1_FocusedRowChanged;
                focusedRowEventAttached = false;
            }

            lock (overlaySync)
            {
                overlayDepth = 0;
                CloseLoadingOverlay();
            }

            lifetimeCancellation.Dispose();
            base.OnFormClosed(e);
        }

        private sealed class LoadingScope : IDisposable
        {
            private FrmSettingRL? owner;

            public LoadingScope(FrmSettingRL owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                FrmSettingRL? currentOwner = Interlocked.Exchange(ref owner, null);
                currentOwner?.EndLoadingScope();
            }
        }

        private sealed record SectionAccountMapping(int SectionAccountId, string KodeAccRoot, string MatchMode, string GroupCode)
        {
            public string DisplayIdentifier => string.Equals(MatchMode, "GRP_LVL", StringComparison.OrdinalIgnoreCase)
                ? $"GRP:{GroupCode ?? "-"}"
                : KodeAccRoot;
        }

        private sealed record SectionLevelValidationResult(
            SectionAccountMapping Mapping,
            int DisplayLevel,
            List<int> AvailableLevels)
        {
            public bool IsValid => AvailableLevels.Contains(DisplayLevel);
        }

        private static string GetString(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? string.Empty : row[column].ToString();
        }

        private static int GetInt(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? 0 : Convert.ToInt32(row[column]);
        }

        private static int GetInt(DataRow row, string column, DataRowVersion version)
        {
            return row[column, version] == DBNull.Value ? 0 : Convert.ToInt32(row[column, version]);
        }

        private static string NormalizeFlag(string value)
        {
            return string.Equals(value, "Y", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";
        }

        private static string NormalizePosisi(string value)
        {
            return string.Equals(value, "K", StringComparison.OrdinalIgnoreCase) ? "K" : "D";
        }
    }
}
