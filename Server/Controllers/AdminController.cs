using LolStatsTracker.API.Services.AdminService;
using LolStatsTracker.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<SystemStatsDto>> GetStats()
    {
        var stats = await _adminService.GetSystemStatsAsync();
        return Ok(stats);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserListDto>>> GetUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("users/{userId}/role")]
    public async Task<ActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleDto dto)
    {
        var result = await _adminService.UpdateUserRoleAsync(userId, dto.Role);
        if (!result)
        {
            return NotFound("User not found or invalid role");
        }
        return Ok(new { message = "Role updated successfully" });
    }

    [HttpDelete("users/{userId}")]
    public async Task<ActionResult> DeleteUser(Guid userId)
    {
        var result = await _adminService.DeleteUserAsync(userId);
        if (!result)
        {
            return NotFound("User not found");
        }
        return Ok(new { message = "User deleted successfully" });
    }

    [HttpGet("matches")]
    public async Task<ActionResult<PaginatedResponse<AdminMatchDto>>> GetMatches(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var matches = await _adminService.GetAllMatchesAsync(page, pageSize, search);
        return Ok(matches);
    }

    [HttpDelete("matches/{matchId}")]
    public async Task<ActionResult> DeleteMatch(Guid matchId)
    {
        var result = await _adminService.DeleteMatchAsync(matchId);
        if (!result)
        {
            return NotFound("Match not found");
        }
        return Ok(new { message = "Match deleted successfully" });
    }

    /// <summary>
    /// Get all profiles
    /// </summary>
    [HttpGet("profiles")]
    public async Task<ActionResult<List<ProfileListDto>>> GetProfiles()
    {
        var profiles = await _adminService.GetAllProfilesAsync();
        return Ok(profiles);
    }

    /// <summary>
    /// Get profiles by user ID
    /// </summary>
    [HttpGet("users/{userId}/profiles")]
    public async Task<ActionResult<List<ProfileListDto>>> GetProfilesByUser(Guid userId)
    {
        var profiles = await _adminService.GetProfilesByUserIdAsync(userId);
        return Ok(profiles);
    }
}
