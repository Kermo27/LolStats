using LolStatsTracker.Shared.DTOs;

namespace LolStatsTracker.Helpers;

public class ActivityMatrixBuilder
{
    private readonly Dictionary<DateOnly, int> _activityData;
    
    public ActivityMatrixBuilder(IEnumerable<ActivityDayDto> activity)
    {
        _activityData = activity.ToDictionary(x => x.Date, x => x.GamesPlayed);
    }

    public List<List<(DateTime Date, int Count)>> Build()
    {
        var matrix = new List<List<(DateTime, int)>>();

        var end = DateTime.Today;
        var start = end.AddMonths(-6);
        
        while (start.DayOfWeek != DayOfWeek.Monday)
            start = start.AddDays(-1);

        var current = start;
        
        while (current <= end)
        {
            var week = Enumerable.Range(0, 7)
                .Select(offset =>
                {
                    var date = current.AddDays(offset);
                    var count = _activityData.GetValueOrDefault(DateOnly.FromDateTime(date), 0);
                    
                    return (date, count);
                }).ToList();

            matrix.Add(week);
            current = current.AddDays(7);
        }

        return matrix;
    }
}