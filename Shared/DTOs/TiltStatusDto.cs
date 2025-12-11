namespace LolStatsTracker.Shared.DTOs;

public class TiltStatusDto
{
    public bool IsTilted { get; set; }
    public int RecentLosses { get; set; }
    public int RecentGames { get; set; }
    public double RecentWinrate { get; set; }
    public string Message { get; set; } = "";
    public TiltLevel Level { get; set; } = TiltLevel.None;
}

public enum TiltLevel
{
    None,       // Playing well
    Warning,    // 2 losses in a row
    Danger,     // 3+ losses in a row
    Critical    // 4+ losses today or <30% WR in last 5+
}
