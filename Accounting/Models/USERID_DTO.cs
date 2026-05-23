using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting
{
    // The USERID_DTO class serves as the master class
    // The USERID_DTO class serves as the master class
    public class USERID_DTO
    {
        public string USERID { get; set; }
        public string NAMA { get; set; }
        public string DEPT { get; set; }
        public string JABATAN { get; set; }
        public string PASSWORD { get; set; }
        public string AKTIF { get; set; }
        public int FAILED_LOGIN_COUNT { get; set; }
        public DateTime? LOCKOUT_UNTIL_UTC { get; set; }
        public DateTime? LAST_LOGIN_AT_UTC { get; set; }
        public DateTime? LAST_FAILED_LOGIN_AT_UTC { get; set; }
        public DateTime? PASSWORD_CHANGED_AT_UTC { get; set; }
        public string PASSWORD_RESET_REQUIRED { get; set; }
    }

    // The USERID_DETAIL_DTO class represents the detail with a master-detail relationship
    public class USERID_DETAIL_DTO
    {
        // Properties specific to USERID_DETAIL_DTO
        public int APPSDETAILID { get; set; }
        public string USERID { get; set; }
        public string APPID { get; set; }
        public string IDDATA { get; set; }
        public int LEVELID { get; set; }

        // Master-Detail Relationship: USERID_DETAIL_DTO has a reference to USERID_DTO
        public USERID_DTO MasterUser { get; set; }
    }
}

