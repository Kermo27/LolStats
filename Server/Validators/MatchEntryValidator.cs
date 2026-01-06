using FluentValidation;
using LolStatsTracker.Shared.Models;

namespace LolStatsTracker.API.Validators;

public class MatchEntryValidator : AbstractValidator<MatchEntry>
{
    private static readonly HashSet<string> ValidRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "ADC", "Support", "Mid", "Jungle", "Top"
    };

    private static readonly HashSet<string> ValidTiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Iron", "Bronze", "Silver", "Gold", "Platinum", "Emerald", "Diamond", "Master", "Grandmaster", "Challenger", "Unranked"
    };

    private static readonly HashSet<string> ValidGameModes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Ranked Solo", "Ranked Flex", "Normal", "ARAM", "Arena", "URF", "Ultimate Spellbook"
    };

    public MatchEntryValidator()
    {
        RuleFor(x => x.Champion)
            .NotEmpty().WithMessage("Champion is required")
            .MaximumLength(50).WithMessage("Champion name is too long");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(r => ValidRoles.Contains(r)).WithMessage("Invalid role. Must be: ADC, Support, Mid, Jungle, or Top");

        RuleFor(x => x.Kills)
            .GreaterThanOrEqualTo(0).WithMessage("Kills cannot be negative")
            .LessThanOrEqualTo(100).WithMessage("Kills value seems unrealistic");

        RuleFor(x => x.Deaths)
            .GreaterThanOrEqualTo(0).WithMessage("Deaths cannot be negative")
            .LessThanOrEqualTo(100).WithMessage("Deaths value seems unrealistic");

        RuleFor(x => x.Assists)
            .GreaterThanOrEqualTo(0).WithMessage("Assists cannot be negative")
            .LessThanOrEqualTo(100).WithMessage("Assists value seems unrealistic");

        RuleFor(x => x.Cs)
            .GreaterThanOrEqualTo(0).WithMessage("CS cannot be negative")
            .LessThanOrEqualTo(1500).WithMessage("CS value seems unrealistic");

        RuleFor(x => x.GameLengthMinutes)
            .GreaterThan(0).WithMessage("Game length must be greater than 0")
            .LessThanOrEqualTo(120).WithMessage("Game length seems unrealistic");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Match date cannot be in the future")
            .GreaterThanOrEqualTo(new DateTime(2009, 1, 1)).WithMessage("Match date is too old");

        RuleFor(x => x.CurrentTier)
            .Must(t => ValidTiers.Contains(t)).WithMessage("Invalid tier")
            .When(x => !string.IsNullOrWhiteSpace(x.CurrentTier));

        RuleFor(x => x.CurrentDivision)
            .InclusiveBetween(1, 4).WithMessage("Division must be between 1 and 4");

        RuleFor(x => x.CurrentLp)
            .InclusiveBetween(0, 100).WithMessage("LP must be between 0 and 100");

        RuleFor(x => x.GameMode)
            .Must(g => ValidGameModes.Contains(g)).WithMessage("Invalid game mode")
            .When(x => !string.IsNullOrWhiteSpace(x.GameMode));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
