using LolStatsTracker.TrayApp.Models;
using LolStatsTracker.TrayApp.Models.Lcu;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LolStatsTracker.TrayApp.Services;

public class TrayBackgroundService : BackgroundService
{
    private readonly ILogger<TrayBackgroundService> _logger;
    private readonly LcuService _lcuService;
    private readonly ApiSyncService _apiSyncService;
    private readonly AppConfiguration _config;
    
    private LcuQueueStats? _cachedRankedStats;
    private readonly HashSet<long> _processedGameIds = new();
    private string? _currentSummonerName;
    private Guid _activeProfileId = Guid.Empty;
    
    public event EventHandler<string>? StatusChanged;
    
    public TrayBackgroundService(
        ILogger<TrayBackgroundService> logger,
        LcuService lcuService,
        ApiSyncService apiSyncService,
        IOptions<AppConfiguration> config)
    {
        _logger = logger;
        _lcuService = lcuService;
        _apiSyncService = apiSyncService;
        _config = config.Value;
        
        // Subscribe to events
        _lcuService.ConnectionChanged += OnLcuConnectionChanged;
        _lcuService.GameEnded += OnGameEnded;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tray Background Service started");
        StatusChanged?.Invoke(this, "Service started - waiting for League Client");
        
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
                
                await Task.Delay(TimeSpan.FromSeconds(_config.CheckIntervalSeconds), stoppingToken);
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
            const int RANKED_SOLO_DUO_QUEUE_ID = 420;
            
            // Fetch game details
            var gameDetails = await _lcuService.GetGameDetailsAsync(eogStats.GameId);
            if (gameDetails != null)
            {
                eogStats.QueueId = gameDetails.QueueId;
            }
            
            if (eogStats.QueueId != RANKED_SOLO_DUO_QUEUE_ID)
            {
                _logger.LogInformation("Skipping non-ranked match (queueId: {QueueId})", eogStats.QueueId);
                StatusChanged?.Invoke(this, "Skipped: Not Ranked Solo/Duo");
                return;
            }
            
            if (_activeProfileId == Guid.Empty)
            {
                _logger.LogWarning("No active profile - skipping sync");
                StatusChanged?.Invoke(this, "Error: No profile available");
                return;
            }
            
            var match = Helpers.DataMapper.MapToMatchEntry(eogStats, _cachedRankedStats, _activeProfileId, gameDetails);
            
            var success = await _apiSyncService.SyncMatchAsync(match);
            
            if (success)
            {
                StatusChanged?.Invoke(this, $"Synced: {match.Champion} ({(match.Win ? "Win" : "Loss")})");
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
    
    public override void Dispose()
    {
        _lcuService.ConnectionChanged -= OnLcuConnectionChanged;
        _lcuService.GameEnded -= OnGameEnded;
        base.Dispose();
    }
}

