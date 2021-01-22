using Macrin.Common;
using System;

namespace Infra.Common.DTO.Identity
{
    public class IdentityRefreshToken : Macrin.Common.BaseDTO
    {
        public DateTime? Utccreated { get; set; }
        public string Subject { get; set; }
        public string Ticket { get; set; }
        public DateTime? Utcexpiry { get; set; }
        public Guid Applicationid { get; set; }
        public Guid Tokenid { get; set; }
    }
}
