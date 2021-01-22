using FluentValidation.Results;
using Infra.Common;
using Infra.Common.DTO.Identity;
using Infra.Core.Business;
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
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Helpers;
using static Infra.Core.Validator.Identity.IdentityUserValidator;
using SecurityErrorMessage = Macrin.Common.SecurityErrorMessage;
using ValidationErrorMessage = Macrin.Common.ValidationErrorMessage;

public class IdentityUserBL : BaseBL<IdentityUser, USERS, Guid>, IIdentityUserBL
{
    private readonly InfraEntities _ctx;
    private readonly int _maxPageSize;
    private readonly LogsBL _logger;
    public IdentityUserBL(InfraEntities ctx)
    {
        _ctx = ctx;
        _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
            : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        _logger = new LogsBL(_ctx);
    }

    #region IEntity
    public async Task<IdentityUser> GetByKeyAsync(Guid key)
    {
        USERS item = await _ctx.USERS.AsNoTracking().FirstOrDefaultAsync(x => x.USERID == key);
        return MapToDTO(item);
    }

    public async Task<DataList<IdentityUser>> List(IdentityUser filter, PageConfig config)
    {
        IQueryable<USERS> query = FilteredEntities(filter);

        string resolved_sort = config.SortBy ?? "Userid";
        bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

        int resolved_size = config.Size ?? _maxPageSize;
        if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
        int resolved_index = config.Index ?? 1;

        query = OrderEntities(query, resolved_sort, resolved_isAscending);
        //var paged = PagedQuery(query, resolved_size, resolved_index);
        return new DataList<IdentityUser>
        {
            Count = await query.CountAsync(),
            Items = await QueryToDTO(query).ToListAsync()
        };
    }
    #endregion

    #region ISavable
    public async Task<IdentityUser> SaveAsync(IdentityUser item)
    {
        item.Validation = new IdentityUserValidator(_ctx).Validate(item);
        if (!item.Validation.IsValid) return item;
        return (item.Id != Guid.Empty ? await Update(item) : await Insert(item));
    }
    #endregion

    #region IDeletable
    public async Task<IdentityUser> DeleteAsync(IdentityUser item)
    {
        USERS data = MapToEntity(item);
        _ctx.Entry(data).State = EntityState.Deleted;
        if (await _ctx.SaveChangesAsync() <= 0)
            item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBDeleteError, "Users");
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

    protected override IdentityUser MapToDTO(USERS item)
    {
        if (item == null) return new IdentityUser();
        return new IdentityUser
        {
            Datelastlockout = item.DATELASTLOCKOUT,
            Department = item.DEPARTMENT,
            Dateaccessexpiry = item.DATEACCESSEXPIRY,
            Datelastactivity = item.DATELASTACTIVITY,
            Datelastlogin = item.DATELASTLOGIN,
            Isapproved = item.ISAPPROVED,
            Datecreated = item.DATECREATED,
            Password = item.PASSWORD,
            Islockedout = item.ISLOCKEDOUT,
            Useremail = item.USEREMAIL,
            Passwordfailcount = item.PASSWORDFAILCOUNT,
            Isdeleted = item.ISDELETED,
            Datechangepass = item.DATECHANGEPASS,
            Userfullname = item.USERFULLNAME,
            Passwordformat = item.PASSWORDFORMAT,
            Passwordsalt = item.PASSWORDSALT,
            UserName = item.USERNAME,
            Id = item.USERID,
            TransContext = new Macrin.Common.TransactionContext(),
            Validation = new ValidationResult()
        };
    }

