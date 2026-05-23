using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accounting.BusinessLayer;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Domain;
using DevExpress.XtraEditors;
using WinFormsComboBox = System.Windows.Forms.ComboBox;

namespace Accounting.Form;

public sealed class FrmFixedAssetCenter : XtraForm
{
    private sealed class TransactionTypeOption
    {
        public required FixedAssetTransactionType Value { get; init; }
        public required string Text { get; init; }
        public override string ToString() => Text;
    }

    private static string CurrentIdData => CompanyInfo.IDDATA?.Trim() ?? string.Empty;

    private readonly TextBox txtIdData = new();
    private readonly TextBox txtPeriod = new();
    private readonly TextBox txtRunId = new();
    private readonly Label lblDepInfo = new();
    private readonly DataGridView gridDepPreview = new();

    private readonly WinFormsComboBox cmbTransactionType = new();
    private readonly TextBox txtAssetId = new();
    private readonly TextBox txtAmount = new();
    private readonly TextBox txtOldAmount = new();
    private readonly TextBox txtNewAmount = new();
    private readonly TextBox txtSourceRef = new();
    private readonly TextBox txtRemarks = new();
    private readonly Label lblLifecycleInfo = new();

    private readonly TextBox txtApprovalTrxId = new();
    private readonly TextBox txtApprovalRole = new();
    private readonly TextBox txtApprovalComment = new();
    private readonly Label lblApprovalInfo = new();

    public FrmFixedAssetCenter()
    {
        Text = "Aset Tetap - Pusat Proses";
        Width = 1200;
        Height = 760;
        StartPosition = FormStartPosition.CenterScreen;

        BuildUi();
        txtIdData.Text = CurrentIdData;
        txtPeriod.Text = $"{DateTime.Now.Month:00}/{DateTime.Now.Year:0000}";
        txtApprovalRole.Text = "APPROVER";
    }

    private void BuildUi()
    {
        TabControl tabControl = new() { Dock = DockStyle.Fill };
        TabPage tabDep = new("Penyusutan");
        TabPage tabLifecycle = new("Siklus");
        TabPage tabApproval = new("Persetujuan");

        tabControl.TabPages.Add(tabDep);
        tabControl.TabPages.Add(tabLifecycle);
        tabControl.TabPages.Add(tabApproval);
        Controls.Add(tabControl);

        BuildDepreciationTab(tabDep);
        BuildLifecycleTab(tabLifecycle);
        BuildApprovalTab(tabApproval);
    }

    private void BuildDepreciationTab(Control parent)
    {
        Panel panelTop = new() { Dock = DockStyle.Top, Height = 88 };
        parent.Controls.Add(panelTop);

        Label lblIdData = new() { Text = "Lokasi Data", Left = 12, Top = 14, Width = 100 };
        Label lblPeriod = new() { Text = "Periode (MM/YYYY)", Left = 12, Top = 44, Width = 120 };
        Label lblRunId = new() { Text = "ID Proses", Left = 372, Top = 14, Width = 80 };

        txtIdData.SetBounds(136, 10, 220, 24);
        txtIdData.ReadOnly = true;
        txtIdData.TabStop = false;
        txtPeriod.SetBounds(136, 40, 220, 24);
        txtRunId.SetBounds(456, 10, 120, 24);

        Button btnPreview = new() { Text = "Pratinjau", Left = 600, Top = 10, Width = 100, Height = 25 };
        btnPreview.Click += async (_, _) => await PreviewDepreciationAsync();

        Button btnPost = new() { Text = "Posting", Left = 708, Top = 10, Width = 100, Height = 25 };
        btnPost.Click += async (_, _) => await PostDepreciationRunAsync();

        lblDepInfo.SetBounds(600, 44, 560, 24);
        lblDepInfo.Text = "Siap.";

        panelTop.Controls.AddRange([lblIdData, lblPeriod, lblRunId, txtIdData, txtPeriod, txtRunId, btnPreview, btnPost, lblDepInfo]);

        gridDepPreview.Dock = DockStyle.Fill;
        gridDepPreview.ReadOnly = true;
        gridDepPreview.AutoGenerateColumns = true;
        gridDepPreview.AllowUserToAddRows = false;
        gridDepPreview.DataBindingComplete += (_, _) => FixedAssetGridLocalization.Apply(gridDepPreview);
        parent.Controls.Add(gridDepPreview);
    }

