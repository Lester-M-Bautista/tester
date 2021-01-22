using Infra.Common;
using Infra.Common.DTO.Identity;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infra.Core
{
    public class InfraRoleStore<TRole> :
        IQueryableRoleStore<TRole, Guid>,
        IRoleStore<TRole, Guid>
        where TRole : IdentityRole
    {
        public IQueryable<TRole> Roles
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public Task<TRole> FindByIdAsync(Guid roleId)
        {
            throw new NotImplementedException();
        }
        public Task<TRole> FindByNameAsync(string roleName)
        {
            throw new NotImplementedException();
        }
        public Task CreateAsync(TRole role)
        {
            throw new NotImplementedException();
        }
        public Task UpdateAsync(TRole role)
        {
            throw new NotImplementedException();
        }
        public Task DeleteAsync(TRole role)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
