using LolStatsTracker.API.Models;
using LolStatsTracker.API.Services.MatchService;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchesController : BaseApiController
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MatchEntry>>> GetAll()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
            return BadRequest("Profile ID header is missing.");
        
        return Ok(await _matchService.GetAllAsync(profileId));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MatchEntry>> Get(Guid id)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
            return BadRequest("Profile ID header is missing.");
        
        var match = await _matchService.GetAsync(id, profileId);
        return match != null ? Ok(match) : NotFound();
    }
    
    [HttpPost]
    public async Task<ActionResult<MatchEntry>> Add([FromBody] MatchEntry match)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
            return BadRequest("Profile ID header is missing.");
        
        match.ProfileId = profileId;
        var added = await _matchService.AddAsync(match);
        return Ok(added);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MatchEntry>> Update(Guid id, [FromBody] MatchEntry match)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing.");
        
        var updated = await _matchService.UpdateAsync(id, match, profileId);
    
        return updated != null ? Ok(updated) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _matchService.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var profileId = GetProfileId();
        
        await _matchService.ClearAsync(profileId);
        return NoContent();
    }
}