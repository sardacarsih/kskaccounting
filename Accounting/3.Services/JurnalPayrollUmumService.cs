using Accounting._1.Interface;
using Accounting.Model;
using System;
using System.Collections.Generic;
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

            if (request.Remise == 1)
            {
                generatedRows.Add(new AIS_JURNAL_FINAL
                {
                    TANGGAL = tanggalJurnal,
                    KODE = "33.00001.001",
                    REKENING = "Gaji dan Upah YMH dibayar",
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
                    KODE = "70.10001.005",
                    REKENING = "Tunjangan Astek",
                    KREDIT = potBpjsTk,
                    KETERANGAN = "Potongan JHT Karyawan, " + ket2,
                    POSTED = true,
                    PERIODE = periodeString
                });

                generatedRows.Add(new AIS_JURNAL_FINAL
                {
                    TANGGAL = tanggalJurnal,
                    KODE = "70.10001.005",
                    REKENING = "Tunjangan Astek",
                    KREDIT = potBpjsKes,
                    KETERANGAN = "Potongan BPJS Karyawan, " + ket2,
                    POSTED = true,
                    PERIODE = periodeString
                });

                generatedRows.Add(new AIS_JURNAL_FINAL
                {
                    TANGGAL = tanggalJurnal,
                    KODE = "70.10001.005",
                    REKENING = "Tunjangan Astek",
                    KREDIT = potBpjsPensiun,
                    KETERANGAN = "Potongan PENSIUN Karyawan, " + ket2,
                    POSTED = true,
                    PERIODE = periodeString
                });

                generatedRows.Add(new AIS_JURNAL_FINAL
                {
                    TANGGAL = tanggalJurnal,
                    KODE = "33.00001.001",
                    REKENING = "Gaji dan Upah YMH dibayar",
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
