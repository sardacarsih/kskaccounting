using System;
using System.Data;
using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;
using System.Media;
using Accounting.BusinessLayer;
using Accounting.Models.Login;
using Accounting.Services;
using Accounting.Utilities;
using System.Windows.Forms;
using System.Drawing;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace Accounting.Form
{
    public partial class FrmLokasi : DevExpress.XtraEditors.XtraForm
    {
        private const int BaseDpi = 96;
        private readonly OracleConnection conn = new(ConnectionManager.GetOracleConnection());
        private bool _isActivating;

        public FrmLokasi()
        {
            InitializeComponent();
        }

        private SoundPlayer Player = new();

        private void FrmLokasi_Load(object sender, EventArgs e)
        {
            
            try
            {
                ApplyResponsiveLayout(initialLayout: true);
                conn.Open();
                string SQL = @"SELECT PM.NAMAPT,PM.IDPT,PD.IDDATA,PD.WILAYAH,PD.JENIS_AKUNTANSI
                            FROM MASTER_LOGIN U
                            JOIN MASTER_USER_ROLES_LOC LOC ON LOC.USER_ID=U.USERID
                            JOIN MASTER_USER_ROLES UR ON UR.USER_ID = LOC.USER_ID
                            JOIN MASTER_MODULES M ON M.MODULE_ID=UR.MODULE_ID
                            JOIN MASTER_ROLES MR ON MR.ROLE_ID=UR.ROLE_ID
                            JOIN MASTER_PT_DTL PD ON PD.IDDATA=LOC.IDDATA
                            JOIN MASTER_PT_HDR PM ON PM.IDPT=PD.IDPT
                            WHERE UR.USER_ID = :p_userid AND M.MODULE_NAME=:p_MODULE AND U.AKTIF='Y'";
                OracleCommand cmd = new OracleCommand(SQL, conn)
                {
                    BindByName = true
                };
                cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 25).Value = LoginInfo.userID;
                cmd.Parameters.Add(":p_MODULE", OracleDbType.Varchar2, 25).Value = LoginInfo.MODULE;
                OracleDataReader dr = cmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(dr);
                gridControl1.DataSource = dt;
                ConfigureGridViewLayout();
                gridView1.BestFitColumns();
                ApplyAdaptiveColumnWidths();

                if (dt.Rows.Count == 0)
                {
                    XtraMessageBox.Show(
                        "Tidak ada lokasi pembukuan yang tersedia untuk user dan modul ini.",
                        "Informasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                dr.Close();
                conn.Close();
                this.Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\iddata.wav";
                this.Player.Play();
            }            
                 catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
                       
        }

        private void FrmLokasi_Resize(object sender, EventArgs e)
        {
            ApplyResponsiveLayout(initialLayout: false);
            ApplyAdaptiveColumnWidths();
        }

        private void ApplyResponsiveLayout(bool initialLayout)
        {
            if (label3 == null || gridControl1 == null)
            {
                return;
            }

            if (initialLayout && WindowState == FormWindowState.Normal)
            {
                Rectangle workingArea = Screen.FromControl(this).WorkingArea;
                bool is1366Range = workingArea.Width <= ScaleForDpi(1366);
                bool is1920Range = workingArea.Width <= ScaleForDpi(1920);

                double widthFactor = is1366Range ? 0.76 : (is1920Range ? 0.64 : 0.58);
                double heightFactor = is1366Range ? 0.78 : (is1920Range ? 0.74 : 0.70);

                int targetWidth = Math.Clamp((int)(workingArea.Width * widthFactor), ScaleForDpi(760), workingArea.Width);
                int targetHeight = Math.Clamp((int)(workingArea.Height * heightFactor), ScaleForDpi(520), workingArea.Height);
                Size = new Size(targetWidth, targetHeight);
                StartPosition = FormStartPosition.CenterScreen;
            }

            int headerHeight = Math.Clamp((int)(ClientSize.Height * 0.105), ScaleForDpi(56), ScaleForDpi(88));
            label3.Height = headerHeight;
        }

        private int ScaleForDpi(int value)
        {
            int dpi = DeviceDpi > 0 ? DeviceDpi : BaseDpi;
            return Math.Max(1, (value * dpi) / BaseDpi);
        }
        

        private void gridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                INIT_PT();
            }


        }

        private void ConfigureGridViewLayout()
        {
            gridView1.BeginUpdate();
            try
            {
                gridView1.OptionsView.ShowIndicator = false;
                gridView1.OptionsView.ColumnAutoWidth = false;
                gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
                gridView1.FocusRectStyle = DrawFocusRectStyle.RowFocus;
                gridView1.BestFitMaxRowCount = 100;

                IDPT.MinWidth = ScaleForDpi(72);
                IDDATA.MinWidth = ScaleForDpi(92);
                NAMAPT.MinWidth = ScaleForDpi(250);
                WILAYAH.MinWidth = ScaleForDpi(170);
            }
            finally
            {
                gridView1.EndUpdate();
            }
        }

        private void ApplyAdaptiveColumnWidths()
        {
            if (gridView1 == null || gridView1.Columns.Count == 0)
            {
                return;
            }

            int viewWidth = gridView1.ViewRect.Width;
            if (viewWidth <= 0)
            {
                return;
            }

            int idPtWidth = Math.Clamp(viewWidth / 10, ScaleForDpi(80), ScaleForDpi(120));
            int idDataWidth = Math.Clamp(viewWidth / 8, ScaleForDpi(100), ScaleForDpi(140));
            int wilayahWidth = Math.Clamp((int)(viewWidth * 0.23), ScaleForDpi(180), ScaleForDpi(320));
            int namaWidth = Math.Max(ScaleForDpi(260), viewWidth - idPtWidth - idDataWidth - wilayahWidth - ScaleForDpi(14));

            IDPT.Width = idPtWidth;
            IDDATA.Width = idDataWidth;
            WILAYAH.Width = wilayahWidth;
            NAMAPT.Width = namaWidth;
        }

        private void INIT_PT()
        {
            if (_isActivating)
            {
                return;
            }

            try
            {
                var rowhandle = gridView1.FocusedRowHandle;
                if (!gridView1.IsDataRow(rowhandle))
                {
                    return;
                }

                string idData = GetCellValue(rowhandle, "IDDATA");
                if (string.IsNullOrWhiteSpace(idData))
                {
                    XtraMessageBox.Show("Baris lokasi tidak valid.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _isActivating = true;
                LOGIN_USERS_DTO locationUser = new()
                {
                    USERID = LoginInfo.userID,
                    LEVEL_USER = LoginInfo.role,
                    IDDATA = idData,
                    JENIS_AKUNTANSI = GetCellValue(rowhandle, "JENIS_AKUNTANSI"),
                    NAMAPT = GetCellValue(rowhandle, "NAMAPT"),
                    WILAYAH = GetCellValue(rowhandle, "WILAYAH")
                };

                CompanyInfo.IDPT = GetCellValue(rowhandle, "IDPT");
                AppSession.ApplyLocationContext(locationUser);
                UserManager_Services.RecordLocationSelection(LoginInfo.userID, LoginInfo.MODULE, idData);

                this.Hide();
                new MainView().Show();
                //_ = RunAsync();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isActivating = false;
                //create daftar perkiraan jurnal dan laporan
                //LoadDataAsync();
               
            }

        }

        private string GetCellValue(int rowHandle, string fieldName)
        {
            object value = gridView1.GetRowCellValue(rowHandle, fieldName);
            return value == null || value == DBNull.Value ? string.Empty : value.ToString();
        }

        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            if (gridView1 == null)
            {
                return;
            }

            Point point = gridControl1.PointToClient(MousePosition);
            GridHitInfo hitInfo = gridView1.CalcHitInfo(point);
            if (!hitInfo.InRow && !hitInfo.InRowCell)
            {
                return;
            }

            INIT_PT();
        }

    }
}
