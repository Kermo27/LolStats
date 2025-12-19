namespace LolStatsTracker.API.Models;

/// <summary>
/// JWT configuration settings
/// </summary>
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "LolStatsTracker";
    public string Audience { get; set; } = "LolStatsTracker";
    public int AccessTokenExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
