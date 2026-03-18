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

        private void Load_Kode_Kasir()
        {
            lookUpEditkasir.Properties.DataSource = GetKasirKode(CompanyInfo.IDDATA);
            lookUpEditkasir.Properties.ValueMember = "KASIR";
            lookUpEditkasir.Properties.DisplayMember = "KASIR";
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
                    if (setahunkasir.Value >= 2022)
                    {
                        int p_glyear = Convert.ToInt32(setahunkasir.Value);
                        int p_glmonth = cmbbulankasir.SelectedIndex + 1;

                        var lastDayOfMonth = DateTime.DaysInMonth(p_glyear, p_glmonth);
                        DateTime tglakhir = new(p_glyear, p_glmonth, lastDayOfMonth);
                        var akhirbulan = Convert.ToDateTime(tglakhir.ToString("dd-MM-yyyy")).Date;

                        DateTime p_dari = new(p_glyear, p_glmonth, 1);
                        DateTime p_sampai = new(p_glyear, p_glmonth, akhirbulan.Day);
                        var p_estate = lookUpEditkasir.EditValue.ToString();
                        var p_iddata = CompanyInfo.IDDATA;

                        string p_ptlokasi = string.Empty;
                        var row = lookUpEditkasir.GetSelectedDataRow() as DataRowView;
                        if (row != null)
                        {
                            p_ptlokasi = row["PTLOKASI"]?.ToString() ?? string.Empty;
                        }



                        //jika periode telah dikunci,  batalkan proses import jurnal
                        var p_periode = FormatPeriod(p_glmonth, p_glyear);
                        var p_userid = LoginInfo.userID;
 

                        //JurnalFromKasir= jurnalRepository.GetJurnalDetails_DapperKasir(dari, sampai, aliasptlokasi, aliaskodekasir);
                        dtJurnalKasir = jurnalRepository.JurnalKasirDetail_DapperKasir(
                            p_dari,
                            p_sampai,
                            p_iddata,
                            p_estate ?? string.Empty,
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

                if (checkEditKAS.Checked || checkEditBANK.Checked)
                {
                    checkEditKAS.Checked = false;
                    checkEditBANK.Checked = false;
                }

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
                if (setahunkasir.Value >= 2022)
                {
                    int ptahun = Convert.ToInt32(setahunkasir.Value);
                    int pbulan = cmbbulankasir.SelectedIndex + 1;
                    var p_estate = lookUpEditkasir.EditValue.ToString();
                    string p_ptlokasi = string.Empty;
                    var row = lookUpEditkasir.GetSelectedDataRow() as DataRowView;
                    if (row != null)
                    {
                        p_ptlokasi = row["PTLOKASI"]?.ToString() ?? string.Empty;
                    }


                    var p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("0#"));
                    KasirJurnalHeader = jurnalRepository.GetJurnalHeader_Kasir(p_periode_int, p_ptlokasi, p_estate ?? string.Empty);
                    GC_KasirHeader.DataSource = KasirJurnalHeader;
                }
            }
        }


        private void checkEditKAS_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalKasir.Rows.Count > 0)
                {
                    if (checkEditKAS.Checked == true)
                    {

                        checkEditBANK.Checked = false;
                        ColumnView view = gridView_KasirHeader;
                        GridColumn colCategory = view.Columns["NOMOR"];
                        ColumnFilterInfo filter = new("Contains([NOMOR], '/KK') OR Contains([NOMOR], '/KT') ", string.Empty);
                        view.ActiveFilter.Add(colCategory, filter);
                    }
                    else
                    {

                        gridView_KasirHeader.Columns["NOMOR"].ClearFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }


        private void checkEditBANK_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalKasir.Rows.Count > 0)
                {
                    if (checkEditBANK.Checked == true)
                    {

                        checkEditKAS.Checked = false;
                        ColumnView view = gridView_KasirHeader;
                        GridColumn colCategory = view.Columns["NOMOR"];
                        ColumnFilterInfo filter = new("Contains([NOMOR], '/BK') OR Contains([NOMOR], '/BT')", string.Empty);
                        view.ActiveFilter.Add(colCategory, filter);
                    }
                    else
                    {

                        gridView_KasirHeader.Columns["NOMOR"].ClearFilter();
                    }
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
                gridView_kasir.Columns[0].Visible = false;
                gridView_kasir.Columns[1].Visible = false;
                gridView_kasir.Columns[2].Visible = false;
                gridView_kasir.Columns[8].Visible = false;
                gridView_kasir.Columns[9].Visible = false;
                gridView_kasir.Columns[10].Visible = false;
                gridView_kasir.Columns[11].Visible = false;
                gridView_kasir.Columns[12].Visible = false;
                gridView_kasir.Columns[13].Visible = false;
                ApplyNumericFormat(gridView_kasir.Columns[5]);
                ApplyNumericFormat(gridView_kasir.Columns[6]);
                ApplyNumericSummary(gridView_kasir.Columns[5], "DEBET");
                ApplyNumericSummary(gridView_kasir.Columns[6], "KREDIT");
                gridView_kasir.BestFitColumns();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on filter Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