    protected override USERS MapToEntity(IdentityUser item)
    {
        if (item == null) return new USERS();
        return new USERS
        {
            DATELASTLOCKOUT = item.Datelastlockout ?? DateTime.MinValue,
            DEPARTMENT = item.Department,
            DATEACCESSEXPIRY = item.Dateaccessexpiry ?? DateTime.MinValue,
            DATELASTACTIVITY = item.Datelastactivity ?? DateTime.MinValue,
            DATELASTLOGIN = item.Datelastlogin ?? DateTime.MinValue,
            ISAPPROVED = item.Isapproved ?? false,
            DATECREATED = item.Datecreated ?? DateTime.MinValue,
            PASSWORD = item.Password,
            ISLOCKEDOUT = item.Islockedout ?? false,
            USEREMAIL = item.Useremail,
            PASSWORDFAILCOUNT = item.Passwordfailcount ?? 0,
            ISDELETED = item.Isdeleted ?? false,
            DATECHANGEPASS = item.Datechangepass ?? DateTime.MinValue,
            USERFULLNAME = item.Userfullname,
            PASSWORDFORMAT = item.Passwordformat ?? 0,
            PASSWORDSALT = item.Passwordsalt,
            USERNAME = item.UserName,
            USERID = item.Id
        };
    }

    protected override IQueryable<IdentityUser> QueryToDTO(IQueryable<USERS> query)
    {
        return query.Select(x => new IdentityUser
        {
            Datelastlockout = x.DATELASTLOCKOUT,
            Department = x.DEPARTMENT,
            Dateaccessexpiry = x.DATEACCESSEXPIRY,
            Datelastactivity = x.DATELASTACTIVITY,
            Datelastlogin = x.DATELASTLOGIN,
            Isapproved = x.ISAPPROVED,
            Datecreated = x.DATECREATED,
            Password = x.PASSWORD,
            Islockedout = x.ISLOCKEDOUT,
            Useremail = x.USEREMAIL,
            Passwordfailcount = x.PASSWORDFAILCOUNT,
            Isdeleted = x.ISDELETED,
            Datechangepass = x.DATECHANGEPASS,
            Userfullname = x.USERFULLNAME,
            Passwordformat = x.PASSWORDFORMAT,
            Passwordsalt = x.PASSWORDSALT,
            UserName = x.USERNAME,
            Id = x.USERID,
            TransContext = new Macrin.Common.TransactionContext(),
            Validation = new ValidationResult()
        });
    }

    protected override IQueryable<USERS> FilteredEntities(IdentityUser filter, IQueryable<USERS> custom_query = null, bool strict = false)
    {
        var predicate = PredicateBuilder.New<USERS>(true);
        if (filter.Datelastlockout != null && filter.Datelastlockout != DateTime.MinValue) predicate = predicate.And(x => DbFunctions.TruncateTime(x.DATELASTLOCKOUT) == filter.Datelastlockout);

        if (filter.Dateaccessexpiry != null && filter.Dateaccessexpiry != DateTime.MinValue) predicate = predicate.And(x => DbFunctions.TruncateTime(x.DATEACCESSEXPIRY) == filter.Dateaccessexpiry);
        if (filter.Datelastactivity != null && filter.Datelastactivity != DateTime.MinValue) predicate = predicate.And(x => DbFunctions.TruncateTime(x.DATELASTACTIVITY) == filter.Datelastactivity);
        if (filter.Datelastlogin != null && filter.Datelastlogin != DateTime.MinValue) predicate = predicate.And(x => DbFunctions.TruncateTime(x.DATELASTLOGIN) == filter.Datelastlogin);
        if (filter.Isapproved != null && filter.Isapproved != false)
            predicate = predicate.And(x => x.ISAPPROVED == filter.Isapproved);
        if (filter.Datecreated != null && filter.Datecreated != DateTime.MinValue) predicate = predicate.And(x => DbFunctions.TruncateTime(x.DATECREATED) == filter.Datecreated);
        if (!string.IsNullOrEmpty(filter.Password)) predicate = (strict)
                        ? predicate.And(x => x.PASSWORD.ToLower() == filter.Password.ToLower())
                        : predicate.And(x => x.PASSWORD.ToLower().Contains(filter.Password.ToLower()));
        if (filter.Islockedout != null && filter.Islockedout != false)
            predicate = predicate.And(x => x.ISLOCKEDOUT == filter.Islockedout);
        if (!string.IsNullOrEmpty(filter.Useremail)) predicate = (strict)
                        ? predicate.And(x => x.USEREMAIL.ToLower() == filter.Useremail.ToLower())
                        : predicate.And(x => x.USEREMAIL.ToLower().Contains(filter.Useremail.ToLower()));
        if (filter.Passwordfailcount != null && filter.Passwordfailcount != 0)
            predicate = predicate.And(x => x.PASSWORDFAILCOUNT == filter.Passwordfailcount);
        if (filter.Isdeleted != null && filter.Isdeleted != false)
            predicate = predicate.And(x => x.ISDELETED == filter.Isdeleted);
        if (filter.Datechangepass != null && filter.Datechangepass != DateTime.MinValue) predicate = predicate.And(x => DbFunctions.TruncateTime(x.DATECHANGEPASS) == filter.Datechangepass);
        if (!string.IsNullOrEmpty(filter.Userfullname)) predicate = (strict)
                        ? predicate.And(x => x.USERFULLNAME.ToLower() == filter.Userfullname.ToLower())
                        : predicate.And(x => x.USERFULLNAME.ToLower().Contains(filter.Userfullname.ToLower()));
        if (filter.Passwordformat != null && filter.Passwordformat != 0)
            predicate = predicate.And(x => x.PASSWORDFORMAT == filter.Passwordformat);
        if (!string.IsNullOrEmpty(filter.Passwordsalt)) predicate = (strict)
                        ? predicate.And(x => x.PASSWORDSALT.ToLower() == filter.Passwordsalt.ToLower())
                        : predicate.And(x => x.PASSWORDSALT.ToLower().Contains(filter.Passwordsalt.ToLower()));
        if (!string.IsNullOrEmpty(filter.UserName)) predicate = (strict)
                        ? predicate.And(x => x.USERNAME.ToLower() == filter.UserName.ToLower())
                        : predicate.And(x => x.USERNAME.ToLower().Contains(filter.UserName.ToLower()));

        var query = custom_query ?? _ctx.USERS;
        return query.Where(predicate);
    }

