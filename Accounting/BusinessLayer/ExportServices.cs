using Accounting.DataLayer;
using System;
using System.Data;

namespace Accounting.BusinessLayer
{
    public class ExportServices
    {
        static IExportRepository repository;
        static ExportServices()
        {
            repository = new ExportRepository();
        }

        public static DataTable ExportJurnalMonthly(string piddata, string periode)
        {
            return repository.ExportJurnalMonthly(piddata, periode);
        }
        public static DataTable ExportJurnalRange(string piddata, DateTime fromDate, DateTime toDate)
        {
            return repository.ExportJurnalRange(piddata, fromDate, toDate);
        }

    }
}
