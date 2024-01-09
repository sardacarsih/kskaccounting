using Accounting.DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.BusinessLayer
{
    public static class LevelAksesServices
    {
        static ILevelAksesRepository repository;
        static LevelAksesServices()
        {
            repository = new LevelAksesRepository();
        }

        public static bool OpenForm(int aksiid, string userid)
        {
            return repository.OpenForm(aksiid, userid);
        }
        public static bool Ubah(int aksiid, string userid)
        {
            return repository.Ubah(aksiid, userid);
        }
        public static bool BaruImport(int aksiid, string userid)
        {
            return repository.BaruImport(aksiid, userid);
        }
        public static bool CetakExport(int aksiid, string userid)
        {
            return repository.CetakExport(aksiid, userid);
        }
        public static bool Hapus(int aksiid, string userid)
        {
            return repository.Hapus(aksiid, userid);
        }
        public static bool Simpan(int aksiid, string userid)
        {
            return repository.Simpan(aksiid, userid);
        }
    }
}
