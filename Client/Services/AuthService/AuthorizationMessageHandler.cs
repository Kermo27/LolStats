using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;

namespace LolStatsTracker.Services.AuthService;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;

    public AuthorizationMessageHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Skip auth header for auth endpoints to prevent infinite refresh loops
        var path = request.RequestUri?.PathAndQuery ?? "";
        if (!path.Contains("/api/auth/login") && 
            !path.Contains("/api/auth/register") &&
            !path.Contains("/api/auth/refresh"))
        {
            try
            {
                // Use IServiceProvider to avoid circular dependency during construction
                using var scope = _serviceProvider.CreateScope();
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                
                var token = await authService.GetAccessTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch
            {
                // LocalStorage not available during pre-rendering or other issues
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
