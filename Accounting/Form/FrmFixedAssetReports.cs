using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accounting.BusinessLayer;
using Accounting.FixedAssets.Application.Models;
using DevExpress.XtraEditors;
using OfficeOpenXml;

namespace Accounting.Form;

public sealed class FrmFixedAssetReports : XtraForm
{
    private static string CurrentIdData => CompanyInfo.IDDATA?.Trim() ?? string.Empty;

    private readonly TextBox txtIdData = new();
    private readonly TextBox txtPeriodFrom = new();
    private readonly TextBox txtPeriodTo = new();
    private readonly Label lblInfo = new();

    private readonly DataGridView gridRegister = new();
    private readonly DataGridView gridDepExpense = new();
    private readonly DataGridView gridMovement = new();
    private readonly DataGridView gridDisposal = new();
    private readonly DataGridView gridRevaluation = new();
    private readonly DataGridView gridSummaryCategory = new();
    private readonly DataGridView gridFullyDepreciated = new();
    private readonly DataGridView gridIdle = new();
    private readonly TabControl tabReports = new();
    private readonly Button btnPost = new();
    private readonly Button btnReverse = new();
    private readonly TabPage tabRegisterPage = new("Register Aset");
    private readonly TabPage tabDepPage = new("Beban Penyusutan");
    private readonly TabPage tabMovePage = new("Mutasi Aset");
    private readonly TabPage tabDisposalPage = new("Penghapusan/Penjualan");
    private readonly TabPage tabRevaluationPage = new("Revaluasi");
    private readonly TabPage tabSummaryPage = new("Ringkasan per Kategori");
    private readonly TabPage tabFullyPage = new("Aset Disusutkan Penuh");
    private readonly TabPage tabIdlePage = new("Aset Idle/Tidak Aktif");

    public FrmFixedAssetReports()
    {
        Text = "Aset Tetap - Laporan";
        Width = 1440;
        Height = 840;
        StartPosition = FormStartPosition.CenterScreen;

        BuildUi();
        txtIdData.Text = CurrentIdData;
        string current = $"{DateTime.Now.Month:00}/{DateTime.Now.Year:0000}";
        txtPeriodFrom.Text = current;
        txtPeriodTo.Text = current;
        _ = LoadReportsAsync();
    }

    private void BuildUi()
    {
        Panel top = new() { Dock = DockStyle.Top, Height = 100 };
        Label lblIdData = new() { Text = "Lokasi Data", Left = 12, Top = 14, Width = 100 };
        Label lblPeriodFrom = new() { Text = "Periode Dari", Left = 12, Top = 44, Width = 100 };
        Label lblPeriodTo = new() { Text = "Periode Sampai", Left = 340, Top = 44, Width = 100 };

        txtIdData.SetBounds(120, 10, 200, 24);
        txtIdData.ReadOnly = true;
        txtIdData.TabStop = false;
        txtPeriodFrom.SetBounds(120, 40, 200, 24);
        txtPeriodTo.SetBounds(450, 40, 200, 24);

        Button btnGenerate = new() { Text = "Proses", Left = 680, Top = 10, Width = 120, Height = 25 };
        btnGenerate.Click += async (_, _) => await LoadReportsAsync();
        Button btnRefresh = new() { Text = "Muat Ulang", Left = 808, Top = 10, Width = 120, Height = 25 };
        btnRefresh.Click += async (_, _) => await LoadReportsAsync();
        Button btnExport = new() { Text = "Ekspor CSV", Left = 936, Top = 10, Width = 120, Height = 25 };
        btnExport.Click += (_, _) => ExportActiveTabToCsv();
        Button btnExportXlsx = new() { Text = "Ekspor XLSX", Left = 1064, Top = 10, Width = 120, Height = 25 };
        btnExportXlsx.Click += (_, _) => ExportActiveTabToXlsx();

        btnPost.Text = "Posting";
        btnPost.SetBounds(1192, 10, 80, 25);
        btnPost.Click += async (_, _) => await PostSelectedTransactionAsync().ConfigureAwait(true);
        btnPost.Enabled = FixedAssetUiRoleHelper.CanPostReverse();

        btnReverse.Text = "Balik";
        btnReverse.SetBounds(1280, 10, 80, 25);
        btnReverse.Click += async (_, _) => await ReverseSelectedTransactionAsync().ConfigureAwait(true);
        btnReverse.Enabled = FixedAssetUiRoleHelper.CanPostReverse();

        lblInfo.SetBounds(680, 44, 560, 40);
        lblInfo.Text = "Siap.";

        top.Controls.AddRange(new Control[] { lblIdData, lblPeriodFrom, lblPeriodTo, txtIdData, txtPeriodFrom, txtPeriodTo, btnGenerate, btnRefresh, btnExport, btnExportXlsx, btnPost, btnReverse, lblInfo });

        tabReports.Dock = DockStyle.Fill;

        ConfigureGrid(gridRegister);
        ConfigureGrid(gridDepExpense);
        ConfigureGrid(gridMovement);
        ConfigureGrid(gridDisposal);
        ConfigureGrid(gridRevaluation);
        ConfigureGrid(gridSummaryCategory);
        ConfigureGrid(gridFullyDepreciated);
        ConfigureGrid(gridIdle);

        tabRegisterPage.Tag = gridRegister;
        tabDepPage.Tag = gridDepExpense;
        tabMovePage.Tag = gridMovement;
        tabDisposalPage.Tag = gridDisposal;
        tabRevaluationPage.Tag = gridRevaluation;
        tabSummaryPage.Tag = gridSummaryCategory;
        tabFullyPage.Tag = gridFullyDepreciated;
        tabIdlePage.Tag = gridIdle;

        tabRegisterPage.Controls.Add(gridRegister);
        tabDepPage.Controls.Add(gridDepExpense);
        tabMovePage.Controls.Add(gridMovement);
        tabDisposalPage.Controls.Add(gridDisposal);
        tabRevaluationPage.Controls.Add(gridRevaluation);
        tabSummaryPage.Controls.Add(gridSummaryCategory);
        tabFullyPage.Controls.Add(gridFullyDepreciated);
        tabIdlePage.Controls.Add(gridIdle);

        tabReports.TabPages.Add(tabRegisterPage);
        tabReports.TabPages.Add(tabDepPage);
        tabReports.TabPages.Add(tabMovePage);
        tabReports.TabPages.Add(tabDisposalPage);
        tabReports.TabPages.Add(tabRevaluationPage);
        tabReports.TabPages.Add(tabSummaryPage);
        tabReports.TabPages.Add(tabFullyPage);
        tabReports.TabPages.Add(tabIdlePage);

        Controls.Add(tabReports);
        Controls.Add(top);
    }

