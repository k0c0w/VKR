using FluentValidation;
using UseCases.UserManagement;
using WebApi.Common.Validation;

namespace WebApi.Controllers.Users.Validation;

public class DeleteUserArgsValidator : AbstractValidator<DeleteUserUseCase.DeleteUserArgs>
{
    public DeleteUserArgsValidator()
    {
        RuleFor(x => x.Email)
            .SetValidator(new EmailValidator());
    }
}