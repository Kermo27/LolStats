using System.Text.Json.Serialization;

namespace LolStatsTracker.Shared.DTOs;

public record ApiError
{
    public string Code { get; init; } = "ERROR";
    public string Message { get; init; } = "An error occurred";
    public string? RequestId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? Errors { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Details { get; init; }
}

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    public const string InternalError = "INTERNAL_ERROR";
    public const string BadRequest = "BAD_REQUEST";
    public const string RateLimited = "RATE_LIMITED";
}
