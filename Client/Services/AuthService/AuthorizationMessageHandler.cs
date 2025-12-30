using System.Net.Http.Headers;

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
        var path = request.RequestUri?.PathAndQuery ?? "";
        if (!path.Contains("/api/auth/login") && 
            !path.Contains("/api/auth/register") &&
            !path.Contains("/api/auth/refresh"))
        {
            try
            {
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