    protected override IQueryable<USERS> OrderEntities(IQueryable<USERS> query, string sortOrder, bool isAscending)
    {
        switch (sortOrder.ToUpper())
        {
            case "DATELASTLOCKOUT":
                query = isAscending ? query.OrderBy(x => x.DATELASTLOCKOUT) : query.OrderByDescending(x => x.DATELASTLOCKOUT);
                break;
            case "DEPARTMENT":
                query = isAscending ? query.OrderBy(x => x.DEPARTMENT) : query.OrderByDescending(x => x.DEPARTMENT);
                break;
            case "DATEACCESSEXPIRY":
                query = isAscending ? query.OrderBy(x => x.DATEACCESSEXPIRY) : query.OrderByDescending(x => x.DATEACCESSEXPIRY);
                break;
            case "DATELASTACTIVITY":
                query = isAscending ? query.OrderBy(x => x.DATELASTACTIVITY) : query.OrderByDescending(x => x.DATELASTACTIVITY);
                break;
            case "DATELASTLOGIN":
                query = isAscending ? query.OrderBy(x => x.DATELASTLOGIN) : query.OrderByDescending(x => x.DATELASTLOGIN);
                break;
            case "ISAPPROVED":
                query = isAscending ? query.OrderBy(x => x.ISAPPROVED) : query.OrderByDescending(x => x.ISAPPROVED);
                break;
            case "DATECREATED":
                query = isAscending ? query.OrderBy(x => x.DATECREATED) : query.OrderByDescending(x => x.DATECREATED);
                break;
            case "PASSWORD":
                query = isAscending ? query.OrderBy(x => x.PASSWORD) : query.OrderByDescending(x => x.PASSWORD);
                break;
            case "ISLOCKEDOUT":
                query = isAscending ? query.OrderBy(x => x.ISLOCKEDOUT) : query.OrderByDescending(x => x.ISLOCKEDOUT);
                break;
            case "USEREMAIL":
                query = isAscending ? query.OrderBy(x => x.USEREMAIL) : query.OrderByDescending(x => x.USEREMAIL);
                break;
            case "PASSWORDFAILCOUNT":
                query = isAscending ? query.OrderBy(x => x.PASSWORDFAILCOUNT) : query.OrderByDescending(x => x.PASSWORDFAILCOUNT);
                break;
            case "ISDELETED":
                query = isAscending ? query.OrderBy(x => x.ISDELETED) : query.OrderByDescending(x => x.ISDELETED);
                break;
            case "DATECHANGEPASS":
                query = isAscending ? query.OrderBy(x => x.DATECHANGEPASS) : query.OrderByDescending(x => x.DATECHANGEPASS);
                break;
            case "USERFULLNAME":
                query = isAscending ? query.OrderBy(x => x.USERFULLNAME) : query.OrderByDescending(x => x.USERFULLNAME);
                break;
            case "PASSWORDFORMAT":
                query = isAscending ? query.OrderBy(x => x.PASSWORDFORMAT) : query.OrderByDescending(x => x.PASSWORDFORMAT);
                break;
            case "PASSWORDSALT":
                query = isAscending ? query.OrderBy(x => x.PASSWORDSALT) : query.OrderByDescending(x => x.PASSWORDSALT);
                break;
            case "USERNAME":
                query = isAscending ? query.OrderBy(x => x.USERNAME) : query.OrderByDescending(x => x.USERNAME);
                break;
            case "USERID":
                query = isAscending ? query.OrderBy(x => x.USERID) : query.OrderByDescending(x => x.USERID);
                break;
            default:
                query = query.OrderBy(x => x.USERID);
                break;
        }
        return query;
    }

