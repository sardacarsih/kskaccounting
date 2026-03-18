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

        private void lookUpEditINV_EditValueChanged(object sender, EventArgs e)
        {
            Load_inv_header();
            Load_Jurnal_INV();
        }


        private void Load_Kode_Inv()
        {
            lookUpEditINV.Properties.DataSource = GetINVKode(CompanyInfo.IDDATA);
            lookUpEditINV.Properties.ValueMember = "PTLOKASI";
            lookUpEditINV.Properties.DisplayMember = "INV";
        }


        private object GetINVKode(string p_iddata)
        {
            return jurnalRepository.GetINVKode(p_iddata);
        }


        private void Load_inv_header()
        {
            if (lookUpEditINV.EditValue != null && SETAHUNINV.Value >= 2022)
            {
                int ptahun = Convert.ToInt32(SETAHUNINV.Value);
                int pbulan = CMBBULANINV.SelectedIndex + 1;
                var p_ptlokasi = lookUpEditINV.EditValue.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(p_ptlokasi))
                {
                    return;
                }

                var p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("0#"));
                InventoryJurnalHeader = jurnalRepository.GetJurnalHeader_Inventory(p_periode_int, p_ptlokasi);
                gc_inv_header.DataSource = InventoryJurnalHeader;
            }

        }


        private void Load_Jurnal_INV()
        {
            if (lookUpEditINV.EditValue != null)
            {
                if (SETAHUNINV.Value >= 2022)
                {
                    int ptahun = Convert.ToInt32(SETAHUNINV.Value);
                    int pbulan = CMBBULANINV.SelectedIndex + 1;


                    var p_ptlokasi = lookUpEditINV.EditValue.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(p_ptlokasi))
                    {
                        return;
                    }

                    var p_iddata = CompanyInfo.IDDATA;

                    //jika periode telah dikunci,  batalkan proses import jurnal
                    var p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("0#"));
                    var p_periode_str = FormatPeriod(pbulan, ptahun);

                    var TOTALNILAI = jurnalRepository.CEK_TOTAL_TRANSAKSI(p_periode_int, p_ptlokasi, "INVENTORY");

                    dtJurnalInventory = jurnalRepository.Jurnal_Inventori(p_periode_int, p_ptlokasi, p_iddata, "True", p_periode_str, LoginInfo.userID, ptahun, pbulan);
                    LBLTOTALTRANSAKSI.Text = string.Format("{0:#,##}", TOTALNILAI);

                }

            }
        }


        private void CMBBULANINV_SelectedIndexChanged(object sender, EventArgs e)
        {
            Load_inv_header();
            Load_Jurnal_INV();
        }


        private void SBJURNALINV_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalInventory == null || dtJurnalInventory.Rows.Count == 0)
                    return;

                if (checkEditlk.Checked || checkEditlt.Checked)
                {
                    checkEditlk.Checked = false;
                    checkEditlt.Checked = false;
                }

                pbulan = CMBBULANINV.SelectedIndex + 1;
                ptahun = Convert.ToInt32(SETAHUNINV.Value);

                var p_periode = FormatPeriod(pbulan, ptahun);
                var Periode = CMBBULANINV.Text + " - " + SETAHUNINV.Value;
                if (!ValidatePeriodNotLockedWithSound(CompanyInfo.IDDATA, p_periode, Periode))
                    return;

                if (XtraMessageBox.Show("Lanjutkan Proses Import Jurnal LT dan LK ? " +
                    "\n\nPeriode : " + Periode + " " +
                    "\nLokasi Data :" + CompanyInfo.IDDATA,
                    "Confirm Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                ExecuteImportJurnal(dtJurnalInventory, pbulan, ptahun, ptahun, "dari Inventori");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void sbexportinv_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalInventory == null || dtJurnalInventory.Rows.Count == 0)
                {
                    return;
                }
                if (dtJurnalInventory.Rows.Count > 0)
                {
                    DataTable dtnew = new();
                    if (checkEditlt.Checked == false && checkEditlk.Checked == false)
                    {
                        dtnew = dtJurnalInventory;
                    }
                    else if (checkEditlt.Checked == true)
                    {
                        var filtered = dtJurnalInventory.AsEnumerable()
                            .Where(y =>
                            {
                                var noJurnal = y.Field<string>("NOJURNAL");
                                return !string.IsNullOrEmpty(noJurnal) && noJurnal.Contains("/LT");
                            });
                        if (filtered.Any())
                        {
                            dtnew = filtered.CopyToDataTable();
                        }
                        else
                        {
                            dtnew = dtJurnalInventory.Clone();
                        }
                    }
                    else if (checkEditlk.Checked == true)
                    {
                        var filtered = dtJurnalInventory.AsEnumerable()
                            .Where(y =>
                            {
                                var noJurnal = y.Field<string>("NOJURNAL");
                                return !string.IsNullOrEmpty(noJurnal) && noJurnal.Contains("/LK");
                            });
                        if (filtered.Any())
                        {
                            dtnew = filtered.CopyToDataTable();
                        }
                        else
                        {
                            dtnew = dtJurnalInventory.Clone();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }


        private void checkEditlt_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalInventory.Rows.Count > 0)
                {
                    if (checkEditlt.Checked == true)
                    {

                        checkEditlk.Checked = false;
                        ColumnView view = gridView_inv_header;
                        GridColumn colCategory = view.Columns["NOMOR"];
                        ColumnFilterInfo filter = new("Contains([NOMOR], '/LT')", string.Empty);
                        view.ActiveFilter.Add(colCategory, filter);
                    }
                    else
                    {

                        gridView_inv_header.Columns["NOMOR"].ClearFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }


        private void checkEditlk_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalInventory.Rows.Count > 0)
                {
                    if (checkEditlk.Checked == true)
                    {

                        checkEditlt.Checked = false;
                        ColumnView view = gridView_inv_header;
                        GridColumn colCategory = view.Columns["NOMOR"];
                        ColumnFilterInfo filter = new("Contains([NOMOR], '/LK')", string.Empty);
                        view.ActiveFilter.Add(colCategory, filter);
                    }
                    else
                    {

                        gridView_inv_header.Columns["NOMOR"].ClearFilter();
                    }
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }


        private void SETAHUNINV_EditValueChanged(object sender, EventArgs e)
        {
            Load_inv_header();
            Load_Jurnal_INV();
        }




        private void gridView_inv_header_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
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

                    DXMenuItem JurnalSelected = CreateMenuItemJurnalInvSelected(view, rowHandle);
                    DXMenuItem exportselected = CreateMenuExportInvSelected(view, rowHandle);


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


        private DXMenuItem CreateMenuItemJurnalInvSelected(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Jurnal Item Terpilih", new EventHandler(OnInvJurnalClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[2];
            return checkItem;
        }


        private void OnInvJurnalClick(object? sender, EventArgs e)
        {
            List<string> selectedValues = GetSelectedRowValues(gridView_inv_header, "NOMOR");

            if (selectedValues.Any())
            {
                if (selectedValues.Count == 1)
                {
                    string periode = $"{CMBBULANINV.SelectedIndex + 1:00}/{SETAHUNINV.Value}";
                    string nomor = Convert.ToString(gridView_inv_header.GetRowCellValue(gridView_inv_header.FocusedRowHandle, "NOMOR")) ?? string.Empty;
                    DateTime tanggal = Convert.ToDateTime(gridView_inv_header.GetRowCellValue(gridView_inv_header.FocusedRowHandle, "TANGGAL"));
                    List<JurnalDetailAdd> query = jurnalImportSelectionService.BuildInputDetails(dtJurnalInventory, selectedValues);

                    LoadSelectedImportJurnalToInputTab(nomor, tanggal, periode, query);
                }
                else
                {
                    XtraMessageBox.Show("Pilih hanya satu Nomor untuk menjurnal ini", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


        private DXMenuItem CreateMenuExportInvSelected(GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Export Item Terpilih", new EventHandler(OnInvExportClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }


        private void OnInvExportClick(object? sender, EventArgs e)
        {
            List<string> selectedValues = GetSelectedRowValues(gridView_inv_header, "NOMOR");

            if (selectedValues.Any())
            {
                ExportNomorInvetoryDipilih(selectedValues);
            }
        }


        private void ExportNomorInvetoryDipilih(List<string> selectedValues)
        {
            try
            {
                List<JurnalDetailDTO> selectedJurnalItems = BuildSelectedJurnalDetails(dtJurnalInventory, selectedValues);
                jurnalExcelExportService.ExportJurnalDetails(selectedJurnalItems, "JurnalInventoryTerpilih");
            }
            catch (InvalidOperationException ex)
            {
                XtraMessageBox.Show(ex.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Export Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private void gc_inv_header_Click(object sender, EventArgs e)
        {
            LoadDataInventoryDetail();
        }


        private void gridView_inv_header_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up | e.KeyCode == Keys.Down)
            {
                LoadDataInventoryDetail();
            }
        }


        private void LoadDataInventoryDetail()
        {
            try
            {
                var filter = gridView_inv_header.GetRowCellValue(gridView_inv_header.FocusedRowHandle, "NOMOR").ToString();
                var filtered = dtJurnalInventory.AsEnumerable().Where(row => row.Field<string>("NOJURNAL") == filter).CopyToDataTable();
                GC_INV.DataSource = filtered;
                gridView_INVDetails.Columns[0].Visible = false;
                gridView_INVDetails.Columns[1].Visible = false;
                gridView_INVDetails.Columns[2].Visible = false;
                gridView_INVDetails.Columns[8].Visible = false;
                gridView_INVDetails.Columns[9].Visible = false;
                gridView_INVDetails.Columns[10].Visible = false;
                gridView_INVDetails.Columns[11].Visible = false;
                gridView_INVDetails.Columns[12].Visible = false;
                gridView_INVDetails.Columns[13].Visible = false;
                ApplyNumericFormat(gridView_INVDetails.Columns[5]);
                ApplyNumericFormat(gridView_INVDetails.Columns[6]);
                ApplyNumericSummary(gridView_INVDetails.Columns[5], "DEBET");
                ApplyNumericSummary(gridView_INVDetails.Columns[6], "KREDIT");
                gridView_INVDetails.BestFitColumns();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on filter Inventory", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
