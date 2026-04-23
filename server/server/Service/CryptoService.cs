using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using server.Models;

namespace server.Service;

public class CryptoService : ICryptoService
{
    private const string SupportedCoinsCacheKey = "coingecko-supported-coins";
    private const string SupportedCoinPricesCacheKey = "coingecko-supported-coin-prices";
    private const string CoinsListRelativeUrl = "coins/list?include_platform=false";
    private const string UsdCurrencyCode = "usd";
    private const string RateLimitMessage = "CoinGecko rate limit reached. Please try again shortly.";

    private static readonly TimeSpan SupportedCoinsCacheDuration = TimeSpan.FromHours(6);
    private static readonly TimeSpan SupportedCoinPricesCacheDuration = TimeSpan.FromMinutes(1);
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CryptoService> _logger;
    private readonly IReadOnlyDictionary<string, int> _supportedCoinOrder;

    public CryptoService(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        ILogger<CryptoService> logger,
        CoinGeckoSettings coinGeckoSettings)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _logger = logger;
        _supportedCoinOrder = coinGeckoSettings.SupportedCoinIds
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select((coinId, index) => new { CoinId = coinId, Index = index })
            .ToDictionary(
                item => item.CoinId,
                item => item.Index,
                StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<CryptoAssetDto>> GetAssetsAsync(
        CancellationToken cancellationToken = default)
    {
        var supportedCoins = await GetSupportedCoinsAsync(cancellationToken);

        if (supportedCoins.Count == 0)
        {
            return [];
        }

        var priceLookup = await GetPriceLookupAsync(supportedCoins, cancellationToken);
        var assets = new List<CryptoAssetDto>(supportedCoins.Count);

        foreach (var supportedCoin in supportedCoins)
        {
            if (!priceLookup.TryGetValue(supportedCoin.Id, out var currentPrice))
            {
                continue;
            }

            assets.Add(new CryptoAssetDto(
                supportedCoin.Id,
                supportedCoin.Name,
                supportedCoin.Symbol,
                currentPrice));
        }

        if (assets.Count == 0)
        {
            throw new ExternalServiceException("CoinGecko returned an invalid price response.");
        }

        return assets;
    }

    private async Task<IReadOnlyList<SupportedCoin>> GetSupportedCoinsAsync(
        CancellationToken cancellationToken)
    {
        return await _memoryCache.GetOrCreateAsync(
            SupportedCoinsCacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SupportedCoinsCacheDuration;

                var coinList = await GetRequiredPayloadAsync<List<CoinListItem>>(
                    CoinsListRelativeUrl,
                    invalidResponseMessage: "CoinGecko returned an invalid coin list.",
                    failureMessage: "Unable to retrieve supported crypto assets right now.",
                    cancellationToken);

                return coinList
                    .Where(item =>
                        !string.IsNullOrWhiteSpace(item.Id) &&
                        !string.IsNullOrWhiteSpace(item.Name) &&
                        !string.IsNullOrWhiteSpace(item.Symbol) &&
                        _supportedCoinOrder.ContainsKey(item.Id))
                    .OrderBy(item => _supportedCoinOrder[item.Id!])
                    .Select(item => new SupportedCoin(
                        item.Id!,
                        item.Name!,
                        item.Symbol!.ToUpperInvariant()))
                    .ToArray();
            })
            ?? [];
    }

    private async Task<IReadOnlyDictionary<string, decimal>> GetPriceLookupAsync(
        IReadOnlyList<SupportedCoin> supportedCoins,
        CancellationToken cancellationToken)
    {
        return await _memoryCache.GetOrCreateAsync(
            SupportedCoinPricesCacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SupportedCoinPricesCacheDuration;

                var joinedIds = Uri.EscapeDataString(string.Join(",", supportedCoins.Select(coin => coin.Id)));
                var relativeUrl = $"simple/price?ids={joinedIds}&vs_currencies={UsdCurrencyCode}";
                var payload = await GetRequiredPayloadAsync<Dictionary<string, SimplePriceItem>>(
                    relativeUrl,
                    invalidResponseMessage: "CoinGecko returned an invalid price response.",
                    failureMessage: "Unable to retrieve crypto prices right now.",
                    cancellationToken);

                return payload
                    .Where(item => item.Value.Usd.HasValue)
                    .ToDictionary(
                        item => item.Key,
                        item => item.Value.Usd!.Value,
                        StringComparer.OrdinalIgnoreCase);
            })
            ?? new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<T> GetRequiredPayloadAsync<T>(
        string relativeUrl,
        string invalidResponseMessage,
        string failureMessage,
        CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            using var response = await _httpClient.GetAsync(relativeUrl, cancellationToken);

            if ((int)response.StatusCode == 429)
            {
                throw new ExternalServiceException(RateLimitMessage, statusCode: 429);
            }

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<T>(
                stream,
                SerializerOptions,
                cancellationToken);

            return payload ?? throw new ExternalServiceException(invalidResponseMessage);
        }
        catch (ExternalServiceException)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogError(ex, "CoinGecko request failed for {RelativeUrl}", relativeUrl);
            throw new ExternalServiceException(failureMessage, ex);
        }
    }

    private sealed record SupportedCoin(
        string Id,
        string Name,
        string Symbol);

    private sealed record CoinListItem(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("symbol")] string? Symbol);

    private sealed record SimplePriceItem(
        [property: JsonPropertyName("usd")] decimal? Usd);
}
