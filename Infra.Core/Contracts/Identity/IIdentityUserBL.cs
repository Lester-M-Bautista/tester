using Infra.Common;
using Infra.Common.DTO.Identity;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Infra.Core.Contract.Identity
{
    public interface IIdentityUserBL : IEntity<IdentityUser, Guid>
    {
        Task<IdentityUser> GetById(Guid id);
        Task<IdentityUser> GetByName(string username);
        Task<IdentityUser> GetByEmail(string email);
        Task<Guid> GetByUserIdByLogin(string name, string key);
        Task<List<IdentityUser>> ListUserRole(Guid id);
        Task<List<Claim>> ListUserClaim(Guid id);
        Task<List<string>> ListUserPermissions(Guid id);
        Task<IdentityUser> ChangePasswordAsync(string username, ChangePasswordModel item);
        Task<DataList<IdentityUser>> GetList(string query, Guid department, int pageIndex, int pageSize);
        Task<DataList<IdentityUser>> GetListByApplication(string query, string application, int pageIndex, int pageSize);
        Task<DataList<IdentityUser>> GetListByRole(string query, string role, int pageIndex, int pageSize);
        Task<IdentityUser> AuthenticateAsync(string username, string password);
        Task<IdentityUser> ResetPasswordAsync(string username, string newpassword);  
        Task<List<string>> ListAdministrativePermissionAsync(string username, string application);
        Task<IdentityUser> ChangePasswordAsync(string username, string currentpassword, string newpassword);

    }
}
