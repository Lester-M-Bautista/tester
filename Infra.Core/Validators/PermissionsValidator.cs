using FluentValidation;
using Infra.Common.DTO;
using Infra.Core.Domain;
using Macrin.Common;
using System;
using System.Linq;

namespace Infra.Core.Validator
{
    public class PermissionsValidator : AbstractValidator<Permissions>
    {
        private readonly InfraEntities _ctx;
        public PermissionsValidator(InfraEntities ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Applicationid)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Applicationid"));

            RuleFor(x => x.Permissionname)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Permissionname", "256"));

            RuleFor(x => x.Permissionname)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Permissionname"));

            RuleFor(x => x.Description)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Description", "256"));

            RuleFor(x => x).Must((x) => InsertItemMustBeUnique(x))
                .WithMessage(string.Format(ValidationErrorMessage.DuplicateError, "Permissions"))
                .WithName("Permissions")
                .When(o => o.Permissionid == Guid.Empty);

            RuleFor(x => x).Must((x) => UpdateItemMustBeUnique(x))
                .WithMessage(string.Format(ValidationErrorMessage.DuplicateError, "Permissions"))
                .WithName("Permissions")
                .When(o => o.Permissionid != Guid.Empty);

        }


        #region Helper Functions

        private bool InsertItemMustBeUnique(Permissions item)
        {
            return !_ctx.PERMISSIONS.AsNoTracking()
                .Any(x => x.PERMISSIONNAME.Trim().ToLower() == item.Permissionname.Trim().ToLower());
        }

        private bool UpdateItemMustBeUnique(Permissions item)
        {
            bool ItemExist = _ctx.PERMISSIONS.AsNoTracking().Any(x => x.PERMISSIONID == item.Permissionid);
            bool ItemIsUnique = !_ctx.PERMISSIONS.AsNoTracking().Any(x => x.PERMISSIONID != item.Permissionid && (x.PERMISSIONNAME.Trim().ToLower() == item.Permissionname.Trim().ToLower()));
            return ItemExist && ItemIsUnique;
        }



        #endregion
    }

    public class PermissionDeleteValidator : AbstractValidator<Permissions>
    {
        private readonly InfraEntities _ctx;
        public PermissionDeleteValidator(InfraEntities ctx)
        {
            _ctx = ctx;
            // CHECK IF THE APPLICATION DOES EXIST
            RuleFor(x => x.Permissionid).Must(x => _ctx.PERMISSIONS.Any(o => o.PERMISSIONID == x))
                .WithMessage(string.Format(ValidationErrorMessage.NotExistError, "Permission"));
            // CAN ONLY BE DELETED IF NO CHILD DATA
            RuleFor(x => x.Permissionid).Must(x => !_ctx.PERMISSIONSOFROLE.Any(o => o.PERMISSIONID == x))
                .WithMessage(string.Format(ValidationErrorMessage.FKViolationError, "Permission", "PermissionOfRole"));

        }

    }
}
