using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace Accounting.Form;

internal static class FixedAssetGridLocalization
{
    private static readonly Dictionary<string, string> HeaderMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["IDDATA"] = "Lokasi Data",
        ["ASSET_ID"] = "ID Aset",
        ["ASSET_CODE"] = "Kode Aset",
        ["ASSET_NAME"] = "Nama Aset",
        ["CATEGORY_ID"] = "ID Kategori",
        ["CATEGORY_NAME"] = "Kategori",
        ["GROUP_ID"] = "ID Kelompok",
        ["GROUP_NAME"] = "Kelompok",
        ["DEPARTMENT_ID"] = "Departemen",
        ["COST_CENTER_ID"] = "Pusat Biaya",
        ["LOCATION_ID"] = "Lokasi",
        ["VENDOR_ID"] = "Vendor",
        ["SERIAL_NO"] = "Nomor Seri",
        ["CURRENCY_CODE"] = "Mata Uang",
        ["EXCHANGE_RATE"] = "Kurs",
        ["ACQUISITION_DATE"] = "Tanggal Perolehan",
        ["IN_SERVICE_DATE"] = "Tanggal Mulai Pakai",
        ["DEPRECIATION_START_DATE"] = "Tanggal Mulai Penyusutan",
        ["ACQUISITION_COST"] = "Nilai Perolehan",
        ["RESIDUAL_VALUE"] = "Nilai Residu",
        ["USEFUL_LIFE_MONTHS"] = "Masa Manfaat (Bulan)",
        ["DEPR_METHOD"] = "Metode Penyusutan",
        ["STATUS"] = "Status",
        ["PERIOD"] = "Periode",
        ["OPENING_NET_BOOK_VALUE"] = "Nilai Buku Awal",
        ["DEPRECIATION_AMOUNT"] = "Nilai Penyusutan",
        ["CLOSING_NET_BOOK_VALUE"] = "Nilai Buku Akhir",
        ["NET_BOOK_VALUE"] = "Nilai Buku",
        ["DEPRECIATION_EXPENSE_ACCOUNT"] = "Akun Beban Penyusutan",
        ["ACCUMULATED_DEPRECIATION_ACCOUNT"] = "Akun Akumulasi Penyusutan",
        ["NOJURNAL"] = "Nomor Jurnal",
        ["JURNAL_ID"] = "ID Jurnal",
        ["TRX_ID"] = "ID Transaksi",
        ["TRX_TYPE"] = "Jenis Transaksi",
        ["TRX_DATE"] = "Tanggal Transaksi",
        ["DOCUMENT_NO"] = "Nomor Dokumen",
        ["SOURCE_REFERENCE_NO"] = "Referensi Sumber",
        ["REMARKS"] = "Catatan",
        ["OLD_AMOUNT_BASE"] = "Nilai Lama",
        ["NEW_AMOUNT_BASE"] = "Nilai Baru",
        ["AMOUNT_BASE"] = "Nilai",
        ["DELTA_AMOUNT"] = "Selisih",
        ["GAIN_LOSS_AMOUNT"] = "Laba/Rugi",
        ["PROCEEDS_AMOUNT"] = "Nilai Hasil Penjualan",
        ["BOOK_VALUE_AT_DISPOSAL"] = "Nilai Buku Saat Penghapusan",
        ["DEPRECIATION_EXPENSE"] = "Beban Penyusutan",
        ["ACCUMULATED_DEPRECIATION"] = "Akumulasi Penyusutan",
        ["RUN_ID"] = "ID Proses",
        ["RUN_STATUS"] = "Status Proses",
        ["POSTED_BY"] = "Diposting Oleh",
        ["POSTED_AT"] = "Waktu Posting",
        ["APPROVED_BY"] = "Disetujui Oleh",
        ["APPROVED_AT"] = "Waktu Persetujuan",
        ["CREATED_BY"] = "Dibuat Oleh",
        ["CREATED_AT"] = "Waktu Buat",
        ["UPDATED_BY"] = "Diubah Oleh",
        ["UPDATED_AT"] = "Waktu Ubah",
        ["PROJECT_NAME"] = "Nama Proyek",
        ["CIP_ID"] = "ID KDP",
        ["CIP_CODE"] = "Kode KDP",
        ["CIP_STATUS"] = "Status KDP",
        ["OUTSTANDING_AMOUNT"] = "Sisa Nilai",
        ["CAPITALIZED_AMOUNT"] = "Nilai Dikapitalisasi"
    };

    public static void Apply(DataGridView grid)
    {
        if (grid.Columns.Count == 0)
        {
            return;
        }

        foreach (DataGridViewColumn column in grid.Columns)
        {
            string key = string.IsNullOrWhiteSpace(column.DataPropertyName)
                ? column.Name
                : column.DataPropertyName;
            if (HeaderMap.TryGetValue(key, out string header))
            {
                column.HeaderText = header;
                continue;
            }

            column.HeaderText = HumanizeHeader(column.HeaderText);
        }
    }

    private static string HumanizeHeader(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        string normalized = raw.Replace("_", " ", StringComparison.Ordinal).Trim();
        string[] tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < tokens.Length; i++)
        {
            tokens[i] = tokens[i].ToUpperInvariant() switch
            {
                "ID" => "ID",
                "ASSET" => "Aset",
                "CATEGORY" => "Kategori",
                "GROUP" => "Kelompok",
                "DEPARTMENT" => "Departemen",
                "LOCATION" => "Lokasi",
                "VENDOR" => "Vendor",
                "CURRENCY" => "Mata Uang",
                "EXCHANGE" => "Kurs",
                "RATE" => "Rate",
                "ACQUISITION" => "Perolehan",
                "DEPRECIATION" => "Penyusutan",
                "ACCUMULATED" => "Akumulasi",
                "EXPENSE" => "Beban",
                "ACCOUNT" => "Akun",
                "PERIOD" => "Periode",
                "AMOUNT" => "Nilai",
                "STATUS" => "Status",
                "REMARKS" => "Catatan",
                "TRANSACTION" => "Transaksi",
                "TYPE" => "Jenis",
                "DATE" => "Tanggal",
                "CODE" => "Kode",
                "NAME" => "Nama",
                "CREATED" => "Dibuat",
                "UPDATED" => "Diubah",
                "APPROVED" => "Disetujui",
                "POSTED" => "Diposting",
                _ => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tokens[i].ToLowerInvariant())
            };
        }

        return string.Join(" ", tokens);
    }
}
