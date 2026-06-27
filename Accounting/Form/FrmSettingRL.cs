using Accounting.BusinessLayer;
using Accounting.Services;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmSettingRL : DevExpress.XtraEditors.XtraForm
    {
        private readonly OracleConnection conn = new(LoginInfo.OracleConnString);
        private DataSet DSSetup;
        private int MAXTAHUN;
        private bool focusedRowEventAttached;
        private SimpleButton addRootButton;
        private SimpleButton deactivateRootButton;
        private SimpleButton moveRootUpButton;
        private SimpleButton moveRootDownButton;
        private XtraTabControl tabControl;
        private string currentReportCode = "LABARUGI";

        public FrmSettingRL()
        {
            InitializeComponent();
        }

        private void FrmSettingRL_Load(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                Close();
                return;
            }

            MAXTAHUN = AccountServices.MaxTahunCOA(CompanyInfo.IDDATA);
            string setlr = ConfigurationManager.AppSettings["setLR"];
            checkEdit1.EditValue = setlr == "Ya";

            ConfigureLayout();
            ConfigureMappingButtons();
            ConfigureSectionGrid();
            ConfigureMappingGrid();
            Load_SettingsRL();
            BindSectionGrid();
            AttachFocusedRowChanged();
            Load_AkunSetting();

            bool canManage = AuthorizationService.CanManageProfitLossSetup();
            gridView1.OptionsBehavior.Editable = canManage;
            simpleButton1.Enabled = canManage;
            checkEdit1.Enabled = canManage;
            addRootButton.Enabled = canManage;
            deactivateRootButton.Enabled = canManage;
            moveRootUpButton.Enabled = canManage;
            moveRootDownButton.Enabled = canManage;
        }

        private void ConfigureLayout()
        {
            Text = "Pengaturan Laporan";
            MinimumSize = new Size(1180, 680);

            // Initialize and configure TabControl programmatically at the top
            tabControl = new XtraTabControl
            {
                Location = new Point(0, 0),
                Size = new Size(ClientSize.Width, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            tabControl.TabPages.Add("Laba Rugi");
            tabControl.TabPages.Add("Neraca");
            tabControl.SelectedPageChanged += TabControl_SelectedPageChanged;
            Controls.Add(tabControl);

            gridControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            gridControl1.Location = new Point(12, 52);
            gridControl1.Size = new Size(520, ClientSize.Height - 132);

            gridControl2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridControl2.Location = new Point(544, 92);
            gridControl2.Size = new Size(ClientSize.Width - 556, ClientSize.Height - 104);

            labelControl1.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            labelControl1.Location = new Point(12, ClientSize.Height - 62);
            labelControl1.Text = "Tampilkan Nilai 0";

            checkEdit1.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            checkEdit1.Location = new Point(150, ClientSize.Height - 70);
            checkEdit1.Properties.OffText = "Tidak";
            checkEdit1.Properties.OnText = "Ya";

            simpleButton1.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            simpleButton1.Location = new Point(402, ClientSize.Height - 70);
            simpleButton1.Size = new Size(130, 34);
            simpleButton1.Text = "Reset Default";
        }

        private void TabControl_SelectedPageChanged(object sender, TabPageChangedEventArgs e)
        {
            currentReportCode = tabControl.SelectedTabPageIndex == 0 ? "LABARUGI" : "NERACA";
            Text = $"Pengaturan Laporan {(currentReportCode == "LABARUGI" ? "Laba Rugi" : "Neraca")}";

            Load_SettingsRL();
            BindSectionGrid();
            if (gridView1.RowCount > 0)
            {
                gridView1.FocusedRowHandle = 0;
            }
            Load_AkunSetting();
        }

        private void ConfigureMappingButtons()
        {
            addRootButton = CreateMappingButton("Add Root", 544, 52, addRootButton_Click);
            deactivateRootButton = CreateMappingButton("Deactivate", 654, 52, deactivateRootButton_Click);
            moveRootUpButton = CreateMappingButton("Move Up", 780, 52, moveRootUpButton_Click);
            moveRootDownButton = CreateMappingButton("Move Down", 890, 52, moveRootDownButton_Click);

            Controls.Add(addRootButton);
            Controls.Add(deactivateRootButton);
            Controls.Add(moveRootUpButton);
            Controls.Add(moveRootDownButton);
        }

        private static SimpleButton CreateMappingButton(string text, int x, int y, EventHandler clickHandler)
        {
            SimpleButton button = new()
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Location = new Point(x, y),
                Size = new Size(104, 30),
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
            gridView1.OptionsBehavior.EditorShowMode = DevExpress.Utils.EditorShowMode.MouseDownFocused;

            NO.Caption = "URUT";
            NO.FieldName = "DISPLAY_ORDER";
            NO.Visible = true;
            NO.VisibleIndex = 0;
            NO.Width = 58;

            KELOMPOK.Caption = "SECTION";
            KELOMPOK.FieldName = "SECTION_NAME";
            KELOMPOK.OptionsColumn.AllowEdit = true;
            KELOMPOK.Visible = true;
            KELOMPOK.VisibleIndex = 1;
            KELOMPOK.Width = 230;

            LVL.Caption = "LVL";
            LVL.FieldName = "DISPLAY_LVL";
            LVL.OptionsColumn.AllowEdit = true;
            LVL.Visible = true;
            LVL.VisibleIndex = 2;
            LVL.Width = 70;

            TAMPILKAN.Caption = "ZERO";
            TAMPILKAN.FieldName = "SHOW_ZERO";
            TAMPILKAN.Visible = true;
            TAMPILKAN.VisibleIndex = 3;
            TAMPILKAN.Width = 82;

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
        }

        private DataSet Load_SettingsRL()
        {
            string selectQuery = $@"
                SELECT SECTION_ID,
                       SECTION_CODE,
                       SECTION_NAME,
                       DISPLAY_ORDER,
                       NORMAL_POSISI,
                       DISPLAY_LVL,
                       SHOW_ZERO,
                       IS_ACTIVE
                  FROM ACCT_REPORT_SECTION
                 WHERE REPORT_CODE = '{currentReportCode}'
                 ORDER BY DISPLAY_ORDER";

            using OracleCommand command = new(selectQuery, conn)
            {
                CommandType = CommandType.Text
            };

            using OracleDataAdapter adapter = new(command);
            DSSetup = new DataSet();
            adapter.Fill(DSSetup, "Setup");
            return DSSetup;
        }

        private void BindSectionGrid()
        {
            gridControl1.DataSource = DSSetup;
            gridControl1.DataMember = "Setup";
            gridView1.BestFitColumns();
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

        private void gridView1_RowUpdated(object sender, RowObjectEventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                return;
            }

            if (e.Row is not DataRowView rowView)
            {
                return;
            }

            SaveSection(rowView.Row);
            Load_SettingsRL();
            BindSectionGrid();
            Load_AkunSetting();
        }

        private void SaveSection(DataRow row)
        {
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
            command.Parameters.Add("displayLvl", OracleDbType.Int32).Value = Math.Max(1, GetInt(row, "DISPLAY_LVL"));
            command.Parameters.Add("showZero", OracleDbType.Char, 1).Value = NormalizeFlag(GetString(row, "SHOW_ZERO"));
            command.Parameters.Add("isActive", OracleDbType.Char, 1).Value = NormalizeFlag(GetString(row, "IS_ACTIVE"));
            command.Parameters.Add("sectionId", OracleDbType.Int32).Value = GetInt(row, "SECTION_ID");
            command.ExecuteNonQuery();
        }

        private void Load_ListAccount(int sectionId)
        {
            const string selectQuery = @"
                SELECT account.SECTION_ACCOUNT_ID,
                       account.DISPLAY_ORDER URUT,
                       account.KODEACC_ROOT KODEACC,
                       coa.NAMAACC,
                       coa.PARENTACC,
                       coa.ISHEADER,
                       coa.LVL,
                       coa.POSISI,
                       account.JENIS_AKUNTING,
                       NVL(account.IDDATA, '*') IDDATA_SCOPE,
                       NVL(TO_CHAR(account.TAHUN), '*') TAHUN_SCOPE,
                       account.INCLUDE_CHILDREN,
                       account.IS_ACTIVE
                  FROM ACCT_REPORT_SECTION_ACCOUNT account
                  LEFT JOIN ACCT_COA coa
                    ON coa.IDDATA = :iddata
                   AND coa.TAHUN = :tahun
                   AND coa.KODEACC = account.KODEACC_ROOT
                 WHERE account.SECTION_ID = :sectionId
                   AND account.IS_ACTIVE = 'Y'
                   AND account.JENIS_AKUNTING IN ('*', :jenisAkunting)
                   AND (account.IDDATA IS NULL OR account.IDDATA = :iddata)
                   AND (account.TAHUN IS NULL OR account.TAHUN = :tahun)
                 ORDER BY account.DISPLAY_ORDER, account.KODEACC_ROOT";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(selectQuery, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };
            command.Parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = CompanyInfo.IDDATA;
            command.Parameters.Add("tahun", OracleDbType.Int32).Value = MAXTAHUN;
            command.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
            command.Parameters.Add("jenisAkunting", OracleDbType.Varchar2, 20).Value = CompanyInfo.JENIS_AKUNTING;

            using OracleDataReader reader = command.ExecuteReader();
            DataTable table = new();
            table.Load(reader);
            gridControl2.DataSource = table;
            gridView2.BestFitColumns();
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

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                return;
            }

            string resetSql = $@"
                UPDATE ACCT_REPORT_SECTION
                   SET SHOW_ZERO = 'N',
                       IS_ACTIVE = 'Y'
                  WHERE REPORT_CODE = '{currentReportCode}'";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(resetSql, connection)
            {
                CommandType = CommandType.Text
            };
            command.ExecuteNonQuery();

            Load_SettingsRL();
            BindSectionGrid();
            Load_AkunSetting();
        }

        private void gridControl1_Click(object sender, EventArgs e)
        {
            Load_AkunSetting();
        }

        private void gridView1_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            Load_AkunSetting();
        }

        private void Load_AkunSetting()
        {
            if (gridView1.FocusedRowHandle < 0)
            {
                gridControl2.DataSource = null;
                return;
            }

            object sectionIdValue = gridView1.GetRowCellValue(gridView1.FocusedRowHandle, "SECTION_ID");
            if (sectionIdValue == null || sectionIdValue == DBNull.Value)
            {
                gridControl2.DataSource = null;
                return;
            }

            Load_ListAccount(Convert.ToInt32(sectionIdValue));
        }

        private void addRootButton_Click(object sender, EventArgs e)
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

            if (!ReactivateInactiveMapping(sectionId, kodeAcc))
            {
                AddRootMapping(sectionId, kodeAcc);
            }

            Load_AkunSetting();
        }

        private void deactivateRootButton_Click(object sender, EventArgs e)
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
            Load_AkunSetting();
        }

        private void moveRootUpButton_Click(object sender, EventArgs e)
        {
            MoveFocusedMapping(-1);
        }

        private void moveRootDownButton_Click(object sender, EventArgs e)
        {
            MoveFocusedMapping(1);
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
        private bool MappingExists(int sectionId, string kodeAcc)
        {
            const string sql = @"
                SELECT COUNT(1)
                  FROM ACCT_REPORT_SECTION_ACCOUNT
                 WHERE SECTION_ID = :sectionId
                   AND KODEACC_ROOT = :kodeAcc
                   AND IS_ACTIVE = 'Y'
                   AND JENIS_AKUNTING = :jenisAkunting
                   AND (IDDATA IS NULL OR IDDATA = :iddata)
                   AND (TAHUN IS NULL OR TAHUN = :tahun)";

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
            command.Parameters.Add("tahun", OracleDbType.Int32).Value = MAXTAHUN;
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
                   AND (IDDATA IS NULL OR IDDATA = :iddata)
                   AND (TAHUN IS NULL OR TAHUN = :tahun)";

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
            command.Parameters.Add("tahun", OracleDbType.Int32).Value = MAXTAHUN;
            return command.ExecuteNonQuery() > 0;
        }
        private void AddRootMapping(int sectionId, string kodeAcc)
        {
            const string sql = @"
                INSERT INTO ACCT_REPORT_SECTION_ACCOUNT (
                    SECTION_ID,
                    JENIS_AKUNTING,
                    IDDATA,
                    TAHUN,
                    KODEACC_ROOT,
                    DISPLAY_ORDER,
                    INCLUDE_CHILDREN,
                    IS_ACTIVE)
                SELECT :sectionId,
                       :jenisAkunting,
                       :iddata,
                       :tahun,
                       :kodeAcc,
                       NVL(MAX(DISPLAY_ORDER), 0) + 10,
                       'Y',
                       'Y'
                  FROM ACCT_REPORT_SECTION_ACCOUNT
                 WHERE SECTION_ID = :sectionId";

            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            using OracleCommand command = new(sql, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };
            command.Parameters.Add("sectionId", OracleDbType.Int32).Value = sectionId;
            command.Parameters.Add("jenisAkunting", OracleDbType.Varchar2, 20).Value = CompanyInfo.JENIS_AKUNTING;
            command.Parameters.Add("iddata", OracleDbType.Varchar2, 20).Value = CompanyInfo.IDDATA;
            command.Parameters.Add("tahun", OracleDbType.Int32).Value = MAXTAHUN;
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

        private void MoveFocusedMapping(int direction)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageProfitLossSetup))
            {
                return;
            }

            if (gridControl2.DataSource is not DataTable table || table.Rows.Count == 0)
            {
                return;
            }

            int currentIndex = gridView2.GetDataSourceRowIndex(gridView2.FocusedRowHandle);
            int targetIndex = currentIndex + direction;
            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= table.Rows.Count)
            {
                return;
            }

            DataRow currentRow = table.Rows[currentIndex];
            DataRow targetRow = table.Rows[targetIndex];
            SwapMappingOrder(
                Convert.ToInt32(currentRow["SECTION_ACCOUNT_ID"]),
                Convert.ToInt32(currentRow["URUT"]),
                Convert.ToInt32(targetRow["SECTION_ACCOUNT_ID"]),
                Convert.ToInt32(targetRow["URUT"]));
            Load_AkunSetting();
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
        private static string GetString(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? string.Empty : row[column].ToString();
        }

        private static int GetInt(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? 0 : Convert.ToInt32(row[column]);
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
