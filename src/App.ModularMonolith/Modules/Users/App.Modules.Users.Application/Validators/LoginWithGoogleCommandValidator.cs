using FluentValidation;
using App.Modules.Users.Application.Commands;

namespace App.Modules.Users.Application.Validators;

public class LoginWithGoogleCommandValidator : AbstractValidator<LoginWithGoogleCommand>
{
    public LoginWithGoogleCommandValidator()
    {
        RuleFor(x => x.GoogleToken)
            .NotEmpty()
            .WithMessage("Google token ID is required");
    }
}
