using Accounting._1.Interface;
using Accounting._2.DataAcces;
using Accounting.BusinessLayer;
using Accounting.JurnalImport.Application;
using Accounting.JurnalImport.Domain;
using Accounting.JurnalImport.Infrastructure;
using Accounting.JurnalImport.Presentation;
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
        private readonly FrmJurnalModuleImportViewModel moduleImportViewModel;

        bool editjurnal, filter = false;
        string periodetujuan = string.Empty, p_iddata = string.Empty;
        int pbulan, ptahun;
        decimal selisihD, selisihK, nilai, nilai2;
        double old_JurnalID;
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
            moduleImportViewModel = JurnalImportModuleFactory.CreateModuleViewModel(CompanyInfo.IDDATA, LoginInfo.userID);

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

            JurnalImportResult result;
            try
            {
                var progress = new Progress<JurnalImportProgress>(p =>
                {
                    System.Diagnostics.Debug.WriteLine($"[Import Jurnal Modul {moduleLabel}] {p.Percent}% - {p.Stage}");
                });
                result = moduleImportViewModel.Import(
                    sourceData,
                    month,
                    year,
                    yearControlValue,
                    p_periode,
                    ToJurnalImportSource(moduleLabel),
                    progress);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(
                    BuildUnexpectedJurnalImportErrorMessage(ex, moduleLabel, p_periode, sourceRows),
                    "Import Jurnal Gagal",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (!result.IsSuccess)
            {
                PlayJurnalImportFailureSound(result);
                XtraMessageBox.Show(BuildJurnalImportResultMessage(result), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            PublishCoaRekalkulasiNotification(result.RecalcJobIds, p_periode, result.ImpactedAccountCodes);

            if (result.HasRecalculationWarning)
            {
                allperiode();
                XtraMessageBox.Show(BuildJurnalImportRecalculationWarningMessage(result), "Import Jurnal Selesai Dengan Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\jurnal_selesai.wav";
            Player.Play();
            allperiode();
            XtraMessageBox.Show($"Import Jurnal {moduleLabel} Selesai \n {FormatJurnalImportElapsed(result.Elapsed)}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static JurnalImportSource ToJurnalImportSource(string moduleLabel)
        {
            if (moduleLabel.Contains("Invent", StringComparison.OrdinalIgnoreCase)
                || moduleLabel.Contains("Inventory", StringComparison.OrdinalIgnoreCase))
            {
                return JurnalImportSource.Inventory;
            }

            if (moduleLabel.Contains("KAS", StringComparison.OrdinalIgnoreCase)
                || moduleLabel.Contains("BANK", StringComparison.OrdinalIgnoreCase))
            {
                return JurnalImportSource.Kasir;
            }

            if (moduleLabel.Contains("HRIS", StringComparison.OrdinalIgnoreCase))
            {
                return JurnalImportSource.Hris;
            }

            if (moduleLabel.Contains("Payroll", StringComparison.OrdinalIgnoreCase))
            {
                return JurnalImportSource.PayrollUmum;
            }

            return JurnalImportSource.Ais;
        }

        private void PlayJurnalImportFailureSound(JurnalImportResult result)
        {
            string sound = result.StatusCode switch
            {
                0 => "jurnal_bedaperiode.wav",
                1 => "jurnal_imbang.wav",
                _ when result.Issues.Any(issue => issue.Code is "PERIOD_LOCKED") => "periode_dikunci.wav",
                _ when result.Issues.Any(issue => issue.Code is "PERIOD_MISMATCH") => "periode_beda.wav",
                _ when result.Issues.Any(issue => issue.Code is "NOJURNAL_EXISTS" or "DUPLICATE_JOURNAL_DIFFERENT_DATE") => "jurnal_duplikasi.wav",
                _ when result.Issues.Any(issue => issue.Code is "ACCOUNT_NOT_FOUND") => "jurnal_daftarperk.wav",
                _ => string.Empty
            };

            if (!string.IsNullOrWhiteSpace(sound))
            {
                Player.SoundLocation = Environment.CurrentDirectory + "\\wav\\" + sound;
                Player.Play();
            }
        }

        private static string BuildJurnalImportResultMessage(JurnalImportResult result)
        {
            if (result.BalanceIssues.Count > 0)
            {
                return result.Message + Environment.NewLine + string.Join(
                    Environment.NewLine,
                    result.BalanceIssues.Select(issue => $"{issue.NoJurnal} | {issue.Tanggal:dd-MMM-yyyy} | Debet {issue.Debet:n2} | Kredit {issue.Kredit:n2} | Selisih {issue.Selisih:n2}"));
            }

            if (result.Issues.Count == 0)
            {
                return result.Message;
            }

            JurnalImportValidationIssue first = result.Issues[0];
            string values = string.Join(
                Environment.NewLine,
                result.Issues
                    .Select(issue => issue.Value)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct());

            return string.IsNullOrWhiteSpace(values) ? first.Message : first.Message + Environment.NewLine + values;
        }

        private static string FormatJurnalImportElapsed(TimeSpan elapsed)
        {
            return $"Waktu Proses : {elapsed.Hours}h {elapsed.Minutes}m {elapsed.Seconds}s {elapsed.Milliseconds}ms";
        }

        private static string BuildJurnalImportRecalculationWarningMessage(JurnalImportResult result)
        {
            return "Import Jurnal Selesai" +
                Environment.NewLine + FormatJurnalImportElapsed(result.Elapsed) +
                Environment.NewLine + Environment.NewLine +
                result.RecalculationWarning;
        }

        private static string BuildUnexpectedJurnalImportErrorMessage(Exception ex, string moduleLabel, string period, int sourceRows)
        {
            string technicalMessage = GetFullExceptionMessage(ex);
            string message = "Import jurnal gagal." +
                Environment.NewLine + Environment.NewLine +
                "Data belum berhasil diimport dan perubahan sudah dibatalkan otomatis." +
                Environment.NewLine + Environment.NewLine +
                "Detail proses:" +
                Environment.NewLine + $"Sumber: {moduleLabel}" +
                Environment.NewLine + $"Periode: {period}" +
                Environment.NewLine + $"Jumlah data: {sourceRows:##,###}" +
                Environment.NewLine + Environment.NewLine +
                "Informasi teknis:" +
                Environment.NewLine + technicalMessage;

            if (technicalMessage.Contains("ORA-01008", StringComparison.OrdinalIgnoreCase))
            {
                message += Environment.NewLine + Environment.NewLine +
                    "Oracle ORA-01008 berarti ada parameter SQL yang belum terisi saat aplikasi menyimpan jurnal.";
            }

            return message;
        }

        private static string GetFullExceptionMessage(Exception ex)
        {
            return ex.InnerException == null
                ? ex.Message
                : ex.Message + Environment.NewLine + ex.InnerException.Message;
        }

    }
}

