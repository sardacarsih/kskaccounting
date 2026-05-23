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
    public enum ImportModule
    {
        None,
        AIS,
        Kasir,
        Inventory
    }

    public partial class FrmJurnal : XtraForm
    {
        private static readonly string[] PayrollHiddenColumns = ["NOJURNAL", "TANGGAL", "POSTED", "PERIODE"];

        private RepositoryItemTextEdit? repositoryItemTextEditKode;
         
        private readonly IJurnalDomainRepository jurnalRepository;
        private readonly IInputJurnalUseCase inputJurnalUseCase;
        private readonly IDaftarJurnalUseCase daftarJurnalUseCase;
        private readonly ICariJurnalUseCase cariJurnalUseCase;
        private readonly IImportJurnalUseCase importJurnalUseCase;

        private readonly UcJurnalInputTab inputTabHost;
        private readonly UcJurnalDaftarTab daftarTabHost;
        private readonly UcJurnalCariTab cariTabHost;
        private readonly UcJurnalKasirTab kasirTabHost;
        private readonly UcJurnalAisTab aisTabHost;
        private readonly UcJurnalInventoryTab inventoryTabHost;
        private readonly UcJurnalHrisTab hrisTabHost;
        private readonly JurnalInputOperationService jurnalInputOperationService;
        private readonly JurnalPayrollUmumService jurnalPayrollUmumService;
        private readonly JurnalDaftarCariService jurnalDaftarCariService;
        private readonly JurnalExcelExportService jurnalExcelExportService;
        private readonly JurnalImportSelectionService jurnalImportSelectionService;

        bool editjurnal, filter = false;
        string periodetujuan = string.Empty, p_iddata = string.Empty;
        int pbulan, ptahun;
        double selisihD, selisihK, nilai, nilai2, old_JurnalID;
        DateTime? old_HeaderVersionUtc;
        private SoundPlayer? _player;
        private SoundPlayer Player => _player ??= new SoundPlayer();
        private bool suppressKodeLeaveValidation;
        private bool isSaveOrUpdateInProgress;
        private bool isReindexingBaris;
        private bool kasirLoaded, aisLoaded, inventoryLoaded, hrisLoaded;
        private bool isInitializing;
        DataTable dtJurnalKasir = new(), dtAISDetail = new(), dtJurnalInventory = new();

        IEnumerable<DTOCOAAktif> ListCoaAktif = Enumerable.Empty<DTOCOAAktif>();
        IQueryable<JurnalHeaderDTO>? JurnalHeader = null;
        IQueryable<JurnalDetailDTO>? JurnalDetail = null;
        IEnumerable<JurnalInventoryHeaderDTO>? InventoryJurnalHeader = null;
        IEnumerable<JurnalKasirHeaderDTO>? KasirJurnalHeader = null;
        IEnumerable<JurnalDetailDTO> PencarianJurnal = Enumerable.Empty<JurnalDetailDTO>();
        IEnumerable<JurnalDetailDTO> ExportPencarian = Enumerable.Empty<JurnalDetailDTO>();
        IEnumerable<JurnalDetailDTO> PencarianJurnal_Bulan = Enumerable.Empty<JurnalDetailDTO>();
        IEnumerable<JurnalDetailDTO> ExportPencarian_Bulan = Enumerable.Empty<JurnalDetailDTO>();
        List<JurnalDetailReffID> ReffID = new();
        List<JurnalHeaderDTO> JurnalHeader_Filtered = new();
        BindingList<JurnalDetailAdd> InputJurnalDetail = new();

        IEnumerable<AIS_JURNAL> aisJurnal = new List<AIS_JURNAL>();
        IEnumerable<JurnalKomponen> komponenjurnal = Enumerable.Empty<JurnalKomponen>();
        IEnumerable<AIS_JURNAL_FINAL> jurnalfinalAIS = new List<AIS_JURNAL_FINAL>();
        IEnumerable<AIS_JURNAL_FINAL> jurnalfinalHRIS = new List<AIS_JURNAL_FINAL>();
        List<AIS_JURNAL_FINAL> payrollumum = new();


        string[] Bulan = { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" };
        public FrmJurnal() : this(new JurnalDomainRepository())
        {
        }

        internal FrmJurnal(IJurnalDomainRepository repository)
        {
            jurnalRepository = repository;
            inputJurnalUseCase = new InputJurnalUseCase(jurnalRepository);
            daftarJurnalUseCase = new DaftarJurnalUseCase(jurnalRepository);
            cariJurnalUseCase = new CariJurnalUseCase(jurnalRepository);
            importJurnalUseCase = new ImportJurnalUseCase(jurnalRepository);

            inputTabHost = new UcJurnalInputTab(inputJurnalUseCase);
            daftarTabHost = new UcJurnalDaftarTab(daftarJurnalUseCase);
            cariTabHost = new UcJurnalCariTab(cariJurnalUseCase);
            kasirTabHost = new UcJurnalKasirTab(importJurnalUseCase);
            aisTabHost = new UcJurnalAisTab(importJurnalUseCase);
            inventoryTabHost = new UcJurnalInventoryTab(importJurnalUseCase);
            hrisTabHost = new UcJurnalHrisTab(importJurnalUseCase);
            jurnalInputOperationService = new JurnalInputOperationService(jurnalRepository);
            jurnalPayrollUmumService = new JurnalPayrollUmumService();
            jurnalDaftarCariService = new JurnalDaftarCariService();
            jurnalExcelExportService = new JurnalExcelExportService();
            jurnalImportSelectionService = new JurnalImportSelectionService();

            InitializeComponent();
            InitializeTabHosts();

            InitializeFormState();
            RegisterEventHandlers();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterEventHandlers();
            resizeDebounceTimer?.Dispose();
            _player?.Dispose();
            DisposeLoadingInfrastructure();

            base.OnFormClosed(e);
        }

        internal static string FormatPeriod(int month, int year) => $"{month:00}/{year}";

        private static bool HasJurnalInputWriteAccess()
        {
            return AuthorizationService.CanCreateJurnal() || AuthorizationService.CanUpdateJurnal();
        }

        private bool CanSaveCurrentJurnal()
        {
            return editjurnal
                ? AuthorizationService.CanUpdateJurnal()
                : AuthorizationService.CanCreateJurnal();
        }

        private bool TryEnsureJurnalAccess(Action ensureAction)
        {
            try
            {
                ensureAction();
                return true;
            }
            catch (InvalidOperationException ex)
            {
                XtraMessageBox.Show(ex.Message, "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void ApplyJurnalAuthorizationState()
        {
            if (JDgridView != null)
            {
                bool canEditInput = HasJurnalInputWriteAccess();
                JDgridView.OptionsBehavior.Editable = canEditInput;
                Hapus.OptionsColumn.AllowEdit = canEditInput;
            }

            if (sbubah != null)
            {
                sbubah.Enabled = AuthorizationService.CanUpdateJurnal();
            }

            if (sbhapus != null)
            {
                sbhapus.Enabled = AuthorizationService.CanDeleteJurnal();
            }

            if (SBSimpan != null)
            {
                SBSimpan.Enabled = !isSaveOrUpdateInProgress && CanSaveCurrentJurnal();
            }
        }

        private bool ValidatePeriodNotLocked(string iddata, string periode, string periodeDisplay)
        {
            Acct.KunciPeriode = jurnalRepository.GetLockStatus(iddata, periode);
            if (Acct.KunciPeriode != "Y") return true;

            XtraMessageBox.Show(
                $"Tidak dapat melakukan proses pada periode ini...!!!\nPeriode Akuntansi : {periodeDisplay} Telah Dikunci.",
                "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private bool ValidatePeriodNotLockedWithSound(string iddata, string periode, string periodeDisplay)
        {
            Acct.KunciPeriode = jurnalRepository.GetLockStatus(iddata, periode);
            if (Acct.KunciPeriode != "Y") return true;

            Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\periode_dikunci.wav";
            Player.Play();
            XtraMessageBox.Show(
                $"Tidak dapat melakukan proses import Jurnal pada periode ini...!!!\nPeriode Akuntansi : {periodeDisplay} Telah Dikunci.",
                "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        internal static void ApplyNumericFormat(GridColumn column, string formatString = "n2")
        {
            column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            column.DisplayFormat.FormatString = formatString;
        }

        internal static void ApplyDateFormat(GridColumn column, string formatString = "dd-MMM-yyyy")
        {
            column.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
            column.DisplayFormat.FormatString = formatString;
        }

        internal static void ApplyNumericSummary(GridColumn column, string fieldName, string formatString = "{0:N2}")
        {
            column.Summary.Clear();
            column.Summary.Add(SummaryItemType.Sum, fieldName, formatString);
        }

        private void ExecuteImportJurnal(DataTable sourceData, int month, int year, int yearControlValue, string moduleLabel)
        {
            if (!TryEnsureJurnalAccess(AuthorizationService.EnsureCanImportJurnal))
            {
                return;
            }

            string p_periode = FormatPeriod(month, year);
            int sourceRows = sourceData?.Rows.Count ?? 0;
            using var perf = BeginPerfMeasurement(
                "FrmJurnal.ExecuteImportJurnal",
                () => $"module={moduleLabel};periode={p_periode};sourceRows={sourceRows}");

            using var loadingScope = BeginGlobalLoadingScope();

            Stopwatch watch = new();
            watch.Start();

            int pexist = jurnalRepository.CekPeriodeExist(CompanyInfo.IDDATA, p_periode);
            if (pexist == 0)
            {
                jurnalRepository.CreateNextPeriode(CompanyInfo.IDDATA, month - 1, year);
            }

            Hapus_Data_Table_Tmp();
            jurnalRepository.SaveUsingOracleBulkCopy("ACCT_JURNAL_TMP", sourceData);

            var KODENULL = jurnalRepository.CekJurnal_KODENULL();
            if (KODENULL.Rows.Count > 0)
            {
                List<string> list = KODENULL.AsEnumerable()
                       .Select(r => r.Field<string>("NOJURNAL"))
                       .Where(x => !string.IsNullOrEmpty(x))
                       .Select(x => x!)
                       .ToList();
                var daftarKODENULL = string.Join(Environment.NewLine, list);
                XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                    "\nKode Jurnal belum diisi sebanyak " + KODENULL.Rows.Count.ToString("##,###") + " Nomor." +
                    "\n" + daftarKODENULL, "Kode Jurnal Kosong", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dup_nojurnal = jurnalRepository.CekNoJurnalExist();
            if (dup_nojurnal.Rows.Count > 0)
            {
                Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_duplikasi.wav";
                Player.Play();
                List<string> list = dup_nojurnal.AsEnumerable()
                       .Select(r => r.Field<string>("NOJURNAL"))
                       .Where(x => !string.IsNullOrEmpty(x))
                       .Select(x => x!)
                       .ToList();
                var daftarnojurnalexist = string.Join(Environment.NewLine, list);
                XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                    "\nDuplikasi NoJurnal sebanyak " + dup_nojurnal.Rows.Count.ToString("##,###") + " Nomor." +
                    "\n" + daftarnojurnalexist, "Nomor Jurnal Telah digunakan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var duplikasi = jurnalRepository.CekDuplikasiJurnal();
            if (duplikasi.Rows.Count > 0)
            {
                Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_duplikasi.wav";
                Player.Play();
                List<string> list = duplikasi.AsEnumerable()
                       .Select(r => r.Field<string>("NOJURNAL"))
                       .Where(x => !string.IsNullOrEmpty(x))
                       .Select(x => x!)
                       .ToList();
                var daftarnojurnal = string.Join(Environment.NewLine, list);
                XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                    "\nDuplikasi NoJurnal pada nomor jurnal yang sama tetapi beda tanggal sebanyak " + duplikasi.Rows.Count.ToString("##,###") + " Nomor." +
                    "\nBerikut ini NoJurnalnya : \n\n" + daftarnojurnal, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var akun = jurnalRepository.CekAkunMaster(yearControlValue);
            if (akun.Rows.Count > 0)
            {
                Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_daftarperk.wav";
                Player.Play();
                List<string> list = akun.AsEnumerable()
                       .Select(r => r.Field<string>("ASAL"))
                       .Where(x => !string.IsNullOrEmpty(x))
                       .Select(x => x!)
                       .ToList();
                var daftarakun = string.Join(Environment.NewLine, list);
                XtraMessageBox.Show("Import Jurnal di Batalkan \n" +
                    "\nJumlah Kode tidak terdaftar sebanyak " + akun.Rows.Count.ToString("##,###") + " Akun." +
                    "\nKode Akun dibawah ini belum ada di Daftar Perkiraan \n" + daftarakun, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var sukses = jurnalRepository.ImportJurnalParsial(CompanyInfo.IDDATA, month, year, p_periode);
            if (sukses == 0)
            {
                Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_bedaperiode.wav";
                Player.Play();
                XtraMessageBox.Show("Import Jurnal di Batalkan \nCek Periode Pada Lembar Excel Double ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (sukses == 1)
            {
                Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_imbang.wav";
                Player.Play();
                XtraMessageBox.Show("Import Jurnal di Batalkan \nJurnal Tidak Seimbang ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            jurnalRepository.RekalkulasiSaldo(CompanyInfo.IDDATA, month, year, LoginInfo.userID);
            watch.Stop();

            TimeSpan timeSpan = watch.Elapsed;
            string waktuproses = string.Format("Waktu Proses : {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

            Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_selesai.wav";
            Player.Play();
            allperiode();
            XtraMessageBox.Show($"Import Jurnal {moduleLabel} Selesai \n {waktuproses}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}

