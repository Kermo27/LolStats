using LolStatsTracker.API.Services.ProfileService;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfilesController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserProfile>>> GetAll()
    {
        var profiles = await _profileService.GetAllAsync();
        return Ok(profiles);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserProfile>> Get(Guid id)
    {
        var profile = await _profileService.GetByIdAsync(id);
        return profile != null ? Ok(profile) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<UserProfile>> Create([FromBody] UserProfile profile)
    {
        // Prosta walidacja po stronie serwera
        if (string.IsNullOrWhiteSpace(profile.Name))
            return BadRequest("Profile name is required.");

        var created = await _profileService.CreateAsync(profile);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserProfile>> Update(Guid id, [FromBody] UserProfile profile)
    {
        if (id != profile.Id)
            return BadRequest("Profile ID in URL does not match ID in body.");

        var existing = await _profileService.GetByIdAsync(id);
        if (existing == null)
            return NotFound();
        
        existing.Name = profile.Name;
        existing.Tag = profile.Tag;
        
        await _profileService.UpdateAsync(existing);
        
        return Ok(existing);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _profileService.DeleteAsync(id);
        if (!result) return NotFound();

        return NoContent();
    }
}