    protected override async Task<IdentityUser> Insert(IdentityUser item)
    {
        item.Id = GenerateId();
        USERS data = MapToEntity(item);
        _ctx.Entry(data).State = EntityState.Added;
        if (await _ctx.SaveChangesAsync() <= 0)
            item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Users");
        return item;
    }

    protected override async Task<IdentityUser> Update(IdentityUser item)
    {
        USERS data = MapToEntity(item);
        _ctx.Entry(data).State = EntityState.Modified;
        if (await _ctx.SaveChangesAsync() <= 0)
            item.Validation = CommonFn.CreateValidationError(ValidationErrorMessage.GenericDBSaveError, "Users");
        return item;
    }

    public async Task<IdentityUser> GetByName(string username)
    {
        return MapToDTO(await _ctx.USERS.AsNoTracking()
                .FirstOrDefaultAsync(x => x.USERNAME.ToLower() == username.ToLower()));
    }

    public async Task<IdentityUser> GetById(Guid id)
    {
        return MapToDTO(await _ctx.USERS.AsNoTracking()
            .FirstOrDefaultAsync(x => x.USERID == id));
    }

    public async Task<Guid> GetByUserIdByLogin(string name, string key)
    {
        IdentityUser identity = await _ctx.USERS.AsNoTracking()
                .Join(_ctx.USERLOGIN.AsNoTracking()
                    .Where(x => x.PROVIDERNAME.ToLower() == name.ToLower() && x.PROVIDERKEY.ToLower() == key.ToLower()),
                    user => user.USERID,
                    userlogin => userlogin.USERACCOUNTID,
                    (item, userlogin) => new IdentityUser
                    {
                        Id = item.USERID,
                        UserName = item.USERNAME,
                        Userfullname = item.USERFULLNAME,
                        Password = item.PASSWORD,
                        Passwordsalt = item.PASSWORDSALT,
                        Passwordformat = item.PASSWORDFORMAT,
                        Passwordfailcount = item.PASSWORDFAILCOUNT,
                        Islockedout = item.ISLOCKEDOUT,
                        Datelastlockout = item.DATELASTLOCKOUT,
                        Dateaccessexpiry = item.DATEACCESSEXPIRY,
                        Datechangepass = item.DATECHANGEPASS,
                        Datelastactivity = item.DATELASTACTIVITY,
                        Useremail = item.USEREMAIL,
                        Department = item.DEPARTMENT,
                        Datecreated = item.DATECREATED,
                        Datelastlogin = item.DATELASTLOGIN,
                        Validation = new ValidationResult()
                    })
                .FirstOrDefaultAsync();
        return identity == null ? Guid.Empty : identity.Id;
    }

