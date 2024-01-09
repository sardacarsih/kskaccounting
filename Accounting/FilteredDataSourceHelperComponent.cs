using System;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.XtraGrid.Views.Grid;
using System.Collections;
using System.Windows.Forms;

namespace Accounting
{
    [System.ComponentModel.DesignerCategory("")]
    public class FilteredDataSourceHelperComponent : Component
    {
        // Fields...
        private BindingSource _FilteredDataSource;
        private GridView _View;


        public GridView View
        {
            get { return _View; }
            set
            {
                GridView prevView = _View;
                _View = value;
                OnViewChanged(prevView, _View);
            }
        }



        public BindingSource FilteredDataSource
        {
            get { return _FilteredDataSource; }
            set
            {
                _FilteredDataSource = value;
            }
        }

        private void OnViewChanged(GridView oldValue, GridView newValue)
        {
            UnSubscribeEvents(oldValue);
            SubscribeEvents(newValue);
            UpdateFilteredSource();
        }
        private void UnSubscribeEvents(GridView gridView)
        {
            if (gridView == null)
                return;
            gridView.DataSourceChanged -= gridView_DataSourceChanged;
            gridView.ColumnFilterChanged -= gridView_ColumnFilterChanged;
        }
        private void SubscribeEvents(GridView gridView)
        {
            if (gridView == null)
                return;
            gridView.DataSourceChanged += gridView_DataSourceChanged;
            gridView.ColumnFilterChanged += gridView_ColumnFilterChanged;
        }

        void gridView_ColumnFilterChanged(object sender, EventArgs e)
        {
            UpdateFilteredSource();
        }

        void gridView_DataSourceChanged(object sender, EventArgs e)
        {
            UpdateFilteredSource();
        }

        private void UpdateFilteredSource()
        {
            if (_View == null || _FilteredDataSource == null)
                return;
            _FilteredDataSource.DataSource = _View.GetFilteredDataSource();
        }
    }
}
