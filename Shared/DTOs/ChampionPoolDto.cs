namespace LolStatsTracker.Shared.DTOs;

public record ChampionPoolDto(
    Guid Id,
    string Champion,
    string Tier,
    int Priority,
    int GamesPlayed,
    double Winrate,
    double AvgKda
);

public record ChampionPoolCreateDto(
    string Champion,
    string Tier,
    int Priority
);

public record ChampionPoolUpdateDto(
    string? Tier,
    int? Priority
);
