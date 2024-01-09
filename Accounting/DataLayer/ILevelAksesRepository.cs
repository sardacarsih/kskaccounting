using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    public interface ILevelAksesRepository
    {
        bool OpenForm(int aksiid,string userid);
        bool Ubah(int aksiid, string userid);
        bool Hapus(int aksiid, string userid);
        bool Simpan(int aksiid, string userid);
        bool CetakExport(int aksiid, string userid);
        bool BaruImport(int aksiid, string userid);
    }
}
