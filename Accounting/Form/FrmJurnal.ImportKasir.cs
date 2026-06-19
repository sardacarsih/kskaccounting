using Accounting.Model;
using DevExpress.Data;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace Accounting.Form
{
    public partial class FrmJurnal
    {
        private const int MinKasirYear = 2022;

        private void Load_Kode_Kasir()
        {
            lookUpEditkasir.Properties.DataSource = GetKasirKode(CompanyInfo.IDDATA);
            lookUpEditkasir.Properties.ValueMember = "ESTATEID";
            lookUpEditkasir.Properties.DisplayMember = "NAMA";
        }


        private DataTable GetKasirKode(string p_iddata)
        {
            return jurnalRepository.GetKasirKode(p_iddata);
        }




        private void cmbbulanimport_SelectedIndexChanged(object sender, EventArgs e)
        {
            Load_Jurnal_Kasir_Header();
            Load_Jurnal_Kasir_Detail();
        }


        private void Load_Jurnal_Kasir_Detail()
        {
            try
            {
                if (lookUpEditkasir.EditValue != null)
                {
                    if (setahunkasir.Value >= MinKasirYear)
                    {
                        int p_glyear = Convert.ToInt32(setahunkasir.Value);
                        int p_glmonth = cmbbulankasir.SelectedIndex + 1;

                        DateTime p_dari = new(p_glyear, p_glmonth, 1);
                        DateTime p_sampai = new(p_glyear, p_glmonth, DateTime.DaysInMonth(p_glyear, p_glmonth));
                        var p_estate = lookUpEditkasir.EditValue.ToString() ?? string.Empty;
                        var p_iddata = CompanyInfo.IDDATA;

                        var p_periode = FormatPeriod(p_glmonth, p_glyear);
                        var p_userid = LoginInfo.userID;

                        dtJurnalKasir = jurnalRepository.JurnalKasirDetail_DapperKasir(
                            p_dari,
                            p_sampai,
                            p_iddata,
                            p_estate,
                            "True",
                            p_periode,
                            p_userid,
                            p_glyear,
                            p_glmonth);
                    }

                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error Load_Jurnal_Kasir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void setahunkasir_EditValueChanged(object sender, EventArgs e)
        {
            Load_Jurnal_Kasir_Header();
            Load_Jurnal_Kasir_Detail();
        }


        private void sbbuatjurnalkasir_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalKasir == null || dtJurnalKasir.Rows.Count == 0)
                    return;

                // Filter KAS/BANK hanya untuk tampilan grid; import selalu memproses seluruh KAS dan BANK.
                pbulan = cmbbulankasir.SelectedIndex + 1;
                ptahun = Convert.ToInt32(setahunkasir.Value);

                var p_periode = FormatPeriod(pbulan, ptahun);
                var Periode = cmbbulankasir.Text + " - " + setahunkasir.Value;
                if (!ValidatePeriodNotLockedWithSound(CompanyInfo.IDDATA, p_periode, Periode))
                    return;

                if (XtraMessageBox.Show("Lanjutkan Proses Import Jurnal KAS dan BANK ? " +
                    "\n\nPeriode : " + Periode + " " +
                    "\nLokasi Data :" + CompanyInfo.IDDATA,
                    "Confirm Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                ExecuteImportJurnal(dtJurnalKasir, pbulan, ptahun, Convert.ToInt32(setahunkasir.Value), "KAS dan BANK");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void Hapus_Data_Table_Tmp()
        {
            try
            {
                jurnalRepository.HapusDataTableTmp();
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void lookUpEditkasir_EditValueChanged(object sender, EventArgs e)
        {
           
            Load_Jurnal_Kasir_Header();
            Load_Jurnal_Kasir_Detail();

        }


        private void Load_Jurnal_Kasir_Header()
        {
            if (lookUpEditkasir.EditValue != null)
            {
                if (setahunkasir.Value >= MinKasirYear)
                {
                    int ptahun = Convert.ToInt32(setahunkasir.Value);
                    int pbulan = cmbbulankasir.SelectedIndex + 1;
                    var p_estate = lookUpEditkasir.EditValue.ToString();

                    var p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("0#"));
                    KasirJurnalHeader = jurnalRepository.GetJurnalHeader_Kasir(p_periode_int, p_estate ?? string.Empty, CompanyInfo.IDDATA);
                    GC_KasirHeader.DataSource = KasirJurnalHeader;
                }
            }
        }


        private void checkEditKAS_CheckedChanged(object sender, EventArgs e)
        {
            ApplyKasirNomorFilter(checkEditBANK, checkEditKAS.Checked, "Contains([NOMOR], '/KK') OR Contains([NOMOR], '/KT')");
        }


        private void checkEditBANK_CheckedChanged(object sender, EventArgs e)
        {
            ApplyKasirNomorFilter(checkEditKAS, checkEditBANK.Checked, "Contains([NOMOR], '/BK') OR Contains([NOMOR], '/BT')");
        }


        private void ApplyKasirNomorFilter(CheckEdit other, bool isChecked, string filterExpression)
        {
            try
            {
                if (dtJurnalKasir == null || dtJurnalKasir.Rows.Count == 0)
                    return;

                GridColumn nomorColumn = gridView_KasirHeader.Columns["NOMOR"];
                if (isChecked)
                {
                    other.Checked = false;
                    gridView_KasirHeader.ActiveFilter.Add(nomorColumn, new ColumnFilterInfo(filterExpression, string.Empty));
                }
                else
                {
                    nomorColumn.ClearFilter();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void gckasir_Detail_Click_1(object sender, EventArgs e)
        {

        }


        private void GC_KasirHeader_Click(object sender, EventArgs e)
        {
            ShowJurnalKasirDetail();
        }


        private void gridView_KasirHeader_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            try
            {
                if (sender is not GridView view)
                {
                    return;
                }

                if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
                {
                    int rowHandle = e.HitInfo.RowHandle;
                    //hapus menu jika ada
                    e.Menu.Items.Clear();

                    DXMenuItem JurnalSelected = CreateMenuItemJurnalKasirSelected(view, rowHandle);
                    DXMenuItem exportselected = CreateMenuExportKasirSelected(view, rowHandle);


                    JurnalSelected.BeginGroup = true;
                    exportselected.BeginGroup = true;

                    e.Menu.Items.Add(JurnalSelected);
                    e.Menu.Items.Add(exportselected);
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Popup Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private DXMenuItem CreateMenuExportKasirSelected(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Export Terpilih", new EventHandler(OnKasirExportClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }


        private void OnKasirExportClick(object? sender, EventArgs e)
        {
            List<string> selectedValues = GetSelectedRowValues(gridView_KasirHeader, "NOMOR");

            if (selectedValues.Any())
            {
                ExportNomorKasirDipilih(selectedValues);
            }
        }


        private void ExportNomorKasirDipilih(List<string> selectedValues)
        {
            try
            {
                List<JurnalDetailDTO> selectedJurnalItems = BuildSelectedJurnalDetails(dtJurnalKasir, selectedValues);
                jurnalExcelExportService.ExportJurnalDetails(selectedJurnalItems, "JurnalKasirTerpilih");
            }
            catch (InvalidOperationException ex)
            {
                XtraMessageBox.Show(ex.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Export Kasir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private DXMenuItem CreateMenuItemJurnalKasirSelected(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Jurnal item Terpilih", new EventHandler(OnKasirJurnalClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[2];
            return checkItem;
        }


        private void OnKasirJurnalClick(object? sender, EventArgs e)
        {
            List<string> selectedValues = GetSelectedRowValues(gridView_KasirHeader, "NOMOR");

            if (selectedValues.Any())
            {
                if (selectedValues.Count == 1)
                {
                    string periode = $"{cmbbulankasir.SelectedIndex + 1:00}/{setahunkasir.Value}";
                    string nomor = Convert.ToString(gridView_KasirHeader.GetRowCellValue(gridView_KasirHeader.FocusedRowHandle, "NOMOR")) ?? string.Empty;
                    DateTime tanggal = Convert.ToDateTime(gridView_KasirHeader.GetRowCellValue(gridView_KasirHeader.FocusedRowHandle, "TANGGAL"));
                    List<JurnalDetailAdd> query = jurnalImportSelectionService.BuildInputDetails(dtJurnalKasir, selectedValues);

                    LoadSelectedImportJurnalToInputTab(nomor, tanggal, periode, query);
                }
                else
                {
                    XtraMessageBox.Show("Pilih hanya satu Nomor untuk menjurnal ini", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


        private void repositoryItemTextEdit_KODE_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

            }
        }



        private void gridView_KasirHeader_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up | e.KeyCode == Keys.Down)
            {
                ShowJurnalKasirDetail();
            }
        }


        private void ShowJurnalKasirDetail()
        {
            try
            {
                var filter = gridView_KasirHeader.GetRowCellValue(gridView_KasirHeader.FocusedRowHandle, "NOMOR").ToString();
                var filtered = dtJurnalKasir.AsEnumerable().Where(row => row.Field<string>("NOJURNAL") == filter).CopyToDataTable();
                gckasir_Detail.DataSource = filtered;
                foreach (var hiddenColumn in new[] { "NOJURNAL", "TANGGAL", "BARIS", "POSTED", "PERIODE", "IDDATA", "USERID", "GLYEAR", "GLMONTH" })
                {
                    GridColumn column = gridView_kasir.Columns[hiddenColumn];
                    if (column != null)
                        column.Visible = false;
                }
                ApplyNumericFormat(gridView_kasir.Columns["DEBET"]);
                ApplyNumericFormat(gridView_kasir.Columns["KREDIT"]);
                ApplyNumericSummary(gridView_kasir.Columns["DEBET"], "DEBET");
                ApplyNumericSummary(gridView_kasir.Columns["KREDIT"], "KREDIT");
                gridView_kasir.BestFitColumns();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on filter Jurnal Kasir", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
