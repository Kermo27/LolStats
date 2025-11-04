namespace LolStatsTracker.Helpers;

public class UiHelpers
{
    public static string GetWrColor(double winRate) =>
        winRate >= 60 ? "text-green-400 font-bold"
        : winRate <= 40 ? "text-red-500 font-bold"
        : "text-yellow-400 font-bold";
}