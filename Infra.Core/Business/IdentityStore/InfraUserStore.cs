using Infra.Common.DTO.Identity;
using Infra.Core.Business.Identity;
using Infra.Core.Contract.Identity;
using Infra.Core.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infra.Core
{
    public class InfraUserStore<TUser> :
        IQueryableUserStore<TUser, Guid>,
        IUserStore<TUser, Guid>,
        IUserPasswordStore<TUser, Guid>
        where TUser : IdentityUser
    {
        #region Constructor Injection Code
        private readonly IIdentityUserBL _userbl;
        private readonly IIdentityLoginBL _loginbl;

        public InfraUserStore(InfraEntities ctx)
        {
            _userbl = new IdentityUserBL(ctx);
            _loginbl = new IdentityLoginBL(ctx);
        }
        #endregion

        public void Dispose()
        {
            return;
        }

        #region IQueryableUserStore
        public IQueryable<TUser> Users
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region IUserStore
        public Task<TUser> FindByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
        public async Task<TUser> FindByNameAsync(string userName)
        {
            return MapToUser(await _userbl.GetByName(userName));
        }
        public Task CreateAsync(TUser user)
        {
            throw new NotImplementedException();
        }
        public Task DeleteAsync(TUser user)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(TUser user)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IUserPasswordStore
        public Task<string> GetPasswordHashAsync(TUser user)
        {
            return Task.FromResult<string>(user.Password);
        }
        public Task SetPasswordHashAsync(TUser user, string passwordHash)
        {
            return Task.FromResult<string>(user.Password);
        }
        public Task<bool> HasPasswordAsync(TUser user)
        {
            return Task.FromResult<bool>(!string.IsNullOrEmpty(user.Password));
        }
        #endregion

        #region Helper Fucntion
        private TUser MapToUser(IdentityUser item)
        {
            TUser user = null;
            user = Activator.CreateInstance(typeof(TUser)) as TUser;
            if (item == null) return user;

            user.Id = item.Id;
            user.UserName = item.UserName;
            user.Userfullname = item.Userfullname;
            user.Useremail = item.Useremail;
            user.Password = item.Password;
            user.Passwordfailcount = item.Passwordfailcount;
            user.Passwordformat = item.Passwordformat;
            user.Passwordsalt = item.Passwordsalt;
            user.Isapproved = item.Isapproved;
            user.Islockedout = item.Islockedout;
            user.Isdeleted = item.Isdeleted;
            user.Datecreated = item.Datecreated;
            user.Datechangepass = item.Datechangepass;
            user.Datelastactivity = item.Datelastactivity;
            user.Datelastlogin = item.Datelastlogin;
            user.Datelastlockout = item.Datelastlockout;
            user.Department = item.Department;
            user.Dateaccessexpiry = item.Dateaccessexpiry;

            return user;
        }

        #endregion
    }
}
