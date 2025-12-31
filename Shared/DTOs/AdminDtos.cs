namespace LolStatsTracker.Shared.DTOs;

public record UserListDto(
    Guid Id,
    string Username,
    string? Email,
    string Role,
    DateTime CreatedAt,
    int ProfileCount,
    int MatchCount
);

public record UpdateUserRoleDto(string Role);

public record SystemStatsDto(
    int TotalUsers,
    int TotalProfiles,
    int TotalMatches,
    int TotalSeasons,
    int MatchesToday,
    int MatchesThisWeek,
    int NewUsersThisWeek
);

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
    int? SoloLP,
    string? FlexTier,
    string? FlexRank,
    int? FlexLP
);

public record PaginatedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
);
