using Macrin.Common;
using System;

namespace Infra.Common.DTO
{
    public class TempPermission : Macrin.Common.BaseDTO
    {
        public string RoleName { get; set; }
        public string ApplicationName { get; set; }
        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string PermissionDesc { get; set; }
    }
}
