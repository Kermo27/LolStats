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
    
    public async Task<List<UserProfile>> GetAllAsync()
    {
        return await _context.UserProfiles
            .OrderByDescending(p => p.IsDefault) // Domyślny na górze
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id)
    {
        return await _context.UserProfiles.FindAsync(id);
    }

    public async Task<UserProfile> CreateAsync(UserProfile profile)
    {
        if (!await _context.UserProfiles.AnyAsync())
        {
            profile.IsDefault = true;
        }

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public Task<UserProfile> UpdateAsync(UserProfile profile)
    {
        _context.Entry(profile).CurrentValues.SetValues(profile);
        return _context.SaveChangesAsync().ContinueWith(_ => profile);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var profile = await _context.UserProfiles.FindAsync(id);
        if (profile == null) return false;

        // If we delete the default profile, it would be a good idea to assign the flag to another one,
        // but for now, let's just allow it to be deleted.
        // Remember: Cascade Delete in DbContext will also delete all matches for that profile!
        
        _context.UserProfiles.Remove(profile);
        await _context.SaveChangesAsync();
        return true;
    }
}