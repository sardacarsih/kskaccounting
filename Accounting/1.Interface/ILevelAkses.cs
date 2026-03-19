using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Interface
{
    public interface ILevelAkses
    {
        bool BaruImport(int aksiid, string userid);
        bool CetakExport(int aksiid, string userid);
        bool Hapus(int aksiid, string userid);
        bool OpenForm(string userId, int aksi, int lvl, string idData, string appId);
        bool Simpan(int aksiid, string userid);
        bool Ubah(int aksiid, string userid);


    }
}
