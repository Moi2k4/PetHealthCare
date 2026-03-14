using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare.API.Security;
using PetCare.Application.DTOs.Auth;
using PetCare.Application.Services.Interfaces;

namespace PetCare.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenBlacklistService _tokenBlacklistService;

    public AuthController(IAuthService authService, ITokenBlacklistService tokenBlacklistService)
    {
        _authService = authService;
        _tokenBlacklistService = tokenBlacklistService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(registerDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(loginDto);
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var rawToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader[7..].Trim()
            : string.Empty;

        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return BadRequest(new { success = false, message = "Missing bearer token" });
        }

        var expiresAtUtc = DateTime.UtcNow.AddHours(1);
        string? jti = null;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(rawToken);
            jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (jwt.ValidTo > DateTime.UtcNow)
            {
                expiresAtUtc = jwt.ValidTo.ToUniversalTime();
            }
        }
        catch
        {
            // Fallback expiration is used when token cannot be parsed.
        }

        _tokenBlacklistService.BlacklistToken(rawToken, jti, expiresAtUtc);

        return Ok(new
        {
            success = true,
            message = "Logout successful. Token has been revoked."
        });
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.IdToken))
            return BadRequest(new { message = "IdToken is required" });

        var result = await _authService.GoogleLoginAsync(dto.IdToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
