using FluentValidation;
using Infra.Common;
using Infra.Common.DTO.Identity;
using Infra.Core.Domain;

namespace Infra.Core.Validator.Identity
{

    public class IdentityLoginValidator : AbstractValidator<IdentityLogin>
    {
        private readonly InfraEntities _ctx;
        public IdentityLoginValidator(InfraEntities ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Useraccountid)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Useraccountid"));

            RuleFor(x => x.Providerkey)
                .Length(0, 100)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Providerkey", "100"));

            RuleFor(x => x.Providerkey)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Providerkey"));

            RuleFor(x => x.Providername)
                .Length(0, 20)
                .WithMessage(string.Format(ValidationErrorMessage.StringLengthExceeded, "Providername", "20"));

            RuleFor(x => x.Providername)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Providername"));
        }
    }
}
