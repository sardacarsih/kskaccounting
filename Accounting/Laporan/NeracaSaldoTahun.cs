using Accounting.Model;
using DevExpress.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace Accounting.Laporan
{
    public partial class NeracaSaldoTahun : DevExpress.XtraReports.UI.XtraReport
    {
        private const float ReportWidth = 1454F;
        private const string AccountingNumberFormat = "{0:#,##0.00;(#,##0.00);-}";

        private readonly XRLabel companyLabel = new();
        private readonly XRLabel regionLabel = new();
        private readonly XRLabel periodLabel = new();
        private readonly XRLabel userLabel = new();

        public NeracaSaldoTahun()
        {
            InitializeComponent();
            ConfigureLayout();
            ConfigureBindings();
            Detail.BeforePrint += Detail_BeforePrint;
        }

        public void BindData(
            IReadOnlyList<NeracaSaldoRow> rows,
            NeracaSaldoReportMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(rows);
            ArgumentNullException.ThrowIfNull(metadata);

            DataSource = rows;
            companyLabel.Text = metadata.CompanyName;
            regionLabel.Text = metadata.Region;
            periodLabel.Text = $"Tahun {metadata.Year}";
            userLabel.Text = $"User: {metadata.UserId} | Dibuat: {metadata.GeneratedAt:dd-MM-yyyy HH:mm}";
            RequestParameters = false;
        }

        private void ConfigureLayout()
        {
            Landscape = true;
            PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A3;
            PageWidth = 1654;
            PageHeight = 1169;
            Margins = new System.Drawing.Printing.Margins(100, 100, 75, 75);

            tableRow1.Cells.Remove(tableCell3);
            tableRow2.Cells.Remove(tableCell19);
            table1.SizeF = new SizeF(ReportWidth, 28F);
            table2.SizeF = new SizeF(ReportWidth, 25F);
            GroupHeader1.RepeatEveryPage = true;

            ReportHeader.HeightF = 112F;
            label1.Text = "NERACA SALDO TAHUNAN";
            label1.SizeF = new SizeF(ReportWidth, 26F);
            label1.TextAlignment = TextAlignment.MiddleCenter;

            ConfigureHeaderLabel(companyLabel, 30F, 20F, DXFontStyle.Bold);
            ConfigureHeaderLabel(regionLabel, 50F, 18F, DXFontStyle.Regular);
            ConfigureHeaderLabel(periodLabel, 69F, 18F, DXFontStyle.Bold);
            ConfigureHeaderLabel(userLabel, 88F, 18F, DXFontStyle.Regular);
            ReportHeader.Controls.AddRange([companyLabel, regionLabel, periodLabel, userLabel]);

            foreach (XRTableCell cell in tableRow1.Cells)
            {
                cell.TextAlignment = TextAlignment.MiddleCenter;
            }

            tableCell1.Text = "Kode Akun";
            tableCell2.Text = "Nama Akun";
            tableCell4.Text = "Saldo Awal";
            tableCell5.Text = "Jan";
            tableCell6.Text = "Feb";
            tableCell7.Text = "Mar";
            tableCell8.Text = "Apr";
            tableCell9.Text = "Mei";
            tableCell10.Text = "Jun";
            tableCell11.Text = "Jul";
            tableCell12.Text = "Agu";
            tableCell13.Text = "Sep";
            tableCell14.Text = "Okt";
            tableCell15.Text = "Nov";
            tableCell16.Text = "Des";
        }

        private void ConfigureBindings()
        {
            SetTextBinding(tableCell17, nameof(NeracaSaldoRow.KodeAkun));
            SetTextBinding(tableCell18, nameof(NeracaSaldoRow.NamaAkun));
            SetNumericBinding(tableCell20, nameof(NeracaSaldoRow.SaldoAwal));
            SetNumericBinding(tableCell21, nameof(NeracaSaldoRow.Januari));
            SetNumericBinding(tableCell22, nameof(NeracaSaldoRow.Februari));
            SetNumericBinding(tableCell23, nameof(NeracaSaldoRow.Maret));
            SetNumericBinding(tableCell24, nameof(NeracaSaldoRow.April));
            SetNumericBinding(tableCell25, nameof(NeracaSaldoRow.Mei));
            SetNumericBinding(tableCell26, nameof(NeracaSaldoRow.Juni));
            SetNumericBinding(tableCell27, nameof(NeracaSaldoRow.Juli));
            SetNumericBinding(tableCell28, nameof(NeracaSaldoRow.Agustus));
            SetNumericBinding(tableCell29, nameof(NeracaSaldoRow.September));
            SetNumericBinding(tableCell30, nameof(NeracaSaldoRow.Oktober));
            SetNumericBinding(tableCell31, nameof(NeracaSaldoRow.November));
            SetNumericBinding(tableCell32, nameof(NeracaSaldoRow.Desember));
        }

        private void Detail_BeforePrint(object sender, CancelEventArgs e)
        {
            if (GetCurrentRow() is not NeracaSaldoRow row)
            {
                return;
            }

            DXFontStyle fontStyle = row.IsHeader ? DXFontStyle.Bold : DXFontStyle.Regular;
            Color backgroundColor = row.IsHeader ? Color.FromArgb(221, 235, 247) : Color.Transparent;
            foreach (XRTableCell cell in tableRow2.Cells)
            {
                cell.Font = new DXFont("Arial", 7.5F, fontStyle);
                cell.BackColor = backgroundColor;
            }

            int indent = Math.Max(0, row.Level - 1) * 10;
            tableCell18.Padding = new PaddingInfo(6 + indent, 6, 0, 0, 100F);
            SetValueColor(tableCell20, row.SaldoAwal);
            SetValueColor(tableCell21, row.Januari);
            SetValueColor(tableCell22, row.Februari);
            SetValueColor(tableCell23, row.Maret);
            SetValueColor(tableCell24, row.April);
            SetValueColor(tableCell25, row.Mei);
            SetValueColor(tableCell26, row.Juni);
            SetValueColor(tableCell27, row.Juli);
            SetValueColor(tableCell28, row.Agustus);
            SetValueColor(tableCell29, row.September);
            SetValueColor(tableCell30, row.Oktober);
            SetValueColor(tableCell31, row.November);
            SetValueColor(tableCell32, row.Desember);
        }

        private static void SetTextBinding(XRTableCell cell, string propertyName)
        {
            cell.ExpressionBindings.Clear();
            cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", $"[{propertyName}]"));
        }

        private static void SetNumericBinding(XRTableCell cell, string propertyName)
        {
            SetTextBinding(cell, propertyName);
            cell.TextFormatString = AccountingNumberFormat;
            cell.TextAlignment = TextAlignment.MiddleRight;
        }

        private static void SetValueColor(XRTableCell cell, decimal value)
        {
            cell.ForeColor = value < 0m ? Color.Red : Color.Black;
        }

        private static void ConfigureHeaderLabel(
            XRLabel label,
            float top,
            float height,
            DXFontStyle fontStyle)
        {
            label.LocationFloat = new DevExpress.Utils.PointFloat(0F, top);
            label.SizeF = new SizeF(ReportWidth, height);
            label.Font = new DXFont("Arial", 9F, fontStyle);
            label.TextAlignment = TextAlignment.MiddleCenter;
        }
    }
}
