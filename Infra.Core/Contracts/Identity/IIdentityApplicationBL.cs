

using Infra.Common.DTO.Identity;
using Macrin.Common;
using System;
using System.Threading.Tasks;

namespace Infra.Core.Contract.Identity
{
    public interface IIdentityApplicationBL : IEntity<IdentityClient, Guid>
    {
        Task<IdentityClient> GetById(Guid id);
        Task<IdentityClient> GetByName(string name);
        IdentityClient GetByNameNotAsync(string name);
    }
}
