using Accounting._1.Interface;
using Accounting.BusinessLayer;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmJurnal
    {
        private bool TryCreateAisJournalBuildContext(out AisJournalBuildContext context)
        {
            context = null!;

            if (lookUpEditEstate.EditValue == null || leremiseAIS.EditValue == null || cmbbulanAIS.SelectedIndex < 0)
            {
                return false;
            }

            if (!int.TryParse(leremiseAIS.EditValue.ToString(), out int remise))
            {
                return false;
            }

            string estate = lookUpEditEstate.EditValue.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(estate))
            {
                return false;
            }

            int month = cmbbulanAIS.SelectedIndex + 1;
            int year = Convert.ToInt32(setahunAIS.Value);
            DateTime journalDate = remise == 1
                ? new DateTime(year, month, 15)
                : new DateTime(year, month, 1).AddMonths(1).AddDays(-1);

            context = new AisJournalBuildContext
            {
                Estate = estate,
                Remise = remise,
                MonthName = cmbbulanAIS.Text,
                Year = year,
                PeriodString = FormatPeriod(month, year),
                JournalDate = journalDate
            };

            return true;
        }

        private List<Division> GetAisHeaderDivisions()
        {
            if (gcAISheader.DataSource is IEnumerable<Division> divisions)
            {
                return divisions.ToList();
            }

            List<Division> result = [];
            for (int rowHandle = 0; rowHandle < gridViewAISheader.RowCount; rowHandle++)
            {
                if (gridViewAISheader.GetRow(rowHandle) is Division division)
                {
                    result.Add(division);
                }
            }

            return result;
        }

        private void SetAisDetailGridDataSource(IEnumerable<AIS_JURNAL_FINAL> rows)
        {
            gcAISdetail.DataSource = rows;

            if (gridViewAISdetail.Columns["NOJURNAL"] != null)
            {
                gridViewAISdetail.Columns["NOJURNAL"].Visible = false;
            }

            if (gridViewAISdetail.Columns["TANGGAL"] != null)
            {
                gridViewAISdetail.Columns["TANGGAL"].Visible = false;
            }

            if (gridViewAISdetail.Columns["POSTED"] != null)
            {
                gridViewAISdetail.Columns["POSTED"].Visible = false;
            }

            if (gridViewAISdetail.Columns["PERIODE"] != null)
            {
                gridViewAISdetail.Columns["PERIODE"].Visible = false;
            }

            gridViewAISdetail.BestFitColumns();
            gridViewAISdetail.OptionsView.ColumnAutoWidth = false;
            gridViewAISdetail.ScrollStyle = ScrollStyleFlags.LiveVertScroll;

            var debitColumn = gridViewAISdetail.Columns["DEBET"];
            var kreditColumn = gridViewAISdetail.Columns["KREDIT"];

            if (debitColumn != null)
            {
                debitColumn.Summary.Clear();
                debitColumn.Summary.Add(DevExpress.Data.SummaryItemType.Sum, "DEBET", "{0:n2}");
                debitColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                debitColumn.DisplayFormat.FormatString = "n2";
            }

            if (kreditColumn != null)
            {
                kreditColumn.Summary.Clear();
                kreditColumn.Summary.Add(DevExpress.Data.SummaryItemType.Sum, "KREDIT", "{0:n2}");
                kreditColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                kreditColumn.DisplayFormat.FormatString = "n2";
            }
        }

        private void ExportAllAisJurnal(bool borongan)
        {
            using var loadingScope = BeginGlobalLoadingScope();

            if (!TryCreateAisJournalBuildContext(out AisJournalBuildContext context))
            {
                XtraMessageBox.Show("Pilih estate, bulan, tahun, dan remise AIS terlebih dahulu.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<Division> divisions = GetAisHeaderDivisions();
            if (divisions.Count == 0)
            {
                XtraMessageBox.Show("Data header AIS belum tersedia.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<AIS_JURNAL_FINAL> rows = AisJournalBuilder.BuildForDivisions(
                aisJurnal ?? [],
                komponenjurnal ?? [],
                divisions,
                context,
                borongan);

            if (rows.Count == 0)
            {
                string label = borongan ? "borongan" : "harian";
                XtraMessageBox.Show($"Data AIS {label} tidak ditemukan untuk filter ini.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ExportAisJurnalWithDialog(rows, BuildAisExportFileName(borongan ? "Borongan" : "Harian"));
        }

        /// <summary>
        /// Prompts the user for a save location, exports the given AIS rows to that file, and confirms the result.
        /// </summary>
        private void ExportAisJurnalWithDialog(IEnumerable<AIS_JURNAL_FINAL> rows, string defaultFileName)
        {
            List<AIS_JURNAL_FINAL> data = rows?.ToList() ?? [];
            if (data.Count == 0)
            {
                XtraMessageBox.Show("Data tidak ditemukan", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using SaveFileDialog dialog = new()
            {
                Title = "Simpan Jurnal AIS ke Excel",
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                AddExtension = true,
                FileName = defaultFileName
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                int exported = jurnalExcelExportService.ExportAisJurnalToFile(data, dialog.FileName);
                XtraMessageBox.Show(
                    $"Berhasil mengekspor {exported} baris ke\n{dialog.FileName}",
                    "Export AIS",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
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

        /// <summary>
        /// Builds a descriptive default file name from the current AIS filter, e.g. "AIS_Borongan_EST1_R1_Mei2026.xlsx".
        /// </summary>
        private string BuildAisExportFileName(string typeLabel)
        {
            string estate = lookUpEditEstate.EditValue?.ToString() ?? "AIS";
            string remise = leremiseAIS.EditValue?.ToString() ?? string.Empty;
            string month = cmbbulanAIS.Text;
            int year = Convert.ToInt32(setahunAIS.Value);

            string raw = $"AIS_{typeLabel}_{estate}_R{remise}_{month}{year}";
            return SanitizeFileName(raw) + ".xlsx";
        }

        private static string SanitizeFileName(string name)
        {
            foreach (char invalid in System.IO.Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalid, '_');
            }

            return name.Replace(' ', '_');
        }
    }
}
