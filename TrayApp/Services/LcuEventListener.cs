using System.Net.WebSockets;
using System.Text;
using LolStatsTracker.TrayApp.Models;
using LolStatsTracker.TrayApp.Models.Lcu;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Websocket.Client;

namespace LolStatsTracker.TrayApp.Services;

public class LcuEventListener : IDisposable
{
    private readonly ILogger<LcuEventListener> _logger;
    private WebsocketClient? _websocketClient;
    private LcuConnectionInfo? _connectionInfo;
    
    public event EventHandler<LcuEndOfGameStats>? GameEnded;
    
    public LcuEventListener(ILogger<LcuEventListener> logger)
    {
        _logger = logger;
    }
    
    public async Task StartAsync(LcuConnectionInfo connectionInfo)
    {
        _connectionInfo = connectionInfo;
        
        try
        {
            // Create WebSocket URL with authentication
            var uri = new Uri($"{connectionInfo.WebSocketUrl}");
            
            var factory = new Func<ClientWebSocket>(() =>
            {
                var ws = new ClientWebSocket();
                ws.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                
                // Add Basic Auth header
                var authToken = connectionInfo.GetBasicAuthToken();
                ws.Options.SetRequestHeader("Authorization", $"Basic {authToken}");
                
                return ws;
            });
            
            _websocketClient = new WebsocketClient(uri, factory);
            
            _websocketClient.ReconnectTimeout = TimeSpan.FromSeconds(30);
            _websocketClient.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);
            
            _websocketClient.ReconnectionHappened.Subscribe(info =>
            {
                _logger.LogInformation("WebSocket reconnected: {Type}", info.Type);
                
                // Subscribe to end of game events
                SubscribeToEndOfGameEvents();
            });
            
            _websocketClient.MessageReceived.Subscribe(msg =>
            {
                if (!string.IsNullOrEmpty(msg.Text))
                    HandleMessage(msg.Text);
            });
            
            _websocketClient.DisconnectionHappened.Subscribe(info =>
            {
                _logger.LogWarning("WebSocket disconnected: {Type}", info.Type);
            });
            
            await _websocketClient.Start();
            _logger.LogInformation("LCU WebSocket listener started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting WebSocket listener");
        }
    }
    
    private void SubscribeToEndOfGameEvents()
    {
        // LCU WebSocket subscription format: [5, "OnJsonApiEvent"]
        var subscribeMessage = "[5, \"OnJsonApiEvent\"]";
        _websocketClient?.Send(subscribeMessage);
        _logger.LogDebug("Subscribed to LCU events");
    }
    
    private void HandleMessage(string message)
    {
        try
        {
            var json = JArray.Parse(message);
            
            // LCU event format: [8, "OnJsonApiEvent", { data, eventType, uri }]
            if (json.Count < 3 || json[0].Value<int>() != 8)
                return;
            
            var eventData = json[2];
            var uri = eventData["uri"]?.Value<string>();
            
            if (uri == null)
                return;
            
            // Check if this is end of game event
            if (uri.Contains("/lol-end-of-game/v1/eog-stats-block"))
            {
                _logger.LogInformation("End of game event received");
                
                var data = eventData["data"];
                if (data != null && data.Type != JTokenType.Null)
                {
                    var eogStats = data.ToObject<LcuEndOfGameStats>();
                    if (eogStats != null)
                    {
                        GameEnded?.Invoke(this, eogStats);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling WebSocket message: {Message}", message);
        }
    }
    
    public async Task StopAsync()
    {
        try
        {
            if (_websocketClient != null)
            {
                await _websocketClient.Stop(WebSocketCloseStatus.NormalClosure, "Closing");
                _websocketClient.Dispose();
                _websocketClient = null;
                _logger.LogInformation("LCU WebSocket listener stopped");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping WebSocket listener");
        }
    }
    
    public void Dispose()
    {
        StopAsync().Wait();
    }
}
