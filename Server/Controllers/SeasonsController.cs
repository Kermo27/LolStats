using LolStatsTracker.API.Data;
using LolStatsTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeasonsController : ControllerBase
{
    private readonly MatchDbContext _context;

    public SeasonsController(MatchDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Season>>> GetAll()
    {
        return await _context.Seasons
            .OrderByDescending(s => s.Number)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Season>> GetById(int id)
    {
        var season = await _context.Seasons.FindAsync(id);
        if (season == null) return NotFound();
        return season;
    }

    [HttpGet("current")]
    public async Task<ActionResult<Season?>> GetCurrent()
    {
        var today = DateTime.Today;
        var season = await _context.Seasons
            .FirstOrDefaultAsync(s => s.StartDate <= today && 
                                      (s.EndDate == null || s.EndDate >= today));
        return season;
    }

    [HttpPost]
    public async Task<ActionResult<Season>> Create(Season season)
    {
        _context.Seasons.Add(season);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = season.Id }, season);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Season season)
    {
        if (id != season.Id) return BadRequest();
        
        _context.Entry(season).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var season = await _context.Seasons.FindAsync(id);
        if (season == null) return NotFound();
        
        _context.Seasons.Remove(season);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
