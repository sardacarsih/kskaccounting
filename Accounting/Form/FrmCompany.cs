using Accounting.CompanyMaster.Domain;
using Accounting.CompanyMaster.Infrastructure;
using Accounting.CompanyMaster.Presentation;
using Accounting.Services;
using DevExpress.Data;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmCompany : XtraForm
    {
        private readonly FrmCompanyViewModel viewModel;

        public FrmCompany()
        {
            InitializeComponent();
            viewModel = CompanyMasterModuleFactory.CreateViewModel(LoginInfo.OracleConnString);
            gridView1.FocusedRowChanged += gridView1_FocusedRowChanged;
            gridView2.FocusedRowChanged += gridView2_FocusedRowChanged;
        }

        private void FrmCompany_Load(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageCompanyMaster))
            {
                Close();
                return;
            }

            try
            {
                viewModel.Load();
                BindViewModel();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void BindViewModel()
        {
            BindCompanies();
            BindIdDataRows();
            BindGroups();
            ApplyViewModelToCompanyFields();
            ApplyViewModelToIdDataFields();
        }

        private void BindCompanies()
        {
            gridControl2.DataSource = viewModel.Companies;
            gridView2.BestFitColumns();
            GridView view = (GridView)gridControl2.MainView;
            if (view.Columns["LookupText"] != null)
            {
                view.Columns["LookupText"].Visible = false;
            }

            if (view.Columns["IDGROUP"] != null)
            {
                GridColumnSortInfo[] sortInfo = { new(view.Columns["IDGROUP"], ColumnSortOrder.Ascending) };
                view.SortInfo.ClearAndAddRange(sortInfo, 2);
                gridView2.ExpandAllGroups();
            }

            LEPT.Properties.DataSource = viewModel.Companies;
            LEPT.Properties.ValueMember = "IDPT";
            LEPT.Properties.DisplayMember = "LookupText";
            LEPT.Properties.ForceInitialize();
            LEPT.Properties.PopulateColumns();
            if (LEPT.Properties.Columns["LookupText"] != null)
            {
                LEPT.Properties.Columns["LookupText"].Visible = false;
            }

            LEPT.Properties.BestFit();
        }

        private void BindIdDataRows()
        {
            gridControl1.DataSource = viewModel.IdDataRows;
            gridView1.BestFitColumns();
        }

        private void BindGroups()
        {
            CMBGROUP.DataSource = viewModel.Groups;
            CMBGROUP.ValueMember = "NAMAGROUP";
            CMBGROUP.DisplayMember = "NAMAGROUP";
        }

        private void ApplyCompanyFieldsToViewModel()
        {
            viewModel.CompanyIDPT = TXTKODEPT.Text;
            viewModel.CompanyNAMAPT = TXTNAMAPT.Text;
            viewModel.CompanyIDGROUP = CMBGROUP.SelectedValue?.ToString() ?? CMBGROUP.Text;
        }

        private void ApplyViewModelToCompanyFields()
        {
            TXTKODEPT.Text = viewModel.CompanyIDPT;
            TXTNAMAPT.Text = viewModel.CompanyNAMAPT;
            CMBGROUP.SelectedValue = viewModel.CompanyIDGROUP;
            if (!string.IsNullOrWhiteSpace(viewModel.CompanyIDGROUP) && CMBGROUP.SelectedValue == null)
            {
                CMBGROUP.Text = viewModel.CompanyIDGROUP;
            }
        }

        private void ApplyIdDataFieldsToViewModel()
        {
            viewModel.IdDataIDDATA = TXTIDDATA.Text;
            viewModel.IdDataWILAYAH = TXTWILAYAH.Text;
            viewModel.IdDataIDPT = LEPT.EditValue?.ToString() ?? string.Empty;
            viewModel.IdDataJENIS_AKUNTANSI = GetSelectedAccountingKind();
        }

        private void ApplyViewModelToIdDataFields()
        {
            TXTIDDATA.Text = viewModel.IdDataIDDATA;
            TXTWILAYAH.Text = viewModel.IdDataWILAYAH;
            LEPT.EditValue = string.IsNullOrWhiteSpace(viewModel.IdDataIDPT) ? null : viewModel.IdDataIDPT;
            radioGroup1.SelectedIndex = GetAccountingKindIndex(viewModel.IdDataJENIS_AKUNTANSI);
        }

        private void SBSIMPAN_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageCompanyMaster))
            {
                return;
            }

            try
            {
                ApplyCompanyFieldsToViewModel();
                CompanyMasterResult result = viewModel.SaveCompany();
                HandleCompanyResult(result, TXTKODEPT);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void SBHAPUS_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageCompanyMaster))
            {
                return;
            }

            ApplyCompanyFieldsToViewModel();
            if (string.IsNullOrWhiteSpace(viewModel.CompanyIDPT))
            {
                return;
            }

            if (XtraMessageBox.Show("Hapus Nama Perusahaan ? : " + viewModel.CompanyNAMAPT, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                CompanyMasterResult result = viewModel.DeleteCompany();
                HandleCompanyResult(result, TXTKODEPT);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnsimpan_Click_1(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageCompanyMaster))
            {
                return;
            }

            try
            {
                ApplyIdDataFieldsToViewModel();
                CompanyMasterResult result = viewModel.SaveIdData();
                HandleIdDataResult(result, TXTIDDATA);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnhapus_Click(object sender, EventArgs e)
        {
            if (!AuthorizationDialogs.TryEnsure(this, AuthorizationService.EnsureCanManageCompanyMaster))
            {
                return;
            }

            ApplyIdDataFieldsToViewModel();
            if (string.IsNullOrWhiteSpace(viewModel.IdDataIDDATA))
            {
                return;
            }

            if (XtraMessageBox.Show("Hapus IDDATA ? : " + viewModel.IdDataIDDATA, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                CompanyMasterResult result = viewModel.DeleteIdData();
                HandleIdDataResult(result, TXTIDDATA);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void HandleCompanyResult(CompanyMasterResult result, Control focusControl)
        {
            if (!result.IsSuccess)
            {
                focusControl.Focus();
                ShowError(result.Message);
                return;
            }

            BindCompanies();
            ShowInfo(result.Message, "Sukses");
        }

        private void HandleIdDataResult(CompanyMasterResult result, Control focusControl)
        {
            if (!result.IsSuccess)
            {
                focusControl.Focus();
                ShowError(result.Message);
                return;
            }

            BindIdDataRows();
            ApplyViewModelToIdDataFields();
            ShowInfo(result.Message, "Sukses");
        }

        private void gridView1_FocusedRowChanged(object? sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            viewModel.SelectIdData(gridView1.GetFocusedRow() as IdDataRecord);
            ApplyViewModelToIdDataFields();
        }

        private void gridView2_FocusedRowChanged(object? sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            viewModel.SelectCompany(gridView2.GetFocusedRow() as CompanyMasterRecord);
            ApplyViewModelToCompanyFields();
        }

        private string GetSelectedAccountingKind()
        {
            return radioGroup1.SelectedIndex switch
            {
                0 => "PUSAT",
                1 => "PWK",
                2 => "KEBUN",
                3 => "PKS",
                _ => "KEBUN"
            };
        }

        private static int GetAccountingKindIndex(string accountingKind)
        {
            return accountingKind?.Trim().ToUpperInvariant() switch
            {
                "PUSAT" => 0,
                "PWK" => 1,
                "KEBUN" => 2,
                "PKS" => 3,
                _ => 2
            };
        }

        private static void ShowInfo(string message, string caption)
        {
            XtraMessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void ShowError(string message)
        {
            XtraMessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void TXTIDDATA_KeyDown(object sender, KeyEventArgs e) => MoveNextOnEnter(e);
        private void TXTWILAYAH_KeyDown(object sender, KeyEventArgs e) => MoveNextOnEnter(e);
        private void LEPT_KeyDown(object sender, KeyEventArgs e) => MoveNextOnEnter(e);
        private void TXTKODEPT_KeyDown(object sender, KeyEventArgs e) => MoveNextOnEnter(e);
        private void TXTNAMAPT_KeyDown(object sender, KeyEventArgs e) => MoveNextOnEnter(e);
        private void CMBGROUP_KeyDown(object sender, KeyEventArgs e) => MoveNextOnEnter(e);

        private static void MoveNextOnEnter(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }
    }
}
