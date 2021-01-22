using Infra.Common.DTO;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Infra.Common.DTO.Permissionsofrole;

namespace Infra.Core.Contract
{
    public interface IPermissionsofroleBL : IEntity<Permissionsofrole, Guid>
    {
        Task<List<RolePermissionDisplay>> ListRolePermissionAsyncByUserId(Guid id);
        Task<Permissionsofrole> GetByRoleIdPermissionId(Guid roleid, Guid permissionid);

        #region Asynchronous Method
        Task<List<Permissionsofrole>> ListRolePermissionAsync(Guid roleid);
        Task<Permissionsofrole> GetByRolePermissionId(Guid id);
      
        #endregion
    }
}
