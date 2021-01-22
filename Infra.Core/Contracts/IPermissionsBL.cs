using Infra.Common.DTO;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infra.Core.Contract
{
    public interface IPermissionsBL : IEntity<Permissions, Guid>
    {
        Task<List<Permissions>> ListByApplicationIdAsync(Guid appId);
        Task<List<Permissions>> ListPermissionByRoleId(Guid RoleId);

        Task<DataList<Permissions>> GetPermissionsbyUsername(string keywords);

        #region Synchronous Methods
        Permissions GetById(Guid id);
        Permissions GetByName(string name);
        List<Permissions> List();
        List<Permissions> List(int page, int size);
        List<Permissions> List(int page, int size, Permissions filter);
        Permissions Save(Permissions item);
        Permissions Delete(Permissions item);
        #endregion

        #region Asynchronous Methods
        Task<Permissions> GetByIdAsync(Guid id);
        Task<Permissions> GetByNameAsync(string name);
        Task<List<Permissions>> ListAsync();
        Task<List<Permissions>> ListAsync(int page, int size);
        Task<List<Permissions>> ListAsync(int page, int size, Permissions filter);     
        #endregion

    }
}
