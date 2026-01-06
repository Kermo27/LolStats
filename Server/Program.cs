using System.Net;
using System.Text;
using System.Text.Json;
using AspNetCoreRateLimit;
using LolStatsTracker.API.Data;
using LolStatsTracker.API.Models;
using LolStatsTracker.API.Services.AdminService;
using LolStatsTracker.API.Services.AuthService;
using LolStatsTracker.API.Services.CacheService;
using LolStatsTracker.API.Services.MatchService;
using LolStatsTracker.API.Services.ProfileService;
using LolStatsTracker.API.Services.StatsService;
using LolStatsTracker.API.Services.DDragonService;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting LolStatsTracker API");

    var builder = WebApplication.CreateBuilder(args);
    
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddOpenApi();
    
    // Railway uses DATABASE_URL, standard config uses ConnectionStrings:PostgreSQL
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
        ?? builder.Configuration.GetConnectionString("PostgreSQL");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        Log.Fatal("PostgreSQL connection string is required. Set DATABASE_URL or ConnectionStrings:PostgreSQL");
        throw new InvalidOperationException("PostgreSQL connection string is required");
    }
    
    // Convert Railway's postgres:// URL format to Npgsql format if needed
    if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
    {
        connectionString = ConvertPostgresUrlToConnectionString(connectionString);
    }
    
    builder.Services.AddDbContext<MatchDbContext>(options =>
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            npgsqlOptions.CommandTimeout(60);
        });
    });
    Log.Information("PostgreSQL configured");

    // Configure Caching - Redis or Memory
    var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL") 
        ?? builder.Configuration.GetConnectionString("Redis");
    var useRedis = !string.IsNullOrEmpty(redisUrl) || builder.Configuration.GetValue<bool>("Caching:UseRedis");
    
    if (useRedis && !string.IsNullOrEmpty(redisUrl))
    {
        // Convert redis:// URL format if needed
        var redisConnection = redisUrl;
        if (redisConnection.StartsWith("redis://"))
        {
            redisConnection = ConvertRedisUrlToConnectionString(redisConnection);
        }
        
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "LolStats:";
        });
        builder.Services.AddScoped<ICacheService, RedisCacheService>();
        Log.Information("Redis caching configured");
    }
    else
    {
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddScoped<ICacheService, MemoryCacheService>();
        Log.Information("Using in-memory caching");
    }

    // Configure JWT Settings
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

    // JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret.PadRight(32)))
        };
    });

    builder.Services.AddAuthorization();

    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:5067"];
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithOrigins(corsOrigins);
        });
    });

    // Health checks - separate liveness and readiness
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
        .AddDbContextCheck<MatchDbContext>("database", tags: ["ready"]);

    // Application Services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IMatchService, MatchService>();
    builder.Services.AddScoped<IStatsService, StatsService>();
    builder.Services.AddScoped<IProfileService, ProfileService>();
    builder.Services.AddHttpClient<IDDragonService, DDragonService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddSingleton<ILoginAttemptService, LoginAttemptService>();

    // FluentValidation
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<LolStatsTracker.API.Filters.ValidationFilter>();
    });
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "LolStatsTracker API",
            Version = "v1",
            Description = "League of Legends statistics tracking API"
        });
        
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        });
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Database migration
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<MatchDbContext>();
        db.Database.Migrate();
        Log.Information("Database migration completed");
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });
    
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandler != null)
            {
                Log.Error(exceptionHandler.Error, "Unhandled exception occurred: {Message}", exceptionHandler.Error.Message);

                var response = new
                {
                    error = "An unexpected error occurred",
                    requestId = context.TraceIdentifier
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        });
    });

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHsts();
    }
    
    app.UseMiddleware<LolStatsTracker.API.Middleware.SecurityHeadersMiddleware>();
    
    app.UseIpRateLimiting();
    app.UseCors();
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // Liveness check - simple, for Railway/Docker healthcheck
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => !check.Tags.Contains("ready")
    });
    
    // Readiness check - includes database
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true
    });

    app.MapControllers();
    
    Log.Information("LolStatsTracker API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Helper function to convert postgres:// URL to Npgsql connection string
static string ConvertPostgresUrlToConnectionString(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':');
    var builder = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : "",
        SslMode = Npgsql.SslMode.Require,
        TrustServerCertificate = true
    };
    return builder.ConnectionString;
}

// Helper function to convert redis:// URL to StackExchange.Redis format
static string ConvertRedisUrlToConnectionString(string url)
{
    var uri = new Uri(url);
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 6379;
    var password = "";
    
    if (!string.IsNullOrEmpty(uri.UserInfo))
    {
        password = uri.UserInfo.Contains(':') 
            ? uri.UserInfo.Split(':')[1] 
            : uri.UserInfo;
    }
    
    return string.IsNullOrEmpty(password) 
        ? $"{host}:{port}" 
        : $"{host}:{port},password={password}";
}