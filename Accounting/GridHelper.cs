using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Windows.Forms;

namespace Accounting
{
    public class GridNewRowHelper
    {

        private readonly GridView _View;
        public GridNewRowHelper(GridView view)
        {
            _View = view;
            _View.HiddenEditor += _View_HiddenEditor;
            view.GridControl.EditorKeyDown += GridControl_EditorKeyDown;
            view.GridControl.KeyDown += GridControl_KeyDown;
        }

        void _View_HiddenEditor(object sender, EventArgs e)
        {
            _View.ClearSelection();
        }

        void GridControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (OnKeyDown(e.KeyCode, e.Modifiers))
                e.Handled = true;
        }

        void GridControl_EditorKeyDown(object sender, KeyEventArgs e)
        {
            if (OnKeyDown(e.KeyCode, e.Modifiers))
                e.Handled = true;
        }
        private bool OnKeyDown(Keys keyCode, Keys modifiers)
        {
            if (modifiers == Keys.None && keyCode == Keys.Tab)
            {
                return CheckAddNewRow();
            }
            return false;
        }

        private bool CheckAddNewRow()
        {
            if (_View.FocusedColumn == null ||
                _View.FocusedColumn.VisibleIndex != _View.VisibleColumns.Count - 2)
                return false;

            if (_View.PostEditor())
                _View.UpdateCurrentRow();

            if (_View.IsLastRow)
                return AddNewRow();

            return false;
        }

        private bool AddNewRow()
        {
            _View.ClearSelection();
            _View.AddNewRow();
            _View.FocusedColumn = _View.VisibleColumns[1];
            return true;
        }
    }
}
