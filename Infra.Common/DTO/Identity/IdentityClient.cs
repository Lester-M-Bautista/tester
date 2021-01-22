using System;

namespace Infra.Common.DTO.Identity
{
    public class IdentityClient : Macrin.Common.BaseDTO
    {
        public Guid Applicationid { get; set; }
        public bool? Isdeleted { get; set; }
        public Guid Applicationsecret { get; set; }
        public string Description { get; set; }
        public string Allowedorigin { get; set; }
        public string Applicationname { get; set; }
        public int? Refreshlifetime { get; set; }
        public bool? Isnative { get; set; }
        public string Redirecturl { get; set; }
    }
}
