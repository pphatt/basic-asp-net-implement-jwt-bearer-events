using MediatR;
using Server.Contracts.Authentication.Login;

namespace Server.Application.Features.Authentication.Login;

public class LoginCommand : IRequest<LoginResult>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
