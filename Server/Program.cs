using LolStatsTracker.API.Data;
using LolStatsTracker.API.Services.ChampionPoolService;
using LolStatsTracker.API.Services.MatchService;
using LolStatsTracker.API.Services.MilestoneService;
using LolStatsTracker.API.Services.ProfileService;
using LolStatsTracker.API.Services.StatsService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<MatchDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=matches.db"));

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
builder.Services.AddScoped<IChampionPoolService, ChampionPoolService>();
builder.Services.AddScoped<IMilestoneService, MilestoneService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<MatchDbContext>();
db.Database.Migrate();

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