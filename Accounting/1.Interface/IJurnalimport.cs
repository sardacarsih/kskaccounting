using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting._1.Interface
{
    public interface IJurnalimport
    {
        Task<List<Estate>> GetEstateAsync(string idData);
        Task<List<Division>> GetDivisionsAsync(string idData,string estateId, int periode, int remise);
        Task<List<AIS_JURNAL>> GetAISforJurnalAsync(string idData, string estateId, int periode, int remise, int tahun, string periodes);
        Task<List<JurnalKomponen>> GetAISforJurnalKomponenAsync(string idData, string estateId, int periode, int remise);
        Task<List<AIS_JURNAL_FINAL>> GetPayrollforJurnalAsync(string idData, int periode, int remise, int tahun);
        Task<List<BPJS_INFO_DTO>> GetBPJSUmumAsync(string idData, int periode, int remise);
        Task<IEnumerable<BKM_KOMPONEN_KHT>> GetKOMPONEN_KHTAsync(string idData, int periode, int remise, string divisiid);
        Task<IEnumerable<BPJS_INFO_DTO>> GetBPJSInfoListAsync(int periode, string divisiid);
        Task<List<SlipGaji_DTO>> viewDaftarGajidanTunjangan_BulananAsync(string idData, string estateId, int periode);
        Task<List<FIN_POTONGAN_KANTOR>> viewPotonganKantorAsync(string idData, string estateId, int periode);
        Task<List<ALOKASI_JURNAL_DTO>> AlokasiJurnalAsync(string idData);
        Task<List<AIS_JURNAL_FINAL>> HitungLampiranKASAsync(
            List<SlipGaji_DTO> sLIPGAJIlist,
            List<FIN_POTONGAN_KANTOR> pot_KANTOR,
            List<ALOKASI_JURNAL_DTO> alokasijurnal,
            IEnumerable<DTOCOAAktif> ListCoaAktif,
             string p_periodeket,
            string p_periode_str,
            string p_estate,
            DateTime TanggalJurnal);
    }

    public class JurnalKomponen
    {
        public bool IsBorongan { get; set; }
        public string Divisi { get; set; }
        public string Keterangan { get; set; }
        public decimal Jumlah { get; set; }
        public string Sisi { get; set; }
    }

    public class FIN_LAMPIRANKAS_DTO
    {
        public int NO { get; set; }
        public string ACCOUNT { get; set; }
        public string KETERANGAN { get; set; }
        public decimal DEBET { get; set; }
        public decimal KREDIT { get; set; }
    }
    public class ALOKASI_JURNAL_DTO
    {
        public string KODE { get; set; }
        public string KETERANGAN { get; set; }
        public string ACJURNAL { get; set; }
        public string BIAYA_UMUM { get; set; }
    }

    public class FIN_POTONGAN_KANTOR
    {
        public DateTime TANGGAL { get; set; }
        public string NIK { get; set; }
        public string NAMA { get; set; }
        public string POTONGAN { get; set; }
        public string KETERANGAN { get; set; }
        public decimal JUMLAH { get; set; }
        public string ACKODE { get; set; }
    }

    public class SlipGaji_DTO
    {
        public int PERIODE { get; set; }
        public int REMISE { get; set; }
        public string IDDATA { get; set; }
        public string ESTATE { get; set; }
        public string DIVISIID { get; set; }
        public int NO { get; set; }
        public string NIK { get; set; }
        public string NAMA { get; set; }
        public string REKENING { get; set; }
        public string DIVISI { get; set; }
        public string JABATAN { get; set; }
        public string GOL { get; set; }
        public decimal GAJI_POKOK { get; set; }
        public decimal TUNJANGAN
        {
            get
            {
                return TJG_JABATAN + TJG_LAPANGAN + TJG_NONLAPANGAN +
                       TJG_LUASAN + TJG_TELP + TJG_PERABOT + TJG_KEBERSIHAN +
                       INSENTIF + UMAKAN + SKENDARAAN;
            }
        }
        public decimal LEMBUR_PREMI { get; set; }
        public decimal POT_TUNJANGAN { get; set; }
        public string NO_BPJS_TK { get; set; }
        public decimal BPJS_KES { get; set; }
        public decimal BPJS_JHT { get; set; }
        public decimal BPJS_JP { get; set; }
        public decimal BPJS
        {
            get
            {
                return BPJS_KES + BPJS_JHT + BPJS_JP;
            }
        }
        public decimal KANTOR { get; set; }
        public decimal KOPERASI { get; set; }
        public decimal LAIN { get; set; }
        public decimal GAJI_BERSIH
        {
            get
            {
                return GAJI_POKOK + TUNJANGAN + LEMBUR_PREMI - (POT_TUNJANGAN + BPJS + KANTOR + KOPERASI + LAIN);
            }

        }

        public decimal TJG_JABATAN { get; set; }
        public decimal TJG_LAPANGAN { get; set; }
        public decimal TJG_NONLAPANGAN { get; set; }
        public decimal TJG_LUASAN { get; set; }
        public decimal TJG_TELP { get; set; }
        public decimal TJG_PERABOT { get; set; }
        public decimal TJG_KEBERSIHAN { get; set; }
        public decimal INSENTIF { get; set; }
        public decimal UMAKAN { get; set; }
        public decimal SKENDARAAN { get; set; }

        public string ALOKASI_JURNAL { get; set; }
    }

    public class BPJS_INFO_DTO
    {
        public string DIVISIID { get; set; }
        public string NIK { get; set; }
        public string NAMA { get; set; }
        public string STATUS { get; set; }
        public decimal POT_BPJS_TK { get; set; }
        public decimal POT_BPJS_KESEHATAN { get; set; }
        public decimal POT_BPJS_TK_PENSIUN { get; set; }
        public decimal TOTAL
        {
            get
            {
                return POT_BPJS_TK + POT_BPJS_KESEHATAN + POT_BPJS_TK_PENSIUN;
            }
        }
    }

    public class BKM_KOMPONEN_KHT
    {
        // Properties matching columns from KomponenKHT_CTE
        public string IDDATA { get; set; }
        public string DIVISIID { get; set; }
        public string NIK { get; set; }
        public string NAMA { get; set; }
        public string REKENING { get; set; }
        public string CATUBERAS { get; set; }
        public decimal NILAICATU { get; set; }
        public decimal UPAH_HARIAN { get; set; }

        // Additional properties calculated in the final SELECT statement
        public decimal TOTALHK { get; set; }
        public decimal HKLIBUR { get; set; }
        public decimal SAKIT { get; set; }
        public decimal IZIN { get; set; }
        public decimal CUTI { get; set; }
        public decimal UMAKAN { get; set; }
        public decimal TJGPERABOT { get; set; }
        public decimal PREMIDANLEMBUR { get; set; }
        public decimal KOMPENSASI { get; set; }
        public decimal LIBURDIBAYAR { get; set; }
        public decimal TOTAL
        {
            get
            {
                // Check if QTY is zero to avoid DivideByZeroException
                return TJGPERABOT + UMAKAN + PREMIDANLEMBUR + KOMPENSASI + LIBURDIBAYAR;
            }
        }
    }

    public class Estate
    {
        public string ID { get; set; }
        public string NAMA { get; set; }
    }

    public class AIS_JURNAL
    {
        public int ISBORONGAN { get; set; }
        public string DIVISI { get; set; }
        public string JENIS { get; set; }
        public string PEKERJAAN { get; set; }
        public string BLOK { get; set; }        
        public string NOJURNAL { get; set; }
        public DateTime TANGGAL { get; set; }
        public int NO { get; set; }
        public string KODE { get; set; }
        public string REKENING { get; set; }
        public decimal DEBET { get; set; }
        public decimal PPH21 { get; set; }
        public decimal DEBETPPH { get; set; }
        public decimal KREDIT { get; set; }
        public string KETERANGAN { get; set; }
        public bool POSTED { get; set; }
        public string PERIODE { get; set; }
    }
    public class AIS_JURNAL_FINAL
    {
      
        public string NOJURNAL { get; set; }
        public DateTime TANGGAL { get; set; }
        public int NO { get; set; }
        public string KODE { get; set; }
        public string REKENING { get; set; }
        public decimal DEBET { get; set; }
        public decimal KREDIT { get; set; }
        public string KETERANGAN { get; set; }
        public bool POSTED { get; set; }
        public string PERIODE { get; set; }
    }


    public class Division
    {
        public int ISBORONGAN { get; set; }
        public string DIVISIID { get; set; }
        public string DIVISI { get; set; }
        public string NOMOR { get; set; }
    }
}
