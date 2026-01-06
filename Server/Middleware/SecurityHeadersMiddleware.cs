namespace LolStatsTracker.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        
        // Prevent clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        
        // Enable XSS protection (legacy, but still useful)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        
        // Control referrer information
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // Permissions Policy (formerly Feature-Policy)
        context.Response.Headers.Append("Permissions-Policy", 
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
        
        // Content Security Policy for APIs
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; frame-ancestors 'none'; form-action 'self'");
        
        // Remove server header (hide server identity)
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
