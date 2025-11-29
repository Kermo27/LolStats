using LolStatsTracker.API.Models;
using LolStatsTracker.API.Services.MatchService;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matchService;

    public MatchesController(IMatchService matchService)
    {
        _matchService = matchService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MatchEntry>>> GetAll()
    {
        var matches = await _matchService.GetAllAsync();
        return Ok(matches);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MatchEntry>> Get(Guid id)
    {
        var match = await _matchService.GetAsync(id);
        return match != null ? Ok(match) : NotFound();
    }
    
    [HttpPost]
    public async Task<ActionResult<MatchEntry>> Add([FromBody] MatchEntry match)
    {
        var added = await _matchService.AddAsync(match);
        return CreatedAtAction(nameof(Get), new {id = added.Id}, added);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MatchEntry>> Update(Guid id, [FromBody] MatchEntry updated)
    {
        try
        {
            var match = await _matchService.UpdateAsync(id, updated);
            return Ok(match);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _matchService.DeleteAsync(id);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        await _matchService.ClearAsync();
        return NoContent();
    }
}