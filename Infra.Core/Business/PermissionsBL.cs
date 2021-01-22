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
using System.Threading.Tasks;

namespace Infra.Core.Business
{
    public class PermissionsBL : BaseBL<Permissions, PERMISSIONS, Guid>, IPermissionsBL
    {
        private readonly InfraEntities _ctx;
        private readonly int _maxPageSize;
        public readonly ILogsBL _logger;
        public PermissionsBL(InfraEntities ctx, ILogsBL logger)
        {
            _ctx = ctx;
            _logger = logger;
            _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
                : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        }

        #region IEntity
        public async Task<Permissions> GetByKeyAsync(Guid key)
        {
            PERMISSIONS item = await _ctx.PERMISSIONS.AsNoTracking().FirstOrDefaultAsync(x => x.PERMISSIONID == key);
            return MapToDTO(item);
        }

        public async Task<DataList<Permissions>> List(Permissions filter, PageConfig config)
        {
            IQueryable<PERMISSIONS> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Permissionid";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<Permissions>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }
        #endregion

        #region ISavable
        public async Task<Permissions> SaveAsync(Permissions item)
        {
            item.Validation = new PermissionsValidator(_ctx).Validate(item);
            if (!item.Validation.IsValid) return item;
            return (item.Permissionid != Guid.Empty) ? await Update(item) : await Insert(item);
        }
        #endregion

        #region IDeletable
        public async Task<Permissions> DeleteAsync(Permissions item)
        {
              var validator = new PermissionDeleteValidator(_ctx);
                item.Validation = validator.Validate(item);
                if (!item.Validation.IsValid)
                {
                    return await Task.FromResult(item);

                }
                PERMISSIONS data = MapToEntity(item);
                _ctx.Entry(data).State = EntityState.Deleted;

            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("Permission", string.Format(ValidationErrorMessage.GenericDBDeleteError, "Permission"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Delete Permission", item, error.ErrorMessage);
                return item;
            }         
                 _logger.Log("INFO", "Delete Permission", item, "Successful.");
                return item;
 
        }
        #endregion

        #region HELPERS
        #endregion

        #region BaseBL
        protected override Guid GenerateId(string sequenceName = null)
        {
            return Guid.NewGuid();
        }

        protected override Permissions MapToDTO(PERMISSIONS item)
        {
            if (item == null) return new Permissions();
            return new Permissions
            {
                Applicationid = item.APPLICATIONID,
                Permissionname = item.PERMISSIONNAME,
                Permissionid = item.PERMISSIONID,
                Description = item.DESCRIPTION,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override PERMISSIONS MapToEntity(Permissions item)
        {
            if (item == null) return new PERMISSIONS();
            return new PERMISSIONS
            {
                APPLICATIONID = item.Applicationid,
                PERMISSIONNAME = item.Permissionname,
                PERMISSIONID = item.Permissionid,
                DESCRIPTION = item.Description
            };
        }

        protected override IQueryable<Permissions> QueryToDTO(IQueryable<PERMISSIONS> query)
        {
            return query.Select(x => new Permissions
            {
                Applicationid = x.APPLICATIONID,
                Permissionname = x.PERMISSIONNAME,
                Permissionid = x.PERMISSIONID,
                Description = x.DESCRIPTION,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected override IQueryable<PERMISSIONS> FilteredEntities(Permissions filter, IQueryable<PERMISSIONS> custom_query = null, bool strict = false)
        {
            var predicate = PredicateBuilder.New<PERMISSIONS>(true);
            if (!string.IsNullOrEmpty(filter.Permissionname)) predicate = (strict)
                        ? predicate.And(x => x.PERMISSIONNAME.ToLower() == filter.Permissionname.ToLower())
                        : predicate.And(x => x.PERMISSIONNAME.ToLower().Contains(filter.Permissionname.ToLower()));

            if (!string.IsNullOrEmpty(filter.Description)) predicate = (strict)
                            ? predicate.And(x => x.DESCRIPTION.ToLower() == filter.Description.ToLower())
                            : predicate.And(x => x.DESCRIPTION.ToLower().Contains(filter.Description.ToLower()));
            var query = custom_query ?? _ctx.PERMISSIONS;
            return query.Where(predicate);
        }

        protected override IQueryable<PERMISSIONS> OrderEntities(IQueryable<PERMISSIONS> query, string sortOrder, bool isAscending)
        {
            switch (sortOrder.ToUpper())
            {
                case "APPLICATIONID":
                    query = isAscending ? query.OrderBy(x => x.APPLICATIONID) : query.OrderByDescending(x => x.APPLICATIONID);
                    break;
                case "PERMISSIONNAME":
                    query = isAscending ? query.OrderBy(x => x.PERMISSIONNAME) : query.OrderByDescending(x => x.PERMISSIONNAME);
                    break;
                case "PERMISSIONID":
                    query = isAscending ? query.OrderBy(x => x.PERMISSIONID) : query.OrderByDescending(x => x.PERMISSIONID);
                    break;
                case "DESCRIPTION":
                    query = isAscending ? query.OrderBy(x => x.DESCRIPTION) : query.OrderByDescending(x => x.DESCRIPTION);
                    break;
                default:
                    query = query.OrderBy(x => x.PERMISSIONID);
                    break;
            }
            return query;
        }

        protected override async Task<Permissions> Insert(Permissions item)
        {         
              item.Permissionid = GenerateId();
                PERMISSIONS data = MapToEntity(item);
                _ctx.Entry(data).State = EntityState.Added;

            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("Permission", string.Format(ValidationErrorMessage.GenericDBDeleteError, "Permission"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Save Permission", item, error.ErrorMessage);
                return item;
            }   
                _logger.Log("INFO", "Add Permission", item, "Successful.");
                return item;
        }

        protected override async Task<Permissions> Update(Permissions item)
        {
               PERMISSIONS data = MapToEntity(item);
                _ctx.Entry(data).State = EntityState.Modified;

            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("Permission", string.Format(ValidationErrorMessage.GenericDBDeleteError, "Permission"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Updated Permission", item, error.ErrorMessage);
                return item;
            }
                _logger.Log("INFO", "Updated Permission", item, "Successful.");
                return item;
    
        }

        public async Task<List<Permissions>> ListByApplicationIdAsync(Guid appId)
        {
            return await _ctx.PERMISSIONS.AsNoTracking()
                .Where(x => x.APPLICATIONID == appId)
                .Select(item => new Permissions
                {
                    Applicationid = item.APPLICATIONID,
                    Permissionid = item.PERMISSIONID,
                    Permissionname = item.PERMISSIONNAME,
                    Description = item.DESCRIPTION
                }).ToListAsync();
        }
        public async Task<List<Permissions>> ListPermissionByRoleId(Guid RoleId)
        {
            List<Permissions> item = new List<Permissions>();
            item = await _ctx.ROLES.AsNoTracking()
                .Where(x => x.ROLEID == RoleId).Join(_ctx.PERMISSIONSOFROLE.AsNoTracking(), 
                    roles => roles.ROLEID,
                    permissionrole => permissionrole.ROLEID,
                    (roles, permissionrole) => new { ROLES = roles, PERMISSIONSOFROLE = permissionrole })
                .Join(_ctx.PERMISSIONS.AsNoTracking(),
                     permissionrole => permissionrole.PERMISSIONSOFROLE.PERMISSIONID,
                     permission => permission.PERMISSIONID,
                    (permissionrole, permission) => new { ROLEID = permissionrole.ROLES, PERMISSIONSOFROLE = permissionrole.PERMISSIONSOFROLE, PERMISSIONS = permission })
                .Select(x => new Permissions
                {
                    Permissionid = x.PERMISSIONS.PERMISSIONID,
                    Description = x.PERMISSIONS.DESCRIPTION,
                    Permissionname = x.PERMISSIONS.PERMISSIONNAME,
                })
                .ToListAsync();
            return item;
        }



        public async Task<DataList<Permissions>> GetPermissionsbyUsername(string keywords)
        {
            if (string.IsNullOrEmpty(keywords)) return new DataList<Permissions> { Count = 0, Items = new List<Permissions>() };
            var table1 = _ctx.USERS.AsEnumerable().Where(x => x.USERNAME.ToLower() == keywords.ToLower());
            var table2 = _ctx.USERSINROLE.AsEnumerable();
            var table3 = _ctx.PERMISSIONSOFROLE.AsEnumerable();
            var table4 = _ctx.PERMISSIONS.AsEnumerable();

            var td =
            from t1 in table1
            join t2 in table2 on t1.USERID equals t2.USERID 
            join t3 in table3 on t2.ROLEID equals t3.ROLEID 
            join t4 in table3 on t3.PERMISSIONID equals t4.PERMISSIONID into leftj
            where t1.USERNAME.ToLower() == keywords.ToLower()
            select new Permissions
            {
                Permissionname = t3.PERMISSIONS.PERMISSIONNAME
            };
            return new DataList<Permissions>
            {
                Count = td.Count(),
                Items = td.ToList()
            };

        }

        public Permissions GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Permissions GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public List<Permissions> List()
        {
            throw new NotImplementedException();
        }

        public List<Permissions> List(int page, int size)
        {
            throw new NotImplementedException();
        }

        public List<Permissions> List(int page, int size, Permissions filter)
        {
            throw new NotImplementedException();
        }

        public Permissions Save(Permissions item)
        {
            throw new NotImplementedException();
        }

        public Permissions Delete(Permissions item)
        {
            throw new NotImplementedException();
        }

        public async Task<Permissions> GetByIdAsync(Guid id)
        {
            return MapToDTO(await _ctx.PERMISSIONS.AsNoTracking()
              .FirstOrDefaultAsync(x => x.PERMISSIONID == id));
        }

        public async Task<Permissions> GetByNameAsync(string name)
        {
            return MapToDTO(await _ctx.PERMISSIONS.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PERMISSIONNAME == name));
        }

        public async Task<List<Permissions>> ListAsync()
        {
            return await _ctx.PERMISSIONS.AsNoTracking()
                .Select(item => new Permissions
                {
                    Applicationid = item.APPLICATIONID,
                    Permissionid = item.PERMISSIONID,
                    Permissionname = item.PERMISSIONNAME,
                    Description = item.DESCRIPTION
                }).ToListAsync();
        }

        public Task<List<Permissions>> ListAsync(int page, int size)
        {
            throw new NotImplementedException();
        }

        public Task<List<Permissions>> ListAsync(int page, int size, Permissions filter)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
