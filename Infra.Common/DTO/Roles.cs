using System;

namespace Infra.Common.DTO
{
   public class Roles : Macrin.Common.BaseDTO
    {
        public Guid Applicationid { get; set; }
        public Guid Roleid { get; set; }
        public string Rolename { get; set; }
        public Nullable<short> Isdeleted { get; set; }
        public string Description { get; set; }
    }
}
