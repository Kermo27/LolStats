using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LolStatsTracker;
using LolStatsTracker.Services.ChampionPoolService;
using LolStatsTracker.Services.LeagueAssetsService;
using LolStatsTracker.Services.MatchService;
using LolStatsTracker.Services.MetaService;
using LolStatsTracker.Services.MilestoneService;
using LolStatsTracker.Services.SeasonState;
using LolStatsTracker.Services.StatsService;
using LolStatsTracker.Services.UserState;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration["ApiUrl"] ?? "http://localhost:5109";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUrl) });

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<UserProfileState>();
builder.Services.AddScoped<SeasonState>();
builder.Services.AddScoped<IMatchService, MatchApiService>();
builder.Services.AddScoped<ILeagueAssetsService, LeagueAssetsService>();
builder.Services.AddScoped<IStatsService, StatsApiService>();
builder.Services.AddScoped<IChampionPoolService, ChampionPoolApiService>();
builder.Services.AddScoped<IMilestoneService, MilestoneApiService>();
builder.Services.AddScoped<IMetaService, MetaApiService>();

await builder.Build().RunAsync();