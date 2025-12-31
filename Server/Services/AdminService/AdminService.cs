using LolStatsTracker.API.Data;
using LolStatsTracker.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LolStatsTracker.API.Services.AdminService;

public class AdminService : IAdminService
{
    private readonly MatchDbContext _context;
    private readonly ILogger<AdminService> _logger;

    public AdminService(MatchDbContext context, ILogger<AdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UserListDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Select(u => new
            {
                User = u,
                ProfileCount = _context.UserProfiles.Count(p => p.UserId == u.Id),
                MatchCount = _context.Matches.Count(m => 
                    _context.UserProfiles.Any(p => p.UserId == u.Id && p.Id == m.ProfileId))
            })
            .ToListAsync();

        return users.Select(u => new UserListDto(
            u.User.Id,
            u.User.Username,
            u.User.Email,
            u.User.Role,
            u.User.CreatedAt,
            u.ProfileCount,
            u.MatchCount
        )).ToList();
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, string role)
    {
        if (role != "User" && role != "Admin")
        {
            _logger.LogWarning("Invalid role: {Role}", role);
            return false;
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return false;
        }

        user.Role = role;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated user {Username} role to {Role}", user.Username, role);
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for deletion: {UserId}", userId);
            return false;
        }

        // Get all profiles for this user
        var profiles = await _context.UserProfiles
            .Where(p => p.UserId == userId)
            .ToListAsync();

        // Delete all matches for these profiles
        foreach (var profile in profiles)
        {
            var matches = await _context.Matches
                .Where(m => m.ProfileId == profile.Id)
                .ToListAsync();
            _context.Matches.RemoveRange(matches);
            
            var milestones = await _context.RankMilestones
                .Where(r => r.ProfileId == profile.Id)
                .ToListAsync();
            _context.RankMilestones.RemoveRange(milestones);
        }

        // Delete profiles
        _context.UserProfiles.RemoveRange(profiles);
        
        // Delete user
        _context.Users.Remove(user);
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted user {Username} with {ProfileCount} profiles", user.Username, profiles.Count);
        return true;
    }

    public async Task<SystemStatsDto> GetSystemStatsAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);

        var totalUsers = await _context.Users.CountAsync();
        var totalProfiles = await _context.UserProfiles.CountAsync();
        var totalMatches = await _context.Matches.CountAsync();
        var totalSeasons = await _context.Seasons.CountAsync();
        var matchesToday = await _context.Matches.CountAsync(m => m.Date >= today);
        var matchesThisWeek = await _context.Matches.CountAsync(m => m.Date >= weekAgo);
        var newUsersThisWeek = await _context.Users.CountAsync(u => u.CreatedAt >= weekAgo);

        return new SystemStatsDto(
            totalUsers,
            totalProfiles,
            totalMatches,
            totalSeasons,
            matchesToday,
            matchesThisWeek,
            newUsersThisWeek
        );
    }

    public async Task<PaginatedResponse<AdminMatchDto>> GetAllMatchesAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        var query = _context.Matches.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => 
                m.Champion.Contains(search) || 
                m.GameMode.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var matches = await query
            .OrderByDescending(m => m.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                Match = m,
                Profile = _context.UserProfiles.FirstOrDefault(p => p.Id == m.ProfileId),
                Username = _context.UserProfiles
                    .Where(p => p.Id == m.ProfileId && p.UserId != null)
                    .Join(_context.Users, p => p.UserId, u => u.Id, (p, u) => u.Username)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var items = matches.Select(m => new AdminMatchDto(
            m.Match.Id,
            m.Match.GameId,
            m.Match.Champion,
            m.Match.Role,
            m.Match.Win,
            m.Match.Date,
            m.Match.GameMode,
            m.Match.ProfileId,
            m.Profile?.Name,
            m.Profile?.Tag,
            m.Username
        )).ToList();

        return new PaginatedResponse<AdminMatchDto>(items, totalCount, page, pageSize);
    }

    public async Task<bool> DeleteMatchAsync(Guid matchId)
    {
        var match = await _context.Matches.FindAsync(matchId);
        if (match == null)
        {
            _logger.LogWarning("Match not found for deletion: {MatchId}", matchId);
            return false;
        }

        _context.Matches.Remove(match);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted match {MatchId}", matchId);
        return true;
    }

    public async Task<List<ProfileListDto>> GetAllProfilesAsync()
    {
        var profiles = await _context.UserProfiles
            .Select(p => new
            {
                Profile = p,
                Username = _context.Users
                    .Where(u => u.Id == p.UserId)
                    .Select(u => u.Username)
                    .FirstOrDefault(),
                MatchCount = _context.Matches.Count(m => m.ProfileId == p.Id)
            })
            .OrderBy(p => p.Profile.Name)
            .ToListAsync();

        return profiles.Select(p => new ProfileListDto(
            p.Profile.Id,
            p.Profile.Name,
            p.Profile.Tag,
            p.Profile.IsDefault,
            p.Profile.RiotPuuid,
            p.Profile.UserId,
            p.Username,
            p.MatchCount,
            p.Profile.ProfileIconId,
            p.Profile.SoloTier,
            p.Profile.SoloRank,
            p.Profile.SoloLP
        )).ToList();
    }

    public async Task<List<ProfileListDto>> GetProfilesByUserIdAsync(Guid userId)
    {
        var profiles = await _context.UserProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new
            {
                Profile = p,
                MatchCount = _context.Matches.Count(m => m.ProfileId == p.Id)
            })
            .OrderBy(p => p.Profile.Name)
            .ToListAsync();

        var username = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync();

        return profiles.Select(p => new ProfileListDto(
            p.Profile.Id,
            p.Profile.Name,
            p.Profile.Tag,
            p.Profile.IsDefault,
            p.Profile.RiotPuuid,
            userId,
            username,
            p.MatchCount,
            p.Profile.ProfileIconId,
            p.Profile.SoloTier,
            p.Profile.SoloRank,
            p.Profile.SoloLP
        )).ToList();
    }
}
