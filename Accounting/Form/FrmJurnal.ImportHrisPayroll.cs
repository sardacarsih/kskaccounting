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
	private void BuatJurnalHRIS()
	{
		if (jurnalfinalHRIS != null && jurnalfinalHRIS.Any())
		{
			string text = gridViewAISheader.GetRowCellValue(gridViewAISheader.FocusedRowHandle, "NOJURNAL")?.ToString() ?? string.Empty;
			noAIS_Bukti = gridViewAISheader.GetRowCellValue(gridViewAISheader.FocusedRowHandle, "NOMOR")?.ToString() ?? string.Empty;
			importModule = ImportModule.AIS;
			string text2 = lookUpEditestatehris.EditValue?.ToString() ?? string.Empty;
			int month = cmbbulanhris.SelectedIndex + 1;
			int year = (int)setahunhris.Value;
			DateTime dateTime = new DateTime(year, month, 1).AddMonths(1).AddDays(-1.0);
			DateTime dateTime2 = dateTime;
			NoJurnaltxt.Text = (string.IsNullOrEmpty(text) ? ("XXX/MK-" + text2) : text);
			deJurnal.EditValue = dateTime2;
			InputJurnalDetail.Clear();
			List<JurnalDetailAdd> list = jurnalPayrollUmumService.BuildDetailRows(jurnalfinalHRIS);
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

	private void lookUpEditestatehris_EditValueChanged(object sender, EventArgs e)
	{
		ExecuteSafeAsync(HandleEditValueChangedOrSelectedIndexChangedHRISAsync(), "loading HRIS data");
	}

	private void cmbbulanhris_SelectedIndexChanged(object sender, EventArgs e)
	{
		ExecuteSafeAsync(HandleEditValueChangedOrSelectedIndexChangedHRISAsync(), "loading HRIS data");
	}

	private void setahunhris_EditValueChanged(object sender, EventArgs e)
	{
		ExecuteSafeAsync(HandleEditValueChangedOrSelectedIndexChangedHRISAsync(), "loading HRIS data");
	}

	private void leremisehris_EditValueChanged(object sender, EventArgs e)
	{
		ExecuteSafeAsync(HandleEditValueChangedOrSelectedIndexChangedHRISAsync(), "loading HRIS data");
	}

	private async Task HandleEditValueChangedOrSelectedIndexChangedHRISAsync()
	{
		decimal? tahunValue = setahunhris.Value;
		if (!tahunValue.HasValue || tahunValue.Value <= 0m)
		{
			return;
		}
		int ptahun = (int)tahunValue.Value;
		int pbulan = cmbbulanhris.SelectedIndex + 1;
		if (pbulan >= 1 && pbulan <= 12)
		{
			int p_periode_int = Convert.ToInt32($"{ptahun}{pbulan:00}");
			string p_estate = lookUpEditestatehris.EditValue?.ToString() ?? string.Empty;
			string p_iddata = CompanyInfo.IDDATA;
			string p_periode_str = $"{pbulan:00}/{ptahun}";
			string p_periodeket = $"{p_estate} {cmbbulanhris.Text} {ptahun}";
			DateTime TanggalJurnal = new DateTime(ptahun, pbulan, 1).AddMonths(1).AddDays(-1.0);
			if (!string.IsNullOrEmpty(p_estate) && !string.IsNullOrEmpty(p_iddata))
			{
				List<SlipGaji_DTO> gaji = await jurnalRepository.viewDaftarGajidanTunjangan_BulananAsync(p_iddata, p_estate, p_periode_int);
				List<FIN_POTONGAN_KANTOR> pot_kantor = await jurnalRepository.viewPotonganKantorAsync(p_iddata, p_estate, p_periode_int);
				List<ALOKASI_JURNAL_DTO> alokasijurnal = await jurnalRepository.AlokasiJurnalAsync(p_iddata);
				jurnalfinalHRIS = await jurnalRepository.HitungLampiranKASAsync(gaji, pot_kantor, alokasijurnal, ListCoaAktif, p_periodeket, p_periode_str, p_estate, TanggalJurnal);
				SetJurnalPayroll(jurnalfinalHRIS);
			}
		}
	}

	private void SetJurnalPayrollUmum(List<AIS_JURNAL_FINAL> payrollumum)
	{
		ConfigurePayrollGrid(gridControlUmum, gridViewUmum, payrollumum);
	}

	private void SetJurnalPayroll(IEnumerable<AIS_JURNAL_FINAL> jurnalpayroll)
	{
		ConfigurePayrollGrid(gridControlHRIS, gridViewhris, jurnalpayroll);
	}

	private void ConfigurePayrollGrid(GridControl gridControl, GridView gridView, object dataSource)
	{
		gridControl.DataSource = dataSource;
		gridView.OptionsBehavior.Editable = false;
		ConfigureSummaryColumn(gridView, "DEBET");
		ConfigureSummaryColumn(gridView, "KREDIT");
		string[] payrollHiddenColumns = PayrollHiddenColumns;
		foreach (string fieldName in payrollHiddenColumns)
		{
			GridColumn gridColumn = gridView.Columns[fieldName];
			if (gridColumn != null)
			{
				gridColumn.Visible = false;
			}
		}
		gridView.BestFitColumns();
	}

	private static void ConfigureSummaryColumn(GridView gridView, string columnName)
	{
		GridColumn gridColumn = gridView.Columns[columnName];
		if (gridColumn != null)
		{
			gridColumn.Summary.Clear();
			gridColumn.Summary.Add(SummaryItemType.Sum, columnName, "{0:n0}");
			gridColumn.DisplayFormat.FormatType = FormatType.Numeric;
			gridColumn.DisplayFormat.FormatString = "n0";
		}
	}

	private void sbbuatjurnalhris_Click(object sender, EventArgs e)
	{
		BuatJurnalHRIS();
	}

	private void sbexportjurnalhris_Click(object sender, EventArgs e)
	{
		if (jurnalfinalHRIS != null)
		{
			ExportJurnal_FromList(jurnalfinalHRIS);
		}
		else
		{
			XtraMessageBox.Show("Pilih Nomor untuk mengExport ke Excel", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}

	private void sbprev_Click(object sender, EventArgs e)
	{
		try
		{
			if (leallperiode.EditValue != null)
			{
				leallperiode.ItemIndex--;
				GCHeader.Focus();
			}
		}
		catch (Exception ex)
		{
			XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void snext_Click(object sender, EventArgs e)
	{
		if (leallperiode.ItemIndex != -1)
		{
			leallperiode.ItemIndex++;
			GCHeader.Focus();
		}
	}

	private void leallperiode_EditValueChanged(object sender, EventArgs e)
	{
		PilihanPeriodeAkuntansi();
	}

	private void cmbbulanumum_SelectedIndexChanged(object sender, EventArgs e)
	{
		Load_GajianUmum();
	}

	private void setahunumum_EditValueChanged(object sender, EventArgs e)
	{
		Load_GajianUmum();
	}

	private void leremiseumum_EditValueChanged(object sender, EventArgs e)
	{
		Load_GajianUmum();
	}

	private async void Load_GajianUmum()
	{
		try
		{
			decimal? tahunValue = setahunumum.Value;
			if (!tahunValue.HasValue || tahunValue.Value <= 0m)
			{
				return;
			}
			int ptahun = (int)tahunValue.Value;
			int pbulan = cmbbulanumum.SelectedIndex + 1;
			if (pbulan >= 1 && pbulan <= 12)
			{
				int p_remise = Convert.ToInt16(leremiseumum.EditValue?.ToString());
				JurnalPayrollUmumRequest request = new JurnalPayrollUmumRequest
				{
					IdData = CompanyInfo.IDDATA,
					Tahun = ptahun,
					Bulan = pbulan,
					Remise = p_remise,
					BulanText = cmbbulanumum.Text
				};
				payrollumum = await jurnalPayrollUmumService.BuildPayrollUmumAsync(jurnalRepository, request);
				if (payrollumum == null || payrollumum.Count == 0)
				{
					gridControlUmum.DataSource = null;
				}
				else
				{
					SetJurnalPayrollUmum(payrollumum);
				}
			}
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			XtraMessageBox.Show("An error occurred: " + ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void sbjunarlumum_Click(object sender, EventArgs e)
	{
		BuatJurnal_payrollumum();
	}

	private void BuatJurnal_payrollumum()
	{
		if (payrollumum != null && payrollumum.Any())
		{
			int month = cmbbulanumum.SelectedIndex + 1;
			int year = (int)setahunumum.Value;
			DateTime dateTime = new DateTime(year, month, 1).AddMonths(1).AddDays(-1.0);
			DateTime dateTime2 = dateTime;
			NoJurnaltxt.Text = "XXX/MK-" + CompanyInfo.ESTATE;
			deJurnal.EditValue = dateTime2;
			InputJurnalDetail.Clear();
			List<JurnalDetailAdd> list = jurnalPayrollUmumService.BuildDetailRows(payrollumum);
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
			string text = ((payrollumum == null) ? "Pilih Nomor untuk membuat Jurnal Payroll Umum" : "Tidak ada transaksi untuk dibuat jurnal");
			XtraMessageBox.Show(text, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}

	private void sbjurnalumumexport_Click(object sender, EventArgs e)
	{
		if (payrollumum != null && payrollumum.Any())
		{
			ExportJurnal_FromList(payrollumum);
		}
		else
		{
			XtraMessageBox.Show("Pilih Nomor untuk mengExport ke Excel", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}

    }
}
