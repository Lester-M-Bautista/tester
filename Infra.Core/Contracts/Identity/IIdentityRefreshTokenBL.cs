

using Infra.Common.DTO.Identity;
using Macrin.Common;
using System;
using System.Threading.Tasks;

namespace Infra.Core.Contract.Identity
{
    public interface IIdentityRefreshTokenBL : IEntity<IdentityRefreshToken, Guid>
    {
        Task AddRefreshTokenAsync(IdentityRefreshToken token);
        Task RemoveRefreshTokenAsync(Guid tokenId);
        IdentityRefreshToken GetToken(string ticket);
    }
}
