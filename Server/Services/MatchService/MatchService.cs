using LolStatsTracker.API.Data;
using LolStatsTracker.API.Models;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.MatchService;

public class MatchService : IMatchService
{
    private readonly MatchDbContext _db;

    public MatchService(MatchDbContext db)
    {
        _db = db;
    }

    public async Task<List<MatchEntry>> GetAllAsync()
    {
        return await _db.Matches
            .OrderByDescending(m => m.Date)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<MatchEntry?> GetAsync(Guid id)
    {
        return await _db.Matches
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<MatchEntry> AddAsync(MatchEntry match)
    {
        if (match.Id == Guid.Empty)
            match.Id = Guid.NewGuid();

        _db.Matches.Add(match);
        await _db.SaveChangesAsync();
        return match;
    }

    public async Task<MatchEntry> UpdateAsync(Guid id, MatchEntry match)
    {
        var existing = await _db.Matches.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"Nie znaleziono meczu o ID {id}");

        _db.Entry(existing).CurrentValues.SetValues(match);
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAsync(Guid id)
    {
        var match = await _db.Matches.FindAsync(id);
        if (match != null)
        {
            _db.Matches.Remove(match);
            await _db.SaveChangesAsync();
        }
    }
    
    public async Task ClearAsync()
    {
        _db.Matches.RemoveRange(_db.Matches);
        await _db.SaveChangesAsync();
    }
}