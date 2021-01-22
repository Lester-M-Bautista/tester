using FluentValidation.Results;
using Infra.Common.DTO.Identity;
using Infra.Core.Contract.Identity;
using Infra.Core.Domain;
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

    public class IdentityProfileBL : BaseBL<IdentityProfile, USERPROFILE, Guid>, IIdentityProfileBL
    {
        private readonly InfraEntities _ctx;
        private readonly int _maxPageSize;
        public IdentityProfileBL(InfraEntities ctx)
        {
            _ctx = ctx;
            _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
                : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        }

        #region IEntity
        public async Task<IdentityProfile> GetByKeyAsync(Guid key)
        {
            USERPROFILE item = await _ctx.USERPROFILE.AsNoTracking().FirstOrDefaultAsync(x => x.USERACCOUNTID == key);
            return MapToDTO(item);
        }

        public async Task<DataList<IdentityProfile>> List(IdentityProfile filter, PageConfig config)
        {
            IQueryable<USERPROFILE> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Useraccountid";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<IdentityProfile>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }
        #endregion

        #region ISavable
        public async Task<IdentityProfile> SaveAsync(IdentityProfile item)
        {
            item.Validation = new ValidationResult();
            if (!item.Validation.IsValid) return item;
            var existing = await GetByKeyAsync(item.Useraccountid);

        return item;
        }
        #endregion

        #region IDeletable
        public async Task<IdentityProfile> DeleteAsync(IdentityProfile item)
        {
            USERPROFILE data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Deleted;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBDeleteError, "Userprofile");
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

        protected override IdentityProfile MapToDTO(USERPROFILE item)
        {
            if (item == null) return new IdentityProfile();
            return new IdentityProfile
            {
                Useraccountid = item.USERACCOUNTID,
                Birthdate = item.BIRTHDATE,
                Gender = item.GENDER,
                Profilepicture = item.PROFILEPICTURE,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override USERPROFILE MapToEntity(IdentityProfile item)
        {
            if (item == null) return new USERPROFILE();
            return new USERPROFILE
            {
                USERACCOUNTID = item.Useraccountid,
                BIRTHDATE = item.Birthdate ?? DateTime.MinValue,
                GENDER = item.Gender,
                PROFILEPICTURE = item.Profilepicture
            };
        }

        protected override IQueryable<IdentityProfile> QueryToDTO(IQueryable<USERPROFILE> query)
        {
            return query.Select(x => new IdentityProfile
            {
                Useraccountid = x.USERACCOUNTID,
                Birthdate = x.BIRTHDATE,
                Gender = x.GENDER,
                Profilepicture = x.PROFILEPICTURE,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected override IQueryable<USERPROFILE> FilteredEntities(IdentityProfile filter, IQueryable<USERPROFILE> custom_query = null, bool strict = false)
        {
            var predicate = PredicateBuilder.New<USERPROFILE>(true);
            if (filter.Birthdate != null && filter.Birthdate != DateTime.MinValue) predicate = predicate.And(x => DbFunctions.TruncateTime(x.BIRTHDATE) == filter.Birthdate);
            if (!string.IsNullOrEmpty(filter.Gender)) predicate = (strict)
                            ? predicate.And(x => x.GENDER.ToLower() == filter.Gender.ToLower())
                            : predicate.And(x => x.GENDER.ToLower().Contains(filter.Gender.ToLower()));

            var query = custom_query ?? _ctx.USERPROFILE;
            return query.Where(predicate);
        }

        protected override IQueryable<USERPROFILE> OrderEntities(IQueryable<USERPROFILE> query, string sortOrder, bool isAscending)
        {
            switch (sortOrder.ToUpper())
            {
                case "USERACCOUNTID":
                    query = isAscending ? query.OrderBy(x => x.USERACCOUNTID) : query.OrderByDescending(x => x.USERACCOUNTID);
                    break;
                case "BIRTHDATE":
                    query = isAscending ? query.OrderBy(x => x.BIRTHDATE) : query.OrderByDescending(x => x.BIRTHDATE);
                    break;
                case "GENDER":
                    query = isAscending ? query.OrderBy(x => x.GENDER) : query.OrderByDescending(x => x.GENDER);
                    break;
                case "PROFILEPICTURE":
                    query = isAscending ? query.OrderBy(x => x.PROFILEPICTURE) : query.OrderByDescending(x => x.PROFILEPICTURE);
                    break;
                default:
                    query = query.OrderBy(x => x.USERACCOUNTID);
                    break;
            }
            return query;
        }

        protected override async Task<IdentityProfile> Insert(IdentityProfile item)
        {
            item.Useraccountid = GenerateId();
            USERPROFILE data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Added;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Userprofile");
            return item;
        }

        protected override async Task<IdentityProfile> Update(IdentityProfile item)
        {
            USERPROFILE data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Userprofile");
            return item;
        }

        public async Task<IdentityProfileFull> GetById(Guid id)
        {
            return await _ctx.USERS
                                     .Where(x => x.USERID == id)
                                     .GroupJoin(
                                           _ctx.USERPROFILE,
                                           user => user.USERID,
                                           profile => profile.USERACCOUNTID,
                                           (x, y) => new { ID_USER = x, ID_USERPROFILE = y })
                                     .SelectMany(
                                           xy => xy.ID_USERPROFILE.DefaultIfEmpty(),
                                           (x, y) => new { ID_USER = x.ID_USER, ID_USERPROFILE = y }
                                     ).Select(s => new IdentityProfileFull
                                     {
                                         Birthdate = s.ID_USERPROFILE.BIRTHDATE,
                                         Fullname = s.ID_USER.USERFULLNAME,
                                         Gender = s.ID_USERPROFILE.GENDER,
                                         Profilepicture = s.ID_USERPROFILE.PROFILEPICTURE,
                                         Useraccountid = s.ID_USERPROFILE.USERACCOUNTID,
                                         Email = s.ID_USER.USEREMAIL,
                                         Department = s.ID_USER.DEPARTMENT,
                                         Username = s.ID_USER.USERNAME
                                     })
                                     .FirstOrDefaultAsync();
        }

        public async Task<IdentityProfileFull> GetByName(string username)
        {
            return await _ctx.USERS
                                      .Where(x => x.USERNAME.ToLower() == username.ToLower())
                                      .GroupJoin(
                                            _ctx.USERPROFILE,
                                            user => user.USERID,
                                            profile => profile.USERACCOUNTID,
                                            (x, y) => new { ID_USER = x, ID_USERPROFILE = y })
                                      .SelectMany(
                                            xy => xy.ID_USERPROFILE.DefaultIfEmpty(),
                                            (x, y) => new { ID_USER = x.ID_USER, ID_USERPROFILE = y }
                                      ).Select(s => new IdentityProfileFull
                                      {
                                          Birthdate = s.ID_USERPROFILE.BIRTHDATE,
                                          Fullname = s.ID_USER.USERFULLNAME,
                                          Gender = s.ID_USERPROFILE.GENDER,
                                          Profilepicture = s.ID_USERPROFILE.PROFILEPICTURE,
                                          Useraccountid = s.ID_USERPROFILE.USERACCOUNTID,
                                          Email = s.ID_USER.USEREMAIL,
                                          Department = s.ID_USER.DEPARTMENT,
                                          Username = s.ID_USER.USERNAME
                                      })
                                      .FirstOrDefaultAsync();
        }

        public async Task<IdentityProfileFull> GetByEmail(string email)
        {
            return await _ctx.USERS
                                     .Where(x => x.USEREMAIL == email)
                                     .GroupJoin(
                                           _ctx.USERPROFILE,
                                           user => user.USERID,
                                           profile => profile.USERACCOUNTID,
                                           (x, y) => new { ID_USER = x, ID_USERPROFILE = y })
                                     .SelectMany(
                                           xy => xy.ID_USERPROFILE.DefaultIfEmpty(),
                                           (x, y) => new { ID_USER = x.ID_USER, ID_USERPROFILE = y }
                                     ).Select(s => new IdentityProfileFull
                                     {
                                         Birthdate = s.ID_USERPROFILE.BIRTHDATE,
                                         Fullname = s.ID_USER.USERFULLNAME,
                                         Gender = s.ID_USERPROFILE.GENDER,
                                         Profilepicture = s.ID_USERPROFILE.PROFILEPICTURE,
                                         Useraccountid = s.ID_USERPROFILE.USERACCOUNTID,
                                         Email = s.ID_USER.USEREMAIL,
                                         Department = s.ID_USER.DEPARTMENT,
                                         Username = s.ID_USER.USERNAME
                                     })
                                     .FirstOrDefaultAsync();
        }


        #endregion

    }
}
