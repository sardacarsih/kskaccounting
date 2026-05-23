using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accounting.BusinessLayer;
using Accounting.FixedAssets.Application.Models;
using DevExpress.XtraEditors;
using WinFormsComboBox = System.Windows.Forms.ComboBox;

namespace Accounting.Form;

public sealed class FrmFixedAssetApproval : XtraForm
{
    private sealed class StatusFilterOption
    {
        public required string Value { get; init; }
        public required string Text { get; init; }
        public override string ToString() => Text;
    }

    private static string CurrentIdData => CompanyInfo.IDDATA?.Trim() ?? string.Empty;

    private readonly TextBox txtIdData = new();
    private readonly TextBox txtPeriodFrom = new();
    private readonly TextBox txtPeriodTo = new();
    private readonly TextBox txtTransactionId = new();
    private readonly TextBox txtRoleCode = new();
    private readonly TextBox txtComment = new();
    private readonly WinFormsComboBox cmbStatus = new();
    private readonly Label lblInfo = new();
    private readonly DataGridView gridInbox = new();
    private readonly Button btnApprove = new();
    private readonly Button btnReject = new();
    private readonly Button btnPost = new();
    private readonly Button btnReverse = new();
    private bool _isLoading;

    public FrmFixedAssetApproval()
    {
        Text = "Aset Tetap - Persetujuan";
        Width = 1360;
        Height = 780;
        StartPosition = FormStartPosition.CenterScreen;

        BuildUi();
        txtIdData.Text = CurrentIdData;
        string current = $"{DateTime.Now.Month:00}/{DateTime.Now.Year:0000}";
        txtPeriodFrom.Text = current;
        txtPeriodTo.Text = current;
        txtRoleCode.Text = "APPROVER";
        cmbStatus.SelectedIndex = 0;
        ApplyRoleAccess();
        _ = LoadWorklistAsync();
    }

    private void BuildUi()
    {
        Panel top = new() { Dock = DockStyle.Top, Height = 220 };
        Label lblIdData = new() { Text = "Lokasi Data", Left = 16, Top = 20, Width = 120 };
        Label lblPeriodFrom = new() { Text = "Periode Dari", Left = 16, Top = 54, Width = 120 };
        Label lblPeriodTo = new() { Text = "Periode Sampai", Left = 16, Top = 88, Width = 120 };
        Label lblStatus = new() { Text = "Status", Left = 16, Top = 122, Width = 120 };
        Label lblTrx = new() { Text = "ID Transaksi", Left = 420, Top = 20, Width = 120 };
        Label lblRole = new() { Text = "Kode Peran", Left = 420, Top = 54, Width = 120 };
        Label lblComment = new() { Text = "Catatan", Left = 420, Top = 88, Width = 120 };

        txtIdData.SetBounds(140, 16, 220, 24);
        txtIdData.ReadOnly = true;
        txtIdData.TabStop = false;
        txtPeriodFrom.SetBounds(140, 50, 220, 24);
        txtPeriodTo.SetBounds(140, 84, 220, 24);
        cmbStatus.SetBounds(140, 118, 220, 24);
        cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbStatus.Items.AddRange(
        [
            new StatusFilterOption { Value = "ALL", Text = "Semua" },
            new StatusFilterOption { Value = "SUBMITTED", Text = "Menunggu Persetujuan" },
            new StatusFilterOption { Value = "APPROVED", Text = "Disetujui" },
            new StatusFilterOption { Value = "POSTED", Text = "Terposting" },
            new StatusFilterOption { Value = "REVERSED", Text = "Dibalik" },
            new StatusFilterOption { Value = "REJECTED", Text = "Ditolak" }
        ]);

        txtTransactionId.SetBounds(544, 16, 220, 24);
        txtRoleCode.SetBounds(544, 50, 220, 24);
        txtComment.SetBounds(544, 84, 760, 24);

        Button btnRefresh = new() { Text = "Muat Ulang", Left = 140, Top = 156, Width = 120, Height = 30 };
        btnRefresh.Click += async (_, _) => await LoadWorklistAsync().ConfigureAwait(true);

        btnApprove.Text = "Setujui";
        btnApprove.SetBounds(544, 122, 100, 30);
        btnApprove.Click += async (_, _) => await ApproveAsync().ConfigureAwait(true);

        btnReject.Text = "Tolak";
        btnReject.SetBounds(652, 122, 100, 30);
        btnReject.Click += async (_, _) => await RejectAsync().ConfigureAwait(true);

        btnPost.Text = "Posting";
        btnPost.SetBounds(760, 122, 100, 30);
        btnPost.Click += async (_, _) => await PostAsync().ConfigureAwait(true);

        btnReverse.Text = "Balik";
        btnReverse.SetBounds(868, 122, 100, 30);
        btnReverse.Click += async (_, _) => await ReverseAsync().ConfigureAwait(true);

        lblInfo.SetBounds(140, 190, 1160, 24);
        lblInfo.Text = "Siap.";

        top.Controls.AddRange([
            lblIdData, lblPeriodFrom, lblPeriodTo, lblStatus, lblTrx, lblRole, lblComment,
            txtIdData, txtPeriodFrom, txtPeriodTo, cmbStatus, txtTransactionId, txtRoleCode, txtComment,
            btnRefresh, btnApprove, btnReject, btnPost, btnReverse, lblInfo
        ]);

        ConfigureGrid(gridInbox);
        gridInbox.SelectionChanged += (_, _) => BindSelectedWorklistRow();

        Controls.Add(gridInbox);
        Controls.Add(top);
    }

