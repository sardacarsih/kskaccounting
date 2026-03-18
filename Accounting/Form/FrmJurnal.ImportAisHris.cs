using Accounting._1.Interface;
using Accounting.BusinessLayer;
using Accounting.Model;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmJurnal
    {
        private string noAIS_Bukti = string.Empty;

	private void sbbuatjurnalAIS_Click(object sender, EventArgs e)
	{
		BuatJurnalAIS();
	}

	private void Call_AIS_JurnalDetail()
	{
		try
		{
			decimal filter = Convert.ToDecimal(gridViewAISheader.GetRowCellValue(gridViewAISheader.FocusedRowHandle, "AISJURNALID").ToString());
			EnumerableRowCollection<DataRow> source = from y in dtAISDetail.AsEnumerable()
				where y.Field<decimal>("REFFID") == filter
				select y;
			DataTable dt = source.Any() ? source.CopyToDataTable() : dtAISDetail.Clone();
			gcAISdetail.DataSource = dt;
			gridViewAISdetail.Columns[0].Visible = false;
			gridViewAISdetail.Columns[4].DisplayFormat.FormatType = FormatType.Numeric;
			gridViewAISdetail.Columns[4].DisplayFormat.FormatString = "n2";
			gridViewAISdetail.Columns[5].DisplayFormat.FormatType = FormatType.Numeric;
			gridViewAISdetail.Columns[5].DisplayFormat.FormatString = "n2";
			gridViewAISdetail.Columns[4].Summary.Clear();
			gridViewAISdetail.Columns[5].Summary.Clear();
			gridViewAISdetail.Columns[4].Summary.Add(SummaryItemType.Sum, "DEBET", "{0:N2}");
			gridViewAISdetail.Columns[5].Summary.Add(SummaryItemType.Sum, "KREDIT", "{0:N2}");
			gridViewAISdetail.BestFitColumns();
		}
		catch (Exception ex)
		{
			gcAISdetail.DataSource = null;
			XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void gridViewAISheader_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
	{
		try
		{
			if (sender is not GridView view)
			{
				return;
			}

			if (e.MenuType == GridMenuType.Row)
			{
				int rowHandle = e.HitInfo.RowHandle;
				e.Menu.Items.Clear();
				DXMenuItem dXMenuItem = CreateMenuItemExpExcelALL_BORONGAN(view, rowHandle);
				DXMenuItem dXMenuItem2 = CreateMenuItemExpExcelALL_HARIAN(view, rowHandle);
				dXMenuItem.BeginGroup = true;
				dXMenuItem2.BeginGroup = true;
				e.Menu.Items.Add(dXMenuItem);
				e.Menu.Items.Add(dXMenuItem2);
			}
		}
		catch (SystemException ex)
		{
			XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private DXMenuItem CreateMenuItemExpExcelALL_HARIAN(GridView? view, int rowHandle)
	{
		DXMenuItem dXMenuItem = new DXMenuItem("Export ke Excel, SEMUA HARIAN remise ini", OnexpexcelALLHARIANClick);
		dXMenuItem.ImageOptions.Image = imageCollection1.Images[1];
		return dXMenuItem;
	}

	private void OnexpexcelALLHARIANClick(object? sender, EventArgs e)
	{
		using IOverlaySplashScreenHandle overlaySplashScreenHandle = SplashScreenManager.ShowOverlayForm(this);
		overlaySplashScreenHandle.QueueFocus(IntPtr.Zero);
		if (lookUpEditEstate.EditValue == null || leremiseAIS.EditValue == null)
		{
			return;
		}

		int year = Convert.ToInt32(setahunAIS.Value);
		int month = cmbbulanAIS.SelectedIndex + 1;
		int pPeriode = Convert.ToInt32(year + month.ToString("00"));
		string pPeriodeStr = FormatPeriod(month, year);
		string iDDATA = CompanyInfo.IDDATA;
		string pEstate = lookUpEditEstate.EditValue.ToString() ?? string.Empty;
		int num = Convert.ToInt32(leremiseAIS.EditValue.ToString());
		DateTime tanggalJurnal;
		if (num == 1)
		{
			tanggalJurnal = new DateTime(year, month, 15);
		}
		else
		{
			DateTime dateTime = new DateTime(year, month, 1).AddMonths(1).AddDays(-1.0);
			tanggalJurnal = dateTime;
		}
		DataTable dataTable = jurnalRepository.AIS_Jurnal_Detail_ALL_HARIAN(tanggalJurnal, pPeriode, pPeriodeStr, string.Empty, pEstate, num, iDDATA);
		ExportJurnal_FromList(jurnalfinalAIS);
	}

	private DXMenuItem CreateMenuItemExpExcelALL_BORONGAN(GridView? view, int rowHandle)
	{
		DXMenuItem dXMenuItem = new DXMenuItem("Export ke Excel, SEMUA BORONGAN remise ini", OnexpexcelALLBORONGANClick);
		dXMenuItem.ImageOptions.Image = imageCollection1.Images[1];
		return dXMenuItem;
	}

	private void OnexpexcelALLBORONGANClick(object? sender, EventArgs e)
	{
	}

	private void OnaddjurnalClick(object? sender, EventArgs e)
	{
		BuatJurnalAIS();
	}

	private void BuatJurnalAIS()
	{
		if (jurnalfinalAIS != null && jurnalfinalAIS.Any())
		{
			string text = gridViewAISheader.GetRowCellValue(gridViewAISheader.FocusedRowHandle, "NOJURNAL")?.ToString() ?? string.Empty;
			noAIS_Bukti = gridViewAISheader.GetRowCellValue(gridViewAISheader.FocusedRowHandle, "NOMOR")?.ToString() ?? string.Empty;
			importModule = ImportModule.AIS;
			string text2 = lookUpEditEstate.EditValue?.ToString() ?? string.Empty;
			short num = Convert.ToInt16(leremiseAIS.EditValue?.ToString());
			int month = cmbbulanAIS.SelectedIndex + 1;
			int year = (int)setahunAIS.Value;
			DateTime dateTime;
			if (num == 1)
			{
				dateTime = new DateTime(year, month, 15);
			}
			else
			{
				DateTime dateTime2 = new DateTime(year, month, 1).AddMonths(1).AddDays(-1.0);
				dateTime = dateTime2;
			}
			NoJurnaltxt.Text = (string.IsNullOrEmpty(text) ? ("XXX/MK-AGRO-" + text2) : text);
			deJurnal.EditValue = dateTime;
			InputJurnalDetail.Clear();
			List<JurnalDetailAdd> list = jurnalPayrollUmumService.BuildDetailRows(jurnalfinalAIS);
			foreach (JurnalDetailAdd item in list)
			{
				InputJurnalDetail.Add(item);
			}
			GCJurnal.DataSource = InputJurnalDetail;
			InputJurnalDetail.AllowNew = true;
			TABJurnal.SelectedTabPage = xtraTabPage1;
			x = InputJurnalDetail.Count - 1;
		}
		else
		{
			string text4 = ((jurnalfinalAIS == null) ? "Pilih Nomor untuk membuat Jurnal MK-AGRO" : "Tidak ada transaksi untuk dibuat jurnal");
			XtraMessageBox.Show(text4, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}

	private async Task LoadDivisionsAsync()
	{
		if (cmbbulanAIS.SelectedIndex >= 0 && leremiseAIS.EditValue != null && lookUpEditEstate.EditValue != null)
		{
			int ptahun = Convert.ToInt32(setahunAIS.Value);
			int pbulan = cmbbulanAIS.SelectedIndex + 1;
			int p_remise = Convert.ToInt16(leremiseAIS.EditValue.ToString());
			int p_periode_int = Convert.ToInt32(ptahun + pbulan.ToString("00"));
			string p_estate = lookUpEditEstate.EditValue.ToString() ?? string.Empty;
			if (string.IsNullOrEmpty(p_estate))
			{
				return;
			}

			string p_iddata = CompanyInfo.IDDATA;
			List<Division> divisions = await jurnalRepository.GetDivisionsAsync(p_iddata, p_estate, p_periode_int, p_remise);
			if (divisions != null)
			{
				gcAISheader.DataSource = divisions;
				gridViewAISheader.Columns["ISBORONGAN"].Visible = false;
				gridViewAISheader.Columns["DIVISIID"].Visible = false;
				gridViewAISheader.BestFitColumns();
				gridViewhris.BestFitColumns();
				gcAISdetail.DataSource = null;
			}
			else
			{
				XtraMessageBox.Show("No divisions found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}

	private async Task LoadJurnalAISPeriodeAsync()
	{
		if (lookUpEditEstate.EditValue != null && setahunAIS.Value != 0m && leremiseAIS.EditValue != null)
		{
			int ptahun = Convert.ToInt32(setahunAIS.Value);
			int pbulan = cmbbulanAIS.SelectedIndex + 1;
			int p_remise = Convert.ToInt16(leremiseAIS.EditValue.ToString());
			int p_periode_int = Convert.ToInt32(ptahun + pbulan.ToString("00"));
			string p_estate = lookUpEditEstate.EditValue.ToString() ?? string.Empty;
			if (string.IsNullOrEmpty(p_estate))
			{
				return;
			}

			string p_iddata = CompanyInfo.IDDATA;
			string periodes = "R" + p_remise + " " + cmbbulanAIS.Text + " " + ptahun;
			aisJurnal = await jurnalRepository.GetAISforJurnalAsync(p_iddata, p_estate, p_periode_int, p_remise, ptahun, periodes);
			komponenjurnal = await jurnalRepository.GetAISforJurnalKomponenAsync(p_iddata, p_estate, p_periode_int, p_remise);
		}
	}

	private async Task HandleEditValueChangedOrSelectedIndexChangedAISAsync()
	{
		await Task.WhenAll(LoadDivisionsAsync(), LoadJurnalAISPeriodeAsync());
	}

	private async void ExecuteSafeAsync(Task task, string operationName)
	{
		try
		{
			await task;
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			XtraMessageBox.Show("An error occurred while " + operationName + ": " + ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void lookUpEditEstate_EditValueChanged(object sender, EventArgs e)
	{
		ExecuteSafeAsync(HandleEditValueChangedOrSelectedIndexChangedAISAsync(), "loading AIS data");
	}

	private void cmbbulanAIS_SelectedIndexChanged(object sender, EventArgs e)
	{
		ExecuteSafeAsync(HandleEditValueChangedOrSelectedIndexChangedAISAsync(), "loading AIS data");
	}

	private void setahunAIS_EditValueChanged(object sender, EventArgs e)
	{
		ExecuteSafeAsync(HandleEditValueChangedOrSelectedIndexChangedAISAsync(), "loading AIS data");
	}

	private void leremiseAIS_EditValueChanged(object sender, EventArgs e)
	{
		ExecuteSafeAsync(HandleEditValueChangedOrSelectedIndexChangedAISAsync(), "loading AIS data");
	}

	private void leremiseAIS_EditValueChanged_1(object sender, EventArgs e)
	{
		ExecuteSafeAsync(HandleEditValueChangedOrSelectedIndexChangedAISAsync(), "loading AIS data");
	}

    }
}
