using Accounting.Model;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmDaftarCOA : DevExpress.XtraEditors.XtraForm
    {
        public IEnumerable<DTOCOAAktif> ListCoaAktif { get; set; } = Array.Empty<DTOCOAAktif>();

        private bool isCommitInProgress;
        private string kode = string.Empty;
        private string rekening = string.Empty;

        public FrmDaftarCOA()
        {
            InitializeComponent();
        }

        public string KODE => kode;
        public string REKENING => rekening;

        public void SetSearchPanelValue(string searchValue)
        {
            gridView1.ApplyFindFilter(searchValue);
        }

        private void FrmDaftarCOA_Load(object sender, EventArgs e)
        {
            gridControl1.DataSource = ListCoaAktif;
            gridView1.OptionsBehavior.Editable = false;
            gridView1.OptionsFind.AlwaysVisible = true;
            gridView1.FocusRectStyle = DrawFocusRectStyle.RowFocus;
            gridView1.OptionsSelection.EnableAppearanceFocusedCell = false;
            gridView1.OptionsSelection.MultiSelect = false;
            gridView1.BestFitColumns();

            FocusFirstVisibleDataRow();
        }

        private void FrmDaftarCOA_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                Close();
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;

            if (keyCode == Keys.Enter)
            {
                _ = TryCommitSelectedKodePerkiraan();
                return true;
            }

            if (keyCode == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            _ = TryCommitSelectedKodePerkiraan();
        }

        private bool TryCommitSelectedKodePerkiraan()
        {
            if (isCommitInProgress)
            {
                return false;
            }

            isCommitInProgress = true;
            try
            {
                gridView1.CloseEditor();
                gridView1.UpdateCurrentRow();

                int rowHandle = GetSelectedDataRowHandle();
                if (rowHandle < 0)
                {
                    return false;
                }

                if (gridView1.GetRow(rowHandle) is not DTOCOAAktif selectedItem)
                {
                    return false;
                }

                kode = selectedItem.KODE ?? string.Empty;
                rekening = selectedItem.PERKIRAAN ?? string.Empty;

                DialogResult = DialogResult.OK;
                Close();
                return true;
            }
            finally
            {
                isCommitInProgress = false;
            }
        }

        private int GetSelectedDataRowHandle()
        {
            int focusedRowHandle = gridView1.FocusedRowHandle;
            if (focusedRowHandle >= 0 && gridView1.IsDataRow(focusedRowHandle))
            {
                return focusedRowHandle;
            }

            int[] selectedRows = gridView1.GetSelectedRows();
            foreach (int selectedRowHandle in selectedRows)
            {
                if (selectedRowHandle >= 0 && gridView1.IsDataRow(selectedRowHandle))
                {
                    return selectedRowHandle;
                }
            }

            for (int visibleIndex = 0; visibleIndex < gridView1.DataRowCount; visibleIndex++)
            {
                int visibleRowHandle = gridView1.GetVisibleRowHandle(visibleIndex);
                if (visibleRowHandle >= 0 && gridView1.IsDataRow(visibleRowHandle))
                {
                    return visibleRowHandle;
                }
            }

            return GridControl.InvalidRowHandle;
        }

        private void FocusFirstVisibleDataRow()
        {
            int firstVisibleRowHandle = gridView1.GetVisibleRowHandle(0);
            if (firstVisibleRowHandle >= 0 && gridView1.IsDataRow(firstVisibleRowHandle))
            {
                gridView1.FocusedRowHandle = firstVisibleRowHandle;
            }
        }
    }
}
