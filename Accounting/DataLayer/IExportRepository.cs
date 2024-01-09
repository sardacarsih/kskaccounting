using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    public interface IExportRepository
    {
        DataTable ExportJurnalMonthly(string piddata, string periode);
        DataTable ExportJurnalRange(string piddata, DateTime fromDate,DateTime toDate);
    }
}
