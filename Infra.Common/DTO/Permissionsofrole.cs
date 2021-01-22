using Macrin.Common;
using System;

namespace Infra.Common.DTO
{
    public class Permissionsofrole : Macrin.Common.BaseDTO
    {
        public Guid Permissionid { get; set; }
        public Guid Roleid { get; set; }
        public Guid Rolepermissionid { get; set; }

        public class RolePermissionDisplay : Permissionsofrole
        {
            public Guid ApplicationId { get; set; }
            public string PermissionName { get; set; }
            public string PermissionDescription { get; set; }
            public string ApplicationName { get; set; }
            public string RoleName { get; set; }

        }
    }
}
