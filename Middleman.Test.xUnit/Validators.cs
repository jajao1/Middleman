using FluentValidation;
using Middleman.Tests.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleman.Test.xUnit
{
    // Validador para request SEM retorno
    public class DeleteUserRequestValidator : AbstractValidator<DeleteUserRequest>
    {
        public DeleteUserRequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId cannot be empty.");
        }
    }
}
