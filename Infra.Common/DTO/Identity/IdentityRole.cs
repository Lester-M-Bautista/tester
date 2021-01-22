using Macrin.Common;
using Microsoft.AspNet.Identity;
using System;

namespace Infra.Common.DTO.Identity
{
    public class IdentityRole : Macrin.Common.BaseDTO, IRole<Guid>
    {
        public Guid Applicationid { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? Isdeleted { get; set; }
        public string Description { get; set; }
    }
}
