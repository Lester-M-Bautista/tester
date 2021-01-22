using System;

namespace Infra.Common.DTO
{
    public class Users : Macrin.Common.BaseDTO
    {
        public DateTime? Datelastlockout { get; set; }
        public Guid Department { get; set; }
        public DateTime? Dateaccessexpiry { get; set; }
        public DateTime? Datelastactivity { get; set; }
        public DateTime? Datelastlogin { get; set; }
        public bool? Isapproved { get; set; }
        public DateTime? Datecreated { get; set; }
        public string Password { get; set; }
        public bool? Islockedout { get; set; }
        public string Useremail { get; set; }
        public int? Passwordfailcount { get; set; }
        public bool? Isdeleted { get; set; }
        public DateTime? Datechangepass { get; set; }
        public string Userfullname { get; set; }
        public int? Passwordformat { get; set; }
        public string Passwordsalt { get; set; }
        public string Username { get; set; }
        public Guid Userid { get; set; }
    }
}
