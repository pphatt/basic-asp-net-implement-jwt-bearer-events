using FluentValidation;

namespace Server.Application.Features.Authentication.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        // this example does not need validator.
    }
}
