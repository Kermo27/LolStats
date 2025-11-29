namespace LolStatsTracker.Shared.DTOs;

public class ChampionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public class DataDragonResponse
{
    public Dictionary<string, ChampionDto> Data { get; set; } = new();
}