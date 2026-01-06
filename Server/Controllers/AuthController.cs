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
    private readonly ILoginAttemptService _loginAttemptService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService, 
        ILoginAttemptService loginAttemptService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _loginAttemptService = loginAttemptService;
        _logger = logger;
    }
    
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TokenResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if (result.IsFailure)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.Conflict => Conflict(CreateApiError(result.ErrorCode, result.Error!)),
                ErrorCodes.ValidationError => BadRequest(CreateApiError(result.ErrorCode, result.Error!)),
                _ => BadRequest(CreateApiError(result.ErrorCode ?? ErrorCodes.BadRequest, result.Error ?? "Registration failed"))
            };
        }

        // Auto-login after registration
        var loginDto = new LoginDto { Username = dto.Username, Password = dto.Password };
        var loginResult = await _authService.LoginAsync(loginDto);

        if (loginResult.IsFailure)
        {
            return Ok(new { message = "Registration successful, please login" });
        }

        _logger.LogInformation("User registered and logged in: {Username}", dto.Username);
        return Ok(loginResult.Value);
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status423Locked)]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
    {
        // Check if account is locked out
        if (await _loginAttemptService.IsLockedOutAsync(dto.Username))
        {
            _logger.LogWarning("Login attempt for locked account: {Username}", dto.Username);
            return StatusCode(StatusCodes.Status423Locked, new ApiError
            {
                Code = "ACCOUNT_LOCKED",
                Message = "Account is temporarily locked due to too many failed login attempts. Please try again later.",
                RequestId = HttpContext.TraceIdentifier
            });
        }

        var result = await _authService.LoginAsync(dto);

        if (result.IsFailure)
        {
            await _loginAttemptService.RecordFailedAttemptAsync(dto.Username);
            var remaining = await _loginAttemptService.GetRemainingAttemptsAsync(dto.Username);
            
            _logger.LogWarning("Failed login attempt for {Username}. {Remaining} attempts remaining", 
                dto.Username, remaining);

            return Unauthorized(new ApiError
            {
                Code = ErrorCodes.Unauthorized,
                Message = result.Error ?? "Invalid credentials",
                RequestId = HttpContext.TraceIdentifier,
                Details = remaining > 0 ? $"{remaining} attempts remaining" : null
            });
        }

        // Clear failed attempts on successful login
        await _loginAttemptService.ClearAttemptsAsync(dto.Username);
        _logger.LogInformation("User logged in: {Username}", dto.Username);
        
        return Ok(result.Value);
    }
    
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);

        if (result.IsFailure)
        {
            return Unauthorized(CreateApiError(ErrorCodes.Unauthorized, result.Error ?? "Token refresh failed"));
        }

        return Ok(result.Value);
    }
    
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Logout()
    {
        var userId = _authService.GetUserIdFromClaims(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _authService.RevokeTokenAsync(userId.Value);
        if (result.IsFailure)
        {
            _logger.LogWarning("Logout failed for user {UserId}: {Error}", userId.Value, result.Error);
        }
        
        _logger.LogInformation("User logged out: {UserId}", userId.Value);
        return Ok(new { message = "Logged out successfully" });
    }
    
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
    {
        var userId = _authService.GetUserIdFromClaims(User);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _authService.GetUserByIdAsync(userId.Value);
        if (result.IsFailure)
        {
            return NotFound(CreateApiError(ErrorCodes.NotFound, result.Error ?? "User not found"));
        }

        var user = result.Value!;
        return Ok(new UserInfoDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        });
    }

    private ApiError CreateApiError(string code, string message)
    {
        return new ApiError
        {
            Code = code,
            Message = message,
            RequestId = HttpContext.TraceIdentifier
        };
    }
}
