using Macrin.Common;
using System;

namespace Infra.Common.DTO
{
    public class Usersinrolestatus : Macrin.Common.BaseDTO
    {
        public Guid ApplicationId { get; set; }
        public Guid RoleId { get; set; }
        public bool Checked { get; set; }
    }
}
