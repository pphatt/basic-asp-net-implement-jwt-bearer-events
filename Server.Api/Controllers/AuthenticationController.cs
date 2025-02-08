using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Application.Features.Authentication.Login;
using Server.Contracts.Authentication.Login;

namespace Server.Api.Controllers;

[ApiController]
[Route("Authentication")]
public class AuthenticationController : ControllerBase
{
    ISender _mediatorSender;
    IMapper _mapper;

    public AuthenticationController(ISender mediatorSender, IMapper mapper)
    {
        _mediatorSender = mediatorSender;
        _mapper = mapper;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var mapper = _mapper.Map<LoginCommand>(request);

        var result = await _mediatorSender.Send(mapper);

        return Ok(result);
    }

    [HttpGet("test-auth")]
    [Authorize]
    public IActionResult TestAuth()
    {
        return Ok("Authentication access successfully.");
    }
}
