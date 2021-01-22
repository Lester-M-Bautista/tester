using Macrin.Common;
using System;

namespace Infra.Common.DTO
{
    public class Permissions : Macrin.Common.BaseDTO
    {
        public Guid Applicationid { get; set; }
        public string Permissionname { get; set; }
        public Guid Permissionid { get; set; }
        public string Description { get; set; }
    }
}
