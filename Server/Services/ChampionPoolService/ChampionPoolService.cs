using LolStatsTracker.API.Data;
using LolStatsTracker.Shared.DTOs;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.ChampionPoolService;

public class ChampionPoolService : IChampionPoolService
{
    private readonly MatchDbContext _db;

    public ChampionPoolService(MatchDbContext db) => _db = db;

    public async Task<List<ChampionPoolDto>> GetPoolAsync(Guid profileId)
    {
        var pool = await _db.ChampionPools
            .Where(c => c.ProfileId == profileId)
            .OrderBy(c => c.Tier)
            .ThenBy(c => c.Priority)
            .ToListAsync();

        var champions = pool.Select(c => c.Champion).Distinct().ToList();
        
        // Get stats for each champion
        var matches = await _db.Matches
            .Where(m => m.ProfileId == profileId && champions.Contains(m.Champion))
            .ToListAsync();

        return pool.Select(c =>
        {
            var champMatches = matches.Where(m => m.Champion == c.Champion).ToList();
            var games = champMatches.Count;
            var wins = champMatches.Count(m => m.Win);
            var winrate = games > 0 ? (double)wins / games : 0;
            var avgKda = games > 0
                ? champMatches.Average(m => m.Deaths == 0 ? m.Kills + m.Assists : (double)(m.Kills + m.Assists) / m.Deaths)
                : 0;

            return new ChampionPoolDto(
                c.Id,
                c.Champion,
                c.Tier,
                c.Priority,
                games,
                winrate,
                Math.Round(avgKda, 2)
            );
        }).ToList();
    }

    public async Task<ChampionPoolDto?> GetByIdAsync(Guid id)
    {
        var item = await _db.ChampionPools.FindAsync(id);
        if (item == null) return null;

        var matches = await _db.Matches
            .Where(m => m.ProfileId == item.ProfileId && m.Champion == item.Champion)
            .ToListAsync();

        var games = matches.Count;
        var wins = matches.Count(m => m.Win);
        var winrate = games > 0 ? (double)wins / games : 0;
        var avgKda = games > 0
            ? matches.Average(m => m.Deaths == 0 ? m.Kills + m.Assists : (double)(m.Kills + m.Assists) / m.Deaths)
            : 0;

        return new ChampionPoolDto(item.Id, item.Champion, item.Tier, item.Priority, games, winrate, Math.Round(avgKda, 2));
    }

    public async Task<ChampionPool> CreateAsync(Guid profileId, ChampionPoolCreateDto dto)
    {
        var entry = new ChampionPool
        {
            ProfileId = profileId,
            Champion = dto.Champion,
            Tier = dto.Tier,
            Priority = dto.Priority
        };

        _db.ChampionPools.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task<ChampionPool?> UpdateAsync(Guid id, ChampionPoolUpdateDto dto)
    {
        var entry = await _db.ChampionPools.FindAsync(id);
        if (entry == null) return null;

        if (dto.Tier != null) entry.Tier = dto.Tier;
        if (dto.Priority.HasValue) entry.Priority = dto.Priority.Value;

        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entry = await _db.ChampionPools.FindAsync(id);
        if (entry == null) return false;

        _db.ChampionPools.Remove(entry);
        await _db.SaveChangesAsync();
        return true;
    }
}
