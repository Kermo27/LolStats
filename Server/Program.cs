using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.MatchService;
using LolStatsTracker.API.Services.ProfileService;
using LolStatsTracker.API.Services.StatsService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Register the real database only when not running integration tests.
// Integration tests will set the environment to "Testing" and register an InMemory provider instead.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<MatchDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=matches.db"));
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("http://localhost:5067");
    });
});

builder.Services.AddScoped<IMatchService ,MatchService>();
builder.Services.AddScoped<IStatsService ,StatsService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<MatchDbContext>();
// Only run migrations for relational providers. This avoids EF provider conflicts in tests
try
{
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
}
catch (Exception ex)
{
    // In test environments the DbContext provider may be swapped (InMemory), which can cause
    // provider registration conflicts when resolving migration services. Ignore migration errors
    // during startup to keep tests focused on API behavior.
    Console.WriteLine($"Skipping migrations during startup: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// Add a partial Program class to make the assembly hostable for WebApplicationFactory in integration tests
public partial class Program { }
