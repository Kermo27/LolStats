using LolStatsTracker.API.Services.MilestoneService;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MilestonesController : BaseApiController
{
    private readonly IMilestoneService _service;

    public MilestonesController(IMilestoneService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<RankMilestoneDto>>> GetMilestones()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID missing");

        var milestones = await _service.GetMilestonesAsync(profileId);
        return Ok(milestones);
    }

    [HttpPost]
    public async Task<ActionResult<RankMilestone>> Create([FromBody] RankMilestoneCreateDto dto)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID missing");

        var created = await _service.CreateAsync(profileId, dto);
        return Ok(created);
    }
}
