namespace LolStatsTracker.Shared.DTOs;

public class TimeAnalysisDto
{
    public List<HourWinrateDto> ByHour { get; set; } = new();
    public List<DayWinrateDto> ByDayOfWeek { get; set; } = new();
    public string BestHourRange { get; set; } = "";
    public string BestDay { get; set; } = "";
    public double BestHourWinrate { get; set; }
    public double BestDayWinrate { get; set; }
}

public record HourWinrateDto(int Hour, int Games, int Wins, double Winrate);
public record DayWinrateDto(string Day, int DayIndex, int Games, int Wins, double Winrate);
