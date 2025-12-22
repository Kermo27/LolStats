using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using LolStatsTracker.TrayApp.Models.Lcu;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Websocket.Client;

namespace LolStatsTracker.TrayApp.Services;

public class LcuService : IDisposable
{
    private readonly ILogger<LcuService> _logger;
    private HttpClient? _httpClient;
    private WebsocketClient? _websocketClient;
    private bool _isClientRunning;
    private LcuConnectionInfo? _connectionInfo;
    
    public event EventHandler<bool>? ConnectionChanged;
    public event EventHandler<LcuEndOfGameStats>? GameEnded;
    
    public bool IsConnected => _isClientRunning && _httpClient != null;

    public LcuService(ILogger<LcuService> logger)
    {
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing LCU Service...");
        StartMonitoring();
        await Task.CompletedTask;
    }

    private void StartMonitoring()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    if (!_isClientRunning)
                    {
                        if (Process.GetProcessesByName("LeagueClientUx").Length > 0)
                        {
                            await TryConnectAsync();
                        }
                    }
                    else
                    {
                        if (Process.GetProcessesByName("LeagueClientUx").Length == 0)
                        {
                            _logger.LogInformation("League Client process lost.");
                            await DisconnectAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in LCU monitoring loop");
                }
                
                await Task.Delay(5000);
            }
        });
    }

    private async Task TryConnectAsync()
    {
        try
        {
            var lockfilePath = await GetLockfilePathAsync();
            
            if (string.IsNullOrEmpty(lockfilePath) || !File.Exists(lockfilePath))
            {
                return;
            }

            using var fileStream = new FileStream(lockfilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fileStream);
            var content = await reader.ReadToEndAsync();
            
            var parts = content.Split(':');
            if (parts.Length != 5) return;
            
            var port = int.Parse(parts[2]);
            var password = parts[3];
            var protocol = parts[4];
            
            _connectionInfo = new LcuConnectionInfo
            {
                Port = port,
                Password = password,
                Protocol = protocol
            };
            
            // Setup HttpClient
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri($"{protocol}://127.0.0.1:{port}/")
            };
            
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            
            // Setup WebSocket
            await SetupWebSocketAsync(port, authToken);
            
            _isClientRunning = true;
            _logger.LogInformation("Connected to LCU on port {Port}", port);
            ConnectionChanged?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to LCU");
            await DisconnectAsync();
        }
    }

    private async Task<string?> GetLockfilePathAsync()
    {
        // 1. Try to get from running process command line (using WMI to bypass some permission issues)
        try
        {
            var query = "SELECT CommandLine FROM Win32_Process WHERE Name = 'LeagueClientUx.exe'";
            using var searcher = new System.Management.ManagementObjectSearcher(query);
            using var collection = searcher.Get();
            
            foreach (var item in collection)
            {
                var commandLine = item["CommandLine"]?.ToString();
                if (!string.IsNullOrEmpty(commandLine) && commandLine.Contains("--install-directory="))
                {
                    var args = commandLine.Split("\" --");
                    foreach(var arg in args)
                    {
                        if (arg.StartsWith("install-directory=") || arg.StartsWith("--install-directory="))
                        {
                            var path = arg.Split('=')[1].Trim('"');
                            var lockfile = Path.Combine(path, "lockfile");
                            if (File.Exists(lockfile)) return lockfile;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get lockfile via WMI");
        }

        // 2. Try process MainModule (might fail if access denied)
        try 
        {
            var process = Process.GetProcessesByName("LeagueClientUx").FirstOrDefault();
            if (process != null && process.MainModule?.FileName != null)
            {
                var dir = Path.GetDirectoryName(process.MainModule.FileName);
                if (dir != null)
                {
                    var lockfile = Path.Combine(dir, "lockfile");
                    if (File.Exists(lockfile)) return lockfile;
                }
            }
        }
        catch {}

        // 3. Try Registry
        try
        {
            // Check HKCU/HKLM for LoL install path
            // Note: Registry paths vary, typical is HKEY_CURRENT_USER\Software\Riot Games\RADS or similar
            // But usually just searching drives is reliable enough if WMI fails.
            // Let's rely on standard "Riot Games" folder scan for now as registry is often messy.
        }
        catch {}

        // 4. Fallback: Search common roots on all drives
        var drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed);
        foreach (var drive in drives)
        {
            var potentialPath = Path.Combine(drive.RootDirectory.FullName, "Riot Games", "League of Legends", "lockfile");
            if (File.Exists(potentialPath)) return potentialPath;
        }

        return null;
    }
    
    private async Task SetupWebSocketAsync(int port, string authToken)
    {
        var url = new Uri($"wss://127.0.0.1:{port}/");
        
        var factory = new Func<ClientWebSocket>(() =>
        {
            var ws = new ClientWebSocket();
            ws.Options.RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true;
            ws.Options.SetRequestHeader("Authorization", $"Basic {authToken}");
            return ws;
        });
        
        _websocketClient = new WebsocketClient(url, factory);
        _websocketClient.ReconnectTimeout = TimeSpan.FromSeconds(30);
        
        _websocketClient.ReconnectionHappened.Subscribe(info =>
        {
            _logger.LogInformation("WebSocket connected: {Type}", info.Type);
            _websocketClient.Send("[5, \"OnJsonApiEvent\"]");
        });
        
        _websocketClient.MessageReceived.Subscribe(msg =>
        {
            HandleMessage(msg.Text);
        });
        
        await _websocketClient.Start();
    }
    
    private void HandleMessage(string message)
    {
        try
        {
            if (string.IsNullOrEmpty(message)) return;
            
             // LCU event format: [8, "OnJsonApiEvent", { data, eventType, uri }]
             var jsonNode = JsonNode.Parse(message);
             if (jsonNode is JsonArray arr && arr.Count >= 3 && arr[0]?.GetValue<int>() == 8)
             {
                 var eventData = arr[2];
                 var uri = eventData?["uri"]?.GetValue<string>();
                 
                 if (uri == "/lol-end-of-game/v1/eog-stats-block")
                 {
                     var data = eventData?["data"];
                     if (data != null)
                     {
                         // Use Newtonsoft for model deserialization to respect [JsonProperty] attributes
                         var eogStats = JsonConvert.DeserializeObject<LcuEndOfGameStats>(data.ToJsonString());
                         if (eogStats != null)
                         {
                             GameEnded?.Invoke(this, eogStats);
                         }
                     }
                 }
             }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling LCU message");
        }
    }

    private async Task DisconnectAsync()
    {
        _isClientRunning = false;
        _httpClient?.Dispose();
        _httpClient = null;
        _websocketClient?.Dispose();
        _websocketClient = null;
        ConnectionChanged?.Invoke(this, false);
        await Task.CompletedTask;
    }

    public async Task<LcuSummoner?> GetCurrentSummonerAsync()
    {
        if (_httpClient == null) return null;
        try
        {
             var response = await _httpClient.GetAsync("/lol-summoner/v1/current-summoner");
             if (response.IsSuccessStatusCode)
             {
                 var json = await response.Content.ReadAsStringAsync();
                 return JsonConvert.DeserializeObject<LcuSummoner>(json);
             }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get summoner");
        }
        return null;
    }
    
    public async Task<LcuQueueStats?> GetRankedStatsAsync()
    {
        if (_httpClient == null) return null;
        try
        { 
             var response = await _httpClient.GetAsync("/lol-ranked/v1/current-ranked-stats");
             if (response.IsSuccessStatusCode)
             {
                 var json = await response.Content.ReadAsStringAsync();
                 var stats = JsonConvert.DeserializeObject<LcuRankedStats>(json);
                 return stats?.Queues.FirstOrDefault(q => q.QueueType == "RANKED_SOLO_5x5");
             }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get ranked stats");
        }
        return null;
    }

    public async Task<LcuGame?> GetGameDetailsAsync(long gameId)
    {
        if (_httpClient == null) return null;
        try
        {
            var response = await _httpClient.GetAsync($"/lol-match-history/v1/games/{gameId}");
             if (response.IsSuccessStatusCode)
             {
                 var json = await response.Content.ReadAsStringAsync();
                 return JsonConvert.DeserializeObject<LcuGame>(json);
             }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get game details");
        }
        return null;
    }

    public void Dispose()
    {
        DisconnectAsync().Wait();
    }
}

public class LcuConnectionInfo
{
    public int Port { get; set; }
    public string Password { get; set; } = "";
    public string Protocol { get; set; } = "https";
}
