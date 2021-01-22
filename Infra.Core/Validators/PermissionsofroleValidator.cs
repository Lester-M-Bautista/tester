using FluentValidation;
using Infra.Common.DTO;
using Infra.Core.Domain;
using Macrin.Common;
using System.Linq;

namespace Infra.Core.Validator
{
    public class PermissionsofroleValidator : AbstractValidator<Permissionsofrole>
    {
        private readonly InfraEntities _ctx;
        public PermissionsofroleValidator(InfraEntities ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Permissionid)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Permissionid"));

            RuleFor(x => x.Roleid)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Roleid"));
        }
        public class RolePermissionDeleteValidator : AbstractValidator<Permissionsofrole>
        {
            private readonly InfraEntities _ctx;
            public RolePermissionDeleteValidator(InfraEntities ctx)
            {
                _ctx = ctx;
                // CHECK IF THE APPLICATION DOES EXIST
                RuleFor(x => x.Rolepermissionid).Must(x => _ctx.PERMISSIONSOFROLE.Any(o => o.ROLEPERMISSIONID == x))
                    .WithMessage(string.Format(ValidationErrorMessage.NotExistError, "PermissionOfRole"));

            }
        }
    }
}
