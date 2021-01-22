using FluentValidation;
using Infra.Common.DTO;
using Infra.Core.Domain;
using Macrin.Common;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Helpers;

namespace Infra.Core.Validator
{
    public class UsersValidator : AbstractValidator<Users>
    {
        private readonly InfraEntities _ctx;
        public UsersValidator(InfraEntities ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Password)
                .Length(0, 128)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Password", "128"));

            RuleFor(x => x.Password)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Password"));

            RuleFor(x => x.Useremail)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Useremail", "256"));

            RuleFor(x => x.Useremail)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Useremail"));

            RuleFor(x => x.Userfullname)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Userfullname", "256"));

            RuleFor(x => x.Userfullname)
               .NotNull()
               .NotEmpty()
               .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Userfullname"));

            RuleFor(x => x.Passwordsalt)
                .Length(0, 128)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Passwordsalt", "128"));

            RuleFor(x => x.Username)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Username", "256"));

            RuleFor(x => x.Username)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Username"));

            RuleFor(x => x.Department)
               .NotNull()
               .NotEmpty()
               .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Designation Name"));

            //RuleFor(x => x).Must((x) => InsertItemMustBeUnique(x))
            //    .WithMessage(string.Format(ValidationErrorMessage.DuplicateError, "Users"))
            //    .WithName("Users")
            //    .When(o => o.Userid == Guid.Empty);

            //RuleFor(x => x).Must((x) => UpdateItemMustBeUnique(x))
            //    .WithMessage(string.Format(ValidationErrorMessage.DuplicateError, "Users"))
            //    .WithName("Users")
            //    .When(o => o.Userid != Guid.Empty);

            RuleFor(x => x).Must((x) => InsertItemMustBeUnique(x))
              .WithMessage(string.Format(ValidationErrorMessage.DuplicateError, "User"))
              .WithName("Username").When(o => o.Userid == Guid.Empty);

            RuleFor(x => x).Must((x) => UpdateItemMustBeUnique(x))
                .WithMessage(string.Format(ValidationErrorMessage.DuplicateError, "User"))
                .WithName("Username").When(o => o.Userid != Guid.Empty);
        }
        private bool InsertItemMustBeUnique(Users item)
        {
            return !_ctx.USERS.AsNoTracking()
                 .Any(x => x.USERNAME.ToLower() == item.Username.ToLower() && x.ISDELETED == false);
        }

        private bool UpdateItemMustBeUnique(Users item)
        {
            bool ItemExist = _ctx.USERS.AsNoTracking().Any(x => x.USERID == item.Userid);
            bool ItemIsUnique = !_ctx.USERS.AsNoTracking()
                .Any(x => x.USERID != item.Userid && x.USERNAME.ToLower() == item.Username.ToLower() && x.ISDELETED == false);
            return ItemExist && ItemIsUnique;
        }

    }

    public class UserLoginValidatator : AbstractValidator<USERS>
        {
            private readonly InfraEntities _ctx;
            private readonly int MaxPasswordFail;
            public UserLoginValidatator(InfraEntities ctx, string password)
            {
                _ctx = ctx;
                bool IsValid = Int32.TryParse(ConfigurationManager.AppSettings["MaxPasswordFail"], out MaxPasswordFail);
                MaxPasswordFail = IsValid ? MaxPasswordFail : 5;
                RuleFor(x => x).Cascade(CascadeMode.StopOnFirstFailure)
                    .Must(x => x != null).WithMessage(SecurityErrorMessage.UserDoesNotExist)
                    .Must(x => x.ISAPPROVED).WithMessage(SecurityErrorMessage.UserIsNotActive)
                    .Must(x => !x.ISLOCKEDOUT).WithMessage(SecurityErrorMessage.UserIsLockedOut)
                    .Must(x => !x.ISDELETED).WithMessage(SecurityErrorMessage.UserDoesNotExist)
                    .Must(x => x.DATEACCESSEXPIRY == null ? true : x.DATEACCESSEXPIRY > DateTime.Now)
                        .WithMessage(SecurityErrorMessage.UserAccessIsExpired)
                    .Must(x => x.PASSWORDFORMAT == 2).WithMessage(SecurityErrorMessage.InvalidPasswordFormat)
                    .Must(x => IsAuthenticationSuccessful(x, password)).WithMessage(SecurityErrorMessage.AuthenticationFailed)
                    .WithName("Password");
            }

            #region Helper Functions


            private bool IsAuthenticationSuccessful(USERS dbUser, string password)
            {
                var IsSuccessful = Crypto.VerifyHashedPassword(dbUser.PASSWORD, password);

                // TODO: Replace DateTime.Now with Database Time
                DateTime now = DateTime.Now;
                if (IsSuccessful)
                {
                    dbUser.PASSWORDFAILCOUNT = 0;
                    dbUser.DATELASTLOGIN = now;
                }
                else
                {
                    dbUser.PASSWORDFAILCOUNT++;
                    if (dbUser.PASSWORDFAILCOUNT >= MaxPasswordFail)
                    {
                        dbUser.ISLOCKEDOUT = true;
                        dbUser.DATELASTLOCKOUT = now;
                    }
                }
                _ctx.USERS.Attach(dbUser);
                _ctx.Entry(dbUser).State = EntityState.Modified;
                IsSuccessful = _ctx.SaveChanges() > 0 && IsSuccessful;
                return IsSuccessful;
            }

    
        #endregion
    }
}
