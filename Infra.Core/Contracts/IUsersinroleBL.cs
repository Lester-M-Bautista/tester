using Infra.Common.DTO;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infra.Core.Contract
{
    public interface IUsersinroleBL : IEntity<Usersinrole, Guid>
    {
        Task<Usersinrole> GetByUserIdRoleId(Guid roleid, Guid userid);

        Task<List<Usersinrole>> ListRolesByRoleId(Guid Id);     
        Task<List<Usersinrole>> ListAsync();
      
        Task<Usersinrole> GetByUserRoleId(Guid id);
    }
}
