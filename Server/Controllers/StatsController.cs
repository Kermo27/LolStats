using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.StatsService;
using LolStatsTracker.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : BaseApiController
{
    private readonly IStatsService _statsService;
    private readonly MatchDbContext _db;

    public StatsController(IStatsService statsService, MatchDbContext db)
    {
        _statsService = statsService;
        _db = db;
    }

    private async Task<(DateTime? startDate, DateTime? endDate)> GetSeasonDatesAsync(int? seasonId)
    {
        if (!seasonId.HasValue) return (null, null);
        
        var season = await _db.Seasons.FindAsync(seasonId.Value);
        if (season == null) return (null, null);
        
        return (season.StartDate, season.EndDate);
    }

    [HttpGet("overview")]
    public async Task<ActionResult<OverviewDto>> GetOverview([FromQuery] int? seasonId = null)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        
        var (startDate, endDate) = await GetSeasonDatesAsync(seasonId);
        return Ok(await _statsService.GetOverviewAsync(profileId, startDate, endDate));
    }

    [HttpGet("champions")]
    public async Task<ActionResult<List<ChampionStatsDto>>> GetChampions([FromQuery] int? seasonId = null)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        
        var (startDate, endDate) = await GetSeasonDatesAsync(seasonId);
        return Ok(await _statsService.GetChampionStatsAsync(profileId, startDate, endDate));
    }

    [HttpGet("enemies")]
    public async Task<ActionResult<List<EnemyStatsDto>>> GetEnemies([FromQuery] string role, [FromQuery] int? seasonId = null)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        if (string.IsNullOrWhiteSpace(role)) return BadRequest("Role required");

        var (startDate, endDate) = await GetSeasonDatesAsync(seasonId);
        return Ok(await _statsService.GetEnemyStatsAsync(profileId, role, startDate, endDate));
    }

    [HttpGet("activity")]
    public async Task<ActionResult<List<ActivityDayDto>>> GetActivity([FromQuery] int months = 6, [FromQuery] int? seasonId = null)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        if (months <= 0)
            return BadRequest("Months parameter must be greater than 0.");

        var (startDate, endDate) = await GetSeasonDatesAsync(seasonId);
        var activity = await _statsService.GetActivityAsync(profileId, months, startDate, endDate);
        return Ok(activity);
    }

    [HttpGet("enchanter-usage")]
    public async Task<ActionResult<EnchanterUsageSummary>> GetEnchanterUsage([FromQuery] int? seasonId = null)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        
        var (startDate, endDate) = await GetSeasonDatesAsync(seasonId);
        var usage = await _statsService.GetEnchanterUsageAsync(profileId, startDate, endDate);
        return Ok(usage);
    }

    [HttpGet("best-duos")]
    public async Task<ActionResult<DuoSummary>> GetBestDuos([FromQuery] int? seasonId = null)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        
        var (startDate, endDate) = await GetSeasonDatesAsync(seasonId);
        var duos = await _statsService.GetBestDuosAsync(profileId, startDate, endDate);
        return Ok(duos);
    }

    [HttpGet("worst-enemy-duos")]
    public async Task<ActionResult<DuoSummary>> GetWorstDuos([FromQuery] int? seasonId = null)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        
        var (startDate, endDate) = await GetSeasonDatesAsync(seasonId);
        var duos = await _statsService.GetWorstEnemyDuosAsync(profileId, startDate, endDate);
        return Ok(duos);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<StatsSummaryDto>> GetStatsSummary([FromQuery] int months = 6, [FromQuery] int? seasonId = null)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID is missing");
        
        var (startDate, endDate) = await GetSeasonDatesAsync(seasonId);
        var summary = await _statsService.GetStatsSummaryAsync(profileId, months, startDate, endDate);
        return Ok(summary);
    }
}