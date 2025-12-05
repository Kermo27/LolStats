using LolStatsTracker.API.Services.StatsService;
using LolStatsTracker.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : BaseApiController
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }

    [HttpGet("overview")]
    public async Task<ActionResult<OverviewDto>> GetOverview()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        return Ok(await _statsService.GetOverviewAsync(profileId));
    }

    [HttpGet("champions")]
    public async Task<ActionResult<List<ChampionStatsDto>>> GetChampions()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        return Ok(await _statsService.GetChampionStatsAsync(profileId));
    }

    [HttpGet("enemies")]
    public async Task<ActionResult<List<EnemyStatsDto>>> GetEnemies([FromQuery] string role)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        if (string.IsNullOrWhiteSpace(role)) return BadRequest("Role required");

        return Ok(await _statsService.GetEnemyStatsAsync(profileId, role));
    }

    [HttpGet("activity")]
    public async Task<ActionResult<List<ActivityDayDto>>> GetActivity([FromQuery] int months = 6)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        if (months <= 0)
            return BadRequest("Months parameter must be greater than 0.");

        var activity = await _statsService.GetActivityAsync(profileId, months);
        return Ok(activity);
    }

    [HttpGet("enchanter-usage")]
    public async Task<ActionResult<EnchanterUsageSummary>> GetEnchanterUsage()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        
        var usage = await _statsService.GetEnchanterUsageAsync(profileId);
        return Ok(usage);
    }

    [HttpGet("best-duos")]
    public async Task<ActionResult<DuoSummary>> GetBestDuos()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        
        var duos = await _statsService.GetBestDuosAsync(profileId);
        return Ok(duos);
    }

    [HttpGet("worst-enemy-duos")]
    public async Task<ActionResult<DuoSummary>> GetWorstDuos()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        
        var duos = await _statsService.GetWorstEnemyDuosAsync(profileId);
        return Ok(duos);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<StatsSummaryDto>> GetStatsSummary([FromQuery] int months = 6)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        
        var summary = await _statsService.GetStatsSummaryAsync(profileId, months);
        return Ok(summary);
    }
}