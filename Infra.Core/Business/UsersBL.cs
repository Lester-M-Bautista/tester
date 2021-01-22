

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
using System.Web.Helpers;

namespace Infra.Core.Business
{
    public class UsersBL : BaseBL<Users, USERS, Guid>, IUsersBL
    {
        private readonly InfraEntities _ctx;
        private readonly int _maxPageSize;
        public readonly ILogsBL _logger;
        public UsersBL(InfraEntities ctx, ILogsBL logger)
        {
            _ctx = ctx;
            _logger = logger;
            _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
                : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        }

        #region IEntity
        public async Task<Users> GetByKeyAsync(Guid key)
        {
            USERS item = await _ctx.USERS.AsNoTracking().FirstOrDefaultAsync(x => x.USERID == key);
            return MapToDTO(item);
        }

        public async Task<DataList<Users>> List(Users filter, PageConfig config)
        {
            IQueryable<USERS> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Userid";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<Users>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };

        }

        public async Task<List<Users>> ListUserByRoleId(Guid id)
        {
            List<Users> item = new List<Users>();
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
                .Select(x => new Users
                {
                    Dateaccessexpiry = x.USERS.DATEACCESSEXPIRY,
                    Userfullname = x.USERS.USERFULLNAME,
                    Useremail = x.USERS.USEREMAIL,
                    Username = x.USERS.USERNAME,
                    Userid = x.USERS.USERID,
                    Department = x.USERS.DEPARTMENT
                })
                .ToListAsync();
            return item;
        }
        #endregion

        #region ISavable
        public async Task<Users> SaveAsync(Users item)
        {
            item.Validation = new UsersValidator(_ctx).Validate(item);
            if (!item.Validation.IsValid) return item;
            return (item.Userid != Guid.Empty) ? await Update(item) : await Insert(item);
        }
        #endregion

        #region IDeletable
        public async Task<Users> DeleteAsync(Users item)
        {
            var data = await _ctx.USERS.FirstOrDefaultAsync(x => x.USERID == item.Userid);
            data.ISDELETED = true;
            _ctx.Entry(data).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("User", string.Format(ValidationErrorMessage.GenericDBDeleteError, "User"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Delete User", item, error.ErrorMessage);
                return await Task.FromResult<Users>(item);

            }
            _logger.Log("INFO", "Delete User", item, "Successful.");
            return await Task.FromResult<Users>(MapToDTO(data));
        }
        #endregion

        #region HELPERS
        #endregion

        #region BaseBL
        protected override Guid GenerateId(string sequenceName = null)
        {
            return Guid.NewGuid();
        }

        protected override Users MapToDTO(USERS item)
        {
            if (item == null) return new Users();
            return new Users
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
                Username = item.USERNAME,
                Userid = item.USERID,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override USERS MapToEntity(Users item)
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
                USERNAME = item.Username,
                USERID = item.Userid
            };
        }