    public async Task<IdentityUser> ChangePasswordAsync(string username, ChangePasswordModel item)
    {
        IdentityUser user = await AuthenticateAsync(username, item.CurrentPassword);
        if (user.Validation.IsValid)
        {
            user.TransContext = item.TransContext;
            return await ResetPasswordAsync(username, item.NewPassword, user);
        }
        return user;
    }
    public async Task<List<IdentityUser>> ListUserRole(Guid id)
    {
        List<IdentityUser> item = new List<IdentityUser>();
        item = await _ctx.ROLES.AsNoTracking()
           .Where(x => x.ROLEID == id)
            .Join(_ctx.USERSINROLE.AsNoTracking(),
                role => role.ROLEID,
                userrole => userrole.ROLEID,
                (role, userrole) => new { ROLES = role, USERSINROLE = userrole })
            .Join(_ctx.USERS.AsNoTracking(),
                userrole => userrole.USERSINROLE.USERID,
                user => user.USERID,
                (userrole, user) => new { ROLES = userrole.ROLES, USERSINROLE = userrole.USERSINROLE, USERS = user })
            .Select(x => new IdentityUser
            {
                Dateaccessexpiry = x.USERS.DATEACCESSEXPIRY,
                Userfullname = x.USERS.USERFULLNAME,
                Useremail = x.USERS.USEREMAIL,
                UserName = x.USERS.USERNAME,
                Id = x.USERS.USERID,
                Department = x.USERS.DEPARTMENT
            })
            .ToListAsync();
        return item;
    }

    public async Task<List<Claim>> ListUserClaim(Guid id)
    {
        List<Claim> result = new List<Claim>();
        List<USERCLAIM> dbClaims = await _ctx.USERCLAIM.AsNoTracking()
            .Where(x => x.USERACCOUNTID == id).ToListAsync();
        foreach (var dbClaim in dbClaims)
        {
            result.Add(new Claim(dbClaim.TYPE, dbClaim.VALUE));
        }
        return result;
    }

    public async Task<List<string>> ListUserPermissions(Guid id)
    {
        return await _ctx.USERS.AsNoTracking()
            .Where(x => x.USERID == id)
            .Join(_ctx.USERSINROLE.AsNoTracking(),
                user => user.USERID,
                userrole => userrole.USERID,
                (user, userrole) => new { userrole.ROLEID })
            .Join(_ctx.ROLES.AsNoTracking(),
                userrole => userrole.ROLEID,
                role => role.ROLEID,
                (userRole, role) => new { role.ROLEID, role.APPLICATIONID })
            .Join(_ctx.PERMISSIONSOFROLE.AsNoTracking(),
                role => role.ROLEID,
                rolepermit => rolepermit.ROLEID,
                (role, rolepermit) => new { rolepermit.PERMISSIONID })
            .Join(_ctx.PERMISSIONS.AsNoTracking(),
                rolepermit => rolepermit.PERMISSIONID,
                permit => permit.PERMISSIONID,
                (rolepermit, permit) => new { permit.PERMISSIONNAME })
            .Select(x => x.PERMISSIONNAME).ToListAsync();
    }

    public async Task<IdentityUser> ResetPasswordAsync(string username, string newpassword, IdentityUser _user)
    {
        USERS user = _ctx.USERS.FirstOrDefault(x => x.USERNAME.ToLower() == username.ToLower());
        user.PASSWORD = Crypto.HashPassword(newpassword);
        _user.Password = user.PASSWORD; //para sa log
        user.DATECHANGEPASS = DateTime.Now;
        _user.Datechangepass = DateTime.Now;
        _ctx.Entry(user).State = EntityState.Modified;
        await _ctx.SaveChangesAsync();
        //if (await _ctx.SaveChangesAsync() <= 0)
        //{
        //    var error = new ValidationFailure("User", string.Format(ValidationErrorMessage.GenericDBSaveError, "User"));
        //    item.Validation.Errors.Add(error);
        _logger.Log("FATAL", "Change Password User", _user, "FATAL.");
        //    return item;
        //}
        return MapToDTO(user);
    }

