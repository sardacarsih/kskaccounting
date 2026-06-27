using Accounting.BusinessLayer;
using Accounting.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;

namespace Accounting.Services
{
    public static class LabaRugiReportService
    {
        public static LabaRugiReadinessResult CheckReadiness(string idData, string periode, int bulan, int tahun)
        {
            try
            {
                int record = JurnalServices.CekRecordJurnalExist(idData, periode);
                if (record == 0)
                {
                    return LabaRugiReadinessResult.MissingJournal();
                }

                decimal selisih = LaporanServices.Balanced_Check(idData, bulan, tahun);
                if (selisih != 0m)
                {
                    return LabaRugiReadinessResult.NotBalanced(periode, selisih);
                }

                return LabaRugiReadinessResult.Ready();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LabaRugi readiness_check_failed iddata={IdData} periode={Periode} bulan={Bulan} tahun={Tahun}", idData, periode, bulan, tahun);
                throw;
            }
        }

        public static List<LabaRugiRow> LoadRows(string idData, int bulan, int tahun, string userId, string jenisAkunting)
        {
            try
            {
                return LaporanServices.ViewLap_LabaRugiRows_V2(idData, bulan, tahun, userId, jenisAkunting);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LabaRugi load_rows_failed iddata={IdData} bulan={Bulan} tahun={Tahun} userid={UserId} jenis={JenisAkunting}", idData, bulan, tahun, userId, jenisAkunting);
                throw;
            }
        }

        public static DataSet LoadReportDataSet(string idData, int bulan, int tahun, string userId, string jenisAkunting)
        {
            List<LabaRugiRow> rows = LoadRows(idData, bulan, tahun, userId, jenisAkunting);
            return LabaRugiReportDataAdapter.CreateReportDataSet(rows);
        }

        public static DataTable LoadExportDataTable(string idData, int bulan, int tahun, string userId, string jenisAkunting)
        {
            List<LabaRugiRow> rows = LoadRows(idData, bulan, tahun, userId, jenisAkunting);
            return LabaRugiReportDataAdapter.CreateExportDataTable(rows);
        }
    }
}
