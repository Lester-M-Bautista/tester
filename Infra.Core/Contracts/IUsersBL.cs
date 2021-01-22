using Infra.Common.DTO;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infra.Core.Contract
{
    public interface IUsersBL : IEntity<Users, Guid>
    {
        Task<Users> ChangePasswordAsync(string username, string currentpassword, string newpassword);
        Task<List<Users>> ListUserByRoleId(Guid id);
        Task<DataList<Users>> GetList(string query, Guid department, int pageIndex, int pageSize);
        Task<DataList<Users>> GetListByApplication(string query, string application, int pageIndex, int pageSize);
        Task<DataList<Users>> GetListByRole(string query, string role, int pageIndex, int pageSize);
        Task<Users> AuthenticateAsync(string username, string password);
        Task<Users> ResetPasswordAsync(string username, string newpassword);


        Task<Users> GetByIdAsync(Guid id);
        Task<Users> GetByNameAsync(string name);
        Task<List<Users>> ListAsync();
        Task<bool> AuthorizeAsync(string username, string application, string permission);
        Task<List<string>> ListAdministrativePermissionAsync(string username, string application);
        Task<List<Users>> UsersContainingPermissions(List<string> Permissions);

        //FOR BOMS
        Task<DataList<Users>> SelectbyRoleName(string keywords);
        Task<DataList<Users>> SelectbyUserName(string keywords);

    }
}
