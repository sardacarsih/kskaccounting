using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Accounting.BusinessLayer;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.LookAndFeel;
using Oracle.ManagedDataAccess.Client;
using DevExpress.CodeParser;

namespace Accounting.Form
{
    public partial class FrmSetAkunAgronomy : XtraForm
    {
        bool editkerja;

        readonly OracleConnection conn = new(Acct.OracleConnString);
        public FrmSetAkunAgronomy()
        {
            InitializeComponent();
           
        }


        int P_KERJAID, kodekerja;
        private void SBUpdate_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(searchLookUpEditkodegl.Text))
            {
                XtraMessageBox.Show("Pilih Kode GL.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string borongan,display_blok,P_AC1,P_AC2,P_AC3,P_AC4, P_KODE_AKUN;
            if(RGBORONGAN.SelectedIndex== 0)
            {
                borongan = "Y";

            }
            else
            {
                borongan = "T";
            }
            if (RGDISPLAYBLOK.SelectedIndex == 0)
            {
                display_blok = "Y";

            }
            else
            {
                display_blok = "T";
            }
            var P_KELOMPOK = LEKELOMPOK.Text;
            var P_BORONGAN = borongan;
            var P_KERJA = txtpekerjaan.Text;
            var P_BLOKDISPLAY = display_blok;
            var p_kode = searchLookUpEditkodegl.EditValue.ToString();
            var P_REKENING = TXTREKENING.Text;
            if (LEKELOMPOK.Text == "PANEN")
            {
                P_AC1 = "80";
                P_AC2 = "XX";
                P_AC3 = "XXX";
            }
            else if(LEKELOMPOK.Text == "RAWAT")
            {
                P_AC1 = "XX";
                P_AC2 = "XX";
                P_AC3 = "XXX";
            }
            else if (LEKELOMPOK.Text == "UMUM")
            {
                P_AC1 = "40";
                P_AC2 = p_kode.Substring(3,3);
                P_AC3 = "XX";
            }
            else if (LEKELOMPOK.Text == "PEMBIBITAN")
            {
                P_AC1 = p_kode.Substring(0, 2);
                P_AC2 = p_kode.Substring(3, 2);
                P_AC3 = p_kode.Substring(5, 3);
            }
            else
            {
                P_AC1 = "XX";
                P_AC2 = "XX";
                P_AC3 = "XXX";
            }
            
            P_AC4= p_kode.Substring(p_kode.Length-3);

            P_KODE_AKUN = P_AC1 + "." + P_AC2 + P_AC3 + "." + P_AC4;
            string query = "UPDATE AIS_BKM_KERJA@DATABASE_LINK " +
                "SET KELOMPOK=:P_KELOMPOK" +
                ",BORONGAN=:P_BORONGAN" +
                ",KERJA=:P_KERJA" +
                ",BLOK_DISPLAY=:P_BLOKDISPLAY" +
                ",AC1=:P_AC1" +
                ",AC2=:P_AC2" +
                ",AC3=:P_AC3" +
                ",AC4=:P_AC4" +
                ",ACKODE=:P_KODE_AKUN" +
                ",NAMA=:P_REKENING " +
                "WHERE KERJAID=:P_KERJAID ";

            OracleCommand _command = new(query, conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":P_KELOMPOK", OracleDbType.Varchar2, 20).Value = P_KELOMPOK;
            _command.Parameters.Add(":P_BORONGAN", OracleDbType.Varchar2, 1).Value = P_BORONGAN;
            _command.Parameters.Add(":P_KERJA", OracleDbType.Varchar2, 50).Value = P_KERJA;
            _command.Parameters.Add(":P_BLOKDISPLAY", OracleDbType.Varchar2, 1).Value = P_BLOKDISPLAY;
            _command.Parameters.Add(":P_AC1", OracleDbType.Varchar2, 2).Value = P_AC1;
            _command.Parameters.Add(":P_AC2", OracleDbType.Varchar2, 3).Value = P_AC2;
            _command.Parameters.Add(":P_AC3", OracleDbType.Varchar2, 3).Value = P_AC3;
            _command.Parameters.Add(":P_AC4", OracleDbType.Varchar2, 3).Value = P_AC4;
            _command.Parameters.Add(":P_KODE_AKUN", OracleDbType.Varchar2, 20).Value = P_KODE_AKUN;
            _command.Parameters.Add(":P_REKENING", OracleDbType.Varchar2, 50).Value = P_REKENING;
            _command.Parameters.Add(":P_KERJAID", OracleDbType.Int32).Value = P_KERJAID;
         _command.ExecuteNonQuery();
            Alokasi_Pekerjaan_ke_KodeAkun();
            Bersihkan();
            XtraMessageBox.Show("Data Telah Disimpan.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Bersihkan()
        {
            SBUpdate.Enabled = false;
            searchLookUpEditkodegl.EditValue = null;
            P_KERJAID = 0;
            txtpekerjaan.Text = "";
            TXTREKENING.Text = "";
            txtkode.Text = "";

        }

        private void TXTKASIR_EditValueChanged(object sender, EventArgs e)
        {
            SBUpdate.Enabled = true;
        }

        private void TXTADMIN_EditValueChanged(object sender, EventArgs e)
        {
            SBUpdate.Enabled = true;
        }

        private void TXTKABAG_EditValueChanged(object sender, EventArgs e)
        {
            SBUpdate.Enabled = true;
        }

        private void TXTMANAGER_EditValueChanged(object sender, EventArgs e)
        {
            SBUpdate.Enabled = true;
        }

        private void FrmSetAkunAgronomy_Load(object sender, EventArgs e)
        {
            NAMAPT.Text = CompanyInfo.NAMAPT;
            IDDATA.Text = CompanyInfo.INIT;
            WILAYAH.Text = CompanyInfo.WILAYAH;

            //load jurnal from other
            string serverip = Acct.OracleConnString.Substring(54, 11);
            if (serverip != "10.10.10.41")
            {
                Alokasi_Pekerjaan_ke_KodeAkun();
                Load_kategori();
                
            }
            else
            {
                SBUpdate.Enabled = false;
                XtraMessageBox.Show("Untuk Sementara hanya berfungsi di Site", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
           

        }

        private void Load_kategori()
        {
            Dictionary<int, string> Kategori = new()
            {
                { 1, "PANEN" },
                { 2, "RAWAT" },
                { 3, "UMUM" },
                { 4, "PEMBIBITAN" },
            };
            LEKELOMPOK.Properties.DataSource = Kategori;
            LEKELOMPOK.ItemIndex = 0;
        }

        private void Alokasi_Pekerjaan_ke_KodeAkun()
        {
            string p_kategori = string.Empty;
                string p_borongan = string.Empty;
            p_kategori = LEKELOMPOK.Text;
            if (RGBORONGAN.SelectedIndex == 0)
            {
                p_borongan = "Y";
            }
            else
            {
                p_borongan = "T";
            }

            string query = "SELECT KERJAID,BORONGAN,KODE,KERJA,KELOMPOK,BLOK_DISPLAY,AC1,AC2,AC3,AC4,ACKODE KODE_AKUN,NAMA REKENING from AIS_BKM_KERJA@DATABASE_LINK WHERE KELOMPOK=:p_kategori AND BORONGAN=:p_borongan ";

            OracleCommand _command = new (query, conn)
            {
                CommandType = CommandType.Text
            };
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            _command.Parameters.Add(":p_kategori", OracleDbType.Varchar2, 20).Value = p_kategori;
            _command.Parameters.Add(":p_borongan", OracleDbType.Varchar2, 1).Value = p_borongan;
            OracleDataReader dr;
            dr = _command.ExecuteReader();
            DataTable _dt = new ();
            _dt.Load(dr);
            dr.Close();
            conn.Close();
            gridControl1.DataSource = _dt;
            gridView1.Columns[0].Visible = false;
            gridView1.Columns[1].Visible = false;
            gridView1.Columns[4].Visible = false;
            gridView1.BestFitColumns();
        }

        

        private void FrmSetAkunAgronomy_FormClosed(object sender, FormClosedEventArgs e)
        {
          
        }

        private void LEKELOMPOK_EditValueChanged(object sender, EventArgs e)
        {
            if(LEKELOMPOK.Text=="PANEN" || LEKELOMPOK.Text == "RAWAT")
            {
                RGDISPLAYBLOK.SelectedIndex= 0;
                RGDISPLAYBLOK.ReadOnly = true;
            }
            else
            {
                RGDISPLAYBLOK.SelectedIndex = 1;
                RGDISPLAYBLOK.ReadOnly = true;

            }
            Alokasi_Pekerjaan_ke_KodeAkun();
            Load_daftar_kode_akun();
        }

        private void Load_daftar_kode_akun()
        {
            var kelompok = LEKELOMPOK.Text;
            var kode = AccountServices.Akun_Agronomy(CompanyInfo.INIT, Acct.TahunMax, kelompok);
            searchLookUpEditkodegl.Properties.DataSource = kode;
            searchLookUpEditkodegl.Properties.ValueMember = "KODE";
            searchLookUpEditkodegl.Properties.DisplayMember= "KODE";

        }

        private void RGBORONGAN_SelectedIndexChanged(object sender, EventArgs e)
        {
            Alokasi_Pekerjaan_ke_KodeAkun();
        }

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            DXMouseEventArgs? ea = e as DXMouseEventArgs;
            GridView? view = sender as GridView;
            GridHitInfo info = view.CalcHitInfo(ea.Location);
            if (info.HitTest == GridHitTest.RowIndicator)
            {
                MessageBox.Show(string.Format("DoubleClick on row indicator, row #{0}", info.RowHandle));
            }
            if (this.gridView1.GetFocusedRowCellValue("KERJAID") == null)
                return;
            var rowhandle = gridView1.FocusedRowHandle;
            P_KERJAID = Convert.ToInt32(gridView1.GetRowCellValue(rowhandle, "KERJAID").ToString());
            txtkode.Text = gridView1.GetRowCellValue(rowhandle, "KODE").ToString();
            txtpekerjaan.Text = gridView1.GetRowCellValue(rowhandle, "KERJA").ToString();
           var BOR = gridView1.GetRowCellValue(rowhandle, "BORONGAN").ToString();
            if (BOR == "Y")
            {
                RGBORONGAN.SelectedIndex = 0;
            }
            else
            {
                RGBORONGAN.SelectedIndex = 1;
            }
            var disp = gridView1.GetRowCellValue(rowhandle, "BLOK_DISPLAY").ToString();
            if (disp == "Y")
            {
                RGDISPLAYBLOK.SelectedIndex = 0;
            }
            else
            {
                RGDISPLAYBLOK.SelectedIndex = 1;
            }
            searchLookUpEditkodegl.EditValue= gridView1.GetRowCellValue(rowhandle, "KODE_AKUN").ToString();
            TXTREKENING.Text = gridView1.GetRowCellValue(rowhandle, "REKENING").ToString();
            txtkode.ReadOnly = true;
            SBUpdate.Enabled = true;
        }

        private void searchLookUpEditkodegl_EditValueChanged(object sender, EventArgs e)
        {
                try
                {
                    SearchLookUpEdit editor = ((SearchLookUpEdit)sender);
                    DataRowView row = (DataRowView)editor.Properties.GetRowByKeyValue(editor.EditValue);
                    if (row != null)
                    {
                        TXTREKENING.Text = row["PERKIRAAN"].ToString().Trim();
                        editkerja = false;
                    }
                   
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        private void FrmSetAkunAgronomy_MouseDown(object sender, MouseEventArgs e)
        {
            Control parentControl = this;
            Point pt;
            pt = e.Location;
            DXPopupMenu dxPopupMenu = new DXPopupMenu();
            DXButtonGroupItem buttonGroup = new DXButtonGroupItem();
            EventHandler handler = new EventHandler(ItemClick);
            buttonGroup.Items.Add(new DXMenuItem("Undo", handler, null));
            buttonGroup.Items.Add(new DXMenuItem("Cut", handler, null));
            buttonGroup.Items.Add(new DXMenuItem("Redo", handler, null));
            dxPopupMenu.Items.Add(buttonGroup);
            buttonGroup = new DXButtonGroupItem();
            buttonGroup.Items.Add(new DXMenuItem("Bold", handler, null));
            buttonGroup.Items.Add(new DXMenuItem("Italic", handler, null));
            buttonGroup.Items.Add(new DXMenuItem("Underline", handler, null));
            dxPopupMenu.Items.Add(buttonGroup);
            dxPopupMenu.Items.Add(new DXMenuItem("About", handler));
            dxPopupMenu.Items.Add(new DXMenuItem("Exit", handler));
            dxPopupMenu.MenuViewType = MenuViewType.RibbonMiniToolbar;
        }

        private void ItemClick(object? sender, EventArgs e)
        {
            DXMenuItem item = sender as DXMenuItem;
            //You can identify an item by its caption
            //...
        }
    }
}