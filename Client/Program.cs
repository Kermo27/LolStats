using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LolStatsTracker;
using LolStatsTracker.Services;
using LolStatsTracker.Services.ChampionService;
using LolStatsTracker.Services.MatchService;
using LolStatsTracker.Services.StatsService;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IMatchService, MatchApiService>();
builder.Services.AddScoped<IStorageProvider, LocalStorageService>();
builder.Services.AddScoped<IChampionService, ChampionService>();
builder.Services.AddScoped<IStatsService, StatsApiService>();

builder.Services.AddMudServices();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();