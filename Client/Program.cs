using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using LolStatsTracker;
using LolStatsTracker.Services.AdminService;
using LolStatsTracker.Services.AuthService;
using LolStatsTracker.Services.LeagueAssetsService;
using LolStatsTracker.Services.MatchService;
using LolStatsTracker.Services.MilestoneService;
using LolStatsTracker.Services.SeasonState;
using LolStatsTracker.Services.StatsService;
using LolStatsTracker.Services.UserState;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrl = builder.Configuration["ApiUrl"] ?? "http://localhost:5109";

// Add LocalStorage first (required by auth services)
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri(apiUrl))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

builder.Services.AddHttpClient("Auth", client => client.BaseAddress = new Uri(apiUrl));

builder.Services.AddScoped<IAuthService, AuthenticationService>(sp => 
{
    var localStorage = sp.GetRequiredService<ILocalStorageService>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var authClient = httpClientFactory.CreateClient("Auth");
    return new AuthenticationService(authClient, localStorage);
});
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddMudServices();

builder.Services.AddScoped<IUserProfileState, UserProfileState>();
builder.Services.AddScoped<ISeasonState, SeasonState>();
builder.Services.AddScoped<IMatchService, MatchApiService>();
builder.Services.AddScoped<ILeagueAssetsService, LeagueAssetsService>();
builder.Services.AddScoped<IStatsService, StatsApiService>();
builder.Services.AddScoped<IMilestoneService, MilestoneApiService>();
builder.Services.AddScoped<IAdminService, AdminApiService>();

await builder.Build().RunAsync();