    private void BuildLifecycleTab(Control parent)
    {
        Label lblAssetId = new() { Text = "ID Aset", Left = 16, Top = 18, Width = 120 };
        Label lblType = new() { Text = "Jenis Transaksi", Left = 16, Top = 52, Width = 120 };
        Label lblAmount = new() { Text = "Nilai", Left = 16, Top = 86, Width = 120 };
        Label lblOldAmount = new() { Text = "Nilai Lama", Left = 16, Top = 120, Width = 120 };
        Label lblNewAmount = new() { Text = "Nilai Baru", Left = 16, Top = 154, Width = 120 };
        Label lblSource = new() { Text = "Referensi Sumber", Left = 16, Top = 188, Width = 120 };
        Label lblRemarks = new() { Text = "Catatan", Left = 16, Top = 222, Width = 120 };

        txtAssetId.SetBounds(140, 14, 220, 24);
        cmbTransactionType.SetBounds(140, 48, 220, 24);
        txtAmount.SetBounds(140, 82, 220, 24);
        txtOldAmount.SetBounds(140, 116, 220, 24);
        txtNewAmount.SetBounds(140, 150, 220, 24);
        txtSourceRef.SetBounds(140, 184, 420, 24);
        txtRemarks.SetBounds(140, 218, 420, 24);

        cmbTransactionType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTransactionType.Items.AddRange([
            new TransactionTypeOption { Value = FixedAssetTransactionType.Improvement, Text = "Penambahan" },
            new TransactionTypeOption { Value = FixedAssetTransactionType.Revaluation, Text = "Revaluasi" },
            new TransactionTypeOption { Value = FixedAssetTransactionType.Transfer, Text = "Transfer" },
            new TransactionTypeOption { Value = FixedAssetTransactionType.FullDisposal, Text = "Penghapusan Penuh" },
            new TransactionTypeOption { Value = FixedAssetTransactionType.Sale, Text = "Penjualan" },
            new TransactionTypeOption { Value = FixedAssetTransactionType.WriteOff, Text = "Penghapusbukuan" }
        ]);
        cmbTransactionType.SelectedIndex = 0;

        Button btnCreate = new() { Text = "Buat Transaksi", Left = 140, Top = 258, Width = 220, Height = 28 };
        btnCreate.Click += async (_, _) => await CreateLifecycleTransactionAsync();

        lblLifecycleInfo.SetBounds(140, 296, 980, 24);
        lblLifecycleInfo.Text = "Siap.";

        parent.Controls.AddRange([
            lblAssetId, lblType, lblAmount, lblOldAmount, lblNewAmount, lblSource, lblRemarks,
            txtAssetId, cmbTransactionType, txtAmount, txtOldAmount, txtNewAmount, txtSourceRef, txtRemarks,
            btnCreate, lblLifecycleInfo
        ]);
    }

    private void BuildApprovalTab(Control parent)
    {
        Label lblTrxId = new() { Text = "ID Transaksi", Left = 16, Top = 22, Width = 120 };
        Label lblRole = new() { Text = "Kode Peran", Left = 16, Top = 56, Width = 120 };
        Label lblComment = new() { Text = "Catatan", Left = 16, Top = 90, Width = 120 };

        txtApprovalTrxId.SetBounds(140, 18, 220, 24);
        txtApprovalRole.SetBounds(140, 52, 220, 24);
        txtApprovalComment.SetBounds(140, 86, 420, 24);

        Button btnApprove = new() { Text = "Setujui", Left = 140, Top = 126, Width = 100, Height = 28 };
        btnApprove.Click += async (_, _) => await ApproveTransactionAsync();

        Button btnReject = new() { Text = "Tolak", Left = 248, Top = 126, Width = 100, Height = 28 };
        btnReject.Click += async (_, _) => await RejectTransactionAsync();

        lblApprovalInfo.SetBounds(140, 166, 980, 24);
        lblApprovalInfo.Text = "Siap.";

        parent.Controls.AddRange([
            lblTrxId, lblRole, lblComment,
            txtApprovalTrxId, txtApprovalRole, txtApprovalComment,
            btnApprove, btnReject, lblApprovalInfo
        ]);
    }

