using Infra.Common.DTO.Identity;
using Macrin.Common;
using System;
using System.Threading.Tasks;

namespace Infra.Core.Contract.Identity
{
    public interface IIdentityLoginBL : IEntity<IdentityLogin, Guid>
    {
        Task<IdentityLogin> GetById(Guid id);
    }
}