    public async Task<IdentityUser> AuthenticateAsync(string username, string password)
    {
        IdentityUserLoginValidator validator = new IdentityUserLoginValidator(_ctx, password);
        USERS dbUser = await _ctx.USERS.FirstOrDefaultAsync(x => x.USERNAME.ToLower() == username.ToLower());

        if (dbUser == null) return null;

        IdentityUser user = MapToDTO(dbUser);
        if (dbUser != null)
        {
            user.Validation = await validator.ValidateAsync(dbUser);
            return user;
        }
        var error = new ValidationFailure("User", SecurityErrorMessage.AuthenticationFailed);
        user.Validation.Errors.Add(error);
        return user;
    }

    public async Task<List<string>> ListAdministrativePermissionAsync(string username, string application)
    {
        return await _ctx.USERS.AsNoTracking().Where(x => x.USERNAME.ToLower() == username.ToLower())
                .Join(_ctx.USERSINROLE.AsNoTracking(),
                    user => user.USERID,
                    userrole => userrole.USERID,
                    (user, userrole) => new { userrole.ROLEID })
                .Join(_ctx.ROLES.AsNoTracking(),
                    userrole => userrole.ROLEID,
                    role => role.ROLEID,
                    (userRole, role) => new { role.ROLEID, role.APPLICATIONID })
                .Join(_ctx.APPLICATIONS.AsNoTracking().Where(x => x.APPLICATIONNAME.ToLower() == application.ToLower()),
                    role => role.APPLICATIONID,
                    app => app.APPLICATIONID,
                    (role, app) => new { role.ROLEID })
                .Join(_ctx.PERMISSIONSOFROLE.AsNoTracking(),
                    role => role.ROLEID,
                    rolepermit => rolepermit.ROLEID,
                    (role, rolepermit) => new { rolepermit.PERMISSIONID })
                .Join(_ctx.PERMISSIONS.AsNoTracking(),
                    rolepermit => rolepermit.PERMISSIONID,
                    permit => permit.PERMISSIONID,
                    (rolepermit, permit) => new { permit.PERMISSIONNAME })
                .Select(x => x.PERMISSIONNAME).ToListAsync();
    }

   
    public async Task<IdentityUser> ChangePasswordAsync(string username, string currentpassword, string newpassword)
    {
        IdentityUser user = await AuthenticateAsync(username, currentpassword);
        if (user.Validation.IsValid)
        {
            return await ResetPasswordAsync(username, newpassword);
        }
        return user;
    }

    public async Task<DataList<IdentityUser>> GetList(string query, Guid department, int pageIndex, int pageSize)
    {
        IQueryable<USERS> q = ListQuery(pageIndex, pageSize);
        if (!string.IsNullOrEmpty(query))
            q = q.Where(x => x.USERNAME.ToUpper().Trim().Contains(query.ToUpper().Trim()) ||
                             x.USERFULLNAME.ToUpper().Trim().Contains(query.ToUpper().Trim()));
        if (department != null)
            q = q.Where(x => x.DEPARTMENT == department);

        DataList<IdentityUser> item = new DataList<IdentityUser>();
        item.Count = await ListQuery().CountAsync();
        item.Items = await MapToUsers(q);
        return item;
    }
    public async Task<IdentityUser> GetByEmail(string email)
    {
        return MapToDTO(await _ctx.USERS.AsNoTracking()
            .FirstOrDefaultAsync(x => x.USEREMAIL.ToLower() == email.ToLower()));
    }
    private async Task<List<IdentityUser>> MapToUsers(IQueryable<USERS> query)
    {
        return await query.Select(x => new IdentityUser
        {
            Id = x.USERID,
            UserName = x.USERNAME,
            Userfullname = x.USERFULLNAME,
            Useremail = x.USEREMAIL,
            Password = "**********",
            Passwordfailcount = x.PASSWORDFAILCOUNT,
            Passwordformat = x.PASSWORDFORMAT,
            Passwordsalt = "**********",
            Isapproved = x.ISAPPROVED,
            Islockedout = x.ISLOCKEDOUT,
            Isdeleted = x.ISDELETED,
            Datecreated = x.DATECREATED,
            Datechangepass = x.DATECHANGEPASS,
            Datelastactivity = x.DATELASTACTIVITY,
            Datelastlogin = x.DATELASTLOGIN,
            Datelastlockout = x.DATELASTLOCKOUT,
            Department = x.DEPARTMENT,
            Dateaccessexpiry = x.DATEACCESSEXPIRY
        }).ToListAsync();
    }
    private IQueryable<USERS> ListQuery(int pageIndex = 0, int pageSize = 0, IQueryable<USERS> custom_query = null)
    {
        IQueryable<USERS> query = custom_query.AsNoTracking() ?? _ctx.USERS.AsNoTracking();
        query = query.OrderBy(x => x.USERNAME)
                    .ThenBy(x => x.USERFULLNAME);

        if (pageIndex > 0 && pageSize > 0) query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        return query;
    }