    private async Task LoadReportsAsync()
    {
        try
        {
            string idData = CurrentIdData;
            string periodFrom = txtPeriodFrom.Text.Trim();
            string periodTo = txtPeriodTo.Text.Trim();

            Task<DataTable> t1 = Task.Run(() => FixedAssetQueryServices.GetAssetRegisterReport(idData));
            Task<DataTable> t2 = Task.Run(() => FixedAssetQueryServices.GetDepreciationExpenseReport(idData, periodFrom, periodTo));
            Task<DataTable> t3 = Task.Run(() => FixedAssetQueryServices.GetMovementReport(idData, periodFrom, periodTo));
            Task<DataTable> t4 = Task.Run(() => FixedAssetQueryServices.GetDisposalReport(idData, periodFrom, periodTo));
            Task<DataTable> t5 = Task.Run(() => FixedAssetQueryServices.GetRevaluationReport(idData, periodFrom, periodTo));
            Task<DataTable> t6 = Task.Run(() => FixedAssetQueryServices.GetSummaryByCategoryReport(idData));
            Task<DataTable> t7 = Task.Run(() => FixedAssetQueryServices.GetFullyDepreciatedAssetsReport(idData));
            Task<DataTable> t8 = Task.Run(() => FixedAssetQueryServices.GetIdleAssetsReport(idData));

            await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8).ConfigureAwait(true);

            gridRegister.DataSource = t1.Result;
            gridDepExpense.DataSource = t2.Result;
            gridMovement.DataSource = t3.Result;
            gridDisposal.DataSource = t4.Result;
            gridRevaluation.DataSource = t5.Result;
            gridSummaryCategory.DataSource = t6.Result;
            gridFullyDepreciated.DataSource = t7.Result;
            gridIdle.DataSource = t8.Result;

            lblInfo.Text =
                $"Berhasil memuat. Register={t1.Result.Rows.Count}, Penyusutan={t2.Result.Rows.Count}, Mutasi={t3.Result.Rows.Count}, " +
                $"Penghapusan/Penjualan={t4.Result.Rows.Count}, Revaluasi={t5.Result.Rows.Count}.";
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Aset Tetap - Laporan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AutoGenerateColumns = true;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        grid.DataBindingComplete += (_, _) => FixedAssetGridLocalization.Apply(grid);
    }

