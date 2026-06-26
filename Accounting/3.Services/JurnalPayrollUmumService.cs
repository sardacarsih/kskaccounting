using Accounting._1.Interface;
using Accounting.Model;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Accounting.BusinessLayer
{
    public sealed class JurnalPayrollUmumRequest
    {
        public string IdData { get; init; } = string.Empty;
        public int Tahun { get; init; }
        public int Bulan { get; init; }
        public int Remise { get; init; }
        public string BulanText { get; init; } = string.Empty;
    }

    public sealed class JurnalPayrollUmumService
    {
        public async Task<List<AIS_JURNAL_FINAL>> BuildPayrollUmumAsync(IJurnalImportRepository importRepository, JurnalPayrollUmumRequest request)
        {
            if (string.IsNullOrEmpty(request.IdData) || request.Tahun <= 0 || request.Bulan < 1 || request.Bulan > 12 || request.Remise <= 0)
            {
                return [];
            }

            int periodeInt = Convert.ToInt32($"{request.Tahun}{request.Bulan:00}");
            string periodeString = $"{request.Bulan:00}/{request.Tahun}";
            string ket2 = $"R{request.Remise} {request.BulanText} {request.Tahun}";
            DateTime tanggalJurnal = new DateTime(request.Tahun, request.Bulan, 1).AddMonths(1).AddDays(-1);

            List<AIS_JURNAL_FINAL> payrollUmum = await importRepository.GetPayrollforJurnalAsync(request.IdData, periodeInt, request.Remise, request.Tahun);
            if (payrollUmum == null || payrollUmum.Count == 0)
            {
                return [];
            }

            List<BPJS_INFO_DTO> bpjsUmum = await importRepository.GetBPJSUmumAsync(request.IdData, periodeInt, request.Remise);

            foreach (AIS_JURNAL_FINAL item in payrollUmum)
            {
                item.TANGGAL = tanggalJurnal;
                item.POSTED = true;
                item.PERIODE = periodeString;
                item.KETERANGAN = $"{item.KETERANGAN} {ket2}";
            }

            decimal totalDebet = payrollUmum.Sum(item => item.DEBET);
            decimal potBpjsTk = bpjsUmum.Any() ? bpjsUmum.Sum(item => item.POT_BPJS_TK) : 0m;
            decimal potBpjsKes = bpjsUmum.Any() ? bpjsUmum.Sum(item => item.POT_BPJS_KESEHATAN) : 0m;
            decimal potBpjsPensiun = bpjsUmum.Any() ? bpjsUmum.Sum(item => item.POT_BPJS_TK_PENSIUN) : 0m;
            decimal operasional = totalDebet - (potBpjsTk + potBpjsKes + potBpjsPensiun);

            List<AIS_JURNAL_FINAL> generatedRows = [];

            Dictionary<string, (string Kode, string Nama)> defaults = new Dictionary<string, (string Kode, string Nama)>(StringComparer.OrdinalIgnoreCase);
            using (IDbConnection dbConnection = new OracleConnection(LoginInfo.OracleConnString))
            {
                dbConnection.Open();
                string query = @"
                    SELECT d.NAMA, d.KODEACC, c.NAMAACC 
                    FROM ACCT_DEFAULT d
                    LEFT JOIN ACCT_COA c ON c.KODEACC = d.KODEACC AND c.IDDATA = d.IDDATA AND c.TAHUN = :tahun
                    WHERE d.IDDATA = :idData";
                var list = dbConnection.Query(query, new { idData = request.IdData, tahun = request.Tahun });
                foreach (var item in list)
                {
                    string name = item.NAMA;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        defaults[name] = ((string)item.KODEACC, (string)(item.NAMAACC ?? item.KODEACC));
                    }
                }
            }

            (string Kode, string Nama) GetAccount(string key, string defaultKode, string defaultNama)
            {
                if (defaults.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val.Kode))
                {
                    return val;
                }
                return (defaultKode, defaultNama);
            }

            (string Kode, string Nama) GetAccountWithFallback(string key, (string Kode, string Nama) fallback)
            {
                if (defaults.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val.Kode))
                {
                    return val;
                }
                return fallback;
            }

            var astekAcc = GetAccount("TUNJANGAN_ASTEK", "70.10001.005", "Tunjangan Astek");
            var potBpjsTkAcc = GetAccountWithFallback("HRIS_POT_BPJS_TK", astekAcc);
            var potBpjsKesAcc = GetAccountWithFallback("HRIS_POT_BPJS_KES", astekAcc);
            var potBpjsPensiunAcc = GetAccountWithFallback("HRIS_POT_BPJS_PENSIUN", astekAcc);
            var gajiYmhdAcc = GetAccount("GAJI_DAN_UPAH_YMH_DIBAYAR", "33.00001.001", "Gaji dan Upah YMH dibayar");

            if (request.Remise == 1)
            {
                generatedRows.Add(new AIS_JURNAL_FINAL
                {
                    TANGGAL = tanggalJurnal,
                    KODE = gajiYmhdAcc.Kode,
                    REKENING = gajiYmhdAcc.Nama,
                    KREDIT = operasional,
                    KETERANGAN = "Bayar Upah THL + Premi Div Kantor, " + ket2,
                    POSTED = true,
                    PERIODE = periodeString
                });
            }
            else
            {
                generatedRows.Add(new AIS_JURNAL_FINAL
                {
                    TANGGAL = tanggalJurnal,
                    KODE = potBpjsTkAcc.Kode,
                    REKENING = potBpjsTkAcc.Nama,
                    KREDIT = potBpjsTk,
                    KETERANGAN = "Potongan JHT Karyawan, " + ket2,
                    POSTED = true,
                    PERIODE = periodeString
                });

                generatedRows.Add(new AIS_JURNAL_FINAL
                {
                    TANGGAL = tanggalJurnal,
                    KODE = potBpjsKesAcc.Kode,
                    REKENING = potBpjsKesAcc.Nama,
                    KREDIT = potBpjsKes,
                    KETERANGAN = "Potongan BPJS Karyawan, " + ket2,
                    POSTED = true,
                    PERIODE = periodeString
                });

                generatedRows.Add(new AIS_JURNAL_FINAL
                {
                    TANGGAL = tanggalJurnal,
                    KODE = potBpjsPensiunAcc.Kode,
                    REKENING = potBpjsPensiunAcc.Nama,
                    KREDIT = potBpjsPensiun,
                    KETERANGAN = "Potongan PENSIUN Karyawan, " + ket2,
                    POSTED = true,
                    PERIODE = periodeString
                });

                generatedRows.Add(new AIS_JURNAL_FINAL
                {
                    TANGGAL = tanggalJurnal,
                    KODE = gajiYmhdAcc.Kode,
                    REKENING = gajiYmhdAcc.Nama,
                    KREDIT = operasional,
                    KETERANGAN = "Bayar Upah THL + Premi Div Kantor, " + ket2,
                    POSTED = true,
                    PERIODE = periodeString
                });
            }

            payrollUmum.AddRange(generatedRows);

            int nomor = 0;
            foreach (AIS_JURNAL_FINAL item in payrollUmum)
            {
                item.NO = ++nomor;
            }

            return payrollUmum;
        }

        public List<JurnalDetailAdd> BuildDetailRows(IEnumerable<AIS_JURNAL_FINAL> rows)
        {
            return rows.Select(ConvertToJurnalDetailAdd).ToList();
        }

        private static JurnalDetailAdd ConvertToJurnalDetailAdd(AIS_JURNAL_FINAL aisJurnal)
        {
            return new JurnalDetailAdd
            {
                BARIS = aisJurnal.NO,
                Kode = aisJurnal.KODE,
                Rekening = aisJurnal.REKENING,
                Debet = aisJurnal.DEBET,
                Kredit = aisJurnal.KREDIT,
                Keterangan = aisJurnal.KETERANGAN,
                Posted = aisJurnal.POSTED ? "Yes" : "No",
                NoJurnal = aisJurnal.NOJURNAL,
                Periode = aisJurnal.PERIODE
            };
        }
    }
}