        protected override IQueryable<Users> QueryToDTO(IQueryable<USERS> query)
        {
            return query.Select(x => new Users
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
                Username = x.USERNAME,
                Userid = x.USERID,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected override IQueryable<USERS> FilteredEntities(Users filter, IQueryable<USERS> custom_query = null, bool strict = false)
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
            //if (filter.Isdeleted != null && filter.Isdeleted != false)
            //    predicate = predicate.And(x => x.ISDELETED == filter.Isdeleted);
            if (filter.Isdeleted != null)
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
            if (!string.IsNullOrEmpty(filter.Username)) predicate = (strict)
                            ? predicate.And(x => x.USERNAME.ToLower() == filter.Username.ToLower())
                            : predicate.And(x => x.USERNAME.ToLower().Contains(filter.Username.ToLower()));

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
        protected async override Task<Users> Insert(Users item)
        {
            USERS data = MapToEntity(item);
            data.USERID = GenerateId();
            DateTime now = DateTime.UtcNow;
            data.DATECREATED = now;
            data.DATELASTLOGIN = now;
            data.DATECHANGEPASS = now;
            data.PASSWORDFORMAT = 2;
            //data.PASSWORDSALT = Crypto.GenerateSalt(32);
            data.PASSWORD = Crypto.HashPassword(item.Password);
            _ctx.USERS.Add(data);
            _ctx.Entry(data).State = EntityState.Added;

            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("User", string.Format(ValidationErrorMessage.GenericDBSaveError, "User"));
                item.Validation.Errors.Add(error);
               _logger.Log("FATAL", "Save User", item, error.ErrorMessage);
                return item;
            }
            _logger.Log("INFO", "Add User", item, "Successful.");
            item.Userid = data.USERID;
            return item;
        }
        public async Task<Users> GetByIdAsync(Guid id)
        {
            return MapToDTO(await _ctx.USERS.AsNoTracking()
                .FirstOrDefaultAsync(x => x.USERID == id));
        }
        public async Task<Users> GetByNameAsync(string name)
        {
            return MapToDTO(await _ctx.USERS.AsNoTracking()
                .FirstOrDefaultAsync(x => x.USERNAME.ToLower() == name.ToLower()));
        }
        public async Task<List<Users>> ListAsync()
        {
            return await _ctx.USERS.AsNoTracking()
                .Where(x => x.ISDELETED == false)
                .Select(item => new Users
                {
                    Userid = item.USERID,
                    Username = item.USERNAME,
                    Userfullname = item.USERFULLNAME,
                    Useremail = item.USEREMAIL,
                    Password = item.PASSWORD,
                    Passwordfailcount = item.PASSWORDFAILCOUNT,
                    Passwordformat = item.PASSWORDFORMAT,
                    Passwordsalt = item.PASSWORDSALT,
                    Isapproved = item.ISAPPROVED,
                    Islockedout = item.ISLOCKEDOUT,
                    Isdeleted = item.ISDELETED,
                    Datecreated = item.DATECREATED,
                    Datechangepass = item.DATECHANGEPASS,
                    Datelastactivity = item.DATELASTACTIVITY,
                    Datelastlogin = item.DATELASTLOGIN,
                    Datelastlockout = item.DATELASTLOCKOUT,
                    Department = item.DEPARTMENT,
                    Dateaccessexpiry = item.DATEACCESSEXPIRY
                }).OrderBy(x => x.Username).ThenBy(x => x.Userfullname).ToListAsync();
        }

        public async Task<bool> AuthorizeAsync(string username, string application, string permission)
        {
            return string.IsNullOrEmpty(permission) ?
                (await ListAdministrativePermissionAsync(username, application)).Any() :
                (await ListAdministrativePermissionAsync(username, application)).Contains(permission);
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

        protected async override Task<Users> Update(Users item)
        {
           
            var dbData = MapToEntity(item);
            _ctx.USERS.Attach(dbData);
            _ctx.Entry(dbData).State = EntityState.Modified;
            if (await _ctx.SaveChangesAsync() <= 0)
            {
                var error = new ValidationFailure("User", string.Format(ValidationErrorMessage.GenericDBSaveError, "User"));
                item.Validation.Errors.Add(error);
                _logger.Log("FATAL", "Update User", item, error.ErrorMessage);
                return item;
            }
            _logger.Log("INFO", "Update User", item, "Successful.");
            return item;
        }

        public async Task<Users> ResetPasswordAsync(string username, string newpassword)
        {
            USERS user = _ctx.USERS.FirstOrDefault(x => x.USERNAME.ToLower() == username.ToLower());
            user.PASSWORD = Crypto.HashPassword(newpassword + user.PASSWORDSALT);
            user.DATECHANGEPASS = DateTime.Now;
            _ctx.Entry(user).State = EntityState.Modified;
            await _ctx.SaveChangesAsync();
            return MapToDTO(user);
        }
        public async Task<Users> ChangePasswordAsync(string username, string currentpassword, string newpassword)
        {
            Users user = await AuthenticateAsync(username, currentpassword);
            if (user.Validation.IsValid)
            {
                return await ResetPasswordAsync(username, newpassword);
            }
            return user;
        }

        public async Task<DataList<Users>> GetList(string query, Guid department, int pageIndex, int pageSize)
        {
            IQueryable<USERS> q = ListQuery(pageIndex, pageSize);
            if (!string.IsNullOrEmpty(query))
                q = q.Where(x => x.USERNAME.ToUpper().Trim().Contains(query.ToUpper().Trim()) ||
                                 x.USERFULLNAME.ToUpper().Trim().Contains(query.ToUpper().Trim()));
            if (department != null)
                q = q.Where(x => x.DEPARTMENT == department);

            DataList<Users> item = new DataList<Users>();
            item.Count = await ListQuery().CountAsync();
            item.Items = await MapToUsers(q);
            return item;
        }

        public async Task<DataList<Users>> GetListByApplication(string query, string application, int pageIndex, int pageSize)
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

            DataList<Users> item = new DataList<Users>();
            item.Count = await ListQuery(0, 0, q).CountAsync();
            item.Items = await MapToUsers(q);
            return item;
        }

        public async Task<DataList<Users>> GetListByRole(string query, string role, int pageIndex, int pageSize)
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

            DataList<Users> item = new DataList<Users>();
            item.Count = await ListQuery(0, 0, q).CountAsync();
            item.Items = await MapToUsers(q);
            return item;
        }
        private async Task<List<Users>> MapToUsers(IQueryable<USERS> query)
        {
            return await query.Select(x => new Users
            {
                Userid = x.USERID,
                Username = x.USERNAME,
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

        public async Task<Users> AuthenticateAsync(string username, string password)
        {
            UserLoginValidatator validator = new UserLoginValidatator(_ctx, password);
            USERS dbUser = _ctx.USERS.FirstOrDefault(x => x.USERNAME.ToLower() == username.ToLower());
            Users user = MapToDTO(dbUser);
            if (dbUser != null)
            {
                user.Validation = await validator.ValidateAsync(dbUser);
                return user;
            }
            var error = new ValidationFailure("User", SecurityErrorMessage.AuthenticationFailed);
            user.Validation.Errors.Add(error);
            return user;
        }

        public async Task<List<Users>> UsersContainingPermissions(List<string> Permissions)
        {
            var listpermissionid = _ctx.PERMISSIONS.Where(x => Permissions.Contains(x.PERMISSIONNAME)).Select(z => z.PERMISSIONID).ToList();
            var listrolesid = _ctx.PERMISSIONSOFROLE.Where(x => listpermissionid.Contains(x.PERMISSIONID)).Select(z => z.ROLEID).ToList();
            var listuserid = _ctx.USERSINROLE.Where(x => listrolesid.Contains(x.ROLEID)).Select(z => z.USERID).ToList();
            var listusers = await _ctx.USERS.Where(x => listuserid.Contains(x.USERID) && x.ISDELETED == false).Select
                (z => new Users {
                    Userid = z.USERID,
                    Username = z.USERNAME,
                    Userfullname = z.USERFULLNAME,
                    Useremail = z.USEREMAIL,
                    Password = "**********",
                    Passwordfailcount = z.PASSWORDFAILCOUNT,
                    Passwordformat = z.PASSWORDFORMAT,
                    Passwordsalt = "**********",
                    Isapproved = z.ISAPPROVED,
                    Islockedout = z.ISLOCKEDOUT,
                    Isdeleted = z.ISDELETED,
                    Datecreated = z.DATECREATED,
                    Datechangepass = z.DATECHANGEPASS,
                    Datelastactivity = z.DATELASTACTIVITY,
                    Datelastlogin = z.DATELASTLOGIN,
                    Datelastlockout = z.DATELASTLOCKOUT,
                    Department = z.DEPARTMENT,
                    Dateaccessexpiry = z.DATEACCESSEXPIRY
                }).ToListAsync();

            return listusers;


            throw new NotImplementedException();
        }

        public async Task<DataList<Users>> SelectbyRoleName(string keywords)
        {
            if (string.IsNullOrEmpty(keywords)) return new DataList<Users> { Count = 0, Items = new List<Users>() };
            var table1 = _ctx.ROLES.AsEnumerable();
            var table2 = _ctx.USERSINROLE.AsEnumerable();
            var table3 = _ctx.USERS.AsEnumerable();

            var td =
            from t1 in table1
            join t2 in table2 on t1.ROLEID equals t2.ROLEID
            join t3 in table3 on t2.USERID equals t3.USERID
            where t3.ISAPPROVED == true && t3.ISDELETED == false && t1.ROLENAME.ToLower() == keywords.ToLower()
            select new Users
            {

                Dateaccessexpiry = t3.DATEACCESSEXPIRY,
                Datechangepass = t3.DATECHANGEPASS,
                Datecreated = t3.DATECREATED,
                Datelastactivity = t3.DATELASTACTIVITY,
                Datelastlockout = t3.DATELASTLOCKOUT,
                Datelastlogin = t3.DATELASTLOGIN,
                Department = t3.DEPARTMENT,
                Isapproved = t3.ISAPPROVED,
                Isdeleted = t3.ISDELETED,
                Islockedout = t3.ISLOCKEDOUT,
                Useremail = t3.USEREMAIL,
                Userfullname = t3.USERFULLNAME,
                Userid = t3.USERID,
                Username = t3.USERNAME,


            };
            return new DataList<Users>
            {
                Count = td.Count(),
                Items = td.ToList()
            };
        }

        public async Task<DataList<Users>> SelectbyUserName(string keywords)
        {
            if (string.IsNullOrEmpty(keywords)) return new DataList<Users> { Count = 0, Items = new List<Users>() };
            var table1 = _ctx.USERS.AsEnumerable();

            var td =
            from t1 in table1
            where t1.ISAPPROVED == true && t1.ISDELETED == false && t1.USERNAME.ToLower() == keywords.ToLower()
            select new Users
            {

                Dateaccessexpiry = t1.DATEACCESSEXPIRY,
                Datechangepass = t1.DATECHANGEPASS,
                Datecreated = t1.DATECREATED,
                Datelastactivity = t1.DATELASTACTIVITY,
                Datelastlockout = t1.DATELASTLOCKOUT,
                Datelastlogin = t1.DATELASTLOGIN,
                Department = t1.DEPARTMENT,
                Isapproved = t1.ISAPPROVED,
                Isdeleted = t1.ISDELETED,
                Islockedout = t1.ISLOCKEDOUT,
                Useremail = t1.USEREMAIL,
                Userfullname = t1.USERFULLNAME,
                Userid = t1.USERID,
                Username = t1.USERNAME,


            };
            return new DataList<Users>
            {
                Count = td.Count(),
                Items = td.ToList()
            };
        }
    
        #endregion

    }
}
