using LolStatsTracker.API.Services.AuthService;
using LolStatsTracker.API.Services.ProfileService;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IAuthService _authService;

    public ProfilesController(IProfileService profileService, IAuthService authService)
    {
        _profileService = profileService;
        _authService = authService;
    }

    private Guid GetCurrentUserId()
    {
        var userId = _authService.GetUserIdFromClaims(User);
        return userId ?? Guid.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserProfile>>> GetAll()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        
        var profiles = await _profileService.GetAllAsync(userId);
        return Ok(profiles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserProfile>> Get(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        
        var profile = await _profileService.GetByIdAsync(id, userId);
        return profile != null ? Ok(profile) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<UserProfile>> Create([FromBody] UserProfile profile)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        if (string.IsNullOrWhiteSpace(profile.Name))
            return BadRequest("Profile name is required.");

        var created = await _profileService.CreateAsync(profile, userId);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserProfile>> Update(Guid id, [FromBody] UserProfile profile)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        if (id != profile.Id)
            return BadRequest("Profile ID in URL does not match ID in body.");

        var existing = await _profileService.GetByIdAsync(id, userId);
        if (existing == null)
            return NotFound();
        
        existing.Name = profile.Name;
        existing.Tag = profile.Tag;
        
        var updated = await _profileService.UpdateAsync(existing, userId);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _profileService.DeleteAsync(id, userId);
        if (!result) return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Update profile rank data (icon, tier, rank, LP) - called by TrayApp
    /// </summary>
    [HttpPatch("{id:guid}/rankdata")]
    public async Task<ActionResult> UpdateRankData(Guid id, [FromBody] UpdateRankDataDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var profile = await _profileService.GetByIdAsync(id, userId);
        if (profile == null) return NotFound();

        profile.ProfileIconId = dto.ProfileIconId;
        profile.SoloTier = dto.SoloTier;
        profile.SoloRank = dto.SoloRank;
        profile.SoloLP = dto.SoloLP;

        await _profileService.UpdateAsync(profile, userId);
        return Ok(new { message = "Rank data updated" });
    }
}

public record UpdateRankDataDto(
    int? ProfileIconId,
    string? SoloTier,
    string? SoloRank,
    int? SoloLP
);