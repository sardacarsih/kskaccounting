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

        private void NoJurnaltxt_EditValueChanged(object sender, EventArgs e)
        {

        }


        private static List<JurnalDetailDTO> BuildSelectedJurnalDetails(DataTable source, List<string> selectedValues)
        {
            return source
                .AsEnumerable()
                .Where(row => selectedValues.Contains(row.Field<string>("NOJURNAL") ?? string.Empty))
                .Select(row => new JurnalDetailDTO
                {
                    NoJurnal = row.Field<string>("NOJURNAL") ?? string.Empty,
                    Tanggal = row.Field<DateTime>("TANGGAL"),
                    BARIS = row["BARIS"] == DBNull.Value ? 0 : Convert.ToInt32(row["BARIS"]),
                    Kode = row.Field<string>("KODE") ?? string.Empty,
                    Rekening = row.Field<string>("REKENING") ?? string.Empty,
                    Debet = row.Field<decimal>("DEBET"),
                    Kredit = row.Field<decimal>("KREDIT"),
                    Keterangan = row.Field<string>("KETERANGAN") ?? string.Empty,
                    Posted = row.Field<string>("POSTED") ?? string.Empty,
                    Periode = row.Field<string>("PERIODE") ?? string.Empty
                })
                .OrderBy(row => row.NoJurnal)
                .ThenBy(row => row.BARIS)
                .ToList();
        }


        private static List<string> GetSelectedRowValues(GridView gridView, string fieldName)
        {
            List<string> selectedValues = new();
            foreach (int rowHandle in gridView.GetSelectedRows())
            {
                string? value = Convert.ToString(gridView.GetRowCellValue(rowHandle, fieldName));
                if (!string.IsNullOrWhiteSpace(value))
                {
                    selectedValues.Add(value);
                }
            }

            return selectedValues;
        }


        private void LoadSelectedImportJurnalToInputTab(string nomor, DateTime tanggal, string periode, List<JurnalDetailAdd> details)
        {
            NoJurnaltxt.Text = nomor;
            deJurnal.EditValue = tanggal;
            leperiode.EditValue = periode;

            InputJurnalDetail = new BindingList<JurnalDetailAdd>(details);
            GCJurnal.DataSource = InputJurnalDetail;
            InputJurnalDetail.AllowNew = true;
            TABJurnal.SelectedTabPageIndex = 0;
        }

        private static List<JurnalDetailDTO> NormalizeJurnalExportOrder(IEnumerable<JurnalDetailDTO> rows)
        {
            return rows
                .OrderBy(row => row.NoJurnal)
                .ThenBy(row => row.BARIS)
                .ToList();
        }

        private List<JurnalDetailDTO> GetActiveDetailRowsForExport()
        {
            if (GVDetail?.DataSource is IEnumerable<JurnalDetailDTO> detailRows)
            {
                return NormalizeJurnalExportOrder(detailRows);
            }

            if (GVHeader?.FocusedRowHandle >= 0
                && GVHeader.GetRowCellValue(GVHeader.FocusedRowHandle, "JURNALID") != null
                && double.TryParse(GVHeader.GetRowCellValue(GVHeader.FocusedRowHandle, "JURNALID")?.ToString(), out double jurnalId))
            {
                IEnumerable<JurnalDetailDTO> fallbackRows = (JurnalDetail ?? Enumerable.Empty<JurnalDetailDTO>())
                    .Where(row => row.REFFID == jurnalId);
                return NormalizeJurnalExportOrder(fallbackRows);
            }

            return [];
        }

        private void ReindexBarisInInputOrder()
        {
            if (isReindexingBaris || InputJurnalDetail == null)
            {
                return;
            }

            isReindexingBaris = true;
            try
            {
                bool hasBarisChanges = false;
                for (int i = 0; i < InputJurnalDetail.Count; i++)
                {
                    int expectedBaris = i + 1;
                    if (InputJurnalDetail[i].BARIS != expectedBaris)
                    {
                        InputJurnalDetail[i].BARIS = expectedBaris;
                        hasBarisChanges = true;
                    }
                }

                if (hasBarisChanges)
                {
                    JDgridView.RefreshData();
                }
            }
            finally
            {
                isReindexingBaris = false;
            }

        }

        private static void DisableUserSorting(GridView gridView)
        {
            if (gridView == null)
            {
                return;
            }

            gridView.OptionsCustomization.AllowSort = false;
            foreach (GridColumn column in gridView.Columns)
            {
                column.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            }
        }

        private void ApplyExportGridSortPolicy()
        {
            DisableUserSorting(GVHeader);
            DisableUserSorting(GVDetail);
            DisableUserSorting(gridView1);
            DisableUserSorting(gridView_KasirHeader);
            DisableUserSorting(gridView_inv_header);
        }


    }
}
