using LolStatsTracker.TrayApp.Helpers;
using LolStatsTracker.TrayApp.Models;
using LolStatsTracker.TrayApp.Models.Lcu;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LolStatsTracker.TrayApp.Services;

public class TrayBackgroundService : BackgroundService
{
    private readonly ILogger<TrayBackgroundService> _logger;
    private readonly LcuService _lcuService;
    private readonly ApiSyncService _apiSyncService;
    private readonly IUserSettingsService _settingsService;
    private readonly ChampionDataService _championDataService;
    
    private LcuQueueStats? _cachedRankedStats;
    private readonly HashSet<long> _processedGameIds = new();
    private string? _currentSummonerName;
    private Guid _activeProfileId = Guid.Empty;
    private int _currentCheckIntervalSeconds;
    
    public event EventHandler<string>? StatusChanged;
    
    public TrayBackgroundService(
        ILogger<TrayBackgroundService> logger,
        LcuService lcuService,
        ApiSyncService apiSyncService,
        IUserSettingsService settingsService,
        ChampionDataService championDataService)
    {
        _logger = logger;
        _lcuService = lcuService;
        _apiSyncService = apiSyncService;
        _settingsService = settingsService;
        _championDataService = championDataService;
        _currentCheckIntervalSeconds = _settingsService.Settings.CheckIntervalSeconds;
        
        // Subscribe to events
        _lcuService.ConnectionChanged += OnLcuConnectionChanged;
        _lcuService.GameEnded += OnGameEnded;
        _settingsService.SettingsChanged += OnSettingsChanged;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tray Background Service started");
        StatusChanged?.Invoke(this, "Service started - loading champion data...");
        
        // Initialize champion data from DDragon
        await _championDataService.InitializeAsync();
        DataMapper.Initialize(_championDataService);
        
        StatusChanged?.Invoke(this, "Waiting for League Client");
        
        // Initialize LCU Service (starts monitoring loop)
        await _lcuService.InitializeAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Logic mostly handled by LcuService events now.
                // We keep this loop if we need periodic tasks like re-fetching stats or health checks.
                
                if (_lcuService.IsConnected)
                {
                    // Refresh stats periodically
                     _cachedRankedStats = await _lcuService.GetRankedStatsAsync();
                     
                     await CheckForAccountChangeAsync();
                }
                
                await Task.Delay(TimeSpan.FromSeconds(_currentCheckIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background service loop");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        
        _logger.LogInformation("Tray Background Service stopped");
    }
    
    private async Task CheckForAccountChangeAsync()
    {
        try
        {
            var summoner = await _lcuService.GetCurrentSummonerAsync();
            if (summoner != null)
            {
                var displayName = $"{summoner.GameName}#{summoner.TagLine}";
                
                if (_currentSummonerName != displayName)
                {
                    _currentSummonerName = displayName;
                    _logger.LogInformation("Account changed to: {DisplayName}", displayName);
                    
                    // Re-setup profile
                    await SetupProfileForSummonerAsync(summoner);
                    
                    StatusChanged?.Invoke(this, $"Connected - {displayName}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check for account change");
        }
    }
    
    private async void OnLcuConnectionChanged(object? sender, bool isConnected)
    {
        if (isConnected)
        {
            _logger.LogInformation("LCU Connected - initializing services");
            StatusChanged?.Invoke(this, "Connected to League Client");
            
            try
            {
                // Get initial summoner info
                await CheckForAccountChangeAsync();
                
                 // Cache ranked stats
                _cachedRankedStats = await _lcuService.GetRankedStatsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing LCU services after connection");
            }
        }
        else
        {
            _logger.LogInformation("LCU Disconnected");
            StatusChanged?.Invoke(this, "Disconnected - waiting for League Client");
            _currentSummonerName = null;
            _activeProfileId = Guid.Empty;
        }
    }
    
    private async Task SetupProfileForSummonerAsync(LcuSummoner summoner)
    {
        try
        {
            var profileId = await _apiSyncService.GetOrCreateProfileAsync(
                summoner.GameName, 
                summoner.TagLine, 
                summoner.Puuid);
            
            if (profileId.HasValue)
            {
                _activeProfileId = profileId.Value;
                _logger.LogInformation("Using profile {ProfileId} for {SummonerName}", 
                    _activeProfileId, _currentSummonerName);
                StatusChanged?.Invoke(this, $"Ready - {_currentSummonerName}");
            }
            else
            {
                _logger.LogWarning("Failed to get/create profile for summoner");
                StatusChanged?.Invoke(this, "Error: Could not setup profile");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up profile for summoner");
        }
    }
    
    private async void OnGameEnded(object? sender, LcuEndOfGameStats eogStats)
    {
        _logger.LogInformation("Game ended - processing match data for GameId: {GameId}", eogStats.GameId);
        
        if (!_processedGameIds.Add(eogStats.GameId))
        {
            _logger.LogInformation("Match {GameId} already processed - skipping", eogStats.GameId);
            return;
        }
        
        StatusChanged?.Invoke(this, "Processing match...");
        
        try
        {
            // Fetch game details for accurate queue info
            var gameDetails = await _lcuService.GetGameDetailsAsync(eogStats.GameId);
            if (gameDetails != null)
            {
                eogStats.QueueId = gameDetails.QueueId;
            }
            
            var gameMode = Helpers.DataMapper.MapQueueIdToGameMode(eogStats.QueueId);
            _logger.LogInformation("Processing {GameMode} match (queueId: {QueueId})", gameMode, eogStats.QueueId);
            
            if (_activeProfileId == Guid.Empty)
            {
                _logger.LogWarning("No active profile - skipping sync");
                StatusChanged?.Invoke(this, "Error: No profile available");
                return;
            }
            
            // Wait for LP to update on Riot's side (takes a few seconds after game ends)
            _logger.LogInformation("Waiting for LP update...");
            await Task.Delay(3000);
            
            // Fetch fresh ranked stats AFTER the delay to get updated LP
            var freshRankedStats = await _lcuService.GetRankedStatsAsync();
            _cachedRankedStats = freshRankedStats ?? _cachedRankedStats;
            
            var match = Helpers.DataMapper.MapToMatchEntry(eogStats, _cachedRankedStats, _activeProfileId, gameDetails);
            
            var success = await _apiSyncService.SyncMatchAsync(match);
            
            if (success)
            {
                StatusChanged?.Invoke(this, $"Synced: {match.Champion} ({gameMode} - {(match.Win ? "Win" : "Loss")})");
            }
            else
            {
                StatusChanged?.Invoke(this, "Failed to sync match");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing end of game stats");
            StatusChanged?.Invoke(this, "Error processing match");
        }
    }
    
    private void OnSettingsChanged(object? sender, UserSettings settings)
    {
        if (_currentCheckIntervalSeconds != settings.CheckIntervalSeconds)
        {
            _currentCheckIntervalSeconds = settings.CheckIntervalSeconds;
            _logger.LogInformation("Check interval updated to {Interval} seconds (hot-reload)", _currentCheckIntervalSeconds);
        }
    }
    
    public override void Dispose()
    {
        _lcuService.ConnectionChanged -= OnLcuConnectionChanged;
        _lcuService.GameEnded -= OnGameEnded;
        _settingsService.SettingsChanged -= OnSettingsChanged;
        base.Dispose();
    }
}

