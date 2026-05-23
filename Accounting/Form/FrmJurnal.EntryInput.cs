using Accounting._1.Interface;
using Accounting._2.DataAcces;
using Accounting.BusinessLayer;
using Accounting.Model;
using Accounting.Services;
using Accounting.UC.Jurnal;
using DevExpress.Data;
using DevExpress.Data.ODataLinq.Helpers;
using DevExpress.Mvvm.Native;
using DevExpress.Utils.DragDrop;
using DevExpress.Utils.Menu;
using DevExpress.Xpf.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace Accounting.Form
{
    public partial class FrmJurnal : XtraForm
    {
        private void PopulateList()
        {

            InputJurnalDetail = new BindingList<JurnalDetailAdd>
            {
                new JurnalDetailAdd
                {
                    BARIS = 1,
                    Kode = "",
                    Rekening = "",
                    Debet = 0,
                    Kredit = 0,
                    Keterangan = ""
                },
                new JurnalDetailAdd
                {
                    BARIS = 2,
                    Kode = "",
                    Rekening = "",
                    Debet = 0,
                    Kredit = 0,
                    Keterangan = ""
                }
            };
            InputJurnalDetail.AllowNew = true;
            GCJurnal.DataSource = InputJurnalDetail;
            x = Math.Max(0, InputJurnalDetail.Count - 1);
        }

        private void Load_PeriodeList(string piddata, string ptahun)
        {
            try
            {
                var periodelist = jurnalRepository.PeriodeList(piddata, ptahun);

                if (periodelist != null && periodelist.Rows.Count > 0)
                {
                    leperiode.Properties.DataSource = periodelist;
                    leperiode.Properties.ValueMember = "PERIODE";
                    leperiode.Properties.DisplayMember = "PERIODE";
                }
                else
                {
                    leperiode.Properties.DataSource = null;
                    XtraMessageBox.Show("No periods available for the selected data and year.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"An error occurred while loading the period list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        int x = 0;
        private void JDgridView_InitNewRow(object sender, InitNewRowEventArgs e)
        {
            try
            {
                if (sender is not GridView view)
                {
                    return;
                }

                int previousRowHandle = e.RowHandle - 1;
                string ket = previousRowHandle >= 0
                    ? Convert.ToString(view.GetRowCellValue(previousRowHandle, "Keterangan")) ?? string.Empty
                    : string.Empty;

                view.SetRowCellValue(e.RowHandle, view.Columns["Kode"], "");
                view.SetRowCellValue(e.RowHandle, view.Columns["Rekening"], "");
                view.SetRowCellValue(e.RowHandle, view.Columns["Debet"], 0);
                view.SetRowCellValue(e.RowHandle, view.Columns["Kredit"], 0);
                view.SetRowCellValue(e.RowHandle, view.Columns["Keterangan"], ket);
            }
            catch (SystemException ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SBSimpan_Click(object sender, EventArgs e)
        {
            if (isSaveOrUpdateInProgress)
            {
                return;
            }

            if (!TryEnsureJurnalAccess(editjurnal
                    ? AuthorizationService.EnsureCanUpdateJurnal
                    : AuthorizationService.EnsureCanCreateJurnal))
            {
                return;
            }

            if (!ValidateDetailRowsBeforeSave())
            {
                return;
            }

            string perfDetails = $"isEdit={editjurnal};detailCount={InputJurnalDetail?.Count ?? 0};periode={leperiode.Text}";
            using var perf = BeginPerfMeasurement(
                "FrmJurnal.SBSimpan_Click",
                () => perfDetails);
            try
            {
                isSaveOrUpdateInProgress = true;
                SBSimpan.Enabled = false;
                ReindexBarisInInputOrder();
                List<JurnalDetailAdd> normalizedDetails = InputJurnalDetail?.ToList() ?? new List<JurnalDetailAdd>();

                JurnalSaveRequest request = new()
                {
                    IdData = CompanyInfo.IDDATA,
                    Nomor = NoJurnaltxt.Text,
                    Tanggal = deJurnal.EditValue as DateTime?,
                    Periode = leperiode.Text,
                    PeriodeLockCheck = leallperiode.Text,
                    IsJurnalBalik = jurnalbalik.Checked,
                    IsEdit = editjurnal,
                    OldJurnalId = old_JurnalID,
                    ExpectedHeaderVersionUtc = old_HeaderVersionUtc,
                    ClientRequestId = Guid.NewGuid(),
                    FromModuleAis = importModule == ImportModule.AIS,
                    FromModuleKasir = importModule == ImportModule.Kasir,
                    FromModuleInv = importModule == ImportModule.Inventory,
                    DebetTotal = GetGridSummaryTotal("Debet"),
                    KreditTotal = GetGridSummaryTotal("Kredit"),
                    Details = normalizedDetails,
                    UserId = LoginInfo.userID
                };

                var loadingScope = BeginGlobalLoadingScope();
                try
                {
                    JurnalSaveResult saveResult = await jurnalInputOperationService.SaveAsync(request);
                    if (!saveResult.IsSuccess)
                    {
                        string caption = saveResult.ErrorCode == JurnalSaveErrorCodes.ConcurrencyConflict
                            ? "Konflik Update"
                            : "Perhatian";
                        XtraMessageBox.Show(saveResult.Message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        HandleSaveFocus(saveResult.FocusTarget);
                        return;
                    }

                    editjurnal = false;
                    importModule = ImportModule.None;
                    SetSaveButtonMode(isEdit: false);
                    PublishCoaRekalkulasiNotification(saveResult.RecalcJobId, request.Periode, saveResult.ImpactedAccountCodes);
                    bersih();
                    RefreshDaftarJurnalIfVisible();
                }
                finally
                {
                    loadingScope?.Dispose();
                    isSaveOrUpdateInProgress = false;
                    ApplyJurnalAuthorizationState();
                }
            }
            catch (SystemException ex)
            {
                isSaveOrUpdateInProgress = false;
                ApplyJurnalAuthorizationState();
                XtraMessageBox.Show(ex.Message, "Error on Simpan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal GetGridSummaryTotal(string columnName)
        {
            object summaryValue = JDgridView.Columns[columnName].SummaryItem.SummaryValue;
            return decimal.TryParse(summaryValue?.ToString(), out decimal parsedValue) ? parsedValue : 0m;
        }

        private void HandleSaveFocus(JurnalInputFocusTarget focusTarget)
        {
            switch (focusTarget)
            {
                case JurnalInputFocusTarget.Nomor:
                    NoJurnaltxt.Focus();
                    break;
                case JurnalInputFocusTarget.Tanggal:
                    deJurnal.Focus();
                    break;
                case JurnalInputFocusTarget.Periode:
                    leperiode.Focus();
                    break;
                case JurnalInputFocusTarget.GridDetail:
                    JDgridView.Focus();
                    break;
            }
        }

        ImportModule importModule = ImportModule.None;



        private void NoJurnaltxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                deJurnal.Select();
            }
        }


        private void cmbperiode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }
        private void FrmJurnal_KeyDown(object sender, KeyEventArgs e)
        {
            if (JDgridView.PostEditor())
                JDgridView.UpdateCurrentRow();

            if (e.KeyCode == Keys.F1)
            {
                TABJurnal.SelectedTabPage = xtraTabPage1;
                NoJurnaltxt.Select();
            }
            else if (e.KeyCode == Keys.F8)
            {
                TABJurnal.SelectedTabPage = xtraTabPage2;
            }
            else if (e.KeyCode == Keys.F9)
            {
                TABJurnal.SelectedTabPage = xtraTabPage3;
                txtcarinomor.Select();
            }

            if (TABJurnal.SelectedTabPage == xtraTabPage1)
            {
                if (e.KeyCode == Keys.F3)
                {
                    if (JDgridView.PostEditor())
                        JDgridView.UpdateCurrentRow();
                    NoJurnaltxt.Focus();
                }
                if (e.KeyCode == Keys.F4)
                {
                    SBSimpan.PerformClick();
                }
                else if (e.KeyCode == Keys.F5)
                {
                    sbbatal.PerformClick();
                }
            }

            if (TABJurnal.SelectedTabPage == xtraTabPage2)
            {
                if (e.KeyCode == Keys.F6)
                {
                    sbubah.PerformClick();
                }
                else if (e.KeyCode == Keys.F7)
                {
                    sbhapus.PerformClick();
                }
            }


        }

        private void sbubah_Click(object sender, EventArgs e)
        {
            Ubah_jurnal();
        }

        private void sbhapus_Click(object sender, EventArgs e)
        {
            if (GetSelectedHeaderRowHandles().Count > 0)
            {
                Hapus_Jurnal_Selected();
                return;
            }

            Hapus_jurnal();
        }


        private void sbbatal_Click(object sender, EventArgs e)
        {
            if (!IsInputJurnalDirty())
            {
                bersih();
                return;
            }

            if (XtraMessageBox.Show("Batalkan Transaksi Jurnal ? ", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            bersih();

        }

        private bool ValidateDetailRowsBeforeSave()
        {
            if (HasMeaningfulDetailRows())
            {
                return true;
            }

            XtraMessageBox.Show(
                "Detail jurnal masih kosong. Isi minimal satu baris detail sebelum simpan/update.",
                "Perhatian",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            TABJurnal.SelectedTabPage = xtraTabPage1;
            FocusFirstInputCell();
            return false;
        }

        private bool HasMeaningfulDetailRows()
        {
            if (InputJurnalDetail == null || InputJurnalDetail.Count == 0)
            {
                return false;
            }

            return InputJurnalDetail.Any(detail =>
                detail != null &&
                (!string.IsNullOrWhiteSpace(detail.Kode)
                || !string.IsNullOrWhiteSpace(detail.Rekening)
                || !string.IsNullOrWhiteSpace(detail.Keterangan)
                || detail.Debet != 0
                || detail.Kredit != 0));
        }

        private bool IsInputJurnalDirty()
        {
            return editjurnal
                || !string.IsNullOrWhiteSpace(NoJurnaltxt.Text)
                || jurnalbalik.Checked
                || HasMeaningfulDetailRows();
        }

        private void bersih()
        {
            PopulateList();
            NoJurnaltxt.Text = "";

            editjurnal = false;
            old_HeaderVersionUtc = null;
            SetSaveButtonMode(isEdit: false);
            x = 0;
            isSaveOrUpdateInProgress = false;
            jurnalbalik.Checked = false;
            NoJurnaltxt.Select();
        }

        private void SetSaveButtonMode(bool isEdit)
        {
            SBSimpan.Text = isEdit ? "&Update F4" : "&Simpan F4";
            ApplyJurnalAuthorizationState();
        }

        private void RefreshDaftarJurnalIfVisible()
        {
            if (TABJurnal.SelectedTabPage == xtraTabPage2 || TABJurnal.SelectedTabPage == xtraTabPage3)
            {
                PilihanPeriodeAkuntansi();
            }
        }

    }
}
