using FluentValidation.Results;
using Infra.Common.DTO.Identity;
using Infra.Core.Contract.Identity;
using Infra.Core.Domain;
using Infra.Core.Validator.Identity;
using LinqKit;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Core.Business.Identity
{
   
    public class IdentityLoginBL : BaseBL<IdentityLogin, USERLOGIN, Guid>, IIdentityLoginBL
    {
        private readonly InfraEntities _ctx;
        private readonly int _maxPageSize;
        public IdentityLoginBL(InfraEntities ctx)
        {
            _ctx = ctx;
            _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
                : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        }

        #region IEntity

        public async Task<IdentityLogin> GetById(Guid id)
        {
            return MapToDTO(await _ctx.USERLOGIN.AsNoTracking()
                .FirstOrDefaultAsync(x => x.LOGINID == id));
        }

        public async Task<IdentityLogin> GetByKeyAsync(Guid key)
        {
            USERLOGIN item = await _ctx.USERLOGIN.AsNoTracking().FirstOrDefaultAsync(x => x.LOGINID == key);
            return MapToDTO(item);
        }

        public async Task<DataList<IdentityLogin>> List(IdentityLogin filter, PageConfig config)
        {
            IQueryable<USERLOGIN> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Loginid";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<IdentityLogin>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }
        #endregion

        #region ISavable
        public async Task<IdentityLogin> SaveAsync(IdentityLogin item)
        {
            item.Validation = new IdentityLoginValidator(_ctx).Validate(item);
            if (!item.Validation.IsValid) return item;
            return (item.Loginid != Guid.Empty) ? await Update(item) : await Insert(item);
        }
        #endregion

        #region IDeletable
        public async Task<IdentityLogin> DeleteAsync(IdentityLogin item)
        {
            USERLOGIN data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Deleted;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBDeleteError, "Userlogin");
            if (item.Validation == null) item.Validation = new ValidationResult();
            return item;
        }
        #endregion

        #region HELPERS
        #endregion

        #region BaseBL
        protected override Guid GenerateId(string sequenceName = null)
        {
            //This function was created only for the sake creating an id from Sequence
            throw new NotImplementedException();
        }

        protected override IdentityLogin MapToDTO(USERLOGIN item)
        {
            if (item == null) return new IdentityLogin();
            return new IdentityLogin
            {
                Loginid = item.LOGINID,
                Useraccountid = item.USERACCOUNTID,
                Providerkey = item.PROVIDERKEY,
                Providername = item.PROVIDERNAME,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override USERLOGIN MapToEntity(IdentityLogin item)
        {
            if (item == null) return new USERLOGIN();
            return new USERLOGIN
            {
                LOGINID = item.Loginid,
                USERACCOUNTID = item.Useraccountid,
                PROVIDERKEY = item.Providerkey,
                PROVIDERNAME = item.Providername
            };
        }

        protected override IQueryable<IdentityLogin> QueryToDTO(IQueryable<USERLOGIN> query)
        {
            return query.Select(x => new IdentityLogin
            {
                Loginid = x.LOGINID,
                Useraccountid = x.USERACCOUNTID,
                Providerkey = x.PROVIDERKEY,
                Providername = x.PROVIDERNAME,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected override IQueryable<USERLOGIN> FilteredEntities(IdentityLogin filter, IQueryable<USERLOGIN> custom_query = null, bool strict = false)
        {
            var predicate = PredicateBuilder.New<USERLOGIN>(true);
            if (!string.IsNullOrEmpty(filter.Providerkey)) predicate = (strict)
                        ? predicate.And(x => x.PROVIDERKEY.ToLower() == filter.Providerkey.ToLower())
                        : predicate.And(x => x.PROVIDERKEY.ToLower().Contains(filter.Providerkey.ToLower()));
            if (!string.IsNullOrEmpty(filter.Providername)) predicate = (strict)
                            ? predicate.And(x => x.PROVIDERNAME.ToLower() == filter.Providername.ToLower())
                            : predicate.And(x => x.PROVIDERNAME.ToLower().Contains(filter.Providername.ToLower()));
            var query = custom_query ?? _ctx.USERLOGIN;
            return query.Where(predicate);
        }

        protected override IQueryable<USERLOGIN> OrderEntities(IQueryable<USERLOGIN> query, string sortOrder, bool isAscending)
        {
            switch (sortOrder.ToUpper())
            {
                case "LOGINID":
                    query = isAscending ? query.OrderBy(x => x.LOGINID) : query.OrderByDescending(x => x.LOGINID);
                    break;
                case "USERACCOUNTID":
                    query = isAscending ? query.OrderBy(x => x.USERACCOUNTID) : query.OrderByDescending(x => x.USERACCOUNTID);
                    break;
                case "PROVIDERKEY":
                    query = isAscending ? query.OrderBy(x => x.PROVIDERKEY) : query.OrderByDescending(x => x.PROVIDERKEY);
                    break;
                case "PROVIDERNAME":
                    query = isAscending ? query.OrderBy(x => x.PROVIDERNAME) : query.OrderByDescending(x => x.PROVIDERNAME);
                    break;
                default:
                    query = query.OrderBy(x => x.LOGINID);
                    break;
            }
            return query;
        }

        protected override async Task<IdentityLogin> Insert(IdentityLogin item)
        {
            item.Loginid = GenerateId();
            USERLOGIN data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Added;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Userlogin");
            return item;
        }

        protected override async Task<IdentityLogin> Update(IdentityLogin item)
        {
            USERLOGIN data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Userlogin");
            return item;
        }
        #endregion

    }
}
