using Accounting.DataLayer;
using System.Data;

namespace Accounting.BusinessLayer
{
    public static class ToolsServices
    {
        static IToolsRepository repository;
        static ToolsServices()
        {
            repository = new ToolsRepository();
        }
        public static DataTable UserLogin()
        {
            return repository.UserLogin();
        }
        public static DataTable GetAksesLocations(string p_userid)
        {
            return repository.GetAksesLocations(p_userid);
        }
        public static DataTable GetBlokList(string p_iddata)
        {
            return repository.GetBlokList(p_iddata);
        }
        public static DataTable Analisa_kesalahan_COA(string p_iddata, int p_tahun)
        {
            return repository.Analisa_kesalahan_COA(p_iddata, p_tahun);
        }
        public static int CekAkunBlok(string p_IDDATA, int p_tahun, string p_divisi, string p_blok)
        {
            return repository.CekAkunBlok(p_IDDATA, p_tahun, p_divisi, p_blok);
        }
        public static DataTable LoadPerkiraanBlok(string p_IDDATA, int p_tahun, string p_divisi, string p_blok)
        {
            return repository.LoadPerkiraanBlok( p_IDDATA,  p_tahun,  p_divisi,  p_blok);
        }

        public static void GenerateAkunTBM(string p_IDDATA, int p_tahun, string p_status, string p_divisiID, string p_divisi, string p_blokid, string p_blok, string p_ttanam, string p_mode)
        {
            repository.GenerateAkunTBM(p_IDDATA, p_tahun, p_status, p_divisiID, p_divisi, p_blokid, p_blok, p_ttanam, p_mode);
        }

        public static void GenerateAkunTM(string p_IDDATA, int p_tahun, string p_status, string p_divisiID, string p_divisi, string p_blokid, string p_blok, string p_ttanam)
        {
            repository.GenerateAkunTM(p_IDDATA, p_tahun, p_status, p_divisiID, p_divisi, p_blokid, p_blok, p_ttanam);
        }
        public static string GetLocalIPAddress()
        {
            return repository.GetLocalIPAddress();
        }
    }
}