    public async Task<DataList<IdentityUser>> GetListByApplication(string query, string application, int pageIndex, int pageSize)
    {
        IQueryable<USERS> q = _ctx.USERS
                          .Join(_ctx.USERSINROLE,
                                  user => user.USERID,
                                  urole => urole.USERID,
                                  (user, urole) => new { USERS = user, USERSINROLE = urole })
                          .Join(_ctx.ROLES,
                                usersroles => usersroles.USERSINROLE.ROLEID,
                                roles => roles.ROLEID,
                                (usersroles, roles) => new { USERS = usersroles.USERS, USERSINROLE = usersroles.USERSINROLE, ROLES = roles })
                          .Join(_ctx.APPLICATIONS,
                                 usersroles => usersroles.ROLES.APPLICATIONID,
                                 app => app.APPLICATIONID,
                                 (usersroles, app) => new { USERS = usersroles.USERS, USERSINROLE = usersroles.USERSINROLE, ROLES = usersroles.ROLES, APPLICATIONS = app })
                          .Where(x => x.APPLICATIONS.APPLICATIONNAME.ToLower().Trim() == application.ToLower().Trim())
                          .Select(x => x.USERS);
        q = ListQuery(pageIndex, pageSize, q);
        if (!string.IsNullOrEmpty(query))
            q = q.Where(x => x.USERNAME.ToUpper().Trim().Contains(query.ToUpper().Trim()) ||
                             x.USERFULLNAME.ToUpper().Trim().Contains(query.ToUpper().Trim()));

        DataList<IdentityUser> item = new DataList<IdentityUser>();
        item.Count = await ListQuery(0, 0, q).CountAsync();
        item.Items = await MapToUsers(q);
        return item;
    }

    public async Task<DataList<IdentityUser>> GetListByRole(string query, string role, int pageIndex, int pageSize)
    {
        IQueryable<USERS> q = _ctx.USERS
                            .Join(_ctx.USERSINROLE,
                                    user => user.USERID,
                                    urole => urole.USERID,
                                    (user, urole) => new { USERS = user, USERSINROLE = urole })
                            .Join(_ctx.ROLES,
                                  usersroles => usersroles.USERSINROLE.ROLEID,
                                  roles => roles.ROLEID,
                                  (usersroles, roles) => new { USERS = usersroles.USERS, USERSINROLE = usersroles.USERSINROLE, ROLES = roles })
                            .Where(x => x.ROLES.ROLENAME.ToLower().Trim() == role.ToLower().Trim())
                            .Select(x => x.USERS);
        q = ListQuery(pageIndex, pageSize, q);
        if (!string.IsNullOrEmpty(query))
            q = q.Where(x => x.USERNAME.ToUpper().Trim().Contains(query.ToUpper().Trim()) ||
                             x.USERFULLNAME.ToUpper().Trim().Contains(query.ToUpper().Trim()));

        DataList<IdentityUser> item = new DataList<IdentityUser>();
        item.Count = await ListQuery(0, 0, q).CountAsync();
        item.Items = await MapToUsers(q);
        return item;
    }

    public async Task<IdentityUser> ResetPasswordAsync(string username, string newpassword)
    {
        USERS user = _ctx.USERS.FirstOrDefault(x => x.USERNAME.ToLower() == username.ToLower());
        user.PASSWORD = Crypto.HashPassword(newpassword + user.PASSWORDSALT);
        user.DATECHANGEPASS = DateTime.Now;
        _ctx.Entry(user).State = EntityState.Modified;
        await _ctx.SaveChangesAsync();
        return MapToDTO(user);
    }

    #endregion

}