using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraBars.Alerter;
using DevExpress.XtraBars.Docking2010;
using System;
using Accounting.Services;
using System.Windows.Forms;
using Accounting._3.Services;
using System.Linq;

namespace Accounting.Form
{
    public partial class FrmUsers : DevExpress.XtraEditors.XtraForm
    {
       
        public FrmUsers()
        {
            InitializeComponent();
        }
        private void FrmUsers_Load(object sender, EventArgs e)
        {
            try
            {
                AuthorizationService.EnsureCanViewUserManagement();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BeginInvoke(new Action(Close));
                return;
            }

            Load_Lokasi_Akses();
            load_user();
            lblmodule.Text = LoginInfo.MODULE;
            lbluser.Text = LoginInfo.userID;

            if (LoginInfo.MODULE == "FINANCE")
            {
                Load_Akses_Estate(LoginInfo.userID);
            }
            else if (LoginInfo.MODULE == "ACCOUNTING")
            {
                Load_Akses_IDDATA(LoginInfo.userID);
            }

            layoutControlItem15.ContentVisible = false;
            ApplyAuthorizationState();
        }

        private void load_user()
        {
            gridControl1.DataSource = UserManager_Services.GetAllUsers();

            // Sembunyikan semua kolom security/audit — hanya tampilkan data esensial
            string[] visibleColumns = { "USERID", "NAMA", "DEPT", "JABATAN", "AKTIF" };
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gridView1.Columns)
            {
                col.Visible = Array.Exists(visibleColumns, c => c == col.FieldName);
            }
            gridView1.Columns["AKTIF"].Width = 50;
            gridView1.BestFitColumns();
            
            // UI UX Enhancements for GridView
            gridView1.RowHeight = 35;
            gridView1.Appearance.Row.Font = new System.Drawing.Font("Segoe UI", 10F);
            gridView1.Appearance.HeaderPanel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            gridView1.OptionsView.ShowIndicator = false;

            GridFormatRule gridFormatRule = new();
            FormatConditionRuleExpression formatConditionRuleExpression = new FormatConditionRuleExpression();


            gridFormatRule.Column = gridView1.Columns["AKTIF"]; // Assuming "Stock" is the correct column name
            gridFormatRule.ApplyToRow = true;

            formatConditionRuleExpression.PredefinedName = "Red Fill, Red Text";
            formatConditionRuleExpression.Expression = "[AKTIF] =='T'";  // Update the condition to check if "Stock" is less than 0

            gridFormatRule.Rule = formatConditionRuleExpression;
            gridView1.FormatRules.Add(gridFormatRule);
        }

        private void ApplyAuthorizationState()
        {
            bool canManageUsers = AuthorizationService.CanManageUsers();
            bool canResetPasswords = AuthorizationService.CanResetPasswords();

            // WindowsUIButtonPanel state management
            SetWUIButtonEnabled("SIMPAN", canManageUsers);
            SetWUIButtonEnabled("BARU", canManageUsers);
            btnaddestate.Enabled = canManageUsers;
        }

        // ── Toast Notification Helpers ──────────────────────────────────
        private void ShowToast(string caption, string message)
        {
            alertControl1.Show(this, caption, message);
        }

        private void ShowToastError(string caption, string message)
        {
            alertControl1.Show(this, caption, message);
        }

        // ── WindowsUIButtonPanel Helpers ────────────────────────────────
        private void SetWUIButtonVisible(string tag, bool visible)
        {
            foreach (var btn in windowsUIButtonPanel1.Buttons.OfType<WindowsUIButton>())
            {
                if (btn.Tag?.ToString() == tag)
                    btn.Visible = visible;
            }
            windowsUIButtonPanel1.Refresh();
        }

        private void SetWUIButtonEnabled(string tag, bool enabled)
        {
            foreach (var btn in windowsUIButtonPanel1.Buttons.OfType<WindowsUIButton>())
            {
                if (btn.Tag?.ToString() == tag)
                    btn.Enabled = enabled;
            }
            windowsUIButtonPanel1.Refresh();
        }

        private void SetWUIButtonCaption(string tag, string caption)
        {
            foreach (var btn in windowsUIButtonPanel1.Buttons.OfType<WindowsUIButton>())
            {
                if (btn.Tag?.ToString() == tag)
                    btn.Caption = caption;
            }
            windowsUIButtonPanel1.Refresh();
        }

