using Macrin.Common;
using System;

namespace Infra.Common.DTO.Identity
{
    public class IdentityClaim : Macrin.Common.BaseDTO
    {
        public Guid Useraccountid { get; set; }
        public Guid Claimid { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
    }
}
