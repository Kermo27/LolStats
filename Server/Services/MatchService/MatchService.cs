using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.CacheService;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.MatchService;

public class MatchService : IMatchService
{
    private readonly MatchDbContext _db;
    private readonly ICacheService _cache;
    private readonly ILogger<MatchService> _logger;

    public MatchService(MatchDbContext db, ICacheService cache, ILogger<MatchService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<MatchEntry>> GetAllAsync(Guid profileId)
    {
        return await _db.Matches
            .Where(m => m.ProfileId == profileId)
            .OrderByDescending(m => m.Date)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<MatchEntry>> GetRecentAsync(Guid profileId, int count, DateTime? startDate, DateTime? endDate, string? gameMode)
    {
        var query = _db.Matches
            .Where(m => m.ProfileId == profileId)
            .AsNoTracking();
        
        if (startDate.HasValue)
            query = query.Where(m => m.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(m => m.Date <= endDate.Value);
        
        if (!string.IsNullOrEmpty(gameMode))
            query = query.Where(m => m.GameMode == gameMode);
        
        return await query
            .OrderByDescending(m => m.Date)
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<PaginatedResponse<MatchEntry>> GetPaginatedAsync(Guid profileId, int page, int pageSize, DateTime? startDate, DateTime? endDate, string? gameMode)
    {
        var query = _db.Matches
            .Where(m => m.ProfileId == profileId)
            .AsNoTracking();
        
        if (startDate.HasValue)
            query = query.Where(m => m.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(m => m.Date <= endDate.Value);
        
        if (!string.IsNullOrEmpty(gameMode))
            query = query.Where(m => m.GameMode == gameMode);
        
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        var items = await query
            .OrderByDescending(m => m.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return new PaginatedResponse<MatchEntry>(items, totalItems, page, pageSize);
    }

    public async Task<MatchEntry?> GetAsync(Guid id, Guid profileId)
    {
        return await _db.Matches.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.ProfileId == profileId);
    }

    public async Task<MatchEntry> AddAsync(MatchEntry match)
    {
        _db.Matches.Add(match);
        await _db.SaveChangesAsync();
        
        // Invalidate stats cache for this profile
        if (match.ProfileId.HasValue)
        {
            await InvalidateStatsCacheAsync(match.ProfileId.Value);
        }
        
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
        existingMatch.GameMode = updatedMatch.GameMode;
        existingMatch.QueueId = updatedMatch.QueueId;
        
        existingMatch.ProfileId = profileId;

        await _db.SaveChangesAsync();
        
        // Invalidate stats cache
        await InvalidateStatsCacheAsync(profileId);
        
        return existingMatch;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var match = await _db.Matches.FindAsync(id);
        if (match == null)
            return false;
        
        var profileId = match.ProfileId;
        
        _db.Matches.Remove(match);
        await _db.SaveChangesAsync();
        
        // Invalidate stats cache
        if (profileId.HasValue)
        {
            await InvalidateStatsCacheAsync(profileId.Value);
        }
        
        return true;
    }
    
    public async Task ClearAsync(Guid profileId)
    {
        var matches = await _db.Matches.Where(m => m.ProfileId == profileId).ToListAsync();
        
        _db.Matches.RemoveRange(matches); 
        await _db.SaveChangesAsync();
        
        // Invalidate stats cache
        await InvalidateStatsCacheAsync(profileId);
    }

    private async Task InvalidateStatsCacheAsync(Guid profileId)
    {
        _logger.LogDebug("Invalidating stats cache for profile: {ProfileId}", profileId);
        
        await _cache.RemoveAsync(CacheKeys.StatsSummary(profileId, null, null));
        await _cache.RemoveAsync(CacheKeys.StatsSummary(profileId, null, "Ranked Solo"));
        await _cache.RemoveAsync(CacheKeys.StatsSummary(profileId, null, "Ranked Flex"));
        await _cache.RemoveAsync(CacheKeys.StatsSummary(profileId, null, "All"));
    }
}