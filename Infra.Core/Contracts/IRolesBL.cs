using Infra.Common.DTO;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infra.Core.Contract
{
    public interface IRolesBL : IEntity<Roles, Guid>
    {
        Task<List<Roles>> ListRoleByAppId(Guid applicationid);
        Task<List<Roles>> ListRoleByUserId(Guid Id);
        Task<Roles> GetByIdAsync(Guid id);
        Task<Roles> GetByNameAsync(string name);
        Task<List<Roles>> ListAsync();
        Task<List<Roles>> ListAsync(int page, int size);
        Task<List<Roles>> ListAsync(int page, int size, Roles filter);
        Task<DataList<Roles>> SelectbyUserName(string keywords);
    }
}
