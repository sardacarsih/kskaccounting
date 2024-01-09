using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmAksesLevel : DevExpress.XtraEditors.XtraForm
    {
        private readonly OracleConnection conn = new(Acct.OracleConnString);
        public FrmAksesLevel()
        {
            InitializeComponent();
        }
        DataSet DSAkses;
        OracleDataAdapter sqlAdapter;
        int levelid;
        private void FrmAksesLevel_Load(object sender, EventArgs e)
        {
            try
            {
                Load_Level();
                if (lookUpEditLevel.EditValue != null)
                {
                  LoadList_AksesLevel();
                }
                else
                {
                    MessageBox.Show("No user levels found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadList_AksesLevel()
        {
            try
            {
                if (lookUpEditLevel.EditValue != null)
                {
                    // Extract the actual value from DataRowView
                    object selectedValue = lookUpEditLevel.EditValue;

                    // Check if the value is of type DataRowView
                    if (selectedValue is DataRowView)
                    {
                        // Extract the underlying value from DataRowView
                        selectedValue = ((DataRowView)selectedValue)[lookUpEditLevel.Properties.ValueMember];
                    }

                    // Now, convert the selectedValue to int
                    levelid = Convert.ToInt32(selectedValue);

                    AksesLevel(levelid);
                    gridControl1.DataSource = DSAkses;
                    gridControl1.DataMember = "Akses";
                    gridView1.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading access levels: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void Load_Level()
        {
            try
            {
                String selectQuery = "SELECT LevelID ID, Nama FROM master_login_level ORDER BY LevelID ASC";
                using OracleCommand _command = new(selectQuery, conn);
                _command.CommandType = CommandType.Text;
                conn.Open();
                using OracleDataReader dr = _command.ExecuteReader();
                DataTable _dt = new();
                _dt.Load(dr);
                lookUpEditLevel.Properties.DataSource = _dt;
                lookUpEditLevel.Properties.ValueMember = "ID";
                lookUpEditLevel.Properties.DisplayMember = "NAMA";

                // Set a default value using a DataRow from the DataTable
                if (_dt.Rows.Count > 0)
                {
                    DataRow defaultRow = _dt.Rows[0]; // Set the default value to the first row in the DataTable
                    lookUpEditLevel.EditValue = defaultRow["ID"];
                    lookUpEditLevel.Text = defaultRow["NAMA"].ToString();// Replace "YourColumnName" with the actual column name you want to use
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user levels: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        public DataSet AksesLevel(int levelid)
        {
            try
            {
                String selectQuery = "SELECT aksesid, AKSIID, KETERANGAN, buka, baru, simpan, ubah, CETAK, HAPUS " +
                                     "FROM master_akses " +
                                     "WHERE appsid = 'GL' AND levelid = :levelid " +
                                     "ORDER BY AKSESID ASC";

                using (OracleCommand _command = new OracleCommand(selectQuery, conn))
                {
                    _command.CommandType = CommandType.Text;
                    _command.Parameters.Add(":levelid", OracleDbType.Int32).Value = levelid;

                    sqlAdapter = new OracleDataAdapter(_command);

                    // Set the UpdateCommand for the OracleDataAdapter
                    OracleCommandBuilder builder = new (sqlAdapter);

                    DSAkses = new DataSet();
                    sqlAdapter.Fill(DSAkses, "Akses");

                    // Optional: If the table has a primary key, set it in the DataTable
                    // DSAkses.Tables["Akses"].PrimaryKey = new DataColumn[] { DSAkses.Tables["Akses"].Columns["aksesid"] };
                }

                return DSAkses;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading access levels: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }




        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void gridView1_RowUpdated(object sender, RowObjectEventArgs e)
        {
            try
            {
                // Check if gridView1 and other necessary objects are not null before using them
                if (gridControl1 != null && gridControl1.FocusedView is ColumnView view && DSAkses != null)
                {
                    view.CloseEditor();

                    if (view.UpdateCurrentRow())
                    {
                        DSAkses.Tables["Akses"].PrimaryKey = new DataColumn[] { DSAkses.Tables["Akses"].Columns["AKSESID"] };

                        // Remove the using block for OracleCommand
                        // using (OracleCommand cmd = new OracleCommand())
                        OracleCommand cmd = new OracleCommand();

                        try
                        {
                            // Set the connection for the command
                            cmd.Connection = conn;

                            // Set the update command for the data adapter
                            sqlAdapter.UpdateCommand = cmd;

                            // Update the data adapter
                            sqlAdapter.Update(DSAkses, "Akses");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error updating database: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            // Dispose the command here to ensure it's cleaned up
                            cmd.Dispose();
                        }
                    }
                }
                else
                {
                    // Handle the case where objects are null
                    MessageBox.Show("Objects are null. Unable to perform the operation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in gridView1_RowUpdated: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void lookUpEditLevel_EditValueChanged(object sender, EventArgs e)
        {
            LoadList_AksesLevel();
        }

        private void btnduplikatnewrole_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TXTNEWROLE.Text) || string.IsNullOrEmpty(lookUpEditLevel.Text) ||lookUpEditLevel.EditValue==null)
                {
                    XtraMessageBox.Show("Tentukan Nama Role Level sumber dan nama baru yang akan dibuat", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    TXTNEWROLE.Focus();
                    return;
                }
                using (OracleCommand cmd = new ("ACCT_TOOLS.DuplikatRole", conn)
                {
                    CommandType = CommandType.StoredProcedure
                })
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    cmd.Parameters.Add(":rolename", OracleDbType.Varchar2, 30).Value = TXTNEWROLE.Text;
                    cmd.Parameters.Add(":darirole", OracleDbType.Int16).Value = lookUpEditLevel.EditValue;
                    cmd.Parameters.Add(":apps", OracleDbType.Varchar2, 10).Value = "GL";
                    cmd.ExecuteReader();
                    conn.Close();
                }
                Load_Level();
                XtraMessageBox.Show("Duplikasi Role Baru Telah dibuat", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    XtraMessageBox.Show("Nama Role Level Sudah ada, gunakan yang lain", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
        }

        private void btnhapus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lookUpEditLevel.Text) || lookUpEditLevel.EditValue == null)
            {
                XtraMessageBox.Show("Tentukan Nama Role Level yang akan dihapus", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lookUpEditLevel.Focus();
                return;
            }
            int value = Convert.ToInt32(lookUpEditLevel.EditValue);
            if (value < 5)
            {
                XtraMessageBox.Show("Roles Master Tidak dapat dihapus", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                // string deletecmd = "delete from ACCT_PERIODE WHERE IDDATA=:p_IDDATA AND PERIODE=:p_periode";
                using (OracleCommand cmd = new OracleCommand("delete from master_login_level where levelid=:lvl", conn)
                {
                    CommandType = CommandType.Text
                })
                {
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }                    
                    cmd.Parameters.Add(":lvl", OracleDbType.Int16).Value = Convert.ToInt32(lookUpEditLevel.EditValue);
                    cmd.ExecuteReader();
                    conn.Close();
                }
                Load_Level();
                gridControl1.DataSource = null;
                XtraMessageBox.Show("Role Telah dihapus", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {                
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
             lookUpEditLevel.EditValue = lookUpEditLevel.Properties.GetDataSourceValue(lookUpEditLevel.Properties.KeyMember, 1);
        }
    }
    
}