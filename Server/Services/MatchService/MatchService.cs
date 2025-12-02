using LolStatsTracker.API.Data;
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

    public async Task<List<MatchEntry>> GetAllAsync(Guid profileId)
    {
        return await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .OrderByDescending(m => m.Date)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<MatchEntry?> GetAsync(Guid id, Guid profileId)
    {
        return await _db.Matches
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.ProfileId == profileId);
    }

    public async Task<MatchEntry> AddAsync(MatchEntry match)
    {
        _db.Matches.Add(match);
        await _db.SaveChangesAsync();
        return match;
    }

    public async Task<MatchEntry> UpdateAsync(Guid id, MatchEntry match)
    {
        var existing = await _db.Matches.FindAsync(id);
        if (existing == null)
            return null;

        _db.Entry(existing).CurrentValues.SetValues(match);
        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var match = await _db.Matches.FindAsync(id);
        if (match == null)
            return false;
        
        _db.Matches.Remove(match);
        await _db.SaveChangesAsync();
        return true;

    }
    
    public async Task ClearAsync(Guid profileId)
    {
        var matches = await _db.Matches.Where(m => m.ProfileId == profileId).ToListAsync();
        _db.Matches.RemoveRange(_db.Matches);
        await _db.SaveChangesAsync();
    }
}