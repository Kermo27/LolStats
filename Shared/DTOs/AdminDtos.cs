namespace LolStatsTracker.Shared.DTOs;

// ===== ADMIN DTOs =====

/// <summary>
/// User data for admin list
/// </summary>
public record UserListDto(
    Guid Id,
    string Username,
    string? Email,
    string Role,
    DateTime CreatedAt,
    int ProfileCount,
    int MatchCount
);

/// <summary>
/// Request to update user role
/// </summary>
public record UpdateUserRoleDto(string Role);

/// <summary>
/// System statistics for admin dashboard
/// </summary>
public record SystemStatsDto(
    int TotalUsers,
    int TotalProfiles,
    int TotalMatches,
    int TotalSeasons,
    int MatchesToday,
    int MatchesThisWeek,
    int NewUsersThisWeek
);

/// <summary>
/// Match data for admin list with profile info
/// </summary>
public record AdminMatchDto(
    Guid Id,
    long? GameId,
    string Champion,
    string Role,
    bool Win,
    DateTime Date,
    string GameMode,
    Guid? ProfileId,
    string? ProfileName,
    string? ProfileTag,
    string? Username
);

/// <summary>
/// Profile data for admin list
/// </summary>
public record ProfileListDto(
    Guid Id,
    string Name,
    string Tag,
    bool IsDefault,
    string? RiotPuuid,
    Guid? UserId,
    string? Username,
    int MatchCount,
    int? ProfileIconId,
    string? SoloTier,
    string? SoloRank,
    int? SoloLP
);

/// <summary>
/// Paginated response wrapper
/// </summary>
public record PaginatedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
