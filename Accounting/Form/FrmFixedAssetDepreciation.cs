using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accounting.BusinessLayer;
using Accounting.FixedAssets.Application.Models;
using Accounting.Services;
using DevExpress.XtraEditors;

namespace Accounting.Form;

public sealed class FrmFixedAssetDepreciation : XtraForm
{
    private static string CurrentIdData => CompanyInfo.IDDATA?.Trim() ?? string.Empty;

    private readonly TextBox txtIdData = new();
    private readonly TextBox txtPeriod = new();
    private readonly TextBox txtRunId = new();
    private readonly Label lblInfo = new();
    private readonly DataGridView gridPreview = new();

    public FrmFixedAssetDepreciation()
    {
        Text = "Aset Tetap - Penyusutan";
        Width = 1100;
        Height = 680;
        StartPosition = FormStartPosition.CenterScreen;

        BuildUi();
        txtIdData.Text = CurrentIdData;
        txtPeriod.Text = $"{DateTime.Now.Month:00}/{DateTime.Now.Year:0000}";
    }

    private void BuildUi()
    {
        Panel top = new() { Dock = DockStyle.Top, Height = 94 };

        Label lblIdData = new() { Text = "Lokasi Data", Left = 12, Top = 14, Width = 90 };
        Label lblPeriod = new() { Text = "Periode (MM/YYYY)", Left = 12, Top = 44, Width = 120 };
        Label lblRunId = new() { Text = "ID Proses", Left = 362, Top = 14, Width = 80 };

        txtIdData.SetBounds(136, 10, 210, 24);
        txtIdData.ReadOnly = true;
        txtIdData.TabStop = false;
        txtPeriod.SetBounds(136, 40, 210, 24);
        txtRunId.SetBounds(446, 10, 120, 24);

        Button btnPreview = new() { Text = "Pratinjau", Left = 586, Top = 10, Width = 100, Height = 25 };
        btnPreview.Click += async (_, _) => await PreviewAsync();
        btnPreview.Enabled = AuthorizationService.CanRunFixedAssetDepreciation();

        Button btnPost = new() { Text = "Posting", Left = 694, Top = 10, Width = 100, Height = 25 };
        btnPost.Click += async (_, _) => await PostAsync();
        btnPost.Enabled = AuthorizationService.CanRunFixedAssetDepreciation();

        lblInfo.SetBounds(586, 44, 470, 40);
        lblInfo.Text = "Siap.";

        top.Controls.AddRange([lblIdData, lblPeriod, lblRunId, txtIdData, txtPeriod, txtRunId, btnPreview, btnPost, lblInfo]);

        gridPreview.Dock = DockStyle.Fill;
        gridPreview.ReadOnly = true;
        gridPreview.AutoGenerateColumns = true;
        gridPreview.AllowUserToAddRows = false;
        gridPreview.DataBindingComplete += (_, _) => FixedAssetGridLocalization.Apply(gridPreview);

        Controls.Add(gridPreview);
        Controls.Add(top);
    }

    private async Task PreviewAsync()
    {
        try
        {
            if (!AuthorizationService.CanRunFixedAssetDepreciation())
            {
                throw new InvalidOperationException("Role Anda tidak memiliki izin menjalankan penyusutan.");
            }

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
            gridPreview.DataSource = rows;
            lblInfo.Text = $"Pratinjau berhasil. Baris={result.Lines.Count}, Total={result.TotalAmount:n2}, RunId={result.RunId}";
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Pratinjau Penyusutan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task PostAsync()
    {
        try
        {
            if (!AuthorizationService.CanRunFixedAssetDepreciation())
            {
                throw new InvalidOperationException("Role Anda tidak memiliki izin melakukan posting penyusutan.");
            }

            if (!long.TryParse(txtRunId.Text.Trim(), out long runId) || runId <= 0)
            {
                throw new InvalidOperationException("ID proses tidak valid.");
            }

            DepreciationPostResult result = await FixedAssetServices.PostDepreciationAsync(
                CurrentIdData,
                txtPeriod.Text.Trim(),
                runId,
                LoginInfo.userID).ConfigureAwait(true);

            lblInfo.Text = $"Posting berhasil. NoJurnal={result.NoJurnal}, JurnalId={result.JurnalId}, Total={result.TotalAmount:n2}";
            XtraMessageBox.Show(
                $"Posting penyusutan berhasil.\nNo Jurnal: {result.NoJurnal}\nJurnal ID: {result.JurnalId}",
                "Aset Tetap",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            lblInfo.Text = "Gagal.";
            XtraMessageBox.Show(ex.Message, "Kesalahan Posting Penyusutan", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
