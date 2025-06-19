using TestHQ.Abstractions.Models;

namespace TestHQ.Abstractions.Interfaces;

public interface IRestClient
{
    Task<IEnumerable<Trade>> GetTradesAsync(string pair, int limit = 125, DateTimeOffset? from = null, DateTimeOffset? to = null);

    Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec = 60, DateTimeOffset? from = null, DateTimeOffset? to = null, int? limit = null);
    
    Task<Ticker> GetTickersAsync(string pair);
}