using Accounting.Interface;
using Accounting.Model;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Controllers
{
    public class JurnalController
    {
        private readonly IJurnalRepository _jurnalRepository;

        public JurnalController(IJurnalRepository jurnalRepository)
        {
            _jurnalRepository = jurnalRepository;
        }

        public void InsertJurnal(JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail)
        {
            _jurnalRepository.InsertJurnalMasterDetail(jurnalHeader, jurnalDetail);
        }
        public void HapusJurnal(double p_JurnalID)
        {
            _jurnalRepository.HapusJurnal(p_JurnalID);
        }
        public void HapusJurnalRange(List<double> selectedValues)
        {
            _jurnalRepository.HapusJurnalRange(selectedValues);
        }
        public void PerformDragAndDrop(GridView targetGrid, GridView sourceGrid, DevExpress.Utils.DragDrop.DragDropEventArgs e)
        {
            _jurnalRepository.PerformDragAndDrop(targetGrid, sourceGrid, e);
        }
        public List<DTOCOAAktif> KodeUntukJurnal(string piddata, int ptahun)
        {
             return _jurnalRepository.KodeUntukJurnal(piddata, ptahun);
        }
    }

}
