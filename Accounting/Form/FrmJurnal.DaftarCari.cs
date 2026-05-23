using Accounting.BusinessLayer;
using Accounting.Model;
using DevExpress.XtraEditors;
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
        private void CariJurnal_Bulan(bool useLoading = true)
        {
            int resultRows = 0;
            int headerRows = 0;
            string periodeSnapshot = leallperiode.Text;
            using var perf = BeginPerfMeasurement(
                "FrmJurnal.CariJurnal_Bulan",
                () => $"periode={periodeSnapshot};resultRows={resultRows};headerRows={headerRows};useLoading={useLoading}");

            IDisposable? loadingScope = null;
            try
            {
                if (useLoading)
                {
                    loadingScope = BeginGlobalLoadingScope();
                }

                MonthlySearchRequest request = new()
                {
                    IdData = CompanyInfo.IDDATA,
                    Periode = leallperiode.Text,
                    NoJurnal = txtfilternojurnal.Text,
                    Tanggal = defiltertanggal.Text,
                    Kode = txtfilterkode.Text,
                    Keterangan = txtfilterketerangan.Text,
                    Jumlah = decimal.Parse(txtfilterjumlah.Text)
                };

                MonthlySearchResult result = jurnalDaftarCariService.SearchMonthly(jurnalRepository, request);
                filter = true;

                if (result.IsFiltered)
                {
                    lblrecordbulan.Visible = true;
                    resultRows = result.DetailRows.Count;
                    headerRows = result.HeaderRows.Count;
                    PencarianJurnal_Bulan = result.DetailRows;
                    JurnalHeader_Filtered = result.HeaderRows;
                    GCHeader.DataSource = result.HeaderRows.Any() ? result.HeaderRows : null;
                    GCDetails.DataSource = result.HeaderRows.Any() ? GCDetails.DataSource : null;
                    ApplyHeaderGridFormat();
                    lblrecordbulan.Text = $"Filter Record : {result.DetailRows.Count}";
                }
                else
                {
                    lblrecordbulan.Visible = false;
                    lblrecordbulan.Text = "Filter Record : 0";
                    resultRows = 0;
                    headerRows = JurnalHeader?.Count() ?? 0;
                    GCHeader.DataSource = JurnalHeader;
                    GCDetails.DataSource = null;
                    PencarianJurnal_Bulan = Enumerable.Empty<JurnalDetailDTO>();
                }

                GCHeader.Focus();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loadingScope?.Dispose();
            }
        }

        private void CariJurnal_Tahun(bool useLoading = true)
        {
            int resultRows = 0;
            string periodRange = $"{(int)daritahun.Value}-{cmbbulan.SelectedIndex + 1:00}_to_{(int)sampaitahun.Value}-{cmbbulan2.SelectedIndex + 1:00}";
            using var perf = BeginPerfMeasurement(
                "FrmJurnal.CariJurnal_Tahun",
                () => $"range={periodRange};resultRows={resultRows};useLoading={useLoading}");

            IDisposable? loadingScope = null;
            try
            {
                if (useLoading)
                {
                    loadingScope = BeginGlobalLoadingScope();
                }

                YearlySearchRequest request = new()
                {
                    IdData = CompanyInfo.IDDATA,
                    DariTahun = (int)daritahun.Value,
                    DariBulan = cmbbulan.SelectedIndex + 1,
                    SampaiTahun = (int)sampaitahun.Value,
                    SampaiBulan = cmbbulan2.SelectedIndex + 1,
                    NoJurnal = txtcarinomor.Text,
                    Tanggal = decaritanggal.Text,
                    Kode = txtcarikode.Text,
                    Keterangan = txtcariketerangan.Text,
                    Jumlah = decimal.Parse(txtcarijumlah.Text)
                };

                YearlySearchResult result = jurnalDaftarCariService.SearchYearly(jurnalRepository, request);
                if (!result.IsValidPeriod)
                {
                    XtraMessageBox.Show("Pilihan Periode Salah", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (result.DetailRows.Any())
                {
                    PencarianJurnal = result.DetailRows;
                    resultRows = result.DetailRows.Count;
                    cmbcariperiode.Properties.Items.Clear();
                    cmbcariperiode.Properties.Items.AddRange(result.Periodes);
                    gridControl1.DataSource = result.DetailRows;
                    ApplyCariGridFormat();
                    lblrecord.Text = "Jumlah Record : " + result.DetailRows.Count;
                    return;
                }

                lblrecord.Text = "Jumlah Record : 0";
                resultRows = 0;
                cmbcariperiode.Properties.Items.Clear();
                gridControl1.DataSource = null;
                PencarianJurnal = Enumerable.Empty<JurnalDetailDTO>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loadingScope?.Dispose();
            }
        }

        private bool HasYearlySearchFilters()
        {
            int jumlah = 0;
            _ = int.TryParse(txtcarijumlah.Text, out jumlah);
            return jumlah > 0
                || !string.IsNullOrEmpty(txtcarikode.Text)
                || !string.IsNullOrEmpty(txtcarinomor.Text)
                || !string.IsNullOrEmpty(txtcariketerangan.Text)
                || decaritanggal.EditValue != null;
        }

        private void ScheduleCariJurnalTahun()
        {
            if (!HasYearlySearchFilters())
            {
                return;
            }

            ScheduleDebounced(
                ref yearlySearchCts,
                cancellationToken =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    CariJurnal_Tahun();
                    return Task.CompletedTask;
                },
                "loading yearly jurnal search");
        }

        private void ScheduleCariJurnalBulan()
        {
            ScheduleDebounced(
                ref monthlySearchCts,
                cancellationToken =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    CariJurnal_Bulan();
                    return Task.CompletedTask;
                },
                "loading monthly jurnal search");
        }

        private void ApplyHeaderGridFormat()
        {
            if (GVHeader.Columns["JURNALID"] == null || GVHeader.Columns["Tanggal"] == null)
            {
                return;
            }

            DisableUserSorting(GVHeader);
            GVHeader.Columns["JURNALID"].Visible = false;
            ApplyDateFormat(GVHeader.Columns["Tanggal"]);
            GVHeader.BestFitColumns();
        }

        private void ApplyCariGridFormat()
        {
            if (gridView1.Columns["REFFID"] == null)
            {
                return;
            }

            DisableUserSorting(gridView1);
            gridView1.Columns["REFFID"].Visible = false;
            gridView1.Columns["HIDREFF"].Visible = false;
            gridView1.Columns["Posted"].Visible = false;
            ApplyDateFormat(gridView1.Columns["Tanggal"]);
            ApplyNumericFormat(gridView1.Columns["Debet"]);
            ApplyNumericFormat(gridView1.Columns["Kredit"]);
            gridView1.BestFitColumns();
        }

        private void txtcarinomor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Tahun();
            }
        }

        private void txtcarikode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Tahun();
            }
        }

        private void txtcarijumlah_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Tahun();
            }
        }

        private void txcariketerangan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Tahun();
            }
        }

        private void sbclear_Click(object sender, EventArgs e)
        {
            txtcarinomor.Text = "";
            lblrecord.Text = "Jumlah Record : 0";
            txtcarikode.Text = "";
            txtcarijumlah.Text = "0";
            txtcariketerangan.Text = "";
            cmbcariperiode.Properties.Items.Clear();
            cmbcariperiode.Text = "";
            decaritanggal.EditValue = null;
            gridControl1.DataSource = null;
            PencarianJurnal = Enumerable.Empty<JurnalDetailDTO>();
            ExportPencarian = Enumerable.Empty<JurnalDetailDTO>();
        }

        private void decaritanggal_EditValueChanged(object sender, EventArgs e)
        {
            if (decaritanggal.EditValue != null)
            {
                ScheduleCariJurnalTahun();
            }

        }
        private void txtfilternojurnal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Bulan();
            }
        }

        private void defiltertanggal_EditValueChanged(object sender, EventArgs e)
        {
            if (defiltertanggal.EditValue != null)
            {
                ScheduleCariJurnalBulan();
            }
        }

        private void txtfilterkode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Bulan();
            }
        }

        private void Sbfilterexport_Click(object sender, EventArgs e)
        {
            using var loadingScope = BeginGlobalLoadingScope();
            try
            {
                if (filter)
                {
                    List<JurnalDetailDTO> exportRows = GetActiveDetailRowsForExport();
                    if (!exportRows.Any())
                    {
                        exportRows = jurnalDaftarCariService.BuildMonthlyExportRows(
                            cefilterlengkap.Checked,
                            PencarianJurnal_Bulan.ToList(),
                            JurnalDetail ?? Enumerable.Empty<JurnalDetailDTO>(),
                            JurnalHeader_Filtered);
                    }

                    exportRows = NormalizeJurnalExportOrder(exportRows);

                    ExportPencarian_Bulan = exportRows;
                    jurnalExcelExportService.ExportJurnalDetails(exportRows, "JurnalFilterPeriode");
                }
                else
                {
                    ExportJurnal_Periode();
                }
            }
            catch (InvalidOperationException ex)
            {
                XtraMessageBox.Show(ex.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }


        private void cmbbulan_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            ScheduleCariJurnalTahun();
        }

        private void daritahun_EditValueChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            ScheduleCariJurnalTahun();
        }

        private void cmbbulan2_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            ScheduleCariJurnalTahun();
        }

        private void sampaitahun_EditValueChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            ScheduleCariJurnalTahun();
        }




        private void sbexportexcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (dtJurnalKasir == null || dtJurnalKasir.Rows.Count == 0)
                    return;

                DataTable dtnew;

                if (!checkEditKAS.Checked && !checkEditBANK.Checked)
                {
                    dtnew = dtJurnalKasir;
                }
                else
                {
                    IEnumerable<DataRow> filtered = Enumerable.Empty<DataRow>();

                    if (checkEditKAS.Checked)
                    {
                        filtered = dtJurnalKasir.AsEnumerable()
                            .Where(y =>
                            {
                                var noJurnal = y.Field<string>("NOJURNAL");
                                return !string.IsNullOrEmpty(noJurnal) &&
                                       (noJurnal.Contains("/KK") || noJurnal.Contains("/KT"));
                            });
                    }
                    else if (checkEditBANK.Checked)
                    {
                        filtered = dtJurnalKasir.AsEnumerable()
                            .Where(y =>
                            {
                                var noJurnal = y.Field<string>("NOJURNAL");
                                return !string.IsNullOrEmpty(noJurnal) &&
                                       (noJurnal.Contains("/BK") || noJurnal.Contains("/BT"));
                            });
                    }

                    dtnew = filtered.Any() ? filtered.CopyToDataTable() : dtJurnalKasir.Clone();
                }

                jurnalExcelExportService.ExportKasirDataTable(dtnew, "JurnalKasir");
            }
            catch (InvalidOperationException ex)
            {
                XtraMessageBox.Show(ex.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Sbfilterclear_Click(object sender, EventArgs e)
        {
            try
            {
                txtfilternojurnal.Text = "";
                defiltertanggal.Text = "";
                txtfilterkode.Text = "";
                txtfilterjumlah.Text = "0";
                txtfilterketerangan.Text = "";
                lblrecordbulan.Visible = false;
                filter = false;
                PencarianJurnal_Bulan = Enumerable.Empty<JurnalDetailDTO>();
                ExportPencarian_Bulan = Enumerable.Empty<JurnalDetailDTO>();
                // GCHeader.DataSource = JurnalHeader;
                // GCDetails.DataSource = null;
                PilihanPeriodeAkuntansi();
                GCHeader.Focus();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void txtfilterjumlah_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Bulan();
            }
        }

        private void txtfilterketerangan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CariJurnal_Bulan();
            }
        }

        private void sbexport_Click(object sender, EventArgs e)
        {
            using var loadingScope = BeginGlobalLoadingScope();
            try
            {
                if (PencarianJurnal.Any())
                {
                    YearlyExportRequest exportRequest = new()
                    {
                        SearchRows = PencarianJurnal,
                        Periode = cmbcariperiode.Text,
                        ExportLengkap = celengkap.Checked
                    };

                    YearlyExportResult exportResult = jurnalDaftarCariService.BuildYearlyExportRows(jurnalRepository, exportRequest);
                    if (!exportResult.IsAllowed)
                    {
                        XtraMessageBox.Show(exportResult.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    List<JurnalDetailDTO> orderedExportRows = NormalizeJurnalExportOrder(exportResult.ExportRows);
                    ExportPencarian = orderedExportRows;
                    ReffID = exportResult.ReffIds;
                    jurnalExcelExportService.ExportJurnalDetails(orderedExportRows, $"{CompanyInfo.IDDATA}_SearchJurnal");
                }
                else
                {
                    XtraMessageBox.Show("Data tidak tersedia", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (InvalidOperationException ex)
            {
                XtraMessageBox.Show(ex.Message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
    }
}
