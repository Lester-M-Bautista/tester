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

namespace Infra.Core.Business
{
    public class RolesBL : BaseBL<Roles, ROLES, Guid>, IRolesBL
    {
        private readonly InfraEntities _ctx;
        private readonly int _maxPageSize;
        public readonly ILogsBL _logger;
        public RolesBL(InfraEntities ctx, ILogsBL logger)
        {
            _ctx = ctx;
            _logger = logger;
            _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
                : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        }

        #region IEntity
        public async Task<Roles> GetByKeyAsync(Guid key)
        {
            ROLES item = await _ctx.ROLES.AsNoTracking().FirstOrDefaultAsync(x => x.ROLEID == key);
            return MapToDTO(item);
        }

        public async Task<DataList<Roles>> List(Roles filter, PageConfig config)
        {
            IQueryable<ROLES> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Roleid";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<Roles>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }
        #endregion

        #region ISavable
        public async Task<Roles> SaveAsync(Roles item)
        {
            item.Validation = new RolesValidator(_ctx).Validate(item);
            if (!item.Validation.IsValid) return item;
            return (item.Roleid != Guid.Empty) ? await Update(item) : await Insert(item);
        }
        #endregion

        #region IDeletable
        public async Task<Roles> DeleteAsync(Roles item)
        {
            var data = await _ctx.ROLES.FirstOrDefaultAsync(x => x.ROLEID == item.Roleid);
            data.ISDELETED = 1;
            _ctx.ROLES.Remove(data);
            _ctx.Entry(data).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("Role", string.Format(ValidationErrorMessage.GenericDBDeleteError, "Role"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Delete Role", item, error.ErrorMessage);
                return await Task.FromResult<Roles>(item);
            }
            _logger.Log("INFO", "Delete Role", item, "Successful.");
            return await Task.FromResult<Roles>(MapToDTO(data));
        }
        #endregion

        #region HELPERS
        #endregion

        #region BaseBL
        protected override Guid GenerateId(string sequenceName = null)
        {
            return Guid.NewGuid();
        }

        protected override Roles MapToDTO(ROLES item)
        {
            if (item == null) return new Roles();
            return new Roles
            {
                Applicationid = item.APPLICATIONID,
                Roleid = item.ROLEID,
                Rolename = item.ROLENAME,
                Isdeleted = (short)item.ISDELETED,
                Description = item.DESCRIPTION,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override ROLES MapToEntity(Roles item)
        {
            if (item == null) return new ROLES();
            return new ROLES
            {
                APPLICATIONID = item.Applicationid,
                ROLEID = item.Roleid,
                ROLENAME = item.Rolename,
                ISDELETED = (short)item.Isdeleted,
                DESCRIPTION = item.Description
            };
        }

        protected override IQueryable<Roles> QueryToDTO(IQueryable<ROLES> query)
        {
            return query.Select(x => new Roles
            {
                Applicationid = x.APPLICATIONID,
                Roleid = x.ROLEID,
                Rolename = x.ROLENAME,
                Isdeleted = (short)x.ISDELETED,
                Description = x.DESCRIPTION,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected override IQueryable<ROLES> FilteredEntities(Roles filter, IQueryable<ROLES> custom_query = null, bool strict = false)
        {
            var predicate = PredicateBuilder.New<ROLES>(true);
            if (!string.IsNullOrEmpty(filter.Rolename)) predicate = (strict)
                        ? predicate.And(x => x.ROLENAME.ToLower() == filter.Rolename.ToLower())
                        : predicate.And(x => x.ROLENAME.ToLower().Contains(filter.Rolename.ToLower()));
            //if (filter.Isdeleted != null && filter.Isdeleted != 0)
            //    predicate = predicate.And(x => x.ISDELETED == filter.Isdeleted);
            if (filter.Isdeleted != null)
                predicate = predicate.And(x => x.ISDELETED == filter.Isdeleted);
            if (!string.IsNullOrEmpty(filter.Description)) predicate = (strict)
                            ? predicate.And(x => x.DESCRIPTION.ToLower() == filter.Description.ToLower())
                            : predicate.And(x => x.DESCRIPTION.ToLower().Contains(filter.Description.ToLower()));
            var query = custom_query ?? _ctx.ROLES;
            return query.Where(predicate);
        }

        protected override IQueryable<ROLES> OrderEntities(IQueryable<ROLES> query, string sortOrder, bool isAscending)
        {
            switch (sortOrder.ToUpper())
            {
                case "APPLICATIONID":
                    query = isAscending ? query.OrderBy(x => x.APPLICATIONID) : query.OrderByDescending(x => x.APPLICATIONID);
                    break;
                case "ROLEID":
                    query = isAscending ? query.OrderBy(x => x.ROLEID) : query.OrderByDescending(x => x.ROLEID);
                    break;
                case "ROLENAME":
                    query = isAscending ? query.OrderBy(x => x.ROLENAME) : query.OrderByDescending(x => x.ROLENAME);
                    break;
                case "ISDELETED":
                    query = isAscending ? query.OrderBy(x => x.ISDELETED) : query.OrderByDescending(x => x.ISDELETED);
                    break;
                case "DESCRIPTION":
                    query = isAscending ? query.OrderBy(x => x.DESCRIPTION) : query.OrderByDescending(x => x.DESCRIPTION);
                    break;
                default:
                    query = query.OrderBy(x => x.ROLEID);
                    break;
            }
            return query;
        }

        protected override async Task<Roles> Insert(Roles item)
        {
            ROLES dbdata = MapToEntity(item);
            dbdata.ROLEID = GenerateId();
            _ctx.ROLES.Add(dbdata);
            _ctx.Entry(dbdata).State = EntityState.Added;
            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("RoleId", string.Format(ValidationErrorMessage.GenericDBSaveError, "Role"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Save Role", item, error.ErrorMessage);
                return item;
            }
            _logger.Log("INFO", "Add Role", item, "Successful.");
            item.Roleid = dbdata.ROLEID;
            return item;
        }

        protected override async Task<Roles> Update(Roles item)
        {
            ROLES dbdata = MapToEntity(item);
            _ctx.ROLES.Attach(dbdata);
            _ctx.Entry(dbdata).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("RoleId", string.Format(ValidationErrorMessage.GenericDBSaveError, "Role"));
                _logger.Log("FATAL", "Update Role", item, error.ErrorMessage);
                item.Validation.Errors.Add(error);
            }
            _logger.Log("INFO", "Update Role", item, "Successful.");
            return item;
        }

        public async Task<List<Roles>> ListRoleByAppId(Guid applicationid)
        {
            return await _ctx.ROLES.AsNoTracking()
                .Where(x => x.APPLICATIONID == applicationid && x.ISDELETED != 1)
                .Select(item => new Roles
                {
                    Applicationid = item.APPLICATIONID,
                    Roleid = item.ROLEID,
                    Rolename  = item.ROLENAME,
                    Description = item.DESCRIPTION,
                    Isdeleted = (short)item.ISDELETED
                }).ToListAsync();
        }

        public async Task<List<Roles>> ListRoleByUserId(Guid Id)
        {
            List<Roles> item = new List<Roles>();
            item = await _ctx.USERS.AsNoTracking()
                .Where(x => x.USERID == Id)
                .Join(_ctx.USERSINROLE.AsNoTracking(),
                     user => user.USERID,
                     userrole => userrole.USERID,
                     (user, userrole) => new { USERS = user, USERSINROLE = userrole })
                .Join(_ctx.ROLES.AsNoTracking(),
                     userrole => userrole.USERSINROLE.ROLEID,
                     role => role.ROLEID,
                    (userrole, role) => new { USERID = userrole.USERS, USERINROLE = userrole.USERSINROLE, ROLE = role })
                .Select(x => new Roles
                {
                    Roleid = x.ROLE.ROLEID,
                    Applicationid = x.ROLE.APPLICATIONID,
                    Description = x.ROLE.DESCRIPTION,
                    Rolename = x.ROLE.ROLENAME
                })
                .ToListAsync();
            return item;
        }

        public async Task<Roles> GetByIdAsync(Guid id)
        {
            return MapToDTO(await _ctx.ROLES.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ROLEID == id));
        }

        public async  Task<Roles> GetByNameAsync(string name)
        {
            return MapToDTO(await _ctx.ROLES.AsNoTracking()
                 .FirstOrDefaultAsync(x => x.ROLENAME.ToLower() == name.ToLower()));
        }

        public async Task<List<Roles>> ListAsync()
        {
            return await _ctx.ROLES.AsNoTracking()
               .Select(item => new Roles
               {
                   Applicationid = item.APPLICATIONID,
                   Roleid = item.ROLEID,
                   Rolename = item.ROLENAME,
                   Description = item.DESCRIPTION,
                   Isdeleted = (short)item.ISDELETED
               }).ToListAsync();
        }

        public async Task<DataList<Roles>> SelectbyUserName(string keywords)
        {           
            var table1 = _ctx.USERS.AsEnumerable();
            var table2 = _ctx.USERSINROLE.AsEnumerable();
            var table3 = _ctx.ROLES.AsEnumerable();

            var td =
            from t1 in table1
            join t2 in table2 on t1.USERID equals t2.USERID
            join t3 in table3 on t2.ROLEID equals t3.ROLEID
            where t3.ISDELETED == 0 && t1.USERNAME.ToLower() == keywords.ToLower()
            select new Roles
            {

                Applicationid = t3.APPLICATIONID,
                Roleid = t3.ROLEID,
                Rolename = t3.ROLENAME,
                Description = t3.DESCRIPTION,
                Isdeleted = (short)t3.ISDELETED

            };
            return new DataList<Roles>
            {
                Count = td.Count(),
                Items = td.ToList()
            };
        }

        public Task<List<Roles>> ListAsync(int page, int size)
        {
            throw new NotImplementedException();
        }

        public Task<List<Roles>> ListAsync(int page, int size, Roles filter)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
