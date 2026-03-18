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
        private void PilihanPeriodeAkuntansi()
        {
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);
                handle.QueueFocus(IntPtr.Zero);

                // Sample input: "01/2024"
                var Periode_daftar_Jurnal = leallperiode.EditValue.ToString();

                // Try parsing the string to DateTime
                DateTime periodeDate;
                if (DateTime.TryParseExact(Periode_daftar_Jurnal, "MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out periodeDate))
                {
                    // Extract month and year
                    int month = periodeDate.Month;
                    int year = periodeDate.Year;

                    // You can now use `month` and `year` as needed
                    Console.WriteLine($"Month: {month}, Year: {year}");

                    // Continue with your logic
                    if (!string.IsNullOrEmpty(defiltertanggal.Text) || int.TryParse(txtfilterjumlah.Text, out var jumlahFilter) && jumlahFilter > 0 ||
                        !string.IsNullOrEmpty(txtfilterkode.Text) || !string.IsNullOrEmpty(txtfilternojurnal.Text) || !string.IsNullOrEmpty(txtfilterketerangan.Text))
                    {
                        JurnalDetail = jurnalRepository.GetJurnalDetails_DapperAsQueryable(CompanyInfo.IDDATA,year,month,month);
                        CariJurnal_Bulan();
                        GCHeader.Focus();
                    }
                    else
                    {
                        JurnalHeader = jurnalRepository.GetJurnalHeader_Dapper(CompanyInfo.IDDATA, Periode_daftar_Jurnal);
                        JurnalDetail = jurnalRepository.GetJurnalDetails_DapperAsQueryable(CompanyInfo.IDDATA, year, month, month);

                        GCHeader.DataSource = JurnalHeader;
                        GCDetails.DataSource = null;

                        GVHeader.Columns["JURNALID"].Visible = false;
                        GVHeader.Columns["HID"].Visible = false;
                        ApplyDateFormat(GVHeader.Columns["Tanggal"]);
                        GVHeader.OptionsCustomization.AllowColumnResizing = true;
                        GVHeader.BestFitColumns();
                    }

                    GVHeader.Focus();
                }
                else
                {
                    XtraMessageBox.Show("Invalid period format. Please use MM/yyyy format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Error during 'Pilihan Periode Akuntansi': {ex.Message}", "Error Pilihan Periode", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GVHeader_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
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

                    DXMenuItem hapus = CreateMenuItemHapus(view, rowHandle);
                    DXMenuItem exportselected = CreateMenuExportSelected(view, rowHandle);


                    hapus.BeginGroup = true;
                    exportselected.BeginGroup = true;

                    e.Menu.Items.Add(hapus);
                    e.Menu.Items.Add(exportselected);
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on Popup Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DXMenuItem CreateMenuExportSelected(DevExpress.XtraGrid.Views.Grid.GridView view, int rowHandle)
        {
            DXMenuItem checkItem = new("Export Terpilih", new EventHandler(OnExportClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[1];
            return checkItem;
        }

        private void OnExportClick(object? sender, EventArgs e)
        {
            List<double> selectedValues = new();

            // Iterate over the selected rows
            for (int i = 0; i < GVHeader.SelectedRowsCount; i++)
            {
                // Get the selected row handle
                int rowHandle = GVHeader.GetSelectedRows()[i];

                // Get the value from a specific column (replace "ColumnName" with the actual column name)
                double value = Convert.ToDouble(GVHeader.GetRowCellValue(rowHandle, "JURNALID").ToString());

                // Add the value to the list
                selectedValues.Add(value);
            }

            if (selectedValues.Any())
            {
                ExportJurnalDipilih(selectedValues);
            }
        }

        private void ExportJurnalDipilih(List<double> selectedValues)
        {
            try
            {
                List<JurnalDetailDTO> selectedJurnalItems = (JurnalDetail ?? Enumerable.Empty<JurnalDetailDTO>())
                    .Where(j => selectedValues.Contains(j.REFFID))
                    .ToList();

                jurnalExcelExportService.ExportJurnalDetails(selectedJurnalItems, "JurnalTerpilih");
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

        private void Ubah_jurnal()
        {
            try
            {
                var Periode_daftar_Jurnal = leallperiode.Text;
                if (!ValidatePeriodNotLocked(CompanyInfo.IDDATA, Periode_daftar_Jurnal, Periode_daftar_Jurnal))
                    return;
                editjurnal = true;
                var periodelist = jurnalRepository.PeriodeList(CompanyInfo.IDDATA, leallperiode.Text[^4..]);

                leperiode.Properties.DataSource = periodelist;
                leperiode.Properties.ValueMember = "PERIODE";
                leperiode.Properties.DisplayMember = "PERIODE";
                if (this.GVHeader.GetFocusedRowCellValue("JURNALID") == null)
                    return;
                var rowhandle = GVHeader.FocusedRowHandle;
                old_JurnalID = Convert.ToDouble(GVHeader.GetRowCellValue(rowhandle, "JURNALID"));
                var NomorJurnal = GVHeader.GetRowCellValue(rowhandle, "NoJurnal").ToString();
                var TanggalJurnal = Convert.ToDateTime(GVHeader.GetRowCellValue(rowhandle, "Tanggal"));

                //header
                NoJurnaltxt.Text = NomorJurnal;
                deJurnal.EditValue = TanggalJurnal;
                leperiode.EditValue = leallperiode.Text;

                var jurnalDetailQuery = JurnalDetail ?? Enumerable.Empty<JurnalDetailDTO>().AsQueryable();
                InputJurnalDetail = new BindingList<JurnalDetailAdd>(
                    jurnalDetailQuery
                        .Where(d => d.REFFID == old_JurnalID)
                        .OrderBy(n => n.BARIS)
                        .Select(n => new JurnalDetailAdd
                        {
                            BARIS = n.BARIS,
                            Kode = n.Kode,
                            Rekening = n.Rekening,
                            Debet = n.Debet,
                            Kredit = n.Kredit,
                            Keterangan = n.Keterangan
                        })
                        .ToList()
                );



                this.GCJurnal.DataSource = InputJurnalDetail;
                InputJurnalDetail.AllowNew = true;

                TABJurnal.SelectedTabPage = xtraTabPage1;
                x = InputJurnalDetail.Count() - 1;
                var jre = jurnalRepository.CekjURNALRJE(old_JurnalID);
                if (jre)
                {
                    jurnalbalik.Checked = true;
                }
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on ubah jurnal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DXMenuItem CreateMenuItemHapus(DevExpress.XtraGrid.Views.Grid.GridView? view, int rowHandle)
        {
            DXMenuItem checkItem = new("Hapus Terpilih", new EventHandler(OnHapusClick));
            checkItem.ImageOptions.Image = imageCollection1.Images[0];
            return checkItem;
        }

        private void OnHapusClick(object? sender, EventArgs e)
        {
            Hapus_Jurnal_Selected();
        }

        private void Hapus_Jurnal_Selected()
        {
            try
            {
                var Periode = leallperiode.Text;
                if (!ValidatePeriodNotLocked(CompanyInfo.IDDATA, Periode, Periode))
                    return;
                List<string> selectedNoJurnal = new();
                List<double> selectedValues = new();

                // Iterate over the selected rows
                for (int i = 0; i < GVHeader.SelectedRowsCount; i++)
                {
                    // Get the selected row handle
                    int rowHandle = GVHeader.GetSelectedRows()[i];

                    // Get the value from a specific column (replace "ColumnName" with the actual column name)
                    string? value = GVHeader.GetRowCellValue(rowHandle, "NoJurnal")?.ToString();
                    double value1 = Convert.ToDouble(GVHeader.GetRowCellValue(rowHandle, "JURNALID")?.ToString());


                    // Add the value to the list
                    if (!string.IsNullOrEmpty(value))
                    {
                        selectedNoJurnal.Add(value);
                    }
                    selectedValues.Add(value1);
                }
                // Create a formatted string containing the selected values
                string NoJurnal = string.Join("\n", selectedNoJurnal);

                if (selectedNoJurnal.Any())
                {
                    if (XtraMessageBox.Show("Hapus Transaksi Jurnal ? \n\nNomor : \n" + NoJurnal + "\n", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    jurnalRepository.HapusJurnalRange(selectedValues);
                    PilihanPeriodeAkuntansi();
                    XtraMessageBox.Show("Jurnal telah dihapus", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on hapus jurnal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void Hapus_jurnal()
        {
            try
            {
                if (this.GVHeader.GetFocusedRowCellValue("HID") == null)
                    return;
                var Periode = leallperiode.Text;
                var rowhandle = GVHeader.FocusedRowHandle;
                var Nomor = GVHeader.GetRowCellValue(rowhandle, "NoJurnal").ToString();
                old_JurnalID = Convert.ToDouble(GVHeader.GetRowCellValue(rowhandle, "JURNALID"));

                if (!ValidatePeriodNotLocked(CompanyInfo.IDDATA, Periode, Periode))
                    return;

                if (XtraMessageBox.Show("Hapus Transaksi Jurnal ? \n\nNomor : " + Nomor + "\nPeriode : " + Periode, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                string dariModule_AIS = jurnalRepository.CekSumber_Jurnal(old_JurnalID);
                jurnalRepository.HapusJurnal(old_JurnalID);
                if (dariModule_AIS == "AIS" && !string.IsNullOrEmpty(Nomor))
                {
                    UpdateStatusJurnal_AIS_DELETED(Nomor, Periode);
                }
                PilihanPeriodeAkuntansi();
                XtraMessageBox.Show("Jurnal telah dihapus", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on hapus jurnal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatusJurnal_AIS_DELETED(string p_NOMOR, string P_PERIODE)
        {
            jurnalRepository.UpdateStatusJurnal_AIS_DELETED(p_NOMOR, P_PERIODE);
        }

        private void GCHeader_Click(object sender, EventArgs e)
        {
            if (this.GVHeader.GetFocusedRowCellValue("NoJurnal") == null)
                return;
            FilterNomorJurnal();
        }

        private void FilterNomorJurnal()
        {

            try
            {
                double jurnalId = Convert.ToDouble(GVHeader.GetRowCellValue(GVHeader.FocusedRowHandle, "JURNALID").ToString());
                List<JurnalDetailDTO> filteredRows = jurnalDaftarCariService.FilterDetailRowsByHeaderId(JurnalDetail ?? Enumerable.Empty<JurnalDetailDTO>(), jurnalId);
                GCDetails.DataSource = filteredRows;
                GVDetail.Columns[0].Visible = false;
                GVDetail.Columns[1].Visible = false;
                GVDetail.Columns[2].Visible = false;
                ApplyNumericFormat(GVDetail.Columns[6]);
                ApplyNumericFormat(GVDetail.Columns[7]);
                ApplyNumericSummary(GVDetail.Columns[6], "Debet");
                ApplyNumericSummary(GVDetail.Columns[7], "Kredit");
                GVDetail.Columns[9].Visible = false;
                GVDetail.Columns[10].Visible = false;
                GVDetail.OptionsCustomization.AllowColumnResizing = true;

                GVDetail.BestFitColumns();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error on filter no jurnal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private void GCHeader_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.GVHeader.GetFocusedRowCellValue("NoJurnal") == null)
                return;
            FilterNomorJurnal();
        }






        private void GVHeader_GotFocus(object sender, EventArgs e)
        {
            if (this.GVHeader.GetFocusedRowCellValue("NoJurnal") == null)
                return;
            FilterNomorJurnal();
        }

        private void txtfilterjurnal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
            else if (e.KeyCode == Keys.Down)
            {
                GCHeader.Focus();
            }
        }



        private void txtfilterket_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
            else if (e.KeyCode == Keys.Down)
            {
                GCHeader.Focus();
            }
        }

    }
}
