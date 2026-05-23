using Accounting._1.Interface;
using Accounting._2.DataAcces;
using Accounting.BusinessLayer;
using Accounting.Model;
using Accounting.Services;
using Accounting.UC.Jurnal;
using DevExpress.Data;
using DevExpress.Mvvm.Native;
using DevExpress.Utils.DragDrop;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
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
        private void Repkredit_KeyDown(object? sender, KeyEventArgs e)
        {
            HandleAmountEditorEnter("Kredit", "Debet", e);
        }


        private void repdebet_KeyDown(object? sender, KeyEventArgs e)
        {
            HandleAmountEditorEnter("Debet", "Kredit", e);
        }

        private void HandleAmountEditorEnter(string sourceColumn, string oppositeColumn, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            int focusedRowHandle = JDgridView.FocusedRowHandle;
            if (focusedRowHandle < 0)
            {
                e.Handled = true;
                return;
            }

            object sourceValue = JDgridView.GetRowCellValue(focusedRowHandle, sourceColumn);
            if (!decimal.TryParse(sourceValue?.ToString(), out decimal parsedValue) || parsedValue <= 0)
            {
                JDgridView.CloseEditor();
                JDgridView.FocusedColumn = JDgridView.Columns[oppositeColumn];
                e.Handled = true;
                return;
            }

            JDgridView.SetRowCellValue(focusedRowHandle, oppositeColumn, 0);
            HitungSelisih();

            JDgridView.FocusedColumn = JDgridView.Columns["Keterangan"];

            e.Handled = true;
        }

        private void HitungSelisih()
        {
            // Ambil nilai summary dari kolom Debet dan Kredit
            var Debet = Convert.ToDecimal(JDgridView.Columns["Debet"].SummaryItem.SummaryValue);
            var Kredit = Convert.ToDecimal(JDgridView.Columns["Kredit"].SummaryItem.SummaryValue);

            // Hitung selisih
            decimal selisih = Math.Abs(Debet - Kredit);


        }

        private void HandleDebetCellChange(GridView gridView, CellValueChangedEventArgs e)
        {
            int rowHandle = e.RowHandle;

            // Set KREDIT menjadi 0
            gridView.SetRowCellValue(rowHandle, gridView.Columns["Kredit"], 0);
        }

        private void HandleKreditCellChange(GridView gridView, CellValueChangedEventArgs e)
        {
            int rowHandle = e.RowHandle;

            // Set DEBET menjadi 0
            gridView.SetRowCellValue(rowHandle, gridView.Columns["Debet"], 0);
        }

        private void ShowKodeForm(JurnalDetailAdd rowData, IEnumerable<DTOCOAAktif> listCoaAktif)
        {
            using FrmDaftarCOA COAForm = new()
            {
                StartPosition = FormStartPosition.CenterParent,
                ListCoaAktif = listCoaAktif
            };

            COAForm.SetSearchPanelValue(rowData.Kode);

            if (COAForm.ShowDialog(this) == DialogResult.OK)
            {
                string Kode = COAForm.KODE;
                string Rekening = COAForm.REKENING;

                rowData.Kode = Kode;
                rowData.Rekening = Rekening;
            }
        }

        private bool TryResolveKodeStyle1(GridView gridView, int rowHandle, string kode, bool openLookupIfInvalid)
        {
            if (string.IsNullOrWhiteSpace(kode))
            {
                return false;
            }

            var kodeacct = ListCoaAktif.FirstOrDefault(t =>
                string.Equals(t.KODE, kode, StringComparison.OrdinalIgnoreCase));

            if (kodeacct != null)
            {
                gridView.SetRowCellValue(rowHandle, "Kode", kodeacct.KODE);
                gridView.SetRowCellValue(rowHandle, "Rekening", kodeacct.PERKIRAAN);
                return true;
            }

            if (!openLookupIfInvalid || gridView.GetRow(rowHandle) is not JurnalDetailAdd rowData)
            {
                return false;
            }

            gridView.SetRowCellValue(rowHandle, "Rekening", string.Empty);
            rowData.Rekening = string.Empty;

            ShowKodeForm(rowData, ListCoaAktif);
            string kodeTerpilih = rowData.Kode?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(kodeTerpilih))
            {
                gridView.RefreshRow(rowHandle);
                return false;
            }

            var selectedKodeAcct = ListCoaAktif.FirstOrDefault(t =>
                string.Equals(t.KODE, kodeTerpilih, StringComparison.OrdinalIgnoreCase));

            if (selectedKodeAcct == null)
            {
                gridView.SetRowCellValue(rowHandle, "Rekening", string.Empty);
                rowData.Rekening = string.Empty;
                gridView.RefreshRow(rowHandle);
                return false;
            }

            gridView.SetRowCellValue(rowHandle, "Kode", selectedKodeAcct.KODE);
            gridView.SetRowCellValue(rowHandle, "Rekening", selectedKodeAcct.PERKIRAAN);
            gridView.RefreshRow(rowHandle);
            return true;
        }

        private void JDgridView_ValidateRow(object sender, ValidateRowEventArgs e)
        {

        }

        private void JDgridView_ShownEditor(object? sender, EventArgs e)
        {
            if (sender is not GridView view)
            {
                return;
            }

            // Check if the editor is active for the "Kode" or "Keterangan" column
            if (view.FocusedColumn != null &&
                (view.FocusedColumn.FieldName == "Kode" || view.FocusedColumn.FieldName == "Keterangan")
                && view.ActiveEditor is TextEdit editor)
            {
                // Prevent selecting the text by setting the SelectionStart at the end of the text
                editor.SelectionStart = editor.Text.Length;
                editor.SelectionLength = 0; // No selection
            }
        }

        private void GridViewAISheader_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            // Reinitialize collections
            List<AIS_JURNAL> jurnalfiltered = new();
            var div = gridViewAISheader.GetFocusedRowCellValue("DIVISIID") as string;
            var namadiv = gridViewAISheader.GetFocusedRowCellValue("DIVISI") as string;
            var isbor = gridViewAISheader.GetFocusedRowCellValue("ISBORONGAN");
            noAIS_Bukti = gridViewAISheader.GetFocusedRowCellValue("NOMOR")?.ToString() ?? string.Empty;

            if (lookUpEditEstate.EditValue == null || leremiseAIS.EditValue == null || cmbbulanAIS.SelectedIndex < 0)
            {
                gcAISdetail.DataSource = null;
                return;
            }

            if (!short.TryParse(leremiseAIS.EditValue.ToString(), out short remise))
            {
                gcAISdetail.DataSource = null;
                return;
            }

            DateTime TanggalJurnal;
            string? estate = lookUpEditEstate.EditValue?.ToString();
            if (string.IsNullOrWhiteSpace(estate))
            {
                gcAISdetail.DataSource = null;
                return;
            }
            var bl = cmbbulanAIS.SelectedIndex + 1;
            var th = (int)setahunAIS.Value;
            int periode = Convert.ToInt32(th.ToString() + bl.ToString("00"));
            var periodestring = FormatPeriod(bl, th);
            var iddata = CompanyInfo.IDDATA;
            string ket2 = $"{namadiv} {estate} R{remise} {cmbbulanAIS.Text} {setahunAIS.EditValue}";

            if (remise == 1)
            {
                TanggalJurnal = new DateTime(th, bl, 15);
            }
            else
            {
                // Get the last day of the month
                DateTime lastDayOfMonth = new DateTime(th, bl, 1).AddMonths(1).AddDays(-1);
                TanggalJurnal = lastDayOfMonth;
            }
            jurnalfiltered.Clear();
            if (!string.IsNullOrEmpty(div) && isbor != null && aisJurnal != null)
            {
                if (int.TryParse(isbor.ToString(), out int isborongan))
                {
                    if (isborongan == 1)
                    {

                        jurnalfiltered = aisJurnal
                        .Where(f => f.DIVISI == div && f.ISBORONGAN == isborongan && f.JENIS == "PANEN")
                        .OrderBy(f => f.JENIS)
                        .ThenBy(f => f.PEKERJAAN)
                        .ThenBy(f => f.BLOK)
                        .ToList();


                        foreach (var item in jurnalfiltered)
                        {
                            decimal debetWithPPH21 = JurnalAmountRounding.RoundJournalAmount(item.DEBETPPH / 0.975m);
                            if (item.DEBET < debetWithPPH21)
                            {
                                decimal pph21 = JurnalAmountRounding.RoundJournalAmount(debetWithPPH21 * 0.025m);
                                item.DEBET = JurnalAmountRounding.RoundJournalAmount(item.DEBET + pph21);
                                item.PPH21 = JurnalAmountRounding.RoundJournalAmount(item.PPH21 + pph21);
                            }
                        }
                        // Assuming komponenjurnal is a List<JurnalKomponen>
                        var komponenfiltered = komponenjurnal
                            .Where(x => x.Divisi == div && x.IsBorongan)  // Apply filter for Divisi and IsBorongan
                            .Select(x => new AIS_JURNAL
                            {
                                // Mapping properties from JurnalKomponen to AIS_JURNAL
                                DEBET = x.Sisi == "D" ? JurnalAmountRounding.RoundJournalAmount(x.Jumlah) : 0,
                                KREDIT = x.Sisi == "K" ? JurnalAmountRounding.RoundJournalAmount(x.Jumlah) : 0,        // Mapping Jumlah to DEBET
                                KETERANGAN = $"{x.Keterangan ?? string.Empty} {ket2 ?? string.Empty}",
                                POSTED = true,
                                PERIODE = periodestring
                            })
                            .ToList();

                        // Adding the filtered and mapped list to jurnalfiltered
                        jurnalfiltered.AddRange(komponenfiltered);


                        // Calculate the total DEBET after updates
                        decimal totaldebet = JurnalAmountRounding.RoundJournalAmount(jurnalfiltered.Sum(f => f.DEBET));
                        decimal totalpph21 = JurnalAmountRounding.RoundJournalAmount(jurnalfiltered.Sum(f => f.PPH21));
                        decimal totalkredit = JurnalAmountRounding.RoundJournalAmount(jurnalfiltered.Sum(f => f.KREDIT));

                        // Calculate operational value
                        decimal operasional = JurnalAmountRounding.RoundJournalAmount(totaldebet - (totalpph21 + totalkredit));


                        // Add new rows for debetPercentage and operasional

                        var newRows = new List<AIS_JURNAL>
                            {
                                new() { KODE = "31.00001.001",REKENING="Hutang PPh Pasal 21",KREDIT = totalpph21,KETERANGAN="TOTAL PPH PASAL 21 " +ket2,POSTED=true,PERIODE=periodestring},
                                new() { KODE = "33.00001.001",REKENING="Gaji dan Upah YMH dibayar", KREDIT = operasional,KETERANGAN="TOTAL BORONGAN PANEN "+ket2 ,POSTED=true,PERIODE=periodestring }
                            };

                        // Add the new rows to your existing list or data source
                        jurnalfiltered.AddRange(newRows);


                    }
                    else
                    {

                        jurnalfiltered = aisJurnal
                        .Where(f => f.DIVISI == div && !(f.ISBORONGAN == 1 && f.JENIS == "PANEN"))
                        .OrderBy(f => f.JENIS)
                        .ThenBy(f => f.PEKERJAAN)
                        .ThenBy(f => f.BLOK)
                        .ToList();

                        // Assuming komponenjurnal is a List<JurnalKomponen>
                        var komponenfiltered = komponenjurnal
                            .Where(x => x.Divisi == div && !x.IsBorongan)  // Apply filter for Divisi and IsBorongan
                            .Select(x => new AIS_JURNAL
                            {
                                // Mapping properties from JurnalKomponen to AIS_JURNAL
                                DEBET = x.Sisi == "D" ? JurnalAmountRounding.RoundJournalAmount(x.Jumlah) : 0,
                                KREDIT = x.Sisi == "K" ? JurnalAmountRounding.RoundJournalAmount(x.Jumlah) : 0,        // Mapping Jumlah to DEBET
                                KETERANGAN = $"{x.Keterangan ?? string.Empty} {ket2 ?? string.Empty}",
                                POSTED = true,
                                PERIODE = periodestring
                            })
                            .ToList();

                        // Adding the filtered and mapped list to jurnalfiltered
                        jurnalfiltered.AddRange(komponenfiltered);

                        decimal biayaoperasional_bruto = JurnalAmountRounding.RoundJournalAmount(jurnalfiltered.Sum(f => f.DEBET));
                        decimal biayaoperasional_netto = JurnalAmountRounding.RoundJournalAmount(biayaoperasional_bruto - jurnalfiltered.Sum(f => f.KREDIT));



                        // Ensure the row with KODE "33.00001.001" is added last
                        var lastRow = new AIS_JURNAL
                        {
                            KODE = "33.00001.001",
                            REKENING = "Gaji dan Upah YMH dibayar",
                            KREDIT = biayaoperasional_netto,
                            KETERANGAN = "TOTAL BIAYA OPERASIONAL " + ket2,
                            POSTED = true,
                            PERIODE = periodestring
                        };
                        // Finally, add the specific last row
                        jurnalfiltered.Add(lastRow);




                    }

                    // Tambahkan nomor urut
                    int nomorUrut = 1;
                    foreach (var item in jurnalfiltered)
                    {
                        item.NO = nomorUrut++;
                    }


                    jurnalfinalAIS = JurnalAmountRounding.NormalizeAisFinalRows(jurnalfiltered.Select(j => new AIS_JURNAL_FINAL
                    {
                        NOJURNAL = noAIS_Bukti,
                        TANGGAL = TanggalJurnal,
                        NO = j.NO,
                        KODE = j.KODE,
                        REKENING = j.REKENING,
                        DEBET = j.DEBET,
                        KREDIT = j.KREDIT,
                        KETERANGAN = j.KETERANGAN,
                        POSTED = j.POSTED,
                        PERIODE = j.PERIODE
                    }));

                    gcAISdetail.DataSource = jurnalfinalAIS;
                    gridViewAISdetail.Columns["NOJURNAL"].Visible = false;
                    gridViewAISdetail.Columns["TANGGAL"].Visible = false;
                    gridViewAISdetail.Columns["POSTED"].Visible = false;
                    gridViewAISdetail.Columns["PERIODE"].Visible = false;

                    gridViewAISdetail.BestFitColumns();
                    gridViewAISdetail.OptionsView.ColumnAutoWidth = false;
                    gridViewAISdetail.ScrollStyle = DevExpress.XtraGrid.Views.Grid.ScrollStyleFlags.LiveVertScroll;

                    // Apply summaries and formatting
                    var debitColumn = gridViewAISdetail.Columns["DEBET"];
                    var kreditColumn = gridViewAISdetail.Columns["KREDIT"];

                    debitColumn.Summary.Clear();
                    kreditColumn.Summary.Clear();

                    debitColumn.Summary.Add(DevExpress.Data.SummaryItemType.Sum, "DEBET", "{0:n2}");
                    kreditColumn.Summary.Add(DevExpress.Data.SummaryItemType.Sum, "KREDIT", "{0:n2}");

                    debitColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    debitColumn.DisplayFormat.FormatString = "n2";

                    kreditColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                    kreditColumn.DisplayFormat.FormatString = "n2";
                }
                else
                {
                    gcAISdetail.DataSource = null;
                }
            }
            else
            {
                gcAISdetail.DataSource = null;
            }
        }




        private void HandleBehaviorDragDropEvents()
        {
            DragDropBehavior gridControlBehavior = behaviorManager1.GetBehavior<DragDropBehavior>(this.JDgridView);
            gridControlBehavior.DragDrop -= Behavior_DragDrop;
            gridControlBehavior.DragOver -= Behavior_DragOver;
            gridControlBehavior.DragDrop += Behavior_DragDrop;
            gridControlBehavior.DragOver += Behavior_DragOver;
        }

        private void Behavior_DragOver(object sender, DragOverEventArgs e)
        {

            DragOverGridEventArgs args = DragOverGridEventArgs.GetDragOverGridEventArgs(e);
            e.InsertType = args.InsertType;
            e.InsertIndicatorLocation = args.InsertIndicatorLocation;
            e.Action = args.Action;
            Cursor.Current = args.Cursor;
            args.Handled = true;
        }

        private void Behavior_DragDrop(object sender, DevExpress.Utils.DragDrop.DragDropEventArgs e)
        {
            if (e.Target is not GridView targetGrid || e.Source is not GridView sourceGrid)
            {
                return;
            }

            if (e.Action == DragDropActions.None || targetGrid != sourceGrid)
                return;
            if (sourceGrid.GridControl.DataSource is not BindingList<JurnalDetailAdd> sourceTable)
            {
                return;
            }

            Point hitPoint = targetGrid.GridControl.PointToClient(Cursor.Position);
            GridHitInfo hitInfo = targetGrid.CalcHitInfo(hitPoint);

            int[]? sourceHandles = e.GetData<int[]>();
            if (sourceHandles == null || sourceHandles.Length == 0)
            {
                return;
            }

            List<int> sourceIndexes = sourceHandles
                .Select(sourceGrid.GetDataSourceRowIndex)
                .Where(index => index >= 0 && index < sourceTable.Count)
                .Distinct()
                .OrderBy(index => index)
                .ToList();

            if (sourceIndexes.Count == 0)
            {
                return;
            }

            int targetRowHandle = hitInfo.RowHandle;
            int targetRowIndex = targetRowHandle >= 0
                ? targetGrid.GetDataSourceRowIndex(targetRowHandle)
                : -1;

            int insertIndex;
            if (targetRowIndex < 0)
            {
                // Drop outside row area defaults to append.
                insertIndex = sourceTable.Count;
            }
            else if (e.InsertType == InsertType.Before)
            {
                insertIndex = targetRowIndex;
            }
            else if (e.InsertType == InsertType.After)
            {
                insertIndex = targetRowIndex + 1;
            }
            else
            {
                return;
            }

            int removedBeforeInsert = sourceIndexes.Count(index => index < insertIndex);
            insertIndex -= removedBeforeInsert;
            insertIndex = Math.Max(0, Math.Min(insertIndex, sourceTable.Count));

            int[] destinationIndexes = Enumerable.Range(insertIndex, sourceIndexes.Count).ToArray();
            if (sourceIndexes.SequenceEqual(destinationIndexes))
            {
                return;
            }

            List<JurnalDetailAdd> movedRows = sourceIndexes
                .Select(index => sourceTable[index])
                .ToList();

            for (int i = sourceIndexes.Count - 1; i >= 0; i--)
            {
                sourceTable.RemoveAt(sourceIndexes[i]);
            }

            for (int i = 0; i < movedRows.Count; i++)
            {
                sourceTable.Insert(insertIndex + i, movedRows[i]);
            }

            targetGrid.ClearSelection();
            int selectedCount = movedRows.Count;
            for (int i = 0; i < selectedCount; i++)
            {
                int rowHandle = targetGrid.GetRowHandle(insertIndex + i);
                if (rowHandle >= 0)
                {
                    targetGrid.SelectRow(rowHandle);
                    if (i == 0)
                    {
                        targetGrid.FocusedRowHandle = rowHandle;
                    }
                }
            }
            ReindexBarisInInputOrder();

        }

        private void JDgridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not GridView gridView)
            {
                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                int focusedRowHandle = gridView.FocusedRowHandle;
                if (focusedRowHandle < 0 || gridView.FocusedColumn == null)
                {
                    e.Handled = true;
                    return;
                }

                string focusedField = gridView.FocusedColumn.FieldName;
                if (string.Equals(focusedField, "Debet", StringComparison.OrdinalIgnoreCase))
                {
                    decimal debetVal = Convert.ToDecimal(gridView.GetRowCellValue(focusedRowHandle, "Debet"));
                    if (debetVal > 0)
                    {
                        gridView.SetRowCellValue(focusedRowHandle, "Kredit", 0m);
                        gridView.FocusedColumn = gridView.Columns["Keterangan"];
                        e.Handled = true;
                        return;
                    }
                    // else: fall through to MoveNext → Kredit
                }
                else if (string.Equals(focusedField, "Kredit", StringComparison.OrdinalIgnoreCase))
                {
                    gridView.SetRowCellValue(focusedRowHandle, "Debet", 0m);
                }

                // Validasi KODE langsung saat Enter (Style 1 only)
                if (string.Equals(focusedField, "Kode", StringComparison.OrdinalIgnoreCase))
                {
                    suppressKodeLeaveValidation = true;
                    try
                    {
                        gridView.CloseEditor();
                        gridView.UpdateCurrentRow();
                    }
                    finally
                    {
                        suppressKodeLeaveValidation = false;
                    }

                    string kode = gridView.GetRowCellValue(focusedRowHandle, "Kode")?.ToString()?.Trim() ?? string.Empty;

                    if (string.IsNullOrEmpty(kode))
                    {
                        gridView.MoveNext();
                    }
                    else
                    {
                        bool isResolved = TryResolveKodeStyle1(gridView, focusedRowHandle, kode, openLookupIfInvalid: true);
                        gridView.FocusedColumn = isResolved ? gridView.Columns["Debet"] : gridView.Columns["Kode"];
                    }
                }
                // Saat selesai keterangan, langsung lanjut ke baris baru agar input cepat.
                else if (string.Equals(focusedField, "Keterangan", StringComparison.OrdinalIgnoreCase))
                {
                    int lastRowHandle = gridView.RowCount - 1;
                    if (focusedRowHandle >= lastRowHandle)
                    {
                        gridView.AddNewRow();
                        gridView.FocusedColumn = gridView.Columns["Kode"];
                    }
                    else
                    {
                        gridView.FocusedRowHandle = focusedRowHandle + 1;
                        gridView.FocusedColumn = gridView.Columns["Kode"];
                    }
                }
                else
                {
                    gridView.MoveNext();
                }

                e.Handled = true;
            }

            if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.Control)
            {
                if (!TryEnsureJurnalAccess(editjurnal
                        ? AuthorizationService.EnsureCanUpdateJurnal
                        : AuthorizationService.EnsureCanCreateJurnal))
                {
                    return;
                }

                if (XtraMessageBox.Show("Hapus Baris?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                if (gridView.FocusedRowHandle >= 0)
                {
                    gridView.DeleteRow(gridView.FocusedRowHandle);
                }
                else
                {
                    XtraMessageBox.Show("Tidak ada baris yang dipilih untuk dihapus.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }



        private string GetNamaPerkiraan(string? changedValue)
        {
            string namakun = ListCoaAktif.Where(x => x.KODE == changedValue).Select(x => x.PERKIRAAN).FirstOrDefault() ?? string.Empty;
            return namakun;
        }







        private void PopulatePeriode(DataTable dt2)
        {
            dt2 = jurnalRepository.PeriodeList(CompanyInfo.IDDATA, DateTime.Today.Year.ToString());

            leperiode.Properties.DataSource = dt2;
            leperiode.Properties.ValueMember = "PERIODE";
            leperiode.Properties.DisplayMember = "PERIODE";
        }

        private void JDgridView_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if ((e.Column.FieldName == "Debet" || e.Column.FieldName == "Kredit")
                && Convert.ToDecimal(e.Value) == 0)
            {
                e.DisplayText = string.Empty;
            }
        }
    }
}
