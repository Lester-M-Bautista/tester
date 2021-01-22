

using FluentValidation.Results;
using Infra.Common.DTO.Identity;
using Infra.Core.Contract.Identity;
using Infra.Core.Domain;
using LinqKit;
using Macrin.Common;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Infra.Core.Business.Identity
{

    public class IdentityRefreshTokenBL : BaseBL<IdentityRefreshToken, REFRESHTOKEN, Guid>, IIdentityRefreshTokenBL
    {
        private readonly InfraEntities _ctx;
        private readonly int _maxPageSize;
        public IdentityRefreshTokenBL(InfraEntities ctx)
        {
            _ctx = ctx;
            _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
                : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        }

        #region IEntity
        public async Task<IdentityRefreshToken> GetByKeyAsync(Guid key)
        {
            REFRESHTOKEN item = await _ctx.REFRESHTOKEN.AsNoTracking().FirstOrDefaultAsync(x => x.TOKENID == key);
            return MapToDTO(item);
        }

        public async Task<DataList<IdentityRefreshToken>> List(IdentityRefreshToken filter, PageConfig config)
        {
            IQueryable<REFRESHTOKEN> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Tokenid";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<IdentityRefreshToken>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }
        #endregion

        #region ISavable
        public async Task<IdentityRefreshToken> SaveAsync(IdentityRefreshToken item)
        {
            item.Validation = new ValidationResult();
            if (!item.Validation.IsValid) return item;
            return (item.Tokenid != Guid.Empty) ? await Update(item) : await Insert(item);
        }
        #endregion

        #region IDeletable
        public async Task<IdentityRefreshToken> DeleteAsync(IdentityRefreshToken item)
        {
            REFRESHTOKEN data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Deleted;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBDeleteError, "Refreshtoken");
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

        protected override IdentityRefreshToken MapToDTO(REFRESHTOKEN item)
        {
            if (item == null) return new IdentityRefreshToken();
            return new IdentityRefreshToken
            {
                Utccreated = item.UTCCREATED,
                Subject = item.SUBJECT,
                Ticket = item.TICKET,
                Utcexpiry = item.UTCEXPIRY,
                Applicationid = item.APPLICATIONID,
                Tokenid = item.TOKENID,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override REFRESHTOKEN MapToEntity(IdentityRefreshToken item)
        {
            if (item == null) return new REFRESHTOKEN();
            return new REFRESHTOKEN
            {
                UTCCREATED = item.Utccreated ?? DateTime.MinValue,
                SUBJECT = item.Subject,
                TICKET = item.Ticket,
                UTCEXPIRY = item.Utcexpiry ?? DateTime.MinValue,
                APPLICATIONID = item.Applicationid,
                TOKENID = item.Tokenid
            };
        }

        protected override IQueryable<IdentityRefreshToken> QueryToDTO(IQueryable<REFRESHTOKEN> query)
        {
            return query.Select(x => new IdentityRefreshToken
            {
                Utccreated = x.UTCCREATED,
                Subject = x.SUBJECT,
                Ticket = x.TICKET,
                Utcexpiry = x.UTCEXPIRY,
                Applicationid = x.APPLICATIONID,
                Tokenid = x.TOKENID,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected override IQueryable<REFRESHTOKEN> FilteredEntities(IdentityRefreshToken filter, IQueryable<REFRESHTOKEN> custom_query = null, bool strict = false)
        {
            var predicate = PredicateBuilder.New<REFRESHTOKEN>(true);
            if (filter.Utccreated != null && filter.Utccreated != DateTime.MinValue) predicate = predicate.And(x => DbFunctions.TruncateTime(x.UTCCREATED) == filter.Utccreated);
            if (!string.IsNullOrEmpty(filter.Subject)) predicate = (strict)
                            ? predicate.And(x => x.SUBJECT.ToLower() == filter.Subject.ToLower())
                            : predicate.And(x => x.SUBJECT.ToLower().Contains(filter.Subject.ToLower()));
            if (!string.IsNullOrEmpty(filter.Ticket)) predicate = (strict)
                            ? predicate.And(x => x.TICKET.ToLower() == filter.Ticket.ToLower())
                            : predicate.And(x => x.TICKET.ToLower().Contains(filter.Ticket.ToLower()));
            if (filter.Utcexpiry != null && filter.Utcexpiry != DateTime.MinValue) predicate = predicate.And(x => DbFunctions.TruncateTime(x.UTCEXPIRY) == filter.Utcexpiry);


            var query = custom_query ?? _ctx.REFRESHTOKEN;
            return query.Where(predicate);
        }

        protected override IQueryable<REFRESHTOKEN> OrderEntities(IQueryable<REFRESHTOKEN> query, string sortOrder, bool isAscending)
        {
            switch (sortOrder.ToUpper())
            {
                case "UTCCREATED":
                    query = isAscending ? query.OrderBy(x => x.UTCCREATED) : query.OrderByDescending(x => x.UTCCREATED);
                    break;
                case "SUBJECT":
                    query = isAscending ? query.OrderBy(x => x.SUBJECT) : query.OrderByDescending(x => x.SUBJECT);
                    break;
                case "TICKET":
                    query = isAscending ? query.OrderBy(x => x.TICKET) : query.OrderByDescending(x => x.TICKET);
                    break;
                case "UTCEXPIRY":
                    query = isAscending ? query.OrderBy(x => x.UTCEXPIRY) : query.OrderByDescending(x => x.UTCEXPIRY);
                    break;
                case "APPLICATIONID":
                    query = isAscending ? query.OrderBy(x => x.APPLICATIONID) : query.OrderByDescending(x => x.APPLICATIONID);
                    break;
                case "TOKENID":
                    query = isAscending ? query.OrderBy(x => x.TOKENID) : query.OrderByDescending(x => x.TOKENID);
                    break;
                default:
                    query = query.OrderBy(x => x.TOKENID);
                    break;
            }
            return query;
        }

        protected override async Task<IdentityRefreshToken> Insert(IdentityRefreshToken item)
        {
            item.Tokenid = GenerateId();
            REFRESHTOKEN data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Added;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Refreshtoken");
            return item;
        }

        protected override async Task<IdentityRefreshToken> Update(IdentityRefreshToken item)
        {
            REFRESHTOKEN data = MapToEntity(item);
            _ctx.Entry(data).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
                item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Refreshtoken");
            return item;
        }

        public async Task AddRefreshTokenAsync(IdentityRefreshToken token)
        {
            var existingToken = _ctx.REFRESHTOKEN.Where(x => x.APPLICATIONID == token.Applicationid && x.SUBJECT == token.Subject).SingleOrDefault();

            if (existingToken != null)
            {
                _ctx.REFRESHTOKEN.Remove(existingToken);
            }

            var resolved_utccreated = token.Utccreated ?? DateTime.Now;
            var resolved_utcexpiry = token.Utcexpiry ?? DateTime.Now;

            _ctx.REFRESHTOKEN.Add(new REFRESHTOKEN
            {
                TOKENID = token.Tokenid,
                APPLICATIONID = token.Applicationid,
                SUBJECT = token.Subject,
                TICKET = token.Ticket,
                UTCCREATED = resolved_utccreated,
                UTCEXPIRY = resolved_utcexpiry
            });
            await _ctx.SaveChangesAsync();
        }

        public async Task RemoveRefreshTokenAsync(Guid tokenId)
        {
            var existingToken = _ctx.REFRESHTOKEN.FirstOrDefault(x => x.TOKENID == tokenId);
            if (existingToken != null)
            {
                _ctx.REFRESHTOKEN.Remove(existingToken);
                await _ctx.SaveChangesAsync();
            }
        }

        public IdentityRefreshToken GetToken(string tokenId)
        {
            try
            {
                var tokenIdKey = Guid.Parse(tokenId);


                var token = _ctx.REFRESHTOKEN.Where(x => x.TOKENID == tokenIdKey).SingleOrDefault();

                return new IdentityRefreshToken
                {
                    Applicationid = token.APPLICATIONID,
                    Subject = token.SUBJECT,
                    Ticket = token.TICKET,
                    Tokenid = token.TOKENID,
                    Utccreated = token.UTCCREATED,
                    Utcexpiry = token.UTCEXPIRY
                };
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

    }

}
