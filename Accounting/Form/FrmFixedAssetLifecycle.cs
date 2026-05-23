using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accounting.BusinessLayer;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Domain;
using DevExpress.XtraEditors;
using WinFormsComboBox = System.Windows.Forms.ComboBox;

namespace Accounting.Form;

public sealed class FrmFixedAssetLifecycle : XtraForm
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
    private readonly TextBox txtAssetId = new();
    private readonly WinFormsComboBox cmbTransactionType = new();
    private readonly TextBox txtAmount = new();
    private readonly TextBox txtOldAmount = new();
    private readonly TextBox txtNewAmount = new();
    private readonly TextBox txtSourceRef = new();
    private readonly TextBox txtRemarks = new();
    private readonly Label lblInfo = new();
    private readonly long? _defaultAssetId;
    private readonly FixedAssetTransactionType? _defaultTransactionType;
    private readonly string _defaultSourceRef;

    public FrmFixedAssetLifecycle()
        : this(null, null, string.Empty)
    {
    }

    public FrmFixedAssetLifecycle(long? defaultAssetId, FixedAssetTransactionType? defaultTransactionType, string defaultSourceRef)
    {
        Text = "Aset Tetap - Transaksi Siklus";
        Width = 860;
        Height = 520;
        StartPosition = FormStartPosition.CenterScreen;
        _defaultAssetId = defaultAssetId;
        _defaultTransactionType = defaultTransactionType;
        _defaultSourceRef = defaultSourceRef ?? string.Empty;

        BuildUi();
        txtIdData.Text = CurrentIdData;
        txtPeriod.Text = $"{DateTime.Now.Month:00}/{DateTime.Now.Year:0000}";
        ApplyDefaults();
    }

    private void BuildUi()
    {
        Label lblIdData = new() { Text = "Lokasi Data", Left = 16, Top = 20, Width = 120 };
        Label lblPeriod = new() { Text = "Periode (MM/YYYY)", Left = 16, Top = 54, Width = 120 };
        Label lblAssetId = new() { Text = "ID Aset", Left = 16, Top = 88, Width = 120 };
        Label lblType = new() { Text = "Jenis Transaksi", Left = 16, Top = 122, Width = 120 };
        Label lblAmount = new() { Text = "Nilai", Left = 16, Top = 156, Width = 120 };
        Label lblOldAmount = new() { Text = "Nilai Lama", Left = 16, Top = 190, Width = 120 };
        Label lblNewAmount = new() { Text = "Nilai Baru", Left = 16, Top = 224, Width = 120 };
        Label lblSource = new() { Text = "Referensi Sumber", Left = 16, Top = 258, Width = 120 };
        Label lblRemarks = new() { Text = "Catatan", Left = 16, Top = 292, Width = 120 };

        txtIdData.SetBounds(140, 16, 220, 24);
        txtIdData.ReadOnly = true;
        txtIdData.TabStop = false;
        txtPeriod.SetBounds(140, 50, 220, 24);
        txtAssetId.SetBounds(140, 84, 220, 24);
        cmbTransactionType.SetBounds(140, 118, 220, 24);
        txtAmount.SetBounds(140, 152, 220, 24);
        txtOldAmount.SetBounds(140, 186, 220, 24);
        txtNewAmount.SetBounds(140, 220, 220, 24);
        txtSourceRef.SetBounds(140, 254, 500, 24);
        txtRemarks.SetBounds(140, 288, 500, 24);

        cmbTransactionType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTransactionType.Items.AddRange(
            [
                new TransactionTypeOption { Value = FixedAssetTransactionType.Improvement, Text = "Penambahan" },
                new TransactionTypeOption { Value = FixedAssetTransactionType.Revaluation, Text = "Revaluasi" },
                new TransactionTypeOption { Value = FixedAssetTransactionType.Transfer, Text = "Transfer" },
                new TransactionTypeOption { Value = FixedAssetTransactionType.CipCapitalization, Text = "Kapitalisasi KDP" },
                new TransactionTypeOption { Value = FixedAssetTransactionType.FullDisposal, Text = "Penghapusan Penuh" },
                new TransactionTypeOption { Value = FixedAssetTransactionType.Sale, Text = "Penjualan" },
                new TransactionTypeOption { Value = FixedAssetTransactionType.WriteOff, Text = "Penghapusbukuan" }
            ]);
        cmbTransactionType.SelectedIndex = 0;

        Button btnCreate = new() { Text = "Buat Transaksi", Left = 140, Top = 328, Width = 220, Height = 30 };
        btnCreate.Click += async (_, _) => await CreateTransactionAsync();
        btnCreate.Enabled = FixedAssetUiRoleHelper.CanLifecycleCreate();

        lblInfo.SetBounds(140, 366, 680, 40);
        lblInfo.Text = "Siap.";

        Controls.AddRange([
            lblIdData, lblPeriod, lblAssetId, lblType, lblAmount, lblOldAmount, lblNewAmount, lblSource, lblRemarks,
            txtIdData, txtPeriod, txtAssetId, cmbTransactionType, txtAmount, txtOldAmount, txtNewAmount, txtSourceRef, txtRemarks,
            btnCreate, lblInfo
        ]);
    }

    private void ApplyDefaults()
    {
        if (_defaultAssetId.HasValue && _defaultAssetId.Value > 0)
        {
            txtAssetId.Text = _defaultAssetId.Value.ToString();
        }

        if (!string.IsNullOrWhiteSpace(_defaultSourceRef))
        {
            txtSourceRef.Text = _defaultSourceRef;
        }

        if (_defaultTransactionType.HasValue)
        {
            for (int index = 0; index < cmbTransactionType.Items.Count; index++)
            {
                if (cmbTransactionType.Items[index] is not TransactionTypeOption option || option.Value != _defaultTransactionType.Value)
                {
                    continue;
                }

                cmbTransactionType.SelectedIndex = index;
                break;
            }
        }
    }

    private async Task CreateTransactionAsync()
    {
        try
        {
            if (!FixedAssetUiRoleHelper.CanLifecycleCreate())
            {
                throw new InvalidOperationException("Role Anda tidak memiliki izin membuat transaksi lifecycle.");
            }

            if (!long.TryParse(txtAssetId.Text.Trim(), out long assetId) || assetId <= 0)
            {
                throw new InvalidOperationException("ID aset tidak valid.");
            }

            if (cmbTransactionType.SelectedItem is not TransactionTypeOption selectedType)
            {
                throw new InvalidOperationException("Jenis transaksi belum dipilih.");
            }
            FixedAssetTransactionType trxType = selectedType.Value;

            FixedAssetTransactionCreateRequest req = new()
            {
                IdData = CurrentIdData,
                AssetId = assetId,
                TransactionType = trxType,
                DocumentDate = DateTime.Today,
                Period = txtPeriod.Text.Trim(),
                AmountBase = ParseDecimal(txtAmount.Text),
                OldAmountBase = ParseOptionalDecimal(txtOldAmount.Text),
                NewAmountBase = ParseOptionalDecimal(txtNewAmount.Text),
                CurrencyCode = "IDR",
                ExchangeRate = 1m,
                SourceReferenceNo = txtSourceRef.Text.Trim(),
                Remarks = txtRemarks.Text.Trim(),
                UserId = LoginInfo.userID
            };

            long trxId = await FixedAssetLifecycleServices.CreateTransactionAsync(req).ConfigureAwait(true);
            lblInfo.Text = $"Transaksi berhasil dibuat. TrxId={trxId}";
            XtraMessageBox.Show($"Transaksi siklus aset berhasil dibuat.\nTRX_ID: {trxId}", "Aset Tetap", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Transaksi Siklus", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
