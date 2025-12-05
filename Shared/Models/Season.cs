using System.ComponentModel.DataAnnotations;

namespace LolStatsTracker.Shared.Models;

public class Season
{
    [Key]
    public int Id { get; set; }
    public int Number { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public string Name => $"Season {Number}";

    public bool ContainsDate(DateTime date)
    {
        if (date < StartDate) return false;
        if (EndDate.HasValue && date > EndDate.Value) return false;
        return true;
    }
}
