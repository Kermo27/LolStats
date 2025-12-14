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
                
                // If connected, cache ranked stats periodically
                if (_isLcuConnected)
                {
                    _cachedRankedStats = await _lcuApiClient.GetRankedStatsAsync();
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
        _logger.LogInformation("Game ended - processing match data");
        StatusChanged?.Invoke(this, "Processing match...");
        
        try
        {
            // Filter: Only Ranked Solo/Duo (queueId 420)
            const int RANKED_SOLO_DUO_QUEUE_ID = 420;
            
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
            
            // Map LCU data to MatchEntry
            var match = Helpers.DataMapper.MapToMatchEntry(eogStats, _cachedRankedStats, _config.ProfileId);
            
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
