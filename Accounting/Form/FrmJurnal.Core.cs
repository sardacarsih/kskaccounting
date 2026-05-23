using Accounting._1.Interface;
using Accounting._2.DataAcces;
using Accounting.BusinessLayer;
using Accounting.Model;
using Accounting.Services;
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
        private void ExportJurnal_FromList(IEnumerable<AIS_JURNAL_FINAL> jurnal)
        {
            try
            {
                jurnalExcelExportService.ExportAisJurnal(jurnal, "JurnalEntries");
            }
            catch (InvalidOperationException ex)
            {
                XtraMessageBox.Show(ex.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtkodeperkiran_Leave(object? sender, EventArgs e)
        {
            if (suppressKodeLeaveValidation || isSaveOrUpdateInProgress || TABJurnal.SelectedTabPage != xtraTabPage1)
            {
                return;
            }

            if (sender is not DevExpress.XtraEditors.TextEdit textEditor)
            {
                return;
            }

            string kode = textEditor.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(kode))
            {
                return;
            }

            int rowHandle = JDgridView.FocusedRowHandle;
            if (rowHandle < 0)
            {
                return;
            }

            bool isResolved = TryResolveKodeStyle1(JDgridView, rowHandle, kode, openLookupIfInvalid: true);
            BeginInvoke(new MethodInvoker(() =>
            {
                if (isSaveOrUpdateInProgress || TABJurnal.SelectedTabPage != xtraTabPage1)
                {
                    return;
                }

                JDgridView.FocusedColumn = isResolved ? JDgridView.Columns["Debet"] : JDgridView.Columns["Kode"];
                JDgridView.ShowEditor();
            }));
        }

        private void ExportJurnal_Periode()
        {
            List<JurnalDetailDTO> exportRows = GetActiveDetailRowsForExport();
            if (!exportRows.Any())
            {
                exportRows = NormalizeJurnalExportOrder(JurnalDetail ?? Enumerable.Empty<JurnalDetailDTO>());
            }

            jurnalExcelExportService.ExportJurnalDetails(exportRows, "JurnalPeriode");
        }


        private void deJurnal_Enter(object sender, EventArgs e)
        {
            if (sender is not DateEdit edit)
            {
                return;
            }

            BeginInvoke(new MethodInvoker(() =>
            {
                edit.SelectionStart = 0;

                edit.SelectionLength = 2;
            }));
        }

        private void GVDetail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (sender is not GridView view)
                {
                    return;
                }

                var valueText = view.GetFocusedValue()?.ToString();
                if (!string.IsNullOrEmpty(valueText))
                {
                    Clipboard.SetText(valueText);
                }
            }
        }


        private void JDgridView_ClipboardRowPasting(object sender, ClipboardRowPastingEventArgs e)
        {
            if (sender is not DevExpress.XtraGrid.Views.Grid.GridView view)
            {
                return;
            }

            var cells = view.GetSelectedCells() as DevExpress.XtraGrid.Views.Base.GridCell[];
            if (cells == null)
            {
                return;
            }

            if (cells.Length <= 1 || e.Values.Count > 1 || System.Windows.Forms.Clipboard.GetText().Contains(System.Environment.NewLine))
                return;

            e.Cancel = true;
            for (int i = 0; i < cells.Length; i++)
            {
                if (!cells[i].Column.ReadOnly)
                    view.SetRowCellValue(cells[i].RowHandle, cells[i].Column, e.OriginalValues[0]);
            }
        }


        private void deJurnal_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {


                if (e.KeyCode == Keys.Enter)
                {
                    if (string.IsNullOrEmpty(deJurnal.Text)) return;

                    UpdateGridEnabledState();

                    if (!GCJurnal.Enabled)
                    {
                        XtraMessageBox.Show("Daftar Perkiraan Akun belum ada...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (editjurnal == false)
                    {
                        if (!NoJurnaltxt.Text.Contains("/ND") && !NoJurnaltxt.Text.Contains("/NK"))
                        {
                            bool nomorexist = jurnalRepository.CekNoJurnalExist_input(CompanyInfo.IDDATA, NoJurnaltxt.Text.ToUpper(), leperiode.Text);
                            if (nomorexist)
                            {
                                XtraMessageBox.Show("Nomor Jurnal : " + NoJurnaltxt.Text + " Sudah ada...!!!", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                NoJurnaltxt.Select();
                                return;
                            }
                        }
                    }

                    SendKeys.Send("{TAB}");

                    // Focus a specific cell.
                    JDgridView.FocusedRowHandle = 0;
                    JDgridView.FocusedColumn = JDgridView.Columns["Kode"];
                }
            }
            catch (SystemException ex)
            {
                if (ex.Message.Contains("valid DateTime"))
                {
                    XtraMessageBox.Show("Tanggal Salah", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void deJurnal_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (deJurnal.EditValue != null)
                {
                    string[] bulanbi = { "Bulan", "Januari", "Pebruari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" };

                    pbulan = DateTime.Parse(deJurnal.Text).Month;
                    ptahun = DateTime.Parse(deJurnal.Text).Year;
                    Load_PeriodeList(CompanyInfo.IDDATA, ptahun.ToString());

                    periodetujuan = FormatPeriod(pbulan, ptahun);
                    leperiode.EditValue = periodetujuan;


                    UpdateGridEnabledState();

                    var Periode = bulanbi[pbulan].ToString() + " - " + ptahun.ToString();
                    //cek apakah periode yg dipilih ada, jika ada tombol proses aktif, jika tidak tombol proses disable
                    var ada = jurnalRepository.CekPeriodeExist(CompanyInfo.IDDATA, pbulan, ptahun);
                    if (ada == 0)
                    {
                        XtraMessageBox.Show("Periode Akuntansi: " + Periode + "\nPilihan Periode belum tersedia...!!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        SBSimpan.Enabled = false;
                    }
                    else
                    {
                        SBSimpan.Enabled = CanSaveCurrentJurnal();
                    }

                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void JDgridView_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            if (e.Item is not GridSummaryItem summaryItem)
            {
                return;
            }

            // Get the summary ID.
            int summaryID = Convert.ToInt32(summaryItem.Tag);

            // Initialization. 
            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {

                selisihD = 0;
                selisihK = 0;
            }
            // Calculation.
            if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            {


                switch (summaryID)
                {
                    case 1: // The total summary calculated against the 'UnitPrice' column. 
                        var Debet = Convert.ToDouble(JDgridView.Columns["Debet"].SummaryItem.SummaryValue.ToString());
                        var Kredit = Convert.ToDouble(JDgridView.Columns["Kredit"].SummaryItem.SummaryValue.ToString());
                        if (Debet > Kredit)
                        {
                            nilai = 0;
                            nilai2 = Debet - Kredit;
                        }
                        else
                        {
                            nilai = Kredit - Debet;
                            nilai2 = 0;
                        }
                        selisihD = nilai;
                        break;
                    case 2:
                        selisihK = nilai2;
                        break;
                }
            }
            // Finalization. 
            if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            {
                switch (summaryID)
                {
                    case 1:
                        e.TotalValue = selisihD;
                        break;
                    case 2:
                        e.TotalValue = selisihK;
                        break;
                }
            }
        }



        private void cmbbulan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }



        private void ribhapus_Click(object sender, EventArgs e)
        {
            if (!TryEnsureJurnalAccess(editjurnal
                    ? AuthorizationService.EnsureCanUpdateJurnal
                    : AuthorizationService.EnsureCanCreateJurnal))
            {
                return;
            }

            JDgridView.DeleteRow(JDgridView.FocusedRowHandle);
            XtraMessageBox.Show("Deleted");
        }
        private DataTable PeriodeListAll(string piddata)
        {
            return jurnalRepository.PeriodeListAll(piddata);
        }


        private void SetColumnEdit(RepositoryItem? item)
        {
            if (item == null)
            {
                return;
            }

            var kodeColumn = JDgridView.Columns["Kode"];
            if (kodeColumn != null)
            {
                kodeColumn.ColumnEdit = item;
            }
            else
            {
                XtraMessageBox.Show("Kode column not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
