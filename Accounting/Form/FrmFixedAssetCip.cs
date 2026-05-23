using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accounting.BusinessLayer;
using Accounting.FixedAssets.Domain;
using DevExpress.XtraEditors;

namespace Accounting.Form;

public sealed class FrmFixedAssetCip : XtraForm
{
    private sealed class StatusOption
    {
        public required string Value { get; init; }
        public required string Text { get; init; }
        public override string ToString() => Text;
    }

    private static string CurrentIdData => CompanyInfo.IDDATA?.Trim() ?? string.Empty;

    private readonly TextBox txtIdData = new();
    private readonly TextBox txtSearch = new();
    private readonly System.Windows.Forms.ComboBox cmbStatus = new();
    private readonly Label lblInfo = new();
    private readonly DataGridView gridSummary = new();
    private readonly DataGridView gridCost = new();
    private readonly DataGridView gridCapitalization = new();
    private readonly Label lblSelected = new();
    private bool _isLoading;

    public FrmFixedAssetCip()
    {
        Text = "Aset Tetap - Konstruksi Dalam Pengerjaan";
        Width = 1360;
        Height = 780;
        StartPosition = FormStartPosition.CenterScreen;

        BuildUi();
        txtIdData.Text = CurrentIdData;
        cmbStatus.SelectedIndex = 0;
        _ = LoadDataAsync();
    }

