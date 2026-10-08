using Accounting.BusinessLayer;
using Accounting.Laporan;
using Accounting.Model;
using DevExpress.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Windows.Forms;

namespace Accounting.Form
{
    internal sealed class FrmCoaDrillDown : XtraForm
    {
        private readonly int bulan;
        private readonly int tahun;
        private readonly string idData;
        private readonly string userId;
        private readonly GridControl gridControl;
        private readonly GridView gridView;

        public FrmCoaDrillDown(DataTable source, string title, int bulan, int tahun, string idData, string userId)
        {
            ArgumentNullException.ThrowIfNull(source);
            CoaDrillDownRow.EnsureRequiredColumns(source);
            this.bulan = bulan;
            this.tahun = tahun;
            this.idData = idData;
            this.userId = userId;

            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            Width = 1100;
            Height = 650;
            MinimizeBox = false;

            gridControl = new GridControl
            {
                Dock = DockStyle.Fill
            };
            gridView = new GridView(gridControl)
            {
                OptionsBehavior = { Editable = false },
                OptionsView = { ShowGroupPanel = false, ShowFooter = true }
            };
            gridControl.MainView = gridView;
            gridControl.DataSource = source;
            Controls.Add(gridControl);

            ConfigureColumns();
            gridView.DoubleClick += GridView_DoubleClick;
        }

        private void ConfigureColumns()
        {
            ConfigureColumn("KODEACC", "Account", 140);
            ConfigureColumn("NAMAACC", "Nama Perkiraan", 310);
            ConfigureColumn("PARENTACC", "Induk", 140);
            ConfigureColumn("POSISI", "Saldo", 55);
            ConfigureColumn("ISHEADER", "Gen", 50);
            ConfigureAmountColumn("DEBET", "Debet");
            ConfigureAmountColumn("KREDIT", "Kredit");
            ConfigureAmountColumn("SALDOAKHIR", "Saldo Akhir");
        }

        private GridColumn ConfigureColumn(string fieldName, string caption, int width)
        {
            GridColumn? column = gridView.Columns.ColumnByFieldName(fieldName);
            if (column == null)
            {
                // Do not depend on PopulateColumns here. The DevExpress view can still be
                // uninitialized when this dialog is constructed, even though the validated
                // DataTable already contains the required contract fields.
                column = new GridColumn
                {
                    FieldName = fieldName
                };
                gridView.Columns.Add(column);
            }

            column.Caption = caption;
            column.Visible = true;
            column.VisibleIndex = gridView.VisibleColumns.Count;
            column.Width = width;
            return column;
        }

        private void ConfigureAmountColumn(string fieldName, string caption)
        {
            GridColumn column = ConfigureColumn(fieldName, caption, 145);
            column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            column.DisplayFormat.FormatString = "n2";
            column.Summary.Add(SummaryItemType.Sum, fieldName, "{0:n2}");
        }

        private void GridView_DoubleClick(object sender, EventArgs e)
        {
            DataRow row = gridView.GetDataRow(gridView.FocusedRowHandle);
            if (row == null)
            {
                return;
            }

            CoaDrillDownRow account = CoaDrillDownRow.FromDataRow(row);
            if (CoaDrillDownPolicy.OpensHierarchy(account.IsHeader))
            {
                XtraMessageBox.Show("Pilih akun detail untuk melihat buku besar.", "Detail Transactions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DataSet dataSet = LaporanServices.ViewLap_BukuBesar_Tree(idData, tahun, bulan, bulan, account.KodeAcc);
            if (dataSet.Tables["BukuBesar"]?.Rows.Count == 0)
            {
                XtraMessageBox.Show("Tidak ada transaksi pada periode yang dipilih.", "Detail Transactions", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string[] monthNames = ["Bulan", "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember"];
            XtraReport report = ReportDrillDownPolicy.GetGeneralLedgerSide(account.Posisi) == "D"
                ? new GeneralLedgerD2 { DataSource = dataSet }
                : new GeneralLedgerK2 { DataSource = dataSet };

            report.Parameters["PBULAN"].Value = bulan;
            report.Parameters["PTAHUN"].Value = tahun;
            report.Parameters["BULAN"].Value = $"{monthNames[bulan]}-{tahun}";
            report.Parameters["PERIODE"].Value = $"{bulan:00}/{tahun}";
            report.Parameters["NAMAPT"].Value = CompanyInfo.NAMAPT;
            report.Parameters["WILAYAH"].Value = CompanyInfo.WILAYAH;
            report.Parameters["USERID"].Value = userId;
            report.RequestParameters = true;

            ReportPrintTool tool = new(report);
            tool.ShowPreview();
        }
    }
}
