using Accounting.BusinessLayer;
using Accounting.Model;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Accounting.Services;

namespace Accounting.Form
{
    public partial class FrmAuditTrail : DevExpress.XtraEditors.XtraForm
    {
        private DateTime _searchFromDate;
        private DateTime _searchToDate;

        public FrmAuditTrail()
        {
            InitializeComponent();
        }

        private void FrmAuditTrail_Load(object sender, EventArgs e)
        {
            try
            {
                if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanViewAuditTrail))
                {
                    Close();
                    return;
                }

                dtDari.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dtSampai.DateTime = DateTime.Now;

                cmbTipe.Properties.Items.AddRange(new[] { "All", "UPDATE", "DELETE" });
                cmbTipe.SelectedIndex = 0;

                Cursor.Current = Cursors.Default;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
            }
        }

        private void btnCari_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanViewAuditTrail))
            {
                return;
            }
            if (dtDari.DateTime == DateTime.MinValue || dtSampai.DateTime == DateTime.MinValue)
            {
                XtraMessageBox.Show("Tanggal Dari dan Sampai harus diisi.", "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _searchFromDate = dtDari.DateTime;
            _searchToDate = dtSampai.DateTime;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var data = JurnalServices.SearchAuditTrail(
                    CompanyInfo.IDDATA,
                    _searchFromDate,
                    _searchToDate,
                    cmbTipe.Text,
                    txtUser.Text.Trim(),
                    txtNoJurnal.Text.Trim());

                gridControlHeader.DataSource = data;
                gridControlAuditEvents.DataSource = null;
                gridControlDetail.DataSource = null;

                if (data.Count == 0)
                {
                    XtraMessageBox.Show("Data audit trail tidak ditemukan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                FormatHeaderGrid();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void FormatHeaderGrid()
        {
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gridViewHeader.Columns)
            {
                switch (col.FieldName)
                {
                    case "JURNALID":
                        col.Visible = false;
                        break;
                    case "NOJURNAL":
                        col.Caption = "No Jurnal";
                        col.Width = 120;
                        break;
                    case "TANGGAL":
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        col.DisplayFormat.FormatString = "dd-MMM-yyyy";
                        col.Caption = "Tanggal";
                        col.Width = 100;
                        break;
                    case "PERIODE":
                        col.Caption = "Periode";
                        col.Width = 70;
                        break;
                    case "JUMLAH_AKSI":
                        col.Caption = "Jml Aksi";
                        col.Width = 60;
                        break;
                    case "LAST_ACTION_DATE":
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        col.DisplayFormat.FormatString = "dd-MMM-yyyy HH:mm";
                        col.Caption = "Terakhir Diubah";
                        col.Width = 140;
                        break;
                    case "ACTION_TYPES":
                        col.Caption = "Tipe Aksi";
                        col.Width = 100;
                        break;
                }
            }
            gridViewHeader.BestFitColumns();
        }

        private void FormatAuditEventsGrid()
        {
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gridViewAuditEvents.Columns)
            {
                switch (col.FieldName)
                {
                    case "AUDIT_ID":
                    case "JURNALID":
                    case "NOJURNAL":
                    case "TANGGAL":
                    case "PERIODE":
                    case "SUMBER":
                    case "IDDATA":
                    case "CHANGED_FIELDS":
                    case "DELETE_REASON":
                        col.Visible = false;
                        break;
                    case "ACTION_DATE":
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                        col.DisplayFormat.FormatString = "dd-MMM-yyyy HH:mm";
                        col.Caption = "Waktu";
                        col.Width = 140;
                        break;
                    case "ACTION_TYPE":
                        col.Caption = "Tipe";
                        col.Width = 70;
                        break;
                    case "ACTION_BY":
                        col.Caption = "User";
                        col.Width = 80;
                        break;
                    case "DETAIL_ROWS_INSERTED":
                        col.Caption = "DTL INS";
                        col.Width = 60;
                        break;
                    case "DETAIL_ROWS_UPDATED":
                        col.Caption = "DTL UPD";
                        col.Width = 60;
                        break;
                    case "DETAIL_ROWS_DELETED":
                        col.Caption = "DTL DEL";
                        col.Width = 60;
                        break;
                    case "ACTION_PC":
                        col.Caption = "PC";
                        col.Width = 100;
                        break;
                    case "ACTION_IP":
                        col.Caption = "IP";
                        col.Width = 100;
                        break;
                }
            }
            gridViewAuditEvents.BestFitColumns();
        }

        private void FormatDetailGrid()
        {
            foreach (DevExpress.XtraGrid.Columns.GridColumn col in gridViewDetail.Columns)
            {
                switch (col.FieldName)
                {
                    case "AUDIT_DTL_ID":
                    case "AUDIT_ID":
                        col.Visible = false;
                        break;
                    case "CHANGE_TYPE":
                        col.Caption = "Tipe";
                        col.Width = 70;
                        break;
                    case "BARIS":
                        col.Caption = "Baris";
                        col.Width = 45;
                        break;
                    case "KODE":
                        col.Caption = "Kode";
                        col.Width = 80;
                        break;
                    case "REKENING":
                        col.Caption = "Rekening";
                        col.Width = 130;
                        break;
                    case "DEBET":
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        col.DisplayFormat.FormatString = "n2";
                        col.Caption = "Debet";
                        col.Width = 110;
                        col.Summary.Clear();
                        col.Summary.Add(DevExpress.Data.SummaryItemType.Sum, "DEBET", "{0:N2}");
                        break;
                    case "KREDIT":
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        col.DisplayFormat.FormatString = "n2";
                        col.Caption = "Kredit";
                        col.Width = 110;
                        col.Summary.Clear();
                        col.Summary.Add(DevExpress.Data.SummaryItemType.Sum, "KREDIT", "{0:N2}");
                        break;
                    case "KETERANGAN":
                        col.Caption = "Keterangan";
                        col.Width = 130;
                        break;
                    case "OLD_KODE":
                        col.Caption = "Old Kode";
                        col.Width = 80;
                        break;
                    case "OLD_DEBET":
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        col.DisplayFormat.FormatString = "n2";
                        col.Caption = "Old Debet";
                        col.Width = 110;
                        break;
                    case "OLD_KREDIT":
                        col.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                        col.DisplayFormat.FormatString = "n2";
                        col.Caption = "Old Kredit";
                        col.Width = 110;
                        break;
                    case "OLD_KETERANGAN":
                        col.Caption = "Old Keterangan";
                        col.Width = 130;
                        break;
                }
            }
            gridViewDetail.BestFitColumns();
        }

        private void gridViewHeader_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            gridControlDetail.DataSource = null;

            if (e.FocusedRowHandle < 0)
            {
                gridControlAuditEvents.DataSource = null;
                return;
            }

            var nojurnal = gridViewHeader.GetRowCellValue(e.FocusedRowHandle, "NOJURNAL")?.ToString();
            var periode = gridViewHeader.GetRowCellValue(e.FocusedRowHandle, "PERIODE")?.ToString();
            if (string.IsNullOrEmpty(nojurnal) || string.IsNullOrEmpty(periode)) return;

            try
            {
                var events = JurnalServices.GetAuditByJurnal(nojurnal, periode, CompanyInfo.IDDATA, _searchFromDate, _searchToDate);
                gridControlAuditEvents.DataSource = events;
                FormatAuditEventsGrid();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridViewAuditEvents_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
            {
                gridControlDetail.DataSource = null;
                return;
            }

            var auditId = gridViewAuditEvents.GetRowCellValue(e.FocusedRowHandle, "AUDIT_ID");
            if (auditId == null) return;

            try
            {
                var detail = JurnalServices.GetAuditDetail(Convert.ToDouble(auditId));
                gridControlDetail.DataSource = detail;
                FormatDetailGrid();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridViewAuditEvents_RowStyle(object sender, RowStyleEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.RowHandle < 0) return;

            var actionType = view.GetRowCellValue(e.RowHandle, "ACTION_TYPE")?.ToString();
            if (actionType == "UPDATE")
            {
                e.Appearance.BackColor = Color.FromArgb(255, 255, 220);
            }
            else if (actionType == "DELETE")
            {
                e.Appearance.BackColor = Color.FromArgb(255, 220, 220);
            }
        }

        private void gridViewDetail_RowStyle(object sender, RowStyleEventArgs e)
        {
            GridView view = sender as GridView;
            if (e.RowHandle < 0) return;

            var changeType = view.GetRowCellValue(e.RowHandle, "CHANGE_TYPE")?.ToString();
            switch (changeType)
            {
                case "ADDED":
                    e.Appearance.BackColor = Color.FromArgb(220, 255, 220);
                    break;
                case "MODIFIED":
                    e.Appearance.BackColor = Color.FromArgb(255, 255, 220);
                    break;
                case "DELETED":
                    e.Appearance.BackColor = Color.FromArgb(255, 220, 220);
                    break;
                case "SNAPSHOT":
                    e.Appearance.BackColor = Color.FromArgb(230, 230, 230);
                    break;
            }
        }
    }
}
