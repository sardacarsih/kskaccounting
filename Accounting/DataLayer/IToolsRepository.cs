using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    public interface IToolsRepository
    {
        DataTable GetAksesLocations(string p_userid);
        DataTable UserLogin();
        DataTable GetBlokList(string p_IDDATA);
        Int32 CekAkunBlok(string p_IDDATA, int p_tahun, string p_divisi, string p_blok);
        DataTable LoadPerkiraanBlok(string p_IDDATA, int p_tahun,string p_divisi,string p_blok);
        DataTable Analisa_kesalahan_COA(string p_IDDATA, int p_tahun);

        void GenerateAkunTBM(string p_IDDATA, int p_tahun, string p_status, string p_divisiID,string p_divisi, string p_blokid, string p_blok, string p_ttanam, string p_mode);
        void GenerateAkunTM(string p_IDDATA, int p_tahun, string p_status, string p_divisiID, string p_divisi, string p_blokid, string p_blok, string p_ttanam);
        string GetLocalIPAddress();
    }
}
