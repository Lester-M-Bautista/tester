using FluentValidation;
using Infra.Common.DTO;
using Infra.Core.Domain;
using Macrin.Common;
using System.Linq;

namespace Infra.Core.Validator
{
    public class UsersinroleValidator : AbstractValidator<Usersinrole>
    {
        private readonly InfraEntities _ctx;
        public UsersinroleValidator(InfraEntities ctx)
        {
            _ctx = ctx;

            RuleFor(x => x.Roleid)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Roleid"));

            RuleFor(x => x.Userid)
                .NotNull()
                .NotEmpty()
                .WithMessage(string.Format(ValidationErrorMessage.RequiredField, "Userid"));
        }
    }
    public class UserRoleDeleteValidator : AbstractValidator<Usersinrole>
    {
        private readonly InfraEntities _ctx;
        public UserRoleDeleteValidator(InfraEntities ctx)
        {
            _ctx = ctx;
            // CHECK IF USERROLE DOES EXIST
            RuleFor(x => x.Userinroleid).Must(x => _ctx.USERSINROLE.Any(o => o.USERINROLEID == x))
                .WithMessage(string.Format(ValidationErrorMessage.NotExistError, "UsersinRole"));
        }
    }
}
