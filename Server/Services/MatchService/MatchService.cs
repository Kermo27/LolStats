using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.MilestoneService;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using LolStatsTracker.Shared.Constants;

namespace LolStatsTracker.API.Services.MatchService;

public class MatchService : IMatchService
{
    private readonly MatchDbContext _db;
    private readonly IMilestoneService _milestoneService;

    public MatchService(MatchDbContext db, IMilestoneService milestoneService)
    {
        _db = db;
        _milestoneService = milestoneService;
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
    
    public async Task<MatchEntry?> UpdateAsync(Guid id, MatchEntry updatedMatch, Guid profileId)
    {
        var existingMatch = await _db.Matches
            .FirstOrDefaultAsync(m => m.Id == id && m.ProfileId == profileId);

        if (existingMatch == null)
        {
            return null; 
        }

        // Map properties
        existingMatch.Champion = updatedMatch.Champion;
        existingMatch.Role = updatedMatch.Role;
        existingMatch.LaneAlly = updatedMatch.LaneAlly;
        existingMatch.LaneEnemy = updatedMatch.LaneEnemy;
        existingMatch.LaneEnemyAlly = updatedMatch.LaneEnemyAlly;
        existingMatch.Kills = updatedMatch.Kills;
        existingMatch.Deaths = updatedMatch.Deaths;
        existingMatch.Assists = updatedMatch.Assists;
        existingMatch.Cs = updatedMatch.Cs;
        existingMatch.GameLengthMinutes = updatedMatch.GameLengthMinutes;
        existingMatch.Win = updatedMatch.Win;
        existingMatch.Date = updatedMatch.Date;
        existingMatch.CurrentTier = updatedMatch.CurrentTier;
        existingMatch.CurrentDivision = updatedMatch.CurrentDivision;
        existingMatch.CurrentLp = updatedMatch.CurrentLp;
        
        existingMatch.ProfileId = profileId;

        await _db.SaveChangesAsync();
        return existingMatch;
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
        
        _db.Matches.RemoveRange(matches); 
        await _db.SaveChangesAsync();
    }
}