using Accounting.Model;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Interface
{
    public interface IJurnalRepository
    {
        void InsertJurnalMasterDetail(JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail);
        void HapusJurnal(double p_JurnalID);
        void HapusJurnalRange(List<double> selectedValues);
        void PerformDragAndDrop(GridView targetGrid, GridView sourceGrid, DevExpress.Utils.DragDrop.DragDropEventArgs e);
        List<DTOCOAAktif> KodeUntukJurnal(string piddata, int ptahun);
    }
}
