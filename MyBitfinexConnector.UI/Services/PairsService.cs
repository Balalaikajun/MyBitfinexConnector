using System.Net.Http;
using System.Text.Json;
using MyBitfinexConnector.UI.Interfaces;

namespace MyBitfinexConnector.UI.Services;

internal class PairsService : IPairsService
{
    private List<string>? _cachedPairs;

    public PairsService()
    {
        _ = GetPairsAsync();
    }

    public async Task<IEnumerable<string>> GetPairsAsync()
    {
        if (_cachedPairs == null)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync("https://api-pub.bitfinex.com/v2/conf/pub:list:pair:exchange")
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var element = JsonSerializer.Deserialize<JsonElement>(json);

            _cachedPairs = element[0].EnumerateArray().Select(e => e.GetString()).ToList();
        }

        return _cachedPairs;
    }
}