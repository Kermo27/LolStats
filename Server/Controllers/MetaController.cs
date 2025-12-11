using LolStatsTracker.API.Services.MetaService;
using LolStatsTracker.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetaController : BaseApiController
{
    private readonly IMetaService _metaService;

    public MetaController(IMetaService metaService) => _metaService = metaService;

    [HttpGet("tiers")]
    public ActionResult<IEnumerable<object>> GetTiers()
    {
        return Ok(_metaService.GetTiers());
    }

    [HttpGet("comparison")]
    public async Task<ActionResult<MetaComparisonSummaryDto>> GetComparison()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID missing");

        var result = await _metaService.GetComparisonAsync(profileId);
        return Ok(result);
    }
}
