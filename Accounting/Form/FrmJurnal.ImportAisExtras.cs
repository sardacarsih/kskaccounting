using Accounting.Model;
using DevExpress.Data;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace Accounting.Form
{
    public partial class FrmJurnal
    {

        private async Task Load_Kode_AISAsync()
        {
            var estate = await jurnalRepository.GetEstateAsync(CompanyInfo.IDDATA);
            lookUpEditEstate.Properties.DataSource = estate;
            lookUpEditEstate.Properties.ValueMember = "ID";
            lookUpEditEstate.Properties.DisplayMember = "NAMA";
            lookUpEditEstate.Properties.NullText = "";
            lookUpEditEstate.Properties.ImmediatePopup = true;
            lookUpEditEstate.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;

            Dictionary<int, string> Remises = new()
            {
                { 1, "1" },
                { 2, "2" }
            };
            leremiseAIS.Properties.DataSource = Remises;
            leremiseAIS.Properties.ValueMember = "Key";
            leremiseAIS.Properties.DisplayMember = "Value";
            leremiseAIS.Properties.NullText = "";
            leremiseAIS.Properties.ImmediatePopup = true;
            leremiseAIS.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            leremiseAIS.EditValue = 1;

            if (estate.Count == 0)
            {
                XtraMessageBox.Show(
                    $"Estate tidak ditemukan untuk IDDATA {CompanyInfo.IDDATA}. Cek MASTER_ESTATE.IDDATA.",
                    "Info",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else if (lookUpEditEstate.EditValue == null)
            {
                lookUpEditEstate.EditValue = estate[0].ID;
                ScheduleAisReload();
            }
        }



        private void gridViewAISheader_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (IsJurnalTrue(gridViewAISheader, e.RowHandle))
            {
                e.Appearance.BackColor = Color.LightGray;
            }
            else if (IsJurnalTrueRevised(gridViewAISheader, e.RowHandle))
            {
                e.Appearance.BackColor = Color.LightCoral;
            }
        }


        private bool IsJurnalTrueRevised(GridView gridViewAISheader, int rowHandle)
        {
            try
            {
                string val = Convert.ToString(gridViewAISheader.GetRowCellValue(rowHandle, "JURNAL")) ?? string.Empty;
                var nojurnal = Convert.ToString(gridViewAISheader.GetRowCellValue(rowHandle, "NOJURNAL"));
                return (val == "T" && !string.IsNullOrEmpty(nojurnal));
            }
            catch
            {
                return false;
            }
        }


        private bool IsJurnalTrue(GridView gridViewAISheader, int rowHandle)
        {
            try
            {
                string val = Convert.ToString(gridViewAISheader.GetRowCellValue(rowHandle, "JURNAL")) ?? string.Empty;
                return (val == "Y");
            }
            catch
            {
                return false;
            }
        }




        private void sbExportExcelAIS_Click(object sender, EventArgs e)
        {
            if (jurnalfinalAIS != null && jurnalfinalAIS.Any())
            {
                string division = gridViewAISheader.GetRowCellValue(gridViewAISheader.FocusedRowHandle, "DIVISI")?.ToString() ?? "Jurnal";
                ExportAisJurnalWithDialog(jurnalfinalAIS, BuildAisExportFileName(division));
            }
            else
            {
                XtraMessageBox.Show("Pilih Nomor untuk mengExport ke Excel", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


    }
}
