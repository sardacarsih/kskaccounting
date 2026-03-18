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

        private void Load_PeriodeList(string piddata, string ptahun)
        {
            try
            {
                var periodelist = jurnalRepository.PeriodeList(piddata, ptahun);

                if (periodelist != null && periodelist.Rows.Count > 0)
                {
                    leperiode.Properties.DataSource = periodelist;
                    leperiode.Properties.ValueMember = "PERIODE";
                    leperiode.Properties.DisplayMember = "PERIODE";
                }
                else
                {
                    leperiode.Properties.DataSource = null;
                    XtraMessageBox.Show("No periods available for the selected data and year.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"An error occurred while loading the period list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        int x = 0;
        private void JDgridView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            try
            {
                var n = x++;
                var ket = JDgridView.GetRowCellValue(n, JDgridView.Columns[5]);

                if (sender is not GridView view)
                {
                    return;
                }

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
            try
            {
                JurnalSaveRequest request = new()
                {
                    IdData = CompanyInfo.IDDATA,
                    Nomor = NoJurnaltxt.Text,
                    Tanggal = deJurnal.EditValue as DateTime?,
                    Periode = leperiode.Text,
                    PeriodeLockCheck = leallperiode.Text,
                    IsJurnalBalik = jurnalbalik.Checked,
                    IsEdit = editjurnal,
                    OldJurnalId = old_JurnalID,
                    FromModuleAis = importModule == ImportModule.AIS,
                    FromModuleKasir = importModule == ImportModule.Kasir,
                    FromModuleInv = importModule == ImportModule.Inventory,
                    DebetTotal = GetGridSummaryTotal("Debet"),
                    KreditTotal = GetGridSummaryTotal("Kredit"),
                    Details = InputJurnalDetail.ToList(),
                    UserId = LoginInfo.userID
                };

                JurnalSaveResult saveResult = jurnalInputOperationService.Save(request);
                if (!saveResult.IsSuccess)
                {
                    XtraMessageBox.Show(saveResult.Message, "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    HandleSaveFocus(saveResult.FocusTarget);
                    return;
                }

                editjurnal = false;
                importModule = ImportModule.None;

                PilihanPeriodeAkuntansi();
                bersih();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Simpan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal GetGridSummaryTotal(string columnName)
        {
            object summaryValue = JDgridView.Columns[columnName].SummaryItem.SummaryValue;
            return decimal.TryParse(summaryValue?.ToString(), out decimal parsedValue) ? parsedValue : 0m;
        }

        private void HandleSaveFocus(JurnalInputFocusTarget focusTarget)
        {
            switch (focusTarget)
            {
                case JurnalInputFocusTarget.Nomor:
                    NoJurnaltxt.Focus();
                    break;
                case JurnalInputFocusTarget.Tanggal:
                    deJurnal.Focus();
                    break;
                case JurnalInputFocusTarget.Periode:
                    leperiode.Focus();
                    break;
                case JurnalInputFocusTarget.GridDetail:
                    JDgridView.Focus();
                    break;
            }
        }

        ImportModule importModule = ImportModule.None;



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
            else if (e.KeyCode == Keys.F8)
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

            if (TABJurnal.SelectedTabPage == xtraTabPage2)
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
            PopulateList();
            NoJurnaltxt.Text = "";

            editjurnal = false;
            x = 0;
            SBSimpan.Enabled = true;
            jurnalbalik.Checked = false;
            NoJurnaltxt.Select();
        }

    }
}
