
using Infra.Common.DTO.Identity;
using Macrin.Common;
using System;
using System.Threading.Tasks;

namespace Infra.Core.Contract.Identity
{
    public interface IIdentityProfileBL : IEntity<IdentityProfile, Guid>
    {
        Task<IdentityProfileFull> GetById(Guid id);
        Task<IdentityProfileFull> GetByName(string username);
        Task<IdentityProfileFull> GetByEmail(string email);
    }
}