    private void BuildUi()
    {
        Panel top = new() { Dock = DockStyle.Top, Height = 96 };
        Label lblIdData = new() { Text = "Lokasi Data", Left = 12, Top = 14, Width = 100 };
        Label lblSearch = new() { Text = "Pencarian", Left = 12, Top = 44, Width = 100 };
        Label lblStatus = new() { Text = "Status KDP", Left = 450, Top = 14, Width = 100 };

        txtIdData.SetBounds(120, 10, 300, 24);
        txtIdData.ReadOnly = true;
        txtIdData.TabStop = false;
        txtSearch.SetBounds(120, 40, 300, 24);
        cmbStatus.SetBounds(560, 10, 220, 24);

        cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbStatus.Items.AddRange(
        [
            new StatusOption { Value = "ALL", Text = "Semua Status" },
            new StatusOption { Value = "OPEN", Text = "Terbuka" },
            new StatusOption { Value = "CLOSED", Text = "Ditutup" },
            new StatusOption { Value = "CANCELED", Text = "Dibatalkan" }
        ]);

        Button btnSearch = new() { Text = "Cari", Left = 810, Top = 10, Width = 100, Height = 25 };
        btnSearch.Click += async (_, _) => await LoadDataAsync();
        Button btnRefresh = new() { Text = "Muat Ulang", Left = 918, Top = 10, Width = 100, Height = 25 };
        btnRefresh.Click += async (_, _) => await LoadDataAsync();
        Button btnCapitalize = new() { Text = "Transaksi Kapitalisasi", Left = 1026, Top = 10, Width = 150, Height = 25 };
        btnCapitalize.Click += async (_, _) => await OpenCapitalizationTransactionAsync().ConfigureAwait(true);
        btnCapitalize.Enabled = FixedAssetUiRoleHelper.CanLifecycleCreate();

        lblInfo.SetBounds(450, 44, 700, 40);
        lblInfo.Text = "Siap.";

        top.Controls.AddRange(new Control[] { lblIdData, lblSearch, lblStatus, txtIdData, txtSearch, cmbStatus, btnSearch, btnRefresh, btnCapitalize, lblInfo });

        SplitContainer split = new() { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300 };

        ConfigureGrid(gridSummary);
        gridSummary.SelectionChanged += async (_, _) => await LoadDetailAsync().ConfigureAwait(true);
        split.Panel1.Controls.Add(gridSummary);

        TabControl tab = new() { Dock = DockStyle.Fill };
        TabPage tabCost = new("Biaya KDP");
        TabPage tabCap = new("Riwayat Kapitalisasi");
        ConfigureGrid(gridCost);
        ConfigureGrid(gridCapitalization);
        tabCost.Controls.Add(gridCost);
        tabCap.Controls.Add(gridCapitalization);
        tab.TabPages.Add(tabCost);
        tab.TabPages.Add(tabCap);

        Panel bottom = new() { Dock = DockStyle.Fill };
        lblSelected.Dock = DockStyle.Top;
        lblSelected.Height = 26;
        lblSelected.Text = "Pilih baris KDP untuk melihat detail.";
        bottom.Controls.Add(tab);
        bottom.Controls.Add(lblSelected);
        split.Panel2.Controls.Add(bottom);

        Controls.Add(split);
        Controls.Add(top);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            string idData = CurrentIdData;
            string statusCode = (cmbStatus.SelectedItem as StatusOption)?.Value ?? "ALL";
            string status = statusCode == "ALL" ? string.Empty : statusCode;
            string search = txtSearch.Text.Trim();

            DataTable table = await Task.Run(() => FixedAssetQueryServices.GetCipSummary(idData, search, status)).ConfigureAwait(true);
            gridSummary.DataSource = table;
            lblInfo.Text = $"Berhasil memuat {table.Rows.Count} data KDP.";
            if (table.Rows.Count > 0)
            {
                gridSummary.ClearSelection();
                gridSummary.Rows[0].Selected = true;
                if (gridSummary.Columns.Count > 0)
                {
                    gridSummary.CurrentCell = gridSummary.Rows[0].Cells[0];
                }
                await LoadDetailAsync().ConfigureAwait(true);
            }
            else
            {
                gridCost.DataSource = null;
                gridCapitalization.DataSource = null;
                lblSelected.Text = "Tidak ada data KDP.";
            }
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Aset Tetap - KDP", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadDetailAsync()
    {
        if (_isLoading)
        {
            return;
        }

        if (gridSummary.CurrentRow?.DataBoundItem is not DataRowView rowView)
        {
            return;
        }

        if (!long.TryParse(rowView["CIP_ID"]?.ToString(), out long cipId) || cipId <= 0)
        {
            return;
        }

        _isLoading = true;
        try
        {
            string idData = CurrentIdData;
            Task<DataTable> costTask = Task.Run(() => FixedAssetQueryServices.GetCipCostDetail(idData, cipId));
            Task<DataTable> capTask = Task.Run(() => FixedAssetQueryServices.GetCipCapitalizationDetail(idData, cipId));

            await Task.WhenAll(costTask, capTask).ConfigureAwait(true);
            gridCost.DataSource = costTask.Result;
            gridCapitalization.DataSource = capTask.Result;

            lblSelected.Text = $"KDP: {rowView["CIP_CODE"]} | Proyek: {rowView["PROJECT_NAME"]} | Sisa: {rowView["OUTSTANDING_AMOUNT"]}";
        }
        finally
        {
            _isLoading = false;
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

    private async Task OpenCapitalizationTransactionAsync()
    {
        try
        {
            if (!FixedAssetUiRoleHelper.CanLifecycleCreate())
            {
                throw new InvalidOperationException("Role Anda tidak memiliki izin transaksi kapitalisasi.");
            }

            DataRowView? row = GetSelectedRow();
            if (row is null)
            {
                throw new InvalidOperationException("Pilih data KDP terlebih dahulu.");
            }

            string cipCode = row["CIP_CODE"]?.ToString() ?? string.Empty;
            string sourceRef = string.IsNullOrWhiteSpace(cipCode) ? "CIP" : $"CIP:{cipCode}";

            using FrmFixedAssetLifecycle dialog = new(null, FixedAssetTransactionType.CipCapitalization, sourceRef);
            dialog.ShowDialog(this);
            await LoadDataAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Kapitalisasi KDP", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private DataRowView? GetSelectedRow()
    {
        return gridSummary.CurrentRow?.DataBoundItem as DataRowView;
    }
}
