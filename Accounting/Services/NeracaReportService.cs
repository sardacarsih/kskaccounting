using Accounting.BusinessLayer;
using Accounting.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;

namespace Accounting.Services
{
    public static class NeracaReportService
    {
        public static NeracaReadinessResult CheckReadiness(string idData, string periode, int bulan, int tahun)
        {
            try
            {
                int record = JurnalServices.CekRecordJurnalExist(idData, periode);
                if (record == 0)
                {
                    return NeracaReadinessResult.MissingJournal();
                }

                decimal selisih = LaporanServices.Balanced_Check(idData, bulan, tahun);
                if (selisih != 0m)
                {
                    return NeracaReadinessResult.NotBalanced(periode, selisih);
                }

                return NeracaReadinessResult.Ready();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Neraca readiness_check_failed iddata={IdData} periode={Periode} bulan={Bulan} tahun={Tahun}", idData, periode, bulan, tahun);
                throw;
            }
        }

        public static List<NeracaRow> LoadRows(string idData, int bulan, int tahun, string userId)
        {
            try
            {
                return LaporanServices.ViewLap_NeracaRows_V2(idData, bulan, tahun, userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Neraca load_rows_failed iddata={IdData} bulan={Bulan} tahun={Tahun} userid={UserId}", idData, bulan, tahun, userId);
                throw;
            }
        }

        public static DataSet LoadReportDataSet(string idData, int bulan, int tahun, string userId)
        {
            List<NeracaRow> rows = LoadRows(idData, bulan, tahun, userId);
            return NeracaReportDataAdapter.CreateReportDataSet(rows);
        }
    }
}
