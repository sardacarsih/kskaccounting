using DevExpress.XtraGrid.Views.Base;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace Accounting
{
    public static class FilteredDataSourceHelper
    {

        public static object GetFilteredDataSource(this ColumnView view)
        {
            if (view == null)
                return null;
            if (view.DataSource is DataView)
                return GetFilteredDataView(view);
            if (view.DataSource is IBindingList)
                return GetFilteredBindingList(view);
            return null;
        }

        public static DataView GetFilteredDataView(ColumnView view)
        {
            if (view == null) return null;
            if (view.ActiveFilter == null || !view.ActiveFilterEnabled
                || view.ActiveFilter.Expression == string.Empty)
                return view.DataSource as DataView;

            DataTable table = ((DataView)view.DataSource).Table;
            DataView filteredDataView = new DataView(table);
            filteredDataView.RowFilter = DevExpress.Data.Filtering.CriteriaToWhereClauseHelper.GetDataSetWhere(view.ActiveFilterCriteria);
            return filteredDataView;
        }
        private static IList GetFilteredBindingList(ColumnView view)
        {
            if (view == null) return null;
            IList list = view.DataSource as IList;
            if (view.ActiveFilter == null || !view.ActiveFilterEnabled
                || view.ActiveFilter.Expression == string.Empty || list.Count == 0)
                return list;
            BindingList<object> result = new BindingList<object>();
            for (int i = 0; i < view.DataRowCount; i++)
                result.Add(view.GetRow(i));
            return result;
        }

    }
}
