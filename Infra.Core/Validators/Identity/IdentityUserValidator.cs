using FluentValidation;
using Infra.Common;
using Infra.Common.DTO.Identity;
using Infra.Core.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Helpers;

namespace Infra.Core.Validator.Identity
{
    public class IdentityUserValidator : AbstractValidator<IdentityUser>
    {
        private readonly InfraEntities _ctx;
        public IdentityUserValidator(InfraEntities ctx)
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

            RuleFor(x => x.Passwordsalt)
                .Length(0, 128)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Passwordsalt", "128"));

            RuleFor(x => x.UserName)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Username", "256"));

            RuleFor(x => x.UserName)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Username"));

            RuleFor(x => x).Must((x) => InsertItemMustBeUnique(x))
                .WithMessage(string.Format(ValidationErrorMessage.DuplicateError, "Users"))
                .WithName("Users")
                .When(o => o.Id == Guid.Empty);

            RuleFor(x => x).Must((x) => UpdateItemMustBeUnique(x))
                .WithMessage(string.Format(ValidationErrorMessage.DuplicateError, "Users"))
                .WithName("Users")
                .When(o => o.Id != Guid.Empty);

        }

        #region Helper Functions

        private bool InsertItemMustBeUnique(IdentityUser item)
        {
            return !_ctx.USERS.AsNoTracking()
                .Any(x => x.USERNAME.Trim().ToLower() == item.UserName.Trim().ToLower());
        }

        private bool UpdateItemMustBeUnique(IdentityUser item)
        {
            bool ItemExist = _ctx.USERS.AsNoTracking().Any(x => x.USERID == item.Id);
            bool ItemIsUnique = !_ctx.USERS.AsNoTracking().Any(x => x.USERID != item.Id && (x.USERNAME.Trim().ToLower() == item.UserName.Trim().ToLower()));
            return ItemExist && ItemIsUnique;
        }

        public class IdentityUserLoginValidator : AbstractValidator<USERS>
        {
            private readonly InfraEntities _ctx;
            private readonly int MaxPasswordFail;
            public IdentityUserLoginValidator(InfraEntities ctx, string password)
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

            private bool IsAuthenticationSuccessful(USERS dbUser, string password)
            {
                var IsSuccessful = Crypto.VerifyHashedPassword(dbUser.PASSWORD, password);

                if (!IsSuccessful)
                {
                    IsSuccessful = Crypto.VerifyHashedPassword(dbUser.PASSWORD, password + dbUser.PASSWORDSALT);
                    if (IsSuccessful)
                    {
                        dbUser.PASSWORD = new PasswordHasher().HashPassword(password);
                        dbUser.PASSWORDSALT = null;
                    }
                }

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
}

