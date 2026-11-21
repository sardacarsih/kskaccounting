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
				DXMenuItem dXMenuItemSelected = CreateMenuItemExpExcelSelectedRow(view, rowHandle);
				DXMenuItem dXMenuItem = CreateMenuItemExpExcelALL_BORONGAN(view, rowHandle);
				DXMenuItem dXMenuItem2 = CreateMenuItemExpExcelALL_HARIAN(view, rowHandle);
				dXMenuItemSelected.BeginGroup = true;
				dXMenuItem.BeginGroup = true;
				dXMenuItem2.BeginGroup = true;
				e.Menu.Items.Add(dXMenuItemSelected);
				e.Menu.Items.Add(dXMenuItem);
				e.Menu.Items.Add(dXMenuItem2);
			}
		}
		catch (SystemException ex)
		{
			XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private DXMenuItem CreateMenuItemExpExcelSelectedRow(GridView view, int rowHandle)
	{
		DXMenuItem dXMenuItem = new DXMenuItem("Export ke Excel, baris terpilih", (s, e) => ExportSelectedAisRows(view, rowHandle));
		dXMenuItem.ImageOptions.Image = imageCollection1.Images[1];
		return dXMenuItem;
	}

	private void ExportSelectedAisRows(GridView view, int clickedRowHandle)
	{
		if (!TryCreateAisJournalBuildContext(out AisJournalBuildContext context))
		{
			XtraMessageBox.Show("Pilih estate, bulan, tahun, dan remise AIS terlebih dahulu.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		List<Division> selectedDivisions = (view.GetSelectedRows() ?? [])
			.Where(rowHandle => rowHandle >= 0)
			.Select(view.GetRow)
			.OfType<Division>()
			.ToList();

		if (selectedDivisions.Count == 0 && clickedRowHandle >= 0 && view.GetRow(clickedRowHandle) is Division clickedDivision)
		{
			selectedDivisions.Add(clickedDivision);
		}

		if (selectedDivisions.Count == 0)
		{
			XtraMessageBox.Show("Tidak ada baris yang dipilih.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		List<AIS_JURNAL_FINAL> rows = selectedDivisions
			.SelectMany(division => AisJournalBuilder.BuildForDivision(aisJurnal ?? [], komponenjurnal ?? [], division, context))
			.ToList();

		if (rows.Count == 0)
		{
			XtraMessageBox.Show("Data AIS tidak ditemukan untuk baris terpilih.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		string label = selectedDivisions.Count == 1 ? selectedDivisions[0].DIVISI ?? "Divisi" : "Terpilih";
		ExportAisJurnalWithDialog(rows, BuildAisExportFileName(label));
	}

	private DXMenuItem CreateMenuItemExpExcelALL_HARIAN(GridView? view, int rowHandle)
	{
		DXMenuItem dXMenuItem = new DXMenuItem("Export ke Excel, SEMUA HARIAN remise ini", OnexpexcelALLHARIANClick);
		dXMenuItem.ImageOptions.Image = imageCollection1.Images[1];
		return dXMenuItem;
	}

	private void OnexpexcelALLHARIANClick(object? sender, EventArgs e)
	{
		ExportAllAisJurnal(borongan: false);
	}

	private DXMenuItem CreateMenuItemExpExcelALL_BORONGAN(GridView? view, int rowHandle)
	{
		DXMenuItem dXMenuItem = new DXMenuItem("Export ke Excel, SEMUA BORONGAN remise ini", OnexpexcelALLBORONGANClick);
		dXMenuItem.ImageOptions.Image = imageCollection1.Images[1];
		return dXMenuItem;
	}

	private void OnexpexcelALLBORONGANClick(object? sender, EventArgs e)
	{
		ExportAllAisJurnal(borongan: true);
	}

	private void sbExportBoronganAIS_Click(object sender, EventArgs e)
	{
		ExportAllAisJurnal(borongan: true);
	}

	private void sbExportHarianAIS_Click(object sender, EventArgs e)
	{
		ExportAllAisJurnal(borongan: false);
	}

	private void OnaddjurnalClick(object? sender, EventArgs e)
	{
		BuatJurnalAIS();
	}

	private void BuatJurnalAIS()
	{
		// Build from the focused division at click time, never from a cached preview.
		RebuildAisPreview();
		if (jurnalfinalAIS != null && jurnalfinalAIS.Any())
		{
			Division? focusedDivision = gridViewAISheader.GetFocusedRow() as Division;
			if (!string.IsNullOrWhiteSpace(focusedDivision?.NOJURNAL)
				&& XtraMessageBox.Show(
					$"Divisi {focusedDivision.DIVISI} untuk remise dan periode ini sudah dijurnal dengan nomor {focusedDivision.NOJURNAL}.\nTetap buat jurnal baru?",
					"Sudah Dijurnal",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					MessageBoxDefaultButton.Button2) != DialogResult.Yes)
			{
				return;
			}

			string text = focusedDivision?.NOMOR ?? string.Empty;
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
			WarnAisJurnalIssues(NoJurnaltxt.Text, year);
		}
		else
		{
			string text4 = ((gridViewAISheader.GetFocusedRow() is not Division) ? "Pilih Nomor untuk membuat Jurnal MK-AGRO" : "Tidak ada transaksi untuk dibuat jurnal");
			XtraMessageBox.Show(text4, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}

	/// <summary>
	/// Warns, before the user tries to save, about problems the save validation would only report later (or not at all).
	/// </summary>
	private void WarnAisJurnalIssues(string nomor, int year)
	{
		List<string> warnings = [];

		// Same limit as JurnalInputOperationService.Save.
		if (nomor.Length > 30)
		{
			warnings.Add($"Nomor Jurnal MAX 30 Karakter, panjang karakter sekarang : {nomor.Length}. Sesuaikan nomor sebelum simpan.");
		}

		List<string> missingCoa = jurnalfinalAIS
			.Where(row => !string.IsNullOrWhiteSpace(row.KODE) && string.IsNullOrWhiteSpace(row.REKENING))
			.Select(row => row.KODE)
			.Distinct()
			.ToList();
		if (missingCoa.Count > 0)
		{
			warnings.Add($"Kode akun berikut belum ada di Daftar Perkiraan (COA) tahun {year}:\n{string.Join("\n", missingCoa)}\nTambahkan ke COA agar laporan tetap balance.");
		}

		List<string> komponenTanpaAkun = jurnalfinalAIS
			.Where(row => string.IsNullOrWhiteSpace(row.KODE) && (row.DEBET != 0 || row.KREDIT != 0))
			.Select(row => row.KETERANGAN)
			.Distinct()
			.ToList();
		if (komponenTanpaAkun.Count > 0)
		{
			warnings.Add(
				$"Baris komponen berikut belum punya kode akun:\n{string.Join("\n", komponenTanpaAkun)}\n" +
				$"Isi kode di grid, atau daftarkan sekali di ACCT_DEFAULT dengan NAMA = {AisJournalBuilder.KomponenAccountKeyPrefix}<KETERANGAN> (contoh {AisJournalBuilder.KomponenAccountKey("BPJS JHT")}).");
		}

		if (warnings.Count == 0)
		{
			return;
		}

		XtraMessageBox.Show(string.Join("\n\n", warnings), "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		if (nomor.Length > 30)
		{
			NoJurnaltxt.Focus();
		}
	}

	private async Task LoadDivisionsAsync(System.Threading.CancellationToken cancellationToken)
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
			cancellationToken.ThrowIfCancellationRequested();
			if (divisions != null && TryCreateAisJournalBuildContext(out AisJournalBuildContext context))
			{
				Dictionary<string, string> journaled = await jurnalRepository.GetJournaledAisMarkersAsync(p_iddata, context.PeriodString);
				cancellationToken.ThrowIfCancellationRequested();
				foreach (Division division in divisions)
				{
					division.NOJURNAL = journaled.TryGetValue(AisJournalBuilder.GetJournaledMarker(division, context), out string? noJurnal)
						? noJurnal
						: string.Empty;
				}
			}

			if (divisions != null)
			{
				gcAISheader.DataSource = divisions;
				gridViewAISheader.Columns["ISBORONGAN"].Visible = false;
				gridViewAISheader.Columns["DIVISIID"].Visible = false;
				if (gridViewAISheader.Columns["NOJURNAL"] != null)
				{
					gridViewAISheader.Columns["NOJURNAL"].Caption = "Sudah Dijurnal (No.)";
				}
				gridViewAISheader.BestFitColumns();
				gridViewhris.BestFitColumns();
			}
			else
			{
				XtraMessageBox.Show("No divisions found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
	}

	private async Task LoadJurnalAISPeriodeAsync(System.Threading.CancellationToken cancellationToken)
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
			Task<List<AIS_JURNAL>> aisTask = jurnalRepository.GetAISforJurnalAsync(p_iddata, p_estate, p_periode_int, p_remise, ptahun, periodes);
			Task<List<JurnalKomponen>> komponenTask = jurnalRepository.GetAISforJurnalKomponenAsync(p_iddata, p_estate, p_periode_int, p_remise);
			await Task.WhenAll(aisTask, komponenTask);
			cancellationToken.ThrowIfCancellationRequested();

			aisJurnal = aisTask.Result;
			komponenjurnal = komponenTask.Result;
		}
	}

	private async Task HandleEditValueChangedOrSelectedIndexChangedAISAsync(System.Threading.CancellationToken cancellationToken, bool useLoading = true)
	{
		int divisionCount = 0;
		int aisCount = 0;
		string estate = lookUpEditEstate.EditValue?.ToString() ?? string.Empty;
		int year = Convert.ToInt32(setahunAIS.Value);
		int month = cmbbulanAIS.SelectedIndex + 1;
		string remise = leremiseAIS.EditValue?.ToString() ?? string.Empty;
		using var perf = BeginPerfMeasurement(
			"FrmJurnal.AIS.Reload",
			() => $"estate={estate};year={year};month={month};remise={remise};divisionCount={divisionCount};aisCount={aisCount};useLoading={useLoading}");

		IDisposable? loadingScope = null;
		try
		{
			if (useLoading)
			{
				loadingScope = BeginGlobalLoadingScope();
			}

			// Drop the previous filter's data so a focus change fired mid-load cannot build a journal from it.
			aisJurnal = [];
			komponenjurnal = [];
			jurnalfinalAIS = [];

			await Task.WhenAll(
				LoadDivisionsAsync(cancellationToken),
				LoadJurnalAISPeriodeAsync(cancellationToken));

			RebuildAisPreview();

			divisionCount = (gcAISheader.DataSource as IEnumerable<Division>)?.Count() ?? 0;
			aisCount = aisJurnal?.Count() ?? 0;
		}
		finally
		{
			loadingScope?.Dispose();
		}
	}

	private void ScheduleAisReload()
	{
		ScheduleDebounced(
			ref aisReloadCts,
			cancellationToken => HandleEditValueChangedOrSelectedIndexChangedAISAsync(cancellationToken),
			"loading AIS data");
	}

	private void lookUpEditEstate_EditValueChanged(object sender, EventArgs e)
	{
		ScheduleAisReload();
	}

	private void cmbbulanAIS_SelectedIndexChanged(object sender, EventArgs e)
	{
		ScheduleAisReload();
	}

	private void setahunAIS_EditValueChanged(object sender, EventArgs e)
	{
		ScheduleAisReload();
	}

	private void leremiseAIS_EditValueChanged(object sender, EventArgs e)
	{
		ScheduleAisReload();
	}

	private void leremiseAIS_EditValueChanged_1(object sender, EventArgs e)
	{
		ScheduleAisReload();
	}

    }
}
