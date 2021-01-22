using Macrin.Common;
using System;

namespace Infra.Common.DTO.Identity
{
    public class IdentityLogin : Macrin.Common.BaseDTO
    {
        public Guid Useraccountid { get; set; }
        public string Providerkey { get; set; }
        public string Providername { get; set; }
        public Guid Loginid { get; set; }
    }
}