    private void ApplyRoleAccess()
    {
        bool canApproval = FixedAssetUiRoleHelper.CanApprovalAction();
        bool canPostReverse = FixedAssetUiRoleHelper.CanPostReverse();

        txtRoleCode.ReadOnly = !canApproval;
        btnApprove.Enabled = canApproval;
        btnReject.Enabled = canApproval;
        btnPost.Enabled = canPostReverse;
        btnReverse.Enabled = canPostReverse;
    }

    private async Task LoadWorklistAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        try
        {
            string idData = CurrentIdData;
            string periodFrom = txtPeriodFrom.Text.Trim();
            string periodTo = txtPeriodTo.Text.Trim();
            string status = (cmbStatus.SelectedItem as StatusFilterOption)?.Value ?? "ALL";

            DataTable table = await Task.Run(() => FixedAssetQueryServices.GetApprovalWorklist(idData, periodFrom, periodTo, status)).ConfigureAwait(true);
            gridInbox.DataSource = table;
            lblInfo.Text = $"Berhasil memuat {table.Rows.Count} transaksi.";
            if (gridInbox.Rows.Count > 0)
            {
                gridInbox.ClearSelection();
                gridInbox.Rows[0].Selected = true;
                if (gridInbox.Columns.Count > 0)
                {
                    gridInbox.CurrentCell = gridInbox.Rows[0].Cells[0];
                }
                BindSelectedWorklistRow();
            }
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal memuat daftar kerja.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Daftar Persetujuan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void BindSelectedWorklistRow()
    {
        if (gridInbox.CurrentRow?.DataBoundItem is not DataRowView row)
        {
            return;
        }

        txtTransactionId.Text = row["TRX_ID"]?.ToString() ?? string.Empty;
        txtComment.Text = row["REMARKS"]?.ToString() ?? string.Empty;
        UpdateActionButtonsByStatus(row["STATUS"]?.ToString() ?? string.Empty);
    }

    private void UpdateActionButtonsByStatus(string statusRaw)
    {
        string status = statusRaw.Trim().ToUpperInvariant();
        bool canApproval = FixedAssetUiRoleHelper.CanApprovalAction();
        bool canPostReverse = FixedAssetUiRoleHelper.CanPostReverse();

        btnApprove.Enabled = canApproval && status == "SUBMITTED";
        btnReject.Enabled = canApproval && status == "SUBMITTED";
        btnPost.Enabled = canPostReverse && status == "APPROVED";
        btnReverse.Enabled = canPostReverse && status == "POSTED";
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

    private async Task ApproveAsync()
    {
        try
        {
            EnsureApprovalRole();
            ApprovalActionRequest req = BuildApprovalRequest();
            await FixedAssetLifecycleServices.ApproveAsync(req).ConfigureAwait(true);
            lblInfo.Text = $"Transaksi {req.TransactionId} berhasil disetujui.";
            XtraMessageBox.Show("Persetujuan berhasil.", "Aset Tetap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadWorklistAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal menyetujui.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Persetujuan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RejectAsync()
    {
        try
        {
            EnsureApprovalRole();
            ApprovalActionRequest req = BuildApprovalRequest();
            await FixedAssetLifecycleServices.RejectAsync(req).ConfigureAwait(true);
            lblInfo.Text = $"Transaksi {req.TransactionId} berhasil ditolak.";
            XtraMessageBox.Show("Penolakan berhasil.", "Aset Tetap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadWorklistAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal menolak.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Penolakan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task PostAsync()
    {
        try
        {
            EnsurePostReverseRole();
            LifecyclePostingActionRequest req = BuildPostingRequest();
            LifecyclePostingActionResult result = await FixedAssetLifecycleServices.PostApprovedAsync(req).ConfigureAwait(true);
            lblInfo.Text = $"Transaksi {result.TransactionId} berhasil diposting. NoJurnal={result.NoJurnal}";
            XtraMessageBox.Show($"Posting berhasil.\nNo Jurnal: {result.NoJurnal}", "Aset Tetap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadWorklistAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal posting.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Posting", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ReverseAsync()
    {
        try
        {
            EnsurePostReverseRole();
            LifecyclePostingActionRequest req = BuildPostingRequest();
            DialogResult confirm = XtraMessageBox.Show(
                $"Balik transaksi terposting {req.TransactionId}?",
                "Konfirmasi Balik",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            await FixedAssetLifecycleServices.ReversePostedAsync(req).ConfigureAwait(true);
            lblInfo.Text = $"Transaksi {req.TransactionId} berhasil dibalik.";
            XtraMessageBox.Show("Proses balik berhasil.", "Aset Tetap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await LoadWorklistAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal proses balik.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Balik", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void EnsureApprovalRole()
    {
        if (!FixedAssetUiRoleHelper.CanApprovalAction())
        {
            throw new InvalidOperationException("Peran Anda tidak memiliki izin persetujuan.");
        }
    }

    private static void EnsurePostReverseRole()
    {
        if (!FixedAssetUiRoleHelper.CanPostReverse())
        {
            throw new InvalidOperationException("Peran Anda tidak memiliki izin posting/balik.");
        }
    }

    private ApprovalActionRequest BuildApprovalRequest()
    {
        if (!long.TryParse(txtTransactionId.Text.Trim(), out long trxId) || trxId <= 0)
        {
            throw new InvalidOperationException("ID transaksi tidak valid.");
        }

        return new ApprovalActionRequest
        {
            IdData = CurrentIdData,
            TransactionId = trxId,
            UserId = LoginInfo.userID,
            RoleCode = txtRoleCode.Text.Trim(),
            Comment = txtComment.Text.Trim()
        };
    }

    private LifecyclePostingActionRequest BuildPostingRequest()
    {
        if (!long.TryParse(txtTransactionId.Text.Trim(), out long trxId) || trxId <= 0)
        {
            throw new InvalidOperationException("ID transaksi tidak valid.");
        }

        return new LifecyclePostingActionRequest
        {
            IdData = CurrentIdData,
            TransactionId = trxId,
            UserId = LoginInfo.userID,
            Comment = txtComment.Text.Trim()
        };
    }
}