        // ── WindowsUIButtonPanel Click Router ──────────────────────────
        private void windowsUIButtonPanel1_ButtonClick(object sender, ButtonEventArgs e)
        {
            string tag = (e.Button as WindowsUIButton)?.Tag?.ToString() ?? "";
            switch (tag)
            {
                case "BARU":
                    sbbaru_Click(sender, EventArgs.Empty);
                    break;
                case "SIMPAN":
                    btnsimpan_Click(sender, EventArgs.Empty);
                    break;
                case "HAPUS":
                    btnhapus_Click(sender, EventArgs.Empty);
                    break;
                case "RESETPWD":
                    btnResetPassword_Click(sender, EventArgs.Empty);
                    break;
            }
        }

        private void Load_Lokasi_Akses()
        {
            if (LoginInfo.MODULE == "FINANCE")
            {
                lookUpEdit1.Properties.DataSource = UserManager_Services.GetWilayahEstate();
                lookUpEdit1.Properties.ValueMember = "ID";
                lookUpEdit1.Properties.DisplayMember = "ESTATE";
                lookUpEdit1.Properties.ForceInitialize();
                lookUpEdit1.Properties.PopulateColumns();
                lookUpEdit1.Properties.Columns[0].Visible = false;
                lookUpEdit1.Properties.BestFit();
            }
            else if (LoginInfo.MODULE == "ACCOUNTING")
            {
                lookUpEdit1.Properties.DataSource = UserManager_Services.GetListIDDATA();
                lookUpEdit1.Properties.ValueMember = "IDDATA";
                lookUpEdit1.Properties.DisplayMember = "IDDATA";
                lookUpEdit1.Properties.ForceInitialize();
                lookUpEdit1.Properties.PopulateColumns();
                // lookUpEdit1.Properties.Columns[0].Visible = false;
                lookUpEdit1.Properties.BestFit();

            }

        }


