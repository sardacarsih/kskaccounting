using System;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace Accounting.Form
{
    public partial class FrmCompanyProfile : DevExpress.XtraEditors.XtraForm
    {
       private readonly OracleConnection conn = new(Acct.OracleConnString);
        public FrmCompanyProfile()
        {
            InitializeComponent();
        }
        string JENIS;
        private void FrmCompanyProfile_Load(object sender, EventArgs e)
        {
           
            lblnamapt.Text = CompanyInfo.NAMAPT;
            lblIDDATA.Text = CompanyInfo.INIT;
            txtwilayah.Text = CompanyInfo.WILAYAH;
            if (CompanyInfo.JENIS_AKUNTING == "PUSAT")
            {
                radioGroup1.SelectedIndex = 0;
            }
            else if (CompanyInfo.JENIS_AKUNTING == "PWK")
            {
                radioGroup1.SelectedIndex = 1;
            }
            else if (CompanyInfo.JENIS_AKUNTING == "KEBUN")
            {
                radioGroup1.SelectedIndex = 2;
            }
            else if (CompanyInfo.JENIS_AKUNTING == "PKS")
            {
                radioGroup1.SelectedIndex = 3;
            }
            else
            {
                radioGroup1.SelectedIndex = 4;
            }
        }

        private void sbcetak_Click(object sender, EventArgs e)
        {
            if (radioGroup1.SelectedIndex == 0)
            {
                JENIS = "PUSAT";
            }
            else if (radioGroup1.SelectedIndex == 1)
            {
                JENIS = "PWK";
            }
            else if (radioGroup1.SelectedIndex == 2)
            {
                JENIS = "KEBUN";
            }
            else if (radioGroup1.SelectedIndex == 3)
            {
                JENIS = "PKS";
            }
            else
            {
                JENIS = "LAIN";
            }
            using (OracleCommand _command = new OracleCommand("UPDATE MASTER_PT_DTL SET WILAYAH=:WILAYAH,JENIS_AKUNTANSI=:JENIS WHERE IDDATA = :IDDATA", conn)
            {
                CommandType = CommandType.Text
            })
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                _command.Parameters.Add(":WILAYAH", OracleDbType.Varchar2, 50).Value = txtwilayah.Text;
                _command.Parameters.Add(":JENIS", OracleDbType.Varchar2, 20).Value = JENIS;
                _command.Parameters.Add(":IDDATA", OracleDbType.Varchar2, 20).Value = lblIDDATA.Text;
                _command.ExecuteReader();
                CompanyInfo.JENIS_AKUNTING = JENIS;
                CompanyInfo.WILAYAH=txtwilayah.Text;
                XtraMessageBox.Show("Updated", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
            }
        }
    }
}