using FluentValidation;
using Infra.Common.DTO;
using Infra.Core.Domain;
using Macrin.Common;

namespace Infra.Core.Validator
{
    public class RolesValidator : AbstractValidator<Roles>
    {
        private readonly InfraEntities _ctx;
        public RolesValidator(InfraEntities ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Applicationid)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Applicationid"));

            RuleFor(x => x.Rolename)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Rolename", "256"));

            RuleFor(x => x.Rolename)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Rolename"));

            RuleFor(x => x.Description)
               .NotNull()
               .NotEmpty()
               .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Description"));

            RuleFor(x => x.Description)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Description", "256"));
        }
    }
}