        private void btnsimpan_Click(object sender, EventArgs e)
        {
            try
            {
                string pwd = pASSWORDTextEdit.Text;
                string savePasswordHash = null;
                if (!string.IsNullOrEmpty(pwd))
                {
                    if (!UserManager_Services.TryValidatePasswordPolicy(pwd, out string validationMessage))
                    {
                        XtraMessageBox.Show(validationMessage, "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        pASSWORDTextEdit.Focus();
                        return;
                    }

                    var p = new PasswordCryptographyPbkdf2();
                    savePasswordHash = p.GetHashPassword(pwd);
                }
                string p_aktive = "T";
                if (checkEditaktif.IsOn == true)
                {
                    p_aktive = "Y";
                }

                // Assuming you have instances of USERID_DTO and USERID_DETAIL_DTO
                USERID_DTO user = new()
                {
                    USERID = uSERIDTextEdit.Text,
                    NAMA = nAMATextEdit.Text,
                    DEPT = dEPTTextEdit.Text,
                    PASSWORD = savePasswordHash,
                    JABATAN = jABATANTextEdit.Text,
                    AKTIF = p_aktive,
                };
                if (btnsimpan.Text == "Add")
                {

                    if (string.IsNullOrEmpty(uSERIDTextEdit.Text) || string.IsNullOrEmpty(pASSWORDTextEdit.Text))
                    {
                        XtraMessageBox.Show("UserID dan Password harus diisi", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        uSERIDTextEdit.Focus();
                        return;
                    }
                    if (pASSWORDTextEdit.Text != confirmasi.Text)
                    {
                        XtraMessageBox.Show("Konfirmasi Password tidak cocok", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        confirmasi.Focus();
                        return;
                    }
                    int moduleid = Permission_Services.GetModuleId(LoginInfo.MODULE);
                    // Call the AddUser method
                    UserManager_Services.AddUser(user);
                    UserManager_Services.AddUserRole(uSERIDTextEdit.Text, 6, moduleid);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(pASSWORDTextEdit.Text) || !string.IsNullOrWhiteSpace(confirmasi.Text))
                    {
                        if (string.IsNullOrEmpty(uSERIDTextEdit.Text) || string.IsNullOrEmpty(pASSWORDTextEdit.Text))
                        {
                            XtraMessageBox.Show("UserID dan Password harus diisi", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            uSERIDTextEdit.Focus();
                            return;
                        }

                        if (pASSWORDTextEdit.Text != confirmasi.Text)
                        {
                            XtraMessageBox.Show("Konfirmasi Password tidak cocok", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            confirmasi.Focus();
                            return;
                        }
                    }
                    UserManager_Services.UpdateUser(user);
                }

                Bersihkan();
                load_user();
                ShowToast("✅ Sukses", "UserID berhasil disimpan");

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    uSERIDTextEdit.Focus();
                    XtraMessageBox.Show("Duplicate UserID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Bersihkan()
        {
            uSERIDTextEdit.Text = "";
            pASSWORDTextEdit.Text = "";
            confirmasi.Text = "";
            nAMATextEdit.Text = "";
            dEPTTextEdit.Text = "";
            jABATANTextEdit.Text = "";
            layoutControlItem15.ContentVisible = false;
            checkEditaktif.Visible = false;
            btnsimpan.Text = "Add";
            SetWUIButtonCaption("SIMPAN", "Simpan");
            uSERIDTextEdit.ReadOnly = false;
            SetWUIButtonVisible("HAPUS", false);
            SetWUIButtonVisible("RESETPWD", false);
            lbluser.Text = "";

        }

        private void gridView1_Click(object sender, EventArgs e)
        {
            LoadUsersAkses();
        }

        private void LoadUsersAkses()
        {
            if (gridView1.SelectedRowsCount > 0)
            {
                int selectedIndex = gridView1.GetSelectedRows()[0];
                int selectedHandle = gridView1.GetVisibleRowHandle(selectedIndex);
                USERID_DTO selectedItem = gridView1.GetRow(selectedHandle) as USERID_DTO;
                uSERIDTextEdit.Text = selectedItem.USERID;
                nAMATextEdit.Text = selectedItem.NAMA;
                dEPTTextEdit.Text = selectedItem.DEPT;
                jABATANTextEdit.Text = selectedItem.JABATAN;
                pASSWORDTextEdit.Text = "";
                confirmasi.Text = "";
                layoutControlItem15.ContentVisible = true;
                if (selectedItem.AKTIF == "Y")
                {
                    checkEditaktif.IsOn = true;
                }
                else
                {
                    checkEditaktif.IsOn = false;
                }

                if (LoginInfo.MODULE == "FINANCE")
                {
                    Load_Akses_Estate(selectedItem.USERID);
                }
                else if (LoginInfo.MODULE == "ACCOUNTING")
                {
                    Load_Akses_IDDATA(selectedItem.USERID);
                }


               
                btnsimpan.Text = "Update";
                SetWUIButtonCaption("SIMPAN", "Update");
                uSERIDTextEdit.ReadOnly = true;
                SetWUIButtonVisible("HAPUS", AuthorizationService.CanManageUsers());
                SetWUIButtonVisible("RESETPWD", AuthorizationService.CanResetPasswords());
            }

        }

        private void Load_Akses_IDDATA(string uSERID)
        {
            gridControl2.DataSource = UserManager_Services.GetAksesIDDATAbyUserID(uSERID, LoginInfo.MODULE);

            gridView2.Columns["ROLE_ID"].Visible = false;
            gridView2.BestFitColumns();
        }

        private void btnhapus_Click(object sender, EventArgs e)
        {
            if (this.gridView1.GetFocusedRowCellValue("USERID") == null)
            {
                return;
            }

            if (XtraMessageBox.Show("Hapus UserID ? : " + lbluser.Text, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                UserManager_Services.DeleteUser(lbluser.Text);
                ShowToast("🗑️ Dihapus", lbluser.Text + " berhasil dihapus");
                gridControl1.DataSource = UserManager_Services.GetAllUsers();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uSERIDTextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void pASSWORDTextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void confirmasi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void nAMATextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void dEPTTextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void jABATANTextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void lEVELIDComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (this.gridView2.GetFocusedRowCellValue("IDDATA") == null)
                return;
            var rowhandle = gridView2.FocusedRowHandle;
            var P_IDDATA = gridView2.GetRowCellValue(rowhandle, "IDDATA").ToString();

            if (XtraMessageBox.Show("Hapus Akses ke Lokasi Data ? : " + P_IDDATA, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            ShowToast("🗑️ Dihapus", "Akses ke lokasi pembukuan " + P_IDDATA + " berhasil dihapus");
            LoadUsersAkses();
        }
      


        private void Load_Akses_Estate(string? p_userid)
        {
            gridControl2.DataSource = UserManager_Services.GetAksesEstatebyUserID(p_userid, LoginInfo.MODULE);
            gridView2.Columns["ESTATE_ID"].Visible = false;
            gridView2.Columns["ROLE_ID"].Visible = false;
            gridView2.BestFitColumns();
        }



        private void gridView2_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                GridView view = sender as GridView;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
                {
                    int rowHandle = e.HitInfo.RowHandle;
                    //hapus menu jika ada
                    e.Menu.Items.Clear();

                    DXMenuItem hapus = CreateMenuItemHapus(view, rowHandle);


                    hapus.BeginGroup = true;
                    e.Menu.Items.Add(hapus);
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Popup Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DXMenuItem CreateMenuItemHapus(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Hapus", new EventHandler(OnHapusClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[0];
            return checkItem;
        }

        private void OnHapusClick(object? sender, EventArgs e)
        {
            var rowhandle = gridView2.FocusedRowHandle;

            try
            {
                if (LoginInfo.MODULE == "FINANCE")
                {
                    var estateid = Convert.ToInt16(gridView2.GetRowCellValue(rowhandle, "ESTATE_ID").ToString());

                    var lokasi = gridView2.GetRowCellValue(rowhandle, "ESTATEID").ToString();
                    DialogResult result = MessageBox.Show("Anda yakin akan menghapus Akses ke ? : " + lokasi + "\n" +
                        "atas nama :" + lbluser.Text + " ", "Hapus Akses", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        UserManager_Services.DeleteAksesEstate(lbluser.Text, estateid);
                        Load_Akses_Estate(lbluser.Text);
                    }
                }
                else if (LoginInfo.MODULE == "ACCOUNTING")
                {
                    var iddata = gridView2.GetRowCellValue(rowhandle, "IDDATA").ToString();
                    DialogResult result = MessageBox.Show("Anda yakin akan menghapus Akses ke ? : " + iddata + "\n" +
                        "atas nama :" + lbluser.Text + " ", "Hapus Akses", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        UserManager_Services.DeleteIDData(lbluser.Text, iddata);
                        Load_Akses_IDDATA(lbluser.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sbbaru_Click(object sender, EventArgs e)
        {
            Bersihkan();
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(lbluser.Text))
            {
                XtraMessageBox.Show("Pilih user terlebih dahulu.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string userId = lbluser.Text;

            if (XtraMessageBox.Show($"Reset password untuk user '{userId}'?", "Konfirmasi",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                string temporaryPassword = UserManager_Services.AdminResetPassword(userId, LoginInfo.MODULE);
                XtraMessageBox.Show(
                    $"Password sementara untuk user '{userId}': {temporaryPassword}\n\nPassword ini wajib diganti saat login berikutnya.",
                    "Sukses",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void btnaddestate_Click(object sender, EventArgs e)
        {
            try
            {
                if (LoginInfo.MODULE == "FINANCE")
                {
                    int p_estateid = (int)lookUpEdit1.EditValue;

                    UserManager_Services.AddUserRole_Estate(lbluser.Text, p_estateid);
                }
                else if (LoginInfo.MODULE == "ACCOUNTING")
                {
                    string iddata = lookUpEdit1.EditValue.ToString();
                    UserManager_Services.AddUserRole_IDDATA(lbluser.Text, iddata);
                }

                LoadUsersAkses();
                ShowToast("✅ Sukses", "Akses " + lookUpEdit1.Text + " berhasil ditambahkan");

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("MASTER_USER_ROLES_EST_PK"))
                {
                    XtraMessageBox.Show("Duplikasi Akses ke Estate : " +
                         lookUpEdit1.Text.ToString() + "", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (ex.Message.Contains("MASTER_USER_ROLES_LOC_PK"))
                {

                    XtraMessageBox.Show("Duplikasi Akses ke IDDATA : " +
                         lookUpEdit1.Text.ToString() + "", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }


        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
           
            if (e.FocusedRowHandle >= 0)
            {
                 var p_userid = gridView1.GetRowCellValue(e.FocusedRowHandle, "USERID").ToString();
                lbluser.Text = p_userid;


            }
            // Load_Akses_Estate(p_userid);
            if (LoginInfo.MODULE == "FINANCE")
            {
                Load_Akses_Estate(lbluser.Text);
            }
            else if (LoginInfo.MODULE == "ACCOUNTING")
            {
                Load_Akses_IDDATA(lbluser.Text);
            }
            LoadUsersAkses();
        }
    }
}
