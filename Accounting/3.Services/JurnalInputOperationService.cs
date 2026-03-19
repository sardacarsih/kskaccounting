using Accounting._1.Interface;
using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Accounting.BusinessLayer
{
    public enum JurnalInputFocusTarget
    {
        None = 0,
        Nomor = 1,
        Tanggal = 2,
        Periode = 3,
        GridDetail = 4
    }

    public sealed class JurnalSaveRequest
    {
        public string IdData { get; init; } = string.Empty;
        public string Nomor { get; init; } = string.Empty;
        public DateTime? Tanggal { get; init; }
        public string Periode { get; init; } = string.Empty;
        public string PeriodeLockCheck { get; init; } = string.Empty;
        public bool IsJurnalBalik { get; init; }
        public bool IsEdit { get; init; }
        public double OldJurnalId { get; init; }
        public bool FromModuleAis { get; init; }
        public bool FromModuleKasir { get; init; }
        public bool FromModuleInv { get; init; }
        public decimal DebetTotal { get; init; }
        public decimal KreditTotal { get; init; }
        public IReadOnlyList<JurnalDetailAdd> Details { get; init; } = [];
        public string UserId { get; init; } = string.Empty;
    }

    public sealed class JurnalSaveResult
    {
        public bool IsSuccess { get; private init; }
        public string Message { get; private init; } = string.Empty;
        public JurnalInputFocusTarget FocusTarget { get; private init; } = JurnalInputFocusTarget.None;

        public static JurnalSaveResult Success()
        {
            return new JurnalSaveResult { IsSuccess = true };
        }

        public static JurnalSaveResult Fail(string message, JurnalInputFocusTarget focusTarget = JurnalInputFocusTarget.None)
        {
            return new JurnalSaveResult
            {
                IsSuccess = false,
                Message = message,
                FocusTarget = focusTarget
            };
        }
    }

    public sealed class JurnalInputOperationService
    {
        private readonly IJurnalEntryRepository jurnalEntryRepository;

        public JurnalInputOperationService(IJurnalEntryRepository repository)
        {
            jurnalEntryRepository = repository;
        }

        public JurnalSaveResult Save(JurnalSaveRequest request)
        {
            string lockStatus = jurnalEntryRepository.GetLockStatus(request.IdData, request.PeriodeLockCheck);
            if (lockStatus == "Y")
            {
                return JurnalSaveResult.Fail($"Transaksi tidak dapat disimpan diperiode ini...!!!\nPeriode Akuntansi : {request.Periode} Telah Dikunci.");
            }

            if (string.IsNullOrEmpty(request.Nomor))
            {
                return JurnalSaveResult.Fail("Nomor Jurnal tidak boleh kosong", JurnalInputFocusTarget.Nomor);
            }

            if (request.Nomor.Length > 30)
            {
                return JurnalSaveResult.Fail($"Nomor Jurnal MAX 30 Karakter,panjang karakter sekarang : {request.Nomor.Length}", JurnalInputFocusTarget.Nomor);
            }

            if (!request.Tanggal.HasValue)
            {
                return JurnalSaveResult.Fail("Tanggal tidak boleh kosong", JurnalInputFocusTarget.Tanggal);
            }

            if (string.IsNullOrEmpty(request.Periode))
            {
                return JurnalSaveResult.Fail("Periode tidak boleh kosong", JurnalInputFocusTarget.Periode);
            }

            if (request.Details.Count > 1)
            {
                IEnumerable<JurnalDetailAdd> kodeKosong = request.Details.Where(detail => string.IsNullOrEmpty(detail.Kode) && ((detail.Debet ?? 0) > 0 || (detail.Kredit ?? 0) > 0));
                if (kodeKosong.Any())
                {
                    string barisText = string.Join(Environment.NewLine, kodeKosong.Select(detail => detail.BARIS));
                    return JurnalSaveResult.Fail($"Baris \n{barisText}\nBelum ada Kode", JurnalInputFocusTarget.GridDetail);
                }

                IEnumerable<object> ketmax200 = request.Details
                    .Where(detail => !string.IsNullOrEmpty(detail.Kode) && (detail.Keterangan?.Length ?? 0) > 200)
                    .Select(detail => new { detail.BARIS, Panjang = detail.Keterangan.Length });

                if (ketmax200.Any())
                {
                    string detailText = string.Join(Environment.NewLine, ketmax200);
                    return JurnalSaveResult.Fail($"{detailText}\nKeterangan Lebih dari 200 Karakter", JurnalInputFocusTarget.GridDetail);
                }
            }

            if (request.DebetTotal == 0 && request.KreditTotal == 0)
            {
                return JurnalSaveResult.Fail("Nilai Transaksi Jurnal Belum diisi");
            }

            if (request.DebetTotal != request.KreditTotal)
            {
                decimal selisih = Math.Abs(request.DebetTotal - request.KreditTotal);
                return JurnalSaveResult.Fail($"Jurnal tidak seimbang,\nselisih {selisih.ToString("N2", CultureInfo.InvariantCulture)}");
            }

            if (!request.IsEdit && !request.Nomor.Contains("/ND") && !request.Nomor.Contains("/NK"))
            {
                bool nomorExist = jurnalEntryRepository.CekNoJurnalExist_input(request.IdData, request.Nomor.ToUpper(), request.Periode);
                if (nomorExist)
                {
                    return JurnalSaveResult.Fail($"Nomor Jurnal : {request.Nomor} Sudah ada...!!!", JurnalInputFocusTarget.Nomor);
                }
            }

            try
            {
                string sumber = ResolveSumber(request.FromModuleAis, request.FromModuleKasir, request.FromModuleInv);
                string isRe = request.IsJurnalBalik ? "Y" : "T";
                string nomorHid = request.IdData + request.Periode + request.Nomor + request.Tanggal.Value.ToString("yyMMdd");
                string hostName = System.Net.Dns.GetHostName();
                string ipAddress = ToolsServices.GetLocalIPAddress();

                JurnalHeaderAdd jurnalHeader = new()
                {
                    HID = nomorHid,
                    NOJURNAL = request.Nomor,
                    TANGGAL = request.Tanggal.Value,
                    PERIODE = request.Periode,
                    IDDATA = request.IdData,
                    USERID = request.UserId,
                    SUMBER = sumber,
                    ISRE = isRe,
                    PC = hostName,
                    IP_ADD = ipAddress
                };

                List<JurnalDetailAdd> jurnalDetails = request.Details.Where(detail => !string.IsNullOrEmpty(detail.Kode)).ToList();

                if (request.IsEdit)
                    jurnalEntryRepository.UpdateJurnalMasterDetail(request.OldJurnalId, jurnalHeader, jurnalDetails);
                else
                    jurnalEntryRepository.InsertJurnalMasterDetail(jurnalHeader, jurnalDetails);

                return JurnalSaveResult.Success();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("ORA-00001"))
                {
                    return JurnalSaveResult.Fail("Duplikasi Data");
                }

                return JurnalSaveResult.Fail(ex.Message);
            }
        }

        private static string ResolveSumber(bool fromModuleAis, bool fromModuleKasir, bool fromModuleInv)
        {
            string sumber = "JV";

            if (fromModuleAis)
            {
                sumber = "AIS";
            }

            if (fromModuleKasir)
            {
                sumber = "KASIR";
            }

            if (fromModuleInv)
            {
                sumber = "INV";
            }

            return sumber;
        }
    }
}
