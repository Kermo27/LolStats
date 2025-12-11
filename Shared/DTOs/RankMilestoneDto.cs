namespace LolStatsTracker.Shared.DTOs;

public record RankMilestoneDto(
    Guid Id,
    string Tier,
    int Division,
    DateTime AchievedAt,
    string Type,
    Guid? MatchId
);

public record RankMilestoneCreateDto(
    string Tier,
    int Division,
    DateTime? AchievedAt = null
);
