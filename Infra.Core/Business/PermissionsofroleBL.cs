using FluentValidation;
using FluentValidation.Results;
using Infra.Common.DTO;
using Infra.Core.Contract;
using Infra.Core.Domain;
using Infra.Core.Validator;
using LinqKit;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infra.Common.DTO.Permissionsofrole;
using static Infra.Core.Validator.PermissionsofroleValidator;

namespace Infra.Core.Business
{
    public class PermissionsofroleBL : BaseBL<Permissionsofrole, PERMISSIONSOFROLE, Guid>, IPermissionsofroleBL
    {
        private readonly InfraEntities _ctx;
        private readonly int _maxPageSize;
        public readonly ILogsBL _logger;
        public PermissionsofroleBL(InfraEntities ctx, ILogsBL logger)
        {
            _ctx = ctx;
            _logger = logger;
            _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
                : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        }

        #region IEntity
        public async Task<Permissionsofrole> GetByKeyAsync(Guid key)
        {
            PERMISSIONSOFROLE item = await _ctx.PERMISSIONSOFROLE.AsNoTracking().FirstOrDefaultAsync(x => x.ROLEPERMISSIONID == key);
            return MapToDTO(item);
        }

        public async Task<DataList<Permissionsofrole>> List(Permissionsofrole filter, PageConfig config)
        {
            IQueryable<PERMISSIONSOFROLE> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Rolepermissionid";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<Permissionsofrole>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }
        #endregion

        #region ISavable
        public async Task<Permissionsofrole> SaveAsync(Permissionsofrole item)
        {
            item.Validation = new PermissionsofroleValidator(_ctx).Validate(item);
            if (!item.Validation.IsValid) return item;
            return (item.Rolepermissionid != Guid.Empty) ? await Update(item) : await Insert(item);
        }
        #endregion

        #region IDeletable
        public async Task<Permissionsofrole> DeleteAsync(Permissionsofrole item)
        {
            var validator = new RolePermissionDeleteValidator(_ctx);
            item.Validation = validator.Validate(item);
            if (!item.Validation.IsValid)
            {
                return await Task.FromResult(item);

            }
            PERMISSIONSOFROLE data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Deleted;

            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("PermissionofRole", string.Format(ValidationErrorMessage.GenericDBDeleteError, "PermissionofRole"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Delete Permission of Role", item, error.ErrorMessage);
                return item;
            }
            _logger.Log("INFO", "Delete Permission of Role", item, "Successful.");
            return item;
            }
        #endregion

        #region HELPERS
        #endregion

        #region BaseBL
        protected override Guid GenerateId(string sequenceName = null)
        {
            //This function was created only for the sake creating an id from Sequence
            return Guid.NewGuid();
        }

        protected override Permissionsofrole MapToDTO(PERMISSIONSOFROLE item)
        {
            if (item == null) return new Permissionsofrole();
            return new Permissionsofrole
            {
                Permissionid = item.PERMISSIONID,
                Roleid = item.ROLEID,
                Rolepermissionid = item.ROLEPERMISSIONID,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override PERMISSIONSOFROLE MapToEntity(Permissionsofrole item)
        {
            if (item == null) return new PERMISSIONSOFROLE();
            return new PERMISSIONSOFROLE
            {
                PERMISSIONID = item.Permissionid,
                ROLEID = item.Roleid,
                ROLEPERMISSIONID = item.Rolepermissionid
            };
        }

        protected override IQueryable<Permissionsofrole> QueryToDTO(IQueryable<PERMISSIONSOFROLE> query)
        {
            return query.Select(x => new Permissionsofrole
            {
                Permissionid = x.PERMISSIONID,
                Roleid = x.ROLEID,
                Rolepermissionid = x.ROLEPERMISSIONID,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected override IQueryable<PERMISSIONSOFROLE> FilteredEntities(Permissionsofrole filter, IQueryable<PERMISSIONSOFROLE> custom_query = null, bool strict = false)
        {
            var predicate = PredicateBuilder.New<PERMISSIONSOFROLE>(true);

            var query = custom_query ?? _ctx.PERMISSIONSOFROLE;
            return query.Where(predicate);
        }

        protected override IQueryable<PERMISSIONSOFROLE> OrderEntities(IQueryable<PERMISSIONSOFROLE> query, string sortOrder, bool isAscending)
        {
            switch (sortOrder.ToUpper())
            {
                case "PERMISSIONID":
                    query = isAscending ? query.OrderBy(x => x.PERMISSIONID) : query.OrderByDescending(x => x.PERMISSIONID);
                    break;
                case "ROLEID":
                    query = isAscending ? query.OrderBy(x => x.ROLEID) : query.OrderByDescending(x => x.ROLEID);
                    break;
                case "ROLEPERMISSIONID":
                    query = isAscending ? query.OrderBy(x => x.ROLEPERMISSIONID) : query.OrderByDescending(x => x.ROLEPERMISSIONID);
                    break;
                default:
                    query = query.OrderBy(x => x.ROLEPERMISSIONID);
                    break;
            }
            return query;
        }

        protected override async Task<Permissionsofrole> Insert(Permissionsofrole item)
        {
            PERMISSIONSOFROLE dbdata = MapToEntity(item);
            dbdata.ROLEPERMISSIONID = Guid.NewGuid();
            _ctx.PERMISSIONSOFROLE.Add(dbdata);
            _ctx.Entry(dbdata).State = EntityState.Added;
            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("RolePermissionId", string.Format(ValidationErrorMessage.GenericDBSaveError, "RolePermission"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Save Role Permission", item, error.ErrorMessage);
                return item;
            }
            _logger.Log("INFO", "Add Role Permission", item, "Successful.");
            return await GetByRolePermissionId(dbdata.ROLEPERMISSIONID);

        }

        protected override async Task<Permissionsofrole> Update(Permissionsofrole item)
        {
            PERMISSIONSOFROLE data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Permissionsofrole");
            return item;
        }

        public async Task<List<RolePermissionDisplay>> ListRolePermissionAsyncByUserId(Guid id)
        {
            return await _ctx.USERSINROLE.AsNoTracking()
                .Where(x => x.USERID == id)
                .Join(_ctx.ROLES.AsNoTracking(),
                    userrole => userrole.ROLEID,
                    role => role.ROLEID,
                    (userrole, role) => new { userrole, role })
                .Join(_ctx.PERMISSIONSOFROLE.AsNoTracking(),
                    rolepermission => rolepermission.role.ROLEID,
                    userpermission => userpermission.ROLEID,
                    (rolepermission, userpermission) => new { rolepermission, userpermission })
                .Join(_ctx.PERMISSIONS.AsNoTracking(),
                    userrolepermission => userrolepermission.userpermission.PERMISSIONID,
                    permission => permission.PERMISSIONID,
                    (userrolepermission, permission) => new { userrolepermission, permission })
                .Join(_ctx.APPLICATIONS.AsNoTracking(),
                    permission => permission.permission.APPLICATIONID,
                    application => application.APPLICATIONID,
                    (permission, application) => new
                    {
                        permission.userrolepermission.userpermission.ROLEID,
                        permission.userrolepermission.userpermission.PERMISSIONID,
                        permission.userrolepermission.rolepermission.role.ROLENAME,
                        permission.userrolepermission.userpermission.ROLEPERMISSIONID,
                        permission.permission.PERMISSIONNAME,
                        permission.permission.DESCRIPTION,
                        application.APPLICATIONID,
                        application.APPLICATIONNAME
                    })
                .Select(item => new RolePermissionDisplay
                {
                    Roleid = item.ROLEID,
                    PermissionName = item.PERMISSIONNAME,
                    PermissionDescription = item.DESCRIPTION,
                    Rolepermissionid = item.PERMISSIONID,
                    Permissionid = item.ROLEPERMISSIONID,
                    ApplicationId = item.APPLICATIONID,
                    ApplicationName = item.APPLICATIONNAME,
                    RoleName = item.ROLENAME


                }).ToListAsync();
        }

        public async Task<List<Permissionsofrole>> ListRolePermissionAsync(Guid roleid)
        {
            return await _ctx.PERMISSIONSOFROLE.AsNoTracking()
                .Where(x => x.ROLEID == roleid)
                .Select(item => new Permissionsofrole
                {
                    Roleid = item.ROLEID,
                    Rolepermissionid = item.ROLEPERMISSIONID,
                    Permissionid = item.ROLEPERMISSIONID
                }).ToListAsync();
        }
     

        public async Task<Permissionsofrole> SaveRolePermission(Permissionsofrole item)
        {
            var Validator = new PermissionsofroleValidator(_ctx);
            item.Validation = Validator.Validate(item);
            if (!item.Validation.IsValid)
            {
                var error = item.Validation.Errors.First();
                return await Task.FromResult(item);
            }
            if (!item.Validation.IsValid) return item;
            return await Insertrole(item);
        }
         private async Task<Permissionsofrole> Insertrole(Permissionsofrole item)
        {
            PERMISSIONSOFROLE dbdata = MapToEntity(item);
            dbdata.ROLEPERMISSIONID = Guid.NewGuid();
            _ctx.PERMISSIONSOFROLE.Add(dbdata);
            _ctx.Entry(dbdata).State = EntityState.Added;
            if(await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("RolePermissionId", string.Format(ValidationErrorMessage.GenericDBSaveError, "RolePermission"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Save RolePermission", item, error.ErrorMessage);
                return item;
            }
            _logger.Log("INFO", "Add RolePermission", item, "Successful.");
            return await GetByRolePermissionId(dbdata.ROLEPERMISSIONID);
        }
        public async Task<Permissionsofrole> GetByRolePermissionId(Guid id)
        {
            return MapToDTO(await _ctx.PERMISSIONSOFROLE.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ROLEPERMISSIONID == id));
        }

        public async Task<Permissionsofrole> GetByRoleIdPermissionId(Guid roleid, Guid permissionid)
        {
            return MapToDTO(await _ctx.PERMISSIONSOFROLE.AsNoTracking()
              .FirstOrDefaultAsync(x => x.ROLEID == roleid && x.PERMISSIONID == permissionid));
        }
        #endregion

    }
}
