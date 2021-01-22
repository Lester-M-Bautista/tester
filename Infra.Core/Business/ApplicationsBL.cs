using FluentValidation.Results;
using Infra.Common.DTO;
using Infra.Core.Contract;
using Infra.Core.Domain;
using LinqKit;
using Macrin.Common;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Infra.Core.Business
{
    public class ApplicationsBL : BaseBL<Applications, APPLICATIONS, Guid>, IApplicationsBL
    {
        private readonly InfraEntities _ctx;
        private readonly int _maxPageSize;
        public readonly ILogsBL _logger;
        public ApplicationsBL(InfraEntities ctx, ILogsBL logger)
        {
            _ctx = ctx;
            _logger = logger;
            _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
                : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        }

        #region IEntity
        public async Task<Applications> GetByKeyAsync(Guid key)
        {
            APPLICATIONS item = await _ctx.APPLICATIONS.AsNoTracking().FirstOrDefaultAsync(x => x.APPLICATIONID == key);
            return MapToDTO(item);
        }

        public async Task<Applications> GetByIdAsync(Guid id)
        {
            return MapToDTO(await _ctx.APPLICATIONS.AsNoTracking()
                .FirstOrDefaultAsync(x => x.APPLICATIONID == id));
        }

        public async Task<Applications> GetByNameAsync(string name)
        {
            return MapToDTO(await _ctx.APPLICATIONS.AsNoTracking()
                .FirstOrDefaultAsync(x => x.APPLICATIONNAME.ToLower() == name.ToLower()));
        }
        public async Task<bool> ValidateAsync(string application, Guid secret)
        {
            return await _ctx.APPLICATIONS
                .AnyAsync(x => x.APPLICATIONNAME.ToLower() == application.ToLower() && x.APPLICAITONSECRET == secret);
        }

        public async Task<DataList<Applications>> List(Applications filter, PageConfig config)
        {
            IQueryable<APPLICATIONS> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Applicationid";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<Applications>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }
        #endregion

        #region ISavable
        public async Task<Applications> SaveAsync(Applications item)
        {
            item.Validation = new ApplicationsValidator(_ctx).Validate(item);
            if (!item.Validation.IsValid) return item;
            return (item.Applicationid != Guid.Empty) ? await Update(item) : await Insert(item);
        }
        #endregion

        #region IDeletable
        public async Task<Applications> DeleteAsync(Applications item)
        {
            var data = await _ctx.APPLICATIONS.FirstOrDefaultAsync(x => x.APPLICATIONID == item.Applicationid);
            data.ISDELETED = true;
            _ctx.Entry(data).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("Applications", string.Format(ValidationErrorMessage.GenericDBDeleteError, "Applications"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Delete Application", item, error.ErrorMessage);
                return await Task.FromResult<Applications>(item);
            }
            _logger.Log("INFO", "Delete Application", item, "Successful.");
            return await Task.FromResult<Applications>(MapToDTO(data));

        }
        #endregion

        #region HELPERS
        #endregion

        #region BaseBL
        protected override Guid GenerateId(string sequenceName = null)
        {
            return Guid.NewGuid();
        }

        protected override Applications MapToDTO(APPLICATIONS item)
        {
            if (item == null) return new Applications();
            return new Applications
            {
                Applicationid = item.APPLICATIONID,
                Isdeleted = item.ISDELETED,
                Applicaitonsecret = item.APPLICAITONSECRET,
                Description = item.DESCRIPTION,
                Allowedorigin = item.ALLOWEDORIGIN,
                Applicationname = item.APPLICATIONNAME,
                Refreshlifetime = item.REFRESHLIFETIME,
                Isnative = item.ISNATIVE,
                Redirecturl = item.REDIRECTURL,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override APPLICATIONS MapToEntity(Applications item)
        {
            if (item == null) return new APPLICATIONS();
            return new APPLICATIONS
            {
                APPLICATIONID = item.Applicationid,
                ISDELETED = item.Isdeleted ?? false,
                APPLICAITONSECRET = item.Applicaitonsecret,
                DESCRIPTION = item.Description,
                ALLOWEDORIGIN = item.Allowedorigin,
                APPLICATIONNAME = item.Applicationname,
                REFRESHLIFETIME = item.Refreshlifetime ?? 0,
                ISNATIVE = item.Isnative ?? false,
                REDIRECTURL = item.Redirecturl
            };
        }

        protected override IQueryable<Applications> QueryToDTO(IQueryable<APPLICATIONS> query)
        {
            return query.Select(x => new Applications
            {
                Applicationid = x.APPLICATIONID,
                Isdeleted = x.ISDELETED,
                Applicaitonsecret = x.APPLICAITONSECRET,
                Description = x.DESCRIPTION,
                Allowedorigin = x.ALLOWEDORIGIN,
                Applicationname = x.APPLICATIONNAME,
                Refreshlifetime = x.REFRESHLIFETIME,
                Isnative = x.ISNATIVE,
                Redirecturl = x.REDIRECTURL,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected override IQueryable<APPLICATIONS> FilteredEntities(Applications filter, IQueryable<APPLICATIONS> custom_query = null, bool strict = false)
        {
            var predicate = PredicateBuilder.New<APPLICATIONS>(true);
            //if (filter.Isdeleted != null && filter.Isdeleted != false)
            //    predicate = predicate.And(x => x.ISDELETED == filter.Isdeleted);

            if (!string.IsNullOrEmpty(filter.Description)) predicate = (strict)
                            ? predicate.And(x => x.DESCRIPTION.ToLower() == filter.Description.ToLower())
                            : predicate.And(x => x.DESCRIPTION.ToLower().Contains(filter.Description.ToLower()));
            if (!string.IsNullOrEmpty(filter.Allowedorigin)) predicate = (strict)
                            ? predicate.And(x => x.ALLOWEDORIGIN.ToLower() == filter.Allowedorigin.ToLower())
                            : predicate.And(x => x.ALLOWEDORIGIN.ToLower().Contains(filter.Allowedorigin.ToLower()));
            if (!string.IsNullOrEmpty(filter.Applicationname)) predicate = (strict)
                            ? predicate.And(x => x.APPLICATIONNAME.ToLower() == filter.Applicationname.ToLower())
                            : predicate.And(x => x.APPLICATIONNAME.ToLower().Contains(filter.Applicationname.ToLower()));
            if (filter.Refreshlifetime != null && filter.Refreshlifetime != 0)
                predicate = predicate.And(x => x.REFRESHLIFETIME == filter.Refreshlifetime);
            if (filter.Isnative != null && filter.Isnative != false)
                predicate = predicate.And(x => x.ISNATIVE == filter.Isnative);
            if (!string.IsNullOrEmpty(filter.Redirecturl)) predicate = (strict)
                            ? predicate.And(x => x.REDIRECTURL.ToLower() == filter.Redirecturl.ToLower())
                            : predicate.And(x => x.REDIRECTURL.ToLower().Contains(filter.Redirecturl.ToLower()));
            var query = custom_query ?? _ctx.APPLICATIONS;
            return query.Where(predicate);
        }

        protected override IQueryable<APPLICATIONS> OrderEntities(IQueryable<APPLICATIONS> query, string sortOrder, bool isAscending)
        {
            switch (sortOrder.ToUpper())
            {
                case "APPLICATIONID":
                    query = isAscending ? query.OrderBy(x => x.APPLICATIONID) : query.OrderByDescending(x => x.APPLICATIONID);
                    break;
                case "ISDELETED":
                    query = isAscending ? query.OrderBy(x => x.ISDELETED) : query.OrderByDescending(x => x.ISDELETED);
                    break;
                case "APPLICAITONSECRET":
                    query = isAscending ? query.OrderBy(x => x.APPLICAITONSECRET) : query.OrderByDescending(x => x.APPLICAITONSECRET);
                    break;
                case "DESCRIPTION":
                    query = isAscending ? query.OrderBy(x => x.DESCRIPTION) : query.OrderByDescending(x => x.DESCRIPTION);
                    break;
                case "ALLOWEDORIGIN":
                    query = isAscending ? query.OrderBy(x => x.ALLOWEDORIGIN) : query.OrderByDescending(x => x.ALLOWEDORIGIN);
                    break;
                case "APPLICATIONNAME":
                    query = isAscending ? query.OrderBy(x => x.APPLICATIONNAME) : query.OrderByDescending(x => x.APPLICATIONNAME);
                    break;
                case "REFRESHLIFETIME":
                    query = isAscending ? query.OrderBy(x => x.REFRESHLIFETIME) : query.OrderByDescending(x => x.REFRESHLIFETIME);
                    break;
                case "ISNATIVE":
                    query = isAscending ? query.OrderBy(x => x.ISNATIVE) : query.OrderByDescending(x => x.ISNATIVE);
                    break;
                case "REDIRECTURL":
                    query = isAscending ? query.OrderBy(x => x.REDIRECTURL) : query.OrderByDescending(x => x.REDIRECTURL);
                    break;
                default:
                    query = query.OrderBy(x => x.APPLICATIONID);
                    break;
            }
            return query;
        }     
        protected override async Task<Applications> Insert(Applications item)
        {
            try
            {
                APPLICATIONS data = MapToEntity(item);
                data.APPLICATIONID = GenerateId();
                data.APPLICAITONSECRET = GenerateId();
                _ctx.APPLICATIONS.Add(data);
                _ctx.Entry(data).State = System.Data.Entity.EntityState.Added;
                if (await _ctx.SaveChangesAsync() <= 0)
                {
                    var error = new ValidationFailure("Application", string.Format(ValidationErrorMessage.GenericDBSaveError, "Application"));
                    item.Validation.Errors.Add(error);
                    _logger.Log("FATAL", "Save Application", item, error.ErrorMessage);
                    item.Validation.Errors.Add(error);
                    return item;
                }
                _logger.Log("FATAL", "Add Application", item, "Successful.");
                return await GetByNameAsync(data.APPLICATIONNAME);
            }
            catch (Exception ex)
            {
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBDeleteError, "PERMISSIONS");
                _logger.Log("INFO", "Save Application", item, ex.Message);
                return item;
            }
        }

        protected override async Task<Applications> Update(Applications item)
        {
            APPLICATIONS data = MapToEntity(item);
            _ctx.APPLICATIONS.Attach(data);
            _ctx.Entry(data).State = System.Data.Entity.EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("Application", string.Format(ValidationErrorMessage.GenericDBSaveError, "Application"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Update Application", item, error.ErrorMessage);
                return item;
            }
            _logger.Log("INFO", "Update Application", item, "Successful.");
            return item;
        }
        #endregion

    }
}
