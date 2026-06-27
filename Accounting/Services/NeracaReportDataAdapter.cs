using Accounting.Model;
using System.Collections.Generic;
using System.Data;

namespace Accounting.Services
{
    public static class NeracaReportDataAdapter
    {
        public static DataSet CreateReportDataSet(IEnumerable<NeracaRow> rows)
        {
            DataSet dataSet = new();
            dataSet.Tables.Add(CreateReportDataTable(rows));
            return dataSet;
        }

        public static DataTable CreateReportDataTable(IEnumerable<NeracaRow> rows)
        {
            DataTable table = new("Neraca");
            // URUT is generated here (the proc returns rows already ordered by KAT, KODE, AKUN);
            // the Skontro reports sort their detail bands by URUT, so the column must exist.
            table.Columns.Add("URUT", typeof(int));
            table.Columns.Add("KODE", typeof(string));
            table.Columns.Add("CAT1", typeof(string));
            table.Columns.Add("KAT", typeof(string));
            table.Columns.Add("CAT2", typeof(string));
            table.Columns.Add("AKUN", typeof(string));
            table.Columns.Add("PARENTAKUN", typeof(string));
            table.Columns.Add("TIPE", typeof(string));
            table.Columns.Add("POSISI", typeof(string));
            table.Columns.Add("BULANINI", typeof(decimal));
            table.Columns.Add("BULANLALU", typeof(decimal));
            table.Columns.Add("AWALTAHUN", typeof(decimal));

            int urut = 0;
            foreach (NeracaRow row in rows)
            {
                table.Rows.Add(
                    ++urut,
                    row.Kode,
                    row.Cat1,
                    row.Kat,
                    row.Cat2,
                    row.Akun,
                    row.ParentAkun,
                    row.Tipe,
                    row.Posisi,
                    row.BulanIni,
                    row.BulanLalu,
                    row.AwalTahun);
            }

            return table;
        }
    }
}
