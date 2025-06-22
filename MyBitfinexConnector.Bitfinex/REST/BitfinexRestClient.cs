using System.Text.Json;
using System.Web;
using MyBitfinexConnector.Abstractions.Interfaces;
using MyBitfinexConnector.Abstractions.Models;
using TestConnector.Bitfinex.Common;
using TestConnector.Bitfinex.Parsing;

namespace TestConnector.Bitfinex.REST;

public class BitfinexRestClient : IRestClient
{
    private readonly HttpClient _httpClient;

    public BitfinexRestClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api-pub.bitfinex.com/v2/")
        };
    }

    public async Task<IEnumerable<Trade>> GetTradesAsync(string pair, int limit = 125, DateTimeOffset? from = null,
        DateTimeOffset? to = null)
    {
        var uri = BuildUri($"trades/{pair.FormatTradePair()}/hist", new Dictionary<string, string?>
        {
            { "limit", limit.ToString() },
            { "start", from?.ToUnixTimeMilliseconds().ToString() },
            { "end", to?.ToUnixTimeMilliseconds().ToString() }
        });

        var response = await _httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var root = JsonSerializer.Deserialize<IEnumerable<JsonElement>>(json);

        return root.Select(x => TradeParser.FromJson(x, pair));
    }

    public async Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec, DateTimeOffset? from = null,
        DateTimeOffset? to = null, int? limit = null)
    {
        return await GetCandlesAsync(pair, CandlePeriodMapper.ToStringPeriod(periodInSec), from, to, limit);
    }

    public async Task<IEnumerable<Candle>> GetCandlesAsync(string pair, string period = "1m",
        DateTimeOffset? from = null,
        DateTimeOffset? to = null, int? limit = null)
    {
        var uri = BuildUri($"candles/trade%3A{period}%3A{pair.FormatTradePair()}/hist", new Dictionary<string, string?>
        {
            { "limit", limit.ToString() },
            { "start", from?.ToUnixTimeMilliseconds().ToString() },
            { "end", to?.ToUnixTimeMilliseconds().ToString() }
        });

        var response = await _httpClient.GetAsync(uri);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var root = JsonSerializer.Deserialize<IEnumerable<JsonElement>>(json);

        return root.Select(x => CandleParser.FromJson(x, pair));
    }

    public async Task<Ticker> GetTickerAsync(string pair)
    {
        var response = await _httpClient.GetAsync($"ticker/{pair.FormatTradePair()}");

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var root = JsonSerializer.Deserialize<JsonElement>(json);

        return TickerParser.FromJson(root, pair);
    }

    /// <summary>
    ///     Метод для постройки URI c query.
    /// </summary>
    /// <param name="path">Конечная точка.</param>
    /// <param name="queryParams">Словарь параметр-значение.</param>
    private Uri BuildUri(string path, Dictionary<string, string?> queryParams)
    {
        var builder = new UriBuilder(_httpClient.BaseAddress + path);
        var query = HttpUtility.ParseQueryString(builder.Query);

        foreach (var kvp in queryParams)
            if (!string.IsNullOrEmpty(kvp.Value))
                query[kvp.Key] = kvp.Value;

        builder.Query = query.ToString();
        return builder.Uri;
    }
}