    private async Task PreviewDepreciationAsync()
    {
        try
        {
            DepreciationPreviewResult result = await FixedAssetServices.PreviewDepreciationAsync(
                CurrentIdData,
                txtPeriod.Text.Trim(),
                LoginInfo.userID,
                persistAsDraftRun: true).ConfigureAwait(true);

            txtRunId.Text = result.RunId?.ToString() ?? string.Empty;
            List<object> rows = result.Lines.Select(x => new
            {
                x.AssetCode,
                x.Period,
                x.OpeningNetBookValue,
                x.DepreciationAmount,
                x.ClosingNetBookValue,
                x.DepreciationExpenseAccount,
                x.AccumulatedDepreciationAccount
            }).Cast<object>().ToList();
            gridDepPreview.DataSource = rows;
            lblDepInfo.Text = $"Pratinjau berhasil. Baris={result.Lines.Count}, Total={result.TotalAmount:n2}, RunId={result.RunId}";
        }
        catch (Exception ex)
        {
            lblDepInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Pratinjau Penyusutan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task PostDepreciationRunAsync()
    {
        try
        {
            if (!long.TryParse(txtRunId.Text.Trim(), out long runId) || runId <= 0)
            {
                throw new InvalidOperationException("ID proses tidak valid.");
            }

            DepreciationPostResult result = await FixedAssetServices.PostDepreciationAsync(
                CurrentIdData,
                txtPeriod.Text.Trim(),
                runId,
                LoginInfo.userID).ConfigureAwait(true);

            lblDepInfo.Text = $"Posting berhasil. NoJurnal={result.NoJurnal}, JurnalId={result.JurnalId}, Total={result.TotalAmount:n2}";
            XtraMessageBox.Show(
                $"Posting penyusutan berhasil.\nNo Jurnal: {result.NoJurnal}\nJurnal ID: {result.JurnalId}",
                "Aset Tetap",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            lblDepInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Posting Penyusutan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CreateLifecycleTransactionAsync()
    {
        try
        {
            if (!long.TryParse(txtAssetId.Text.Trim(), out long assetId) || assetId <= 0)
            {
                throw new InvalidOperationException("ID aset tidak valid.");
            }

            if (cmbTransactionType.SelectedItem is not TransactionTypeOption option)
            {
                throw new InvalidOperationException("Jenis transaksi belum dipilih.");
            }
            FixedAssetTransactionType trxType = option.Value;

            decimal amount = ParseDecimal(txtAmount.Text);
            decimal? oldAmount = ParseOptionalDecimal(txtOldAmount.Text);
            decimal? newAmount = ParseOptionalDecimal(txtNewAmount.Text);

            FixedAssetTransactionCreateRequest req = new()
            {
                IdData = CurrentIdData,
                AssetId = assetId,
                TransactionType = trxType,
                DocumentDate = DateTime.Today,
                Period = txtPeriod.Text.Trim(),
                AmountBase = amount,
                OldAmountBase = oldAmount,
                NewAmountBase = newAmount,
                CurrencyCode = "IDR",
                ExchangeRate = 1m,
                SourceReferenceNo = txtSourceRef.Text.Trim(),
                Remarks = txtRemarks.Text.Trim(),
                UserId = LoginInfo.userID
            };

            long trxId = await FixedAssetLifecycleServices.CreateTransactionAsync(req).ConfigureAwait(true);
            lblLifecycleInfo.Text = $"Transaksi berhasil dibuat. TrxId={trxId}";
            txtApprovalTrxId.Text = trxId.ToString();
        }
        catch (Exception ex)
        {
            lblLifecycleInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Transaksi Siklus", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task ApproveTransactionAsync()
    {
        try
        {
            long trxId = ParseTransactionIdForApproval();
            ApprovalActionRequest request = new()
            {
                IdData = CurrentIdData,
                TransactionId = trxId,
                UserId = LoginInfo.userID,
                RoleCode = txtApprovalRole.Text.Trim(),
                Comment = txtApprovalComment.Text.Trim()
            };

            await FixedAssetLifecycleServices.ApproveAsync(request).ConfigureAwait(true);
            lblApprovalInfo.Text = $"Transaksi {trxId} berhasil disetujui.";
        }
        catch (Exception ex)
        {
            lblApprovalInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Persetujuan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RejectTransactionAsync()
    {
        try
        {
            long trxId = ParseTransactionIdForApproval();
            ApprovalActionRequest request = new()
            {
                IdData = CurrentIdData,
                TransactionId = trxId,
                UserId = LoginInfo.userID,
                RoleCode = txtApprovalRole.Text.Trim(),
                Comment = txtApprovalComment.Text.Trim()
            };

            await FixedAssetLifecycleServices.RejectAsync(request).ConfigureAwait(true);
            lblApprovalInfo.Text = $"Transaksi {trxId} berhasil ditolak.";
        }
        catch (Exception ex)
        {
            lblApprovalInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Penolakan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private long ParseTransactionIdForApproval()
    {
        if (!long.TryParse(txtApprovalTrxId.Text.Trim(), out long trxId) || trxId <= 0)
        {
            throw new InvalidOperationException("ID transaksi tidak valid.");
        }

        return trxId;
    }

    private static decimal ParseDecimal(string text)
    {
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal c))
        {
            return c;
        }

        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal i))
        {
            return i;
        }

        throw new InvalidOperationException($"Nilai numerik tidak valid: '{text}'");
    }

    private static decimal? ParseOptionalDecimal(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return ParseDecimal(text);
    }
}
