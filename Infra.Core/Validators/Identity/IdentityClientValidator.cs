using FluentValidation;
using Infra.Common;
using Infra.Common.DTO.Identity;
using Infra.Core.Domain;

namespace Infra.Core.Validator.Identity
{
    public class IdentityClientValidator : AbstractValidator<IdentityClient>
    {
        private readonly InfraEntities _ctx;
        public IdentityClientValidator(InfraEntities ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Description)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Description", "256"));

            RuleFor(x => x.Allowedorigin)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Allowedorigin", "256"));

            RuleFor(x => x.Applicationname)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Applicationname", "256"));

            RuleFor(x => x.Applicationname)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Applicationname"));

            RuleFor(x => x.Redirecturl)
                .Length(0, 256)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Redirecturl", "256"));
        }
    }
}
