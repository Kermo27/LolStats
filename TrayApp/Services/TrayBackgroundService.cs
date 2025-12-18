using LolStatsTracker.TrayApp.Models;
using LolStatsTracker.TrayApp.Models.Lcu;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LolStatsTracker.TrayApp.Services;

public class TrayBackgroundService : BackgroundService
{
    private readonly ILogger<TrayBackgroundService> _logger;
    private readonly LcuConnectionManager _connectionManager;
    private readonly LcuApiClient _lcuApiClient;
    private readonly LcuEventListener _eventListener;
    private readonly ApiSyncService _apiSyncService;
    private readonly AppConfiguration _config;
    
    private bool _isLcuConnected = false;
    private LcuQueueStats? _cachedRankedStats;
    private readonly HashSet<long> _processedGameIds = new();
    private string? _currentSummonerName;
    
    public event EventHandler<string>? StatusChanged;
    
    public TrayBackgroundService(
        ILogger<TrayBackgroundService> logger,
        LcuConnectionManager connectionManager,
        LcuApiClient lcuApiClient,
        LcuEventListener eventListener,
        ApiSyncService apiSyncService,
        IOptions<AppConfiguration> config)
    {
        _logger = logger;
        _connectionManager = connectionManager;
        _lcuApiClient = lcuApiClient;
        _eventListener = eventListener;
        _apiSyncService = apiSyncService;
        _config = config.Value;
        
        // Subscribe to events
        _connectionManager.Connected += OnLcuConnected;
        _connectionManager.Disconnected += OnLcuDisconnected;
        _eventListener.GameEnded += OnGameEnded;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tray Background Service started");
        StatusChanged?.Invoke(this, "Service started - waiting for League Client");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Check LCU connection every interval
                await _connectionManager.TryConnectAsync();
                
                // If connected, cache ranked stats and check for account changes
                if (_isLcuConnected)
                {
                    _cachedRankedStats = await _lcuApiClient.GetRankedStatsAsync();
                    
                    // Check if summoner changed (account switch detection)
                    await CheckForAccountChangeAsync();
                }
                
                await Task.Delay(TimeSpan.FromSeconds(_config.CheckIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
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
            var summoner = await _lcuApiClient.GetCurrentSummonerAsync();
            if (summoner != null)
            {
                var displayName = $"{summoner.GameName}#{summoner.TagLine}";
                
                if (_currentSummonerName != displayName)
                {
                    _currentSummonerName = displayName;
                    _logger.LogInformation("Account changed to: {DisplayName}", displayName);
                    StatusChanged?.Invoke(this, $"Connected - {displayName}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check for account change");
        }
    }
    
    private async void OnLcuConnected(object? sender, LcuConnectionInfo connectionInfo)
    {
        _isLcuConnected = true;
        _logger.LogInformation("LCU Connected - initializing services");
        StatusChanged?.Invoke(this, "Connected to League Client");
        
        try
        {
            // Initialize API client
            _lcuApiClient.Initialize(connectionInfo);
            
            // Start WebSocket listener
            await _eventListener.StartAsync(connectionInfo);
            
            // Get initial summoner info
            var summoner = await _lcuApiClient.GetCurrentSummonerAsync();
            if (summoner != null)
            {
                _logger.LogInformation("Logged in as: {DisplayName}", summoner.GameName);
                StatusChanged?.Invoke(this, $"Connected - {summoner.GameName}#{summoner.TagLine}");
            }
            
            // Cache ranked stats
            _cachedRankedStats = await _lcuApiClient.GetRankedStatsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing LCU services");
        }
    }
    
    private async void OnLcuDisconnected(object? sender, EventArgs e)
    {
        _isLcuConnected = false;
        _logger.LogInformation("LCU Disconnected");
        StatusChanged?.Invoke(this, "Disconnected - waiting for League Client");
        
        try
        {
            await _eventListener.StopAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping event listener");
        }
    }
    
    private async void OnGameEnded(object? sender, LcuEndOfGameStats eogStats)
    {
        _logger.LogInformation("Game ended - processing match data for GameId: {GameId}", eogStats.GameId);
        
        // Prevent duplicate processing - add to set BEFORE processing
        if (!_processedGameIds.Add(eogStats.GameId))
        {
            _logger.LogInformation("Match {GameId} already processed - skipping", eogStats.GameId);
            return;
        }
        
        StatusChanged?.Invoke(this, "Processing match...");
        
        try
        {
            // Filter: Only Ranked Solo/Duo (queueId 420)
            const int RANKED_SOLO_DUO_QUEUE_ID = 420;
            
            // Fetch game details from match history to get reliable lane/role info
            _logger.LogInformation("Fetching game details for GameId: {GameId}", eogStats.GameId);
            var gameDetails = await _lcuApiClient.GetGameDetailsAsync(eogStats.GameId);
            if (gameDetails != null)
            {
                eogStats.QueueId = gameDetails.QueueId;
                _logger.LogInformation("Retrieved detailed game info for GameId: {GameId}", eogStats.GameId);
            }
            
            if (eogStats.QueueId != RANKED_SOLO_DUO_QUEUE_ID)
            {
                _logger.LogInformation("Skipping non-ranked match (queueId: {QueueId})", eogStats.QueueId);
                StatusChanged?.Invoke(this, "Skipped: Not Ranked Solo/Duo");
                return;
            }
            
            if (_config.ProfileId == Guid.Empty)
            {
                _logger.LogWarning("Profile ID not configured - skipping sync");
                StatusChanged?.Invoke(this, "Error: Profile ID not configured");
                return;
            }
            
            // Map LCU data to MatchEntry, passing gameDetails for better mapping
            var match = Helpers.DataMapper.MapToMatchEntry(eogStats, _cachedRankedStats, _config.ProfileId, gameDetails);
            
            _logger.LogInformation("Match mapped: {Champion} {Result} ({KDA})", 
                match.Champion, 
                match.Win ? "Win" : "Loss", 
                match.KdaDisplay);
            
            // Sync to API
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
        _eventListener.Dispose();
        _lcuApiClient.Dispose();
        base.Dispose();
    }
}
