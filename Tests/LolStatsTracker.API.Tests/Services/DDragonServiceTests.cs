using LolStatsTracker.API.Services.DDragonService;
using LolStatsTracker.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace LolStatsTracker.API.Tests.Services;

public class DDragonServiceTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly DDragonService _service;

    public DDragonServiceTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri("https://ddragon.leagueoflegends.com/")
        };
        _cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<DDragonService>>();
        
        _service = new DDragonService(_httpClient, _cache, logger.Object);
    }

    #region GetLatestVersionAsync Tests

    [Fact]
    public async Task GetLatestVersionAsync_ReturnsVersionFromApi()
    {
        var versions = new List<string> { "14.24.1", "14.23.1", "14.22.1" };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(versions));

        var result = await _service.GetLatestVersionAsync();

        Assert.Equal("14.24.1", result);
    }

    [Fact]
    public async Task GetLatestVersionAsync_CachesResult()
    {
        var versions = new List<string> { "14.24.1" };
        SetupHttpResponse(HttpStatusCode.OK, JsonSerializer.Serialize(versions));

        // First call
        await _service.GetLatestVersionAsync();
        
        // Second call should use cache
        var result = await _service.GetLatestVersionAsync();

        Assert.Equal("14.24.1", result);
        // Verify HTTP was only called once
        _httpHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetLatestVersionAsync_ApiFails_ReturnsFallback()
    {
        SetupHttpResponse(HttpStatusCode.InternalServerError, "");

        var result = await _service.GetLatestVersionAsync();

        Assert.Equal("14.23.1", result); // Fallback version
    }

    #endregion

    #region GetChampionsAsync Tests

    [Fact]
    public async Task GetChampionsAsync_ReturnsChampionData()
    {
        // Setup version response
        var versions = new List<string> { "14.24.1" };
        
        var championsResponse = new DataDragonResponse
        {
            Data = new Dictionary<string, ChampionDto>
            {
                ["Jinx"] = new ChampionDto { Id = "Jinx", Name = "Jinx" },
                ["Vayne"] = new ChampionDto { Id = "Vayne", Name = "Vayne" }
            }
        };

        _httpHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(versions))
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(championsResponse))
            });

        var result = await _service.GetChampionsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Count);
        Assert.True(result.Data.ContainsKey("Jinx"));
    }

    [Fact]
    public async Task GetChampionsAsync_CachesResult()
    {
        var versions = new List<string> { "14.24.1" };
        var championsResponse = new DataDragonResponse
        {
            Data = new Dictionary<string, ChampionDto>
            {
                ["Jinx"] = new ChampionDto { Id = "Jinx", Name = "Jinx" }
            }
        };

        _httpHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(versions))
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(championsResponse))
            });

        // First call
        await _service.GetChampionsAsync();
        
        // Second call should use cache - no additional HTTP calls
        var result = await _service.GetChampionsAsync();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetChampionsAsync_ApiFails_ReturnsNull()
    {
        var versions = new List<string> { "14.24.1" };
        
        _httpHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(versions))
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("")
            });

        var result = await _service.GetChampionsAsync();

        Assert.Null(result);
    }

    #endregion

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _cache.Dispose();
    }
}
