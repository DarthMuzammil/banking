using Banking.Application.Auth.Commands;
using Banking.Application.Auth.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterCustomerCommandHandler _registerHandler;
    private readonly LoginQueryHandler _loginHandler;
    private readonly RefreshTokenCommandHandler _refreshHandler;

    public AuthController(
        RegisterCustomerCommandHandler registerHandler,
        LoginQueryHandler loginHandler,
        RefreshTokenCommandHandler refreshHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _refreshHandler = refreshHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await _registerHandler.HandleAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "EMAIL_ALREADY_EXISTS" => Conflict(new { error = result.Error, code = result.ErrorCode }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            };
        }

        return CreatedAtAction(nameof(Register), result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var query = new LoginQuery(request.Email, request.Password);
        var result = await _loginHandler.HandleAsync(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { error = result.Error, code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _refreshHandler.HandleAsync(
            new RefreshTokenCommand(request.RefreshToken),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { error = result.Error, code = result.ErrorCode });
        }

        return Ok(result.Value);
    }
}

public sealed record RegisterCustomerRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);
