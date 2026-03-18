using Accounting.BusinessLayer;
using Accounting.Model;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Accounting.Form
{
    public partial class FrmJurnal
    {
        private void CariJurnal_Bulan()
        {
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);
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
        }

        private void CariJurnal_Tahun()
        {
            try
            {
                using var handle = SplashScreenManager.ShowOverlayForm(this);

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
                    cmbcariperiode.Properties.Items.Clear();
                    cmbcariperiode.Properties.Items.AddRange(result.Periodes);
                    gridControl1.DataSource = result.DetailRows;
                    ApplyCariGridFormat();
                    lblrecord.Text = "Jumlah Record : " + result.DetailRows.Count;
                    return;
                }

                lblrecord.Text = "Jumlah Record : 0";
                cmbcariperiode.Properties.Items.Clear();
                gridControl1.DataSource = null;
                PencarianJurnal = Enumerable.Empty<JurnalDetailDTO>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyHeaderGridFormat()
        {
            if (GVHeader.Columns["JURNALID"] == null || GVHeader.Columns["Tanggal"] == null)
            {
                return;
            }

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

            gridView1.Columns["REFFID"].Visible = false;
            gridView1.Columns["HIDREFF"].Visible = false;
            gridView1.Columns["Posted"].Visible = false;
            ApplyDateFormat(gridView1.Columns["Tanggal"]);
            ApplyNumericFormat(gridView1.Columns["Debet"]);
            ApplyNumericFormat(gridView1.Columns["Kredit"]);
            gridView1.Columns["Periode"].GroupIndex = 0;
            gridView1.Columns["NoJurnal"].GroupIndex = 1;
            gridView1.Columns["BARIS"].SortIndex = 1;
            gridView1.ExpandAllGroups();
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
                CariJurnal_Tahun();
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
                CariJurnal_Bulan();
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
            using var handle = SplashScreenManager.ShowOverlayForm(this);
            try
            {
                if (filter)
                {
                    List<JurnalDetailDTO> exportRows = jurnalDaftarCariService.BuildMonthlyExportRows(
                        cefilterlengkap.Checked,
                        PencarianJurnal_Bulan.ToList(),
                        JurnalDetail ?? Enumerable.Empty<JurnalDetailDTO>(),
                        JurnalHeader_Filtered);

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
            if (Int32.Parse(txtcarijumlah.Text) > 0 || !string.IsNullOrEmpty(txtcarikode.Text) || !string.IsNullOrEmpty(txtcarinomor.Text) || !string.IsNullOrEmpty(txtcariketerangan.Text))
            {
                CariJurnal_Tahun();
            }
        }

        private void daritahun_EditValueChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            if (Int32.Parse(txtcarijumlah.Text) > 0 || !string.IsNullOrEmpty(txtcarikode.Text) || !string.IsNullOrEmpty(txtcarinomor.Text) || !string.IsNullOrEmpty(txtcariketerangan.Text))
            {
                CariJurnal_Tahun();
            }
        }

        private void cmbbulan2_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            if (Int32.Parse(txtcarijumlah.Text) > 0 || !string.IsNullOrEmpty(txtcarikode.Text) || !string.IsNullOrEmpty(txtcarinomor.Text) || !string.IsNullOrEmpty(txtcariketerangan.Text))
            {
                CariJurnal_Tahun();
            }
        }

        private void sampaitahun_EditValueChanged(object sender, EventArgs e)
        {
            gridControl1.DataSource = null;
            if (Int32.Parse(txtcarijumlah.Text) > 0 || !string.IsNullOrEmpty(txtcarikode.Text) || !string.IsNullOrEmpty(txtcarinomor.Text) || !string.IsNullOrEmpty(txtcariketerangan.Text))
            {
                CariJurnal_Tahun();
            }
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
            using var handle = SplashScreenManager.ShowOverlayForm(this);
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

                    ExportPencarian = exportResult.ExportRows;
                    ReffID = exportResult.ReffIds;
                    jurnalExcelExportService.ExportJurnalDetails(exportResult.ExportRows, $"{CompanyInfo.IDDATA}_SearchJurnal");
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
