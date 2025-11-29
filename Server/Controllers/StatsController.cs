using LolStatsTracker.API.Services.StatsService;
using LolStatsTracker.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService)
    {
        _statsService = statsService;
    }
    
    [HttpGet("overview")]
    public async Task<ActionResult<OverviewDto>> GetOverview()
    {
        var overview = await _statsService.GetOverviewAsync();
        return Ok(overview);
    }

    [HttpGet("champions")]
    public async Task<ActionResult<List<ChampionStatsDto>>> GetChampions()
    {
        var champions = await _statsService.GetChampionStatsAsync();
        return Ok(champions);
    }

    [HttpGet("enemies")]
    public async Task<ActionResult<List<EnemyStatsDto>>> GetEnemies([FromQuery] string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return BadRequest("Role parameter is required ('bot' or 'support').");

        var enemies = await _statsService.GetEnemyStatsAsync(role);
        return Ok(enemies);
    }

    [HttpGet("activity")]
    public async Task<ActionResult<List<ActivityDayDto>>> GetActivity([FromQuery] int months = 6)
    {
        if (months <= 0)
            return BadRequest("Months parameter must be greater than 0.");

        var activity = await _statsService.GetActivityAsync(months);
        return Ok(activity);
    }

    [HttpGet("enchanter-usage")]
    public async Task<ActionResult<EnchanterUsageSummary>> GetEnchanterUsage()
    {
        var usage = await _statsService.GetEnchanterUsageAsync();
        return Ok(usage);
    }

    [HttpGet("best-duos")]
    public async Task<ActionResult<DuoSummary>> GetBestDuos()
    {
        var duos = await _statsService.GetBestDuosAsync();
        return Ok(duos);
    }

    [HttpGet("worst-enemy-duos")]
    public async Task<ActionResult<DuoSummary>> GetWorstDuos()
    {
        var duos = await _statsService.GetWorstEnemyDuosAsync();
        return Ok(duos);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<StatsSummaryDto>> GetStatsSummary([FromQuery] int months = 6)
    {
        var summary = await _statsService.GetStatsSummaryAsync(months);
        return Ok(summary);
    }
}