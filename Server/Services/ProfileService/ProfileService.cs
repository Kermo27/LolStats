using LolStatsTracker.API.Data;
using LolStatsTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.ProfileService;

public class ProfileService : IProfileService
{
    private readonly MatchDbContext _context;

    public ProfileService(MatchDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<UserProfile>> GetAllAsync(Guid userId)
    {
        return await _context.UserProfiles
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
    }

    public async Task<UserProfile> CreateAsync(UserProfile profile, Guid userId)
    {
        profile.UserId = userId;
        
        // Check if this is the first profile for this user
        var hasProfiles = await _context.UserProfiles.AnyAsync(p => p.UserId == userId);
        if (!hasProfiles)
        {
            profile.IsDefault = true;
        }

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<UserProfile> UpdateAsync(UserProfile profile, Guid userId)
    {
        var existing = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.Id == profile.Id && p.UserId == userId);
            
        if (existing == null)
            throw new InvalidOperationException("Profile not found or access denied");

        existing.Name = profile.Name;
        existing.Tag = profile.Tag;
        existing.RiotPuuid = profile.RiotPuuid;
        
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            
        if (profile == null) return false;

        _context.UserProfiles.Remove(profile);
        await _context.SaveChangesAsync();
        return true;
    }
}