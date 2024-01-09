namespace Accounting
{
    public static class LoginInfo
    {
        public static string KODEUSER;
        public static string userID;
        public static string role;
        public static string MGR;
        public static string KASIR;
        public static string ASST;
        public static string KABAG;
        public static string MANAGER;
    }

    public static class CompanyInfo
    {
        public static string INIT;
        public static string JENIS_AKUNTING;
        public static string GROUP;
        public static string IDPT;
        public static string NAMAPT;
        public static string WILAYAH;
        public static string ESTATE;

    }

    public static class Acct
    {
        public static string AppVersion;
        public static int PeriodeMin;
        public static int PeriodeMax;
        public static int TahunMin;
        public static int TahunMax;
        public static string KunciPeriode;
        public static string OracleConnString;
        
        public static int p_bulan;
        public static int p_tahun;
        public static string p_periode;
     
    }
    public static class EditCOA
    {
        public static string COAID { get; set; }
        public static int TAHUN { get; set; }
        public static string JENIS { get; set; }
        public static string INDUK { get; set; }
        public static char GD { get; set; }
        public static char AKTIF { get; set; }
        public static char DK { get; set; }

        public static string KODE { get; set; }
        public static int LEVEL { get; set; }        
        public static string PERKIRAAN { get; set; }
        
    }

    public  class Jurnal
    {
        public int NO { get; set; }
        public string KODE { get; set; }
        public string KET { get; set; }
        public decimal JUMLAH { get; set; }
    }



}
