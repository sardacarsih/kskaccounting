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
        private bool UseInventoryBaruSource => checkEditInvBaru.Checked;

        private void lookUpEditINV_EditValueChanged(object sender, EventArgs e)
        {
            RefreshInventoryData();
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
            if (!TryGetInventoryContext(out int ptahun, out int pbulan, out string p_ptlokasi))
            {
                ClearInventoryHeader();
                return;
            }

            int p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("0#"));
            InventoryJurnalHeader = UseInventoryBaruSource
                ? jurnalRepository.GetJurnalHeader_InventoryBaru(p_periode_int, p_ptlokasi)
                : jurnalRepository.GetJurnalHeader_Inventory(p_periode_int, p_ptlokasi);

            gc_inv_header.DataSource = InventoryJurnalHeader?.ToList() ?? new List<JurnalInventoryHeaderDTO>();
            ApplyInventoryHeaderFilter();
        }


        private void Load_Jurnal_INV()
        {
            if (!TryGetInventoryContext(out int ptahun, out int pbulan, out string p_ptlokasi))
            {
                ClearInventoryDetail();
                return;
            }

            string p_iddata = CompanyInfo.IDDATA;
            int p_periode_int = Convert.ToInt32(ptahun.ToString() + pbulan.ToString("0#"));
            string p_periode_str = FormatPeriod(pbulan, ptahun);

            dtJurnalInventory = UseInventoryBaruSource
                ? jurnalRepository.Jurnal_InventoriBaru(p_periode_int, p_ptlokasi, p_iddata, "True", p_periode_str, LoginInfo.userID, ptahun, pbulan)
                : jurnalRepository.Jurnal_Inventori(p_periode_int, p_ptlokasi, p_iddata, "True", p_periode_str, LoginInfo.userID, ptahun, pbulan);

            LBLTOTALTRANSAKSI.Text = string.Format("{0:#,##}", CalculateInventoryTotal(dtJurnalInventory));

            if (gridView_inv_header.RowCount > 0)
            {
                gridView_inv_header.FocusedRowHandle = 0;
                LoadDataInventoryDetail();
            }
            else
            {
                GC_INV.DataSource = null;
            }
        }


        private void CMBBULANINV_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshInventoryData();
        }


        private void SBJURNALINV_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable sourceData = GetFilteredInventoryData();
                if (sourceData.Rows.Count == 0)
                {
                    return;
                }

                pbulan = CMBBULANINV.SelectedIndex + 1;
                ptahun = Convert.ToInt32(SETAHUNINV.Value);

                string p_periode = FormatPeriod(pbulan, ptahun);
                string Periode = CMBBULANINV.Text + " - " + SETAHUNINV.Value;
                if (!ValidatePeriodNotLockedWithSound(CompanyInfo.IDDATA, p_periode, Periode))
                {
                    return;
                }

                string sourceLabel = UseInventoryBaruSource ? "Inventory Baru" : "Inventori";
                string filterLabel = GetInventoryFilterLabel();
                if (XtraMessageBox.Show("Lanjutkan Proses Import Jurnal " + sourceLabel + " " + filterLabel + " ? " +
                    "\n\nPeriode : " + Periode + " " +
                    "\nLokasi Data :" + CompanyInfo.IDDATA,
                    "Confirm Proses", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }

                ExecuteImportJurnal(sourceData, pbulan, ptahun, ptahun, UseInventoryBaruSource ? "dari Inventory Baru" : "dari Inventori");
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
                DataTable dtnew = GetFilteredInventoryData();
                List<string> selectedNomor = dtnew.AsEnumerable()
                    .Select(row => row.Field<string>("NOJURNAL"))
                    .Where(noJurnal => !string.IsNullOrWhiteSpace(noJurnal))
                    .Select(noJurnal => noJurnal!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                List<JurnalDetailDTO> exportRows = BuildSelectedJurnalDetails(dtnew, selectedNomor);
                jurnalExcelExportService.ExportJurnalDetails(exportRows, UseInventoryBaruSource ? "JurnalInventoryBaru" : "JurnalInventory");
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


        private void checkEditlt_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalInventory.Rows.Count > 0)
                {
                    if (checkEditlt.Checked)
                    {
                        checkEditlk.Checked = false;
                    }

                    ApplyInventoryHeaderFilter();
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
                    if (checkEditlk.Checked)
                    {
                        checkEditlt.Checked = false;
                    }

                    ApplyInventoryHeaderFilter();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }


        private void SETAHUNINV_EditValueChanged(object sender, EventArgs e)
        {
            RefreshInventoryData();
        }


        private void checkEditInvBaru_CheckedChanged(object sender, EventArgs e)
        {
            RefreshInventoryData();
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
                string filter = Convert.ToString(gridView_inv_header.GetRowCellValue(gridView_inv_header.FocusedRowHandle, "NOMOR")) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(filter) || dtJurnalInventory.Rows.Count == 0)
                {
                    GC_INV.DataSource = null;
                    return;
                }

                IEnumerable<DataRow> filteredRows = dtJurnalInventory.AsEnumerable()
                    .Where(row => string.Equals(row.Field<string>("NOJURNAL"), filter, StringComparison.OrdinalIgnoreCase));
                DataTable filtered = filteredRows.Any() ? filteredRows.CopyToDataTable() : dtJurnalInventory.Clone();
                GC_INV.DataSource = filtered;
                if (gridView_INVDetails.Columns.Count == 0)
                {
                    return;
                }

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

        private void RefreshInventoryData()
        {
            Load_inv_header();
            Load_Jurnal_INV();
        }

        private bool TryGetInventoryContext(out int ptahun, out int pbulan, out string p_ptlokasi)
        {
            ptahun = Convert.ToInt32(SETAHUNINV.Value);
            pbulan = CMBBULANINV.SelectedIndex + 1;
            p_ptlokasi = lookUpEditINV.EditValue?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(p_ptlokasi) || SETAHUNINV.Value < 2022 || CMBBULANINV.SelectedIndex < 0)
            {
                return false;
            }

            return true;
        }

        private void ClearInventoryHeader()
        {
            InventoryJurnalHeader = Enumerable.Empty<JurnalInventoryHeaderDTO>();
            gc_inv_header.DataSource = null;
        }

        private void ClearInventoryDetail()
        {
            dtJurnalInventory = new DataTable();
            GC_INV.DataSource = null;
            LBLTOTALTRANSAKSI.Text = "0";
        }

        private void ApplyInventoryHeaderFilter()
        {
            GridColumn? nomorColumn = gridView_inv_header.Columns["NOMOR"];
            if (nomorColumn == null)
            {
                return;
            }

            nomorColumn.ClearFilter();
            if (checkEditlt.Checked)
            {
                ColumnFilterInfo filter = new("Contains([NOMOR], '/LT')", string.Empty);
                gridView_inv_header.ActiveFilter.Add(nomorColumn, filter);
                return;
            }

            if (checkEditlk.Checked)
            {
                ColumnFilterInfo filter = new("Contains([NOMOR], '/LK')", string.Empty);
                gridView_inv_header.ActiveFilter.Add(nomorColumn, filter);
            }
        }

        private DataTable GetFilteredInventoryData()
        {
            if (dtJurnalInventory.Rows.Count == 0)
            {
                return dtJurnalInventory.Clone();
            }

            IEnumerable<DataRow> filteredRows = dtJurnalInventory.AsEnumerable();
            if (checkEditlt.Checked)
            {
                filteredRows = filteredRows.Where(row => InventoryNomorContains(row, "/LT"));
            }
            else if (checkEditlk.Checked)
            {
                filteredRows = filteredRows.Where(row => InventoryNomorContains(row, "/LK"));
            }

            return filteredRows.Any() ? filteredRows.CopyToDataTable() : dtJurnalInventory.Clone();
        }

        private static bool InventoryNomorContains(DataRow row, string token)
        {
            string nomor = row.Field<string>("NOJURNAL") ?? string.Empty;
            return nomor.Contains(token, StringComparison.OrdinalIgnoreCase);
        }

        private static decimal CalculateInventoryTotal(DataTable source)
        {
            decimal total = 0m;
            if (source == null || !source.Columns.Contains("DEBET"))
            {
                return total;
            }

            foreach (DataRow row in source.Rows)
            {
                if (row["DEBET"] != DBNull.Value)
                {
                    total += Convert.ToDecimal(row["DEBET"]);
                }
            }

            return total;
        }

        private string GetInventoryFilterLabel()
        {
            if (checkEditlt.Checked)
            {
                return "(LT)";
            }

            if (checkEditlk.Checked)
            {
                return "(LK)";
            }

            return "(Semua)";
        }


    }
}
