using LolStatsTracker.Models;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.Helpers;

public class ActivityMatrixBuilder
{
    private readonly IEnumerable<MatchEntry> _matches;

    public ActivityMatrixBuilder(IEnumerable<MatchEntry> matches) => _matches = matches;

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
                    var count = date <= end
                        ? _matches.Count(m => m.Date.Date == date.Date)
                        : 0;
                    return (date, count);
                }).ToList();

            matrix.Add(week);
            current = current.AddDays(7);
        }

        return matrix;
    }
}