using LolStatsTracker.API.Services.DDragonService;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly IDDragonService _dDragonService;

    public AssetsController(IDDragonService dDragonService)
    {
        _dDragonService = dDragonService;
    }

    [HttpGet("version")]
    public async Task<ActionResult<string>> GetVersion()
    {
        var version = await _dDragonService.GetLatestVersionAsync();
        return Ok(version);
    }

    [HttpGet("champions")]
    public async Task<IActionResult> GetChampions()
    {
        var champions = await _dDragonService.GetChampionsAsync();
        if (champions == null)
        {
            return NotFound("Failed to fetch champion data");
        }
        return Ok(champions);
    }
}
