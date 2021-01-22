using Macrin.Common;
using System;

namespace Infra.Common.DTO
{
    public class Permissionsofrolestatus : Macrin.Common.BaseDTO
    {
        public Guid PermissionId { get; set; }
        public bool Checked { get; set; }
    }
}
