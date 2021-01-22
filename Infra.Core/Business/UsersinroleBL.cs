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
    public class UsersinroleBL : BaseBL<Usersinrole, USERSINROLE, Guid>, IUsersinroleBL
    {
        private readonly InfraEntities _ctx;
        private readonly int _maxPageSize;
        public readonly ILogsBL _logger;
        public UsersinroleBL(InfraEntities ctx, ILogsBL logger)
        {
            _ctx = ctx;
            _logger = logger;
            _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
                : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        }

        #region IEntity
        public async Task<Usersinrole> GetByKeyAsync(Guid key)
        {
            USERSINROLE item = await _ctx.USERSINROLE.AsNoTracking().FirstOrDefaultAsync(x => x.USERINROLEID == key);
            return MapToDTO(item);
        }

        public async Task<DataList<Usersinrole>> List(Usersinrole filter, PageConfig config)
        {
            IQueryable<USERSINROLE> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Userinroleid";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<Usersinrole>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }
        #endregion

        #region ISavable
        public async Task<Usersinrole> SaveAsync(Usersinrole item)
        {
            item.Validation = new UsersinroleValidator(_ctx).Validate(item);
            if (!item.Validation.IsValid) return item;
            return (item.Userinroleid != Guid.Empty) ? await Update(item) : await Insert(item);
        }
        #endregion

        #region IDeletable
        public async Task<Usersinrole> DeleteAsync(Usersinrole item)
        {
            try
            {
                var validator = new UserRoleDeleteValidator(_ctx);
                item.Validation = validator.Validate(item);
                if (!item.Validation.IsValid)
                {
                    return await Task.FromResult(item);
                }

                USERSINROLE data = MapToEntity(item);
                _ctx.Entry(data).State = EntityState.Deleted;
                var error = new ValidationFailure("Role", string.Format(ValidationErrorMessage.GenericDBDeleteError, "Role"));
                item.Validation.Errors.Add(error);
                await _ctx.SaveChangesAsync();
                _logger.Log("FATAL", "Delete User in Role", item, error.ErrorMessage);


                return item;
            }

            catch(Exception ex)
            {
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBDeleteError, "Usersinrole");
                _logger.Log("INFO", "Delete User in Role", item, "Successful.");
                return item;
            }
           
        }

        public async Task<Usersinrole> GetByUserIdRoleId(Guid roleid, Guid userid)
        {
            return MapToDTO(await _ctx.USERSINROLE.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ROLEID == roleid && x.USERID == userid));
        }

        #endregion

        #region HELPERS
        #endregion

        #region BaseBL
        protected override Guid GenerateId(string sequenceName = null)
        {
            return Guid.NewGuid();
        }

        protected override Usersinrole MapToDTO(USERSINROLE item)
        {
            if (item == null) return new Usersinrole();
            return new Usersinrole
            {
                Roleid = item.ROLEID,
                Userid = item.USERID,
                Userinroleid = item.USERINROLEID,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override USERSINROLE MapToEntity(Usersinrole item)
        {
            if (item == null) return new USERSINROLE();
            return new USERSINROLE
            {
                ROLEID = item.Roleid,
                USERID = item.Userid,
                USERINROLEID = item.Userinroleid
            };
        }

        protected override IQueryable<Usersinrole> QueryToDTO(IQueryable<USERSINROLE> query)
        {
            return query.Select(x => new Usersinrole
            {
                Roleid = x.ROLEID,
                Userid = x.USERID,
                Userinroleid = x.USERINROLEID,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected override IQueryable<USERSINROLE> FilteredEntities(Usersinrole filter, IQueryable<USERSINROLE> custom_query = null, bool strict = false)
        {
            var predicate = PredicateBuilder.New<USERSINROLE>(true);

            var query = custom_query ?? _ctx.USERSINROLE;
            return query.Where(predicate);
        }

        protected override IQueryable<USERSINROLE> OrderEntities(IQueryable<USERSINROLE> query, string sortOrder, bool isAscending)
        {
            switch (sortOrder.ToUpper())
            {
                case "ROLEID":
                    query = isAscending ? query.OrderBy(x => x.ROLEID) : query.OrderByDescending(x => x.ROLEID);
                    break;
                case "USERID":
                    query = isAscending ? query.OrderBy(x => x.USERID) : query.OrderByDescending(x => x.USERID);
                    break;
                case "USERINROLEID":
                    query = isAscending ? query.OrderBy(x => x.USERINROLEID) : query.OrderByDescending(x => x.USERINROLEID);
                    break;
                default:
                    query = query.OrderBy(x => x.USERINROLEID);
                    break;
            }
            return query;
        }

        protected override async Task<Usersinrole> Insert(Usersinrole item)
        {
                item.Userinroleid = GenerateId();
                USERSINROLE data = MapToEntity(item);
                _ctx.Entry(data).State = EntityState.Added;

                if (await _ctx.SaveChangesAsync() <= 0)
                {
                    var error = new ValidationFailure("Usersinrole", string.Format(ValidationErrorMessage.GenericDBSaveError, "Usersinrole"));
                    item.Validation.Errors.Add(error);
                    _logger.Log("FATAL", "Save Users in Role", item, error.ErrorMessage);
                    return item;
                }
            _logger.Log("INFO", "Add Users in Role", item, "Successful.");
            return item;
        }

        protected override async Task<Usersinrole> Update(Usersinrole item)
        {
            USERSINROLE data = MapToEntity(item);
           _ctx.Entry(data).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
            { 
                var error = new ValidationFailure("Usersinrole", string.Format(ValidationErrorMessage.GenericDBSaveError, "Usersinrole"));
                item.Validation.Errors.Add(error);
                _logger.Log("INFO", "Update User in Role", item, error.ErrorMessage);
                return item;
            }     
                 _logger.Log("ERROR", "Update User in Role", item, "Successful.");
                 return item;
           
        }

        public async Task<List<Usersinrole>> ListRolesByRoleId(Guid Id)
        {
            return await _ctx.USERSINROLE.AsNoTracking()
                .Where(x => x.ROLEID == Id)
                .Select(item => new Usersinrole
                {
                    Userinroleid = item.USERINROLEID,
                    Userid = item.USERID,
                    Roleid = item.ROLEID
                }).ToListAsync();
        }

        public async Task<List<Usersinrole>> ListAsync()
        {
            return await _ctx.USERSINROLE.AsNoTracking()
                .Select(item => new Usersinrole
                {
                    Userinroleid = item.USERINROLEID,
                    Userid = item.USERID,
                    Roleid = item.ROLEID
                }).ToListAsync();
        }

        public async Task<Usersinrole> GetByUserRoleId(Guid id)
        {
            return MapToDTO(await _ctx.USERSINROLE.AsNoTracking()
                .FirstOrDefaultAsync(x => x.USERINROLEID == id));
        }


        #endregion

    }
}
