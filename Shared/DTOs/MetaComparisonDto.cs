namespace LolStatsTracker.Shared.DTOs;

public record MetaChampionDto(
    string Champion,
    string Tier,
    string Role,
    bool IsPlayed,
    int GamesPlayed,
    double Winrate
);

public record MetaComparisonSummaryDto(
    List<MetaChampionDto> MetaChampions,
    int TotalMetaPlayed,
    int TotalOffMeta,
    string Recommendation
);
