using LolStatsTracker.API.Services.AuthService;
using LolStatsTracker.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<TokenResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Username and password are required");
        }

        var (success, error, user) = await _authService.RegisterAsync(dto);

        if (!success)
        {
            return BadRequest(error);
        }

        // Auto-login after registration
        var loginDto = new LoginDto { Username = dto.Username, Password = dto.Password };
        var (loginSuccess, loginError, token) = await _authService.LoginAsync(loginDto);

        if (!loginSuccess)
        {
            return Ok(new { message = "Registration successful, please login" });
        }

        return Ok(token);
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Username and password are required");
        }

        var (success, error, token) = await _authService.LoginAsync(dto);

        if (!success)
        {
            return Unauthorized(error);
        }

        return Ok(token);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            return BadRequest("Refresh token is required");
        }

        var (success, error, token) = await _authService.RefreshTokenAsync(dto.RefreshToken);

        if (!success)
        {
            return Unauthorized(error);
        }

        return Ok(token);
    }

    /// <summary>
    /// Logout (revoke refresh token)
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var userId = _authService.GetUserIdFromClaims(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        await _authService.RevokeTokenAsync(userId.Value);
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
    {
        var userId = _authService.GetUserIdFromClaims(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userId.Value);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        });
    }
}
