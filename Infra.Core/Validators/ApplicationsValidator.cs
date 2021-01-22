using FluentValidation;
using Infra.Common.DTO;
using Infra.Core.Domain;
using Macrin.Common;

namespace Infra.Core
{
    public class ApplicationsValidator : AbstractValidator<Applications>
    {
        private readonly InfraEntities _ctx;
        public ApplicationsValidator(InfraEntities ctx)
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
