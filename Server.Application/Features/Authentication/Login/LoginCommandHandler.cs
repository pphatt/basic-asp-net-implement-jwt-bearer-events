using MediatR;
using Server.Application.Common.Interfaces.Authentication;
using Server.Application.Common.Interfaces.Persistence;
using Server.Contracts.Authentication.Login;

namespace Server.Application.Features.Authentication.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserRepository _userRepository;

    public LoginCommandHandler(IJwtTokenGenerator jwtTokenGenerator, IUserRepository userRepository)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _userRepository = userRepository;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = _userRepository.GetUserByEmail(request.Email);

        if (user is null)
        {
            throw new Exception("Invalid credentials.");
        }

        if (request.Password != user.Password)
        {
            throw new Exception("Invalid credential.");
        }

        var accessToken = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResult(accessToken, "refresh-token in this example is obsolete.");
    }
}