    private void ExportActiveTabToCsv()
    {
        try
        {
            DataGridView? activeGrid = GetActiveGrid();
            if (activeGrid?.DataSource is not DataTable table || table.Rows.Count == 0)
            {
                throw new InvalidOperationException("Tidak ada data untuk diexport pada tab aktif.");
            }

            string tabName = tabReports.SelectedTab?.Text?.Replace("/", "-") ?? "LaporanAsetTetap";
            string suggestedName = $"{tabName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            using SaveFileDialog dialog = new()
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = suggestedName
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            WriteCsv(table, dialog.FileName);
            lblInfo.Text = $"Berhasil mengekspor {table.Rows.Count} baris ke {Path.GetFileName(dialog.FileName)}.";
            XtraMessageBox.Show("Ekspor CSV berhasil.", "Aset Tetap - Laporan", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Kesalahan Ekspor CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private DataGridView? GetActiveGrid()
    {
        return tabReports.SelectedTab?.Tag as DataGridView;
    }

    private async Task PostSelectedTransactionAsync()
    {
        try
        {
            if (!FixedAssetUiRoleHelper.CanPostReverse())
            {
                throw new InvalidOperationException("Role Anda tidak memiliki izin post.");
            }

            if (!TryGetSelectedLifecycleRow(out long trxId, out string status))
            {
                throw new InvalidOperationException("Pilih baris transaksi siklus pada tab Penghapusan/Penjualan atau Revaluasi.");
            }

            if (!string.Equals(status, "APPROVED", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Status transaksi {status} tidak dapat diposting.");
            }

            LifecyclePostingActionResult result = await FixedAssetLifecycleServices.PostApprovedAsync(new LifecyclePostingActionRequest
            {
                IdData = CurrentIdData,
                TransactionId = trxId,
                UserId = LoginInfo.userID,
                Comment = $"Posting manual dari laporan oleh {LoginInfo.userID}"
            }).ConfigureAwait(true);

            lblInfo.Text = $"Posting transaksi {trxId} berhasil. NoJurnal={result.NoJurnal}";
            await LoadReportsAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Kesalahan Posting", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ReverseSelectedTransactionAsync()
    {
        try
        {
            if (!FixedAssetUiRoleHelper.CanPostReverse())
            {
                throw new InvalidOperationException("Role Anda tidak memiliki izin reverse.");
            }

            if (!TryGetSelectedLifecycleRow(out long trxId, out string status))
            {
                throw new InvalidOperationException("Pilih baris transaksi siklus pada tab Penghapusan/Penjualan atau Revaluasi.");
            }

            if (!string.Equals(status, "POSTED", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Status transaksi {status} tidak dapat dibalik.");
            }

            DialogResult confirm = XtraMessageBox.Show(
                $"Balik transaksi {trxId}?",
                "Konfirmasi Balik",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            await FixedAssetLifecycleServices.ReversePostedAsync(new LifecyclePostingActionRequest
            {
                IdData = CurrentIdData,
                TransactionId = trxId,
                UserId = LoginInfo.userID,
                Comment = $"Balik manual dari laporan oleh {LoginInfo.userID}"
            }).ConfigureAwait(true);

            lblInfo.Text = $"Balik transaksi {trxId} berhasil.";
            await LoadReportsAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Kesalahan Balik", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool TryGetSelectedLifecycleRow(out long trxId, out string status)
    {
        trxId = 0;
        status = string.Empty;

        DataGridView? activeGrid = GetActiveGrid();
        if (activeGrid is null || activeGrid.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            return false;
        }

        if (!row.Row.Table.Columns.Contains("TRX_ID") || !row.Row.Table.Columns.Contains("STATUS"))
        {
            return false;
        }

        if (!long.TryParse(row["TRX_ID"]?.ToString(), out trxId) || trxId <= 0)
        {
            return false;
        }

        status = row["STATUS"]?.ToString() ?? string.Empty;
        return true;
    }

    private static void WriteCsv(DataTable table, string filePath)
    {
        static string Escape(string? value)
        {
            string text = value ?? string.Empty;
            if (text.Contains('"'))
            {
                text = text.Replace("\"", "\"\"");
            }

            return text.IndexOfAny([',', '"', '\n', '\r']) >= 0
                ? $"\"{text}\""
                : text;
        }

        using StreamWriter writer = new(filePath, false, new UTF8Encoding(true));
        for (int i = 0; i < table.Columns.Count; i++)
        {
            if (i > 0)
            {
                writer.Write(",");
            }

            writer.Write(Escape(table.Columns[i].ColumnName));
        }

        writer.WriteLine();

        foreach (DataRow row in table.Rows)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0)
                {
                    writer.Write(",");
                }

                string value = row[i] == DBNull.Value ? string.Empty : Convert.ToString(row[i]) ?? string.Empty;
                writer.Write(Escape(value));
            }

            writer.WriteLine();
        }
    }

    private void ExportActiveTabToXlsx()
    {
        try
        {
            DataGridView? activeGrid = GetActiveGrid();
            if (activeGrid?.DataSource is not DataTable table || table.Rows.Count == 0)
            {
                throw new InvalidOperationException("Tidak ada data untuk diexport pada tab aktif.");
            }

            string tabName = tabReports.SelectedTab?.Text?.Replace("/", "-") ?? "LaporanAsetTetap";
            string suggestedName = $"{tabName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            using SaveFileDialog dialog = new()
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = suggestedName
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet ws = package.Workbook.Worksheets.Add("Report");

            for (int col = 0; col < table.Columns.Count; col++)
            {
                ws.Cells[1, col + 1].Value = table.Columns[col].ColumnName;
                ws.Cells[1, col + 1].Style.Font.Bold = true;
            }

            for (int row = 0; row < table.Rows.Count; row++)
            {
                for (int col = 0; col < table.Columns.Count; col++)
                {
                    ws.Cells[row + 2, col + 1].Value = table.Rows[row][col] == DBNull.Value ? null : table.Rows[row][col];
                }
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            package.SaveAs(new FileInfo(dialog.FileName));

            lblInfo.Text = $"Berhasil mengekspor {table.Rows.Count} baris ke {Path.GetFileName(dialog.FileName)}.";
            XtraMessageBox.Show("Ekspor XLSX berhasil.", "Aset Tetap - Laporan", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Kesalahan Ekspor XLSX", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
