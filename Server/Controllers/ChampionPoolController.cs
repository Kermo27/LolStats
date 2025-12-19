using LolStatsTracker.API.Services.ChampionPoolService;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChampionPoolController : BaseApiController
{
    private readonly IChampionPoolService _service;

    public ChampionPoolController(IChampionPoolService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<ChampionPoolDto>>> GetPool()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID missing");

        var pool = await _service.GetPoolAsync(profileId);
        return Ok(pool);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChampionPoolDto>> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ChampionPool>> Create([FromBody] ChampionPoolCreateDto dto)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty) return BadRequest("Profile ID missing");

        var created = await _service.CreateAsync(profileId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ChampionPool>> Update(Guid id, [FromBody] ChampionPoolUpdateDